﻿using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;
using Weather.Model;

namespace Weather.Services
{
    public class WeatherService : Weather.WeatherBase
    {
        private IMemoryCache _cache;

        public WeatherService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public override async Task GetWeatherStream(Empty request, IServerStreamWriter<WeatherResponse> responseStream, ServerCallContext context)
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                var cachedForecast = _cache.Get<Forecast>(Constants.LATEST_FORECAST_CACHE_KEY);
                await responseStream.WriteAsync(GetCurrentWeatherResponse(cachedForecast));
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }

        public override Task<WeatherResponse> GetWeather(Empty request, ServerCallContext context)
        {
            var forecast = _cache.Get<Forecast>(Constants.LATEST_FORECAST_CACHE_KEY);
            return Task.FromResult(GetCurrentWeatherResponse(forecast));
        }

        public static WeatherResponse GetCurrentWeatherResponse(Forecast forecast)
        {
            if (forecast == null)
            {
                return new WeatherResponse()
                {
                    WeatherText = "Sunny",
                    IsDayTime = true,
                    Pressure = 29.99f,
                    RelativeHumidity = 45,
                    RetrievedTime = Timestamp.FromDateTime(DateTime.UtcNow),
                    Temperature = 57,
                    UvIndex = 2,
                    WeatherIcon = 1,
                    WeartherUri = "https://developer.accuweather.com/sites/default/files/01-s.png",
                    WindSpeed = 5.8f,
                    WindDirection = "SSE",
                    Past6HourMax = 57,
                    Past6HourMin = 42,
                    Location = "Seattle"
                };
            }

            return new WeatherResponse()
            {
                WeatherText = forecast.WeatherText,
                IsDayTime = forecast.IsDayTime,
                Pressure = forecast.Pressure.Metric.Value,
                RelativeHumidity = forecast.RelativeHumidity,
                RetrievedTime = Timestamp.FromDateTime(DateTime.UtcNow),
                Temperature = forecast.Temperature.Metric.Value,
                UvIndex = forecast.UVIndex,
                WeatherIcon = forecast.WeatherIcon,
                WeartherUri = $"https://developer.accuweather.com/sites/default/files/{forecast.WeatherIcon:D2}-s.png",
                WindSpeed = forecast.Wind.Speed.Metric.Value,
                WindDirection = forecast.Wind.Direction.English,
                Past6HourMax = forecast.TemperatureSummary.Past6HourRange.Maximum.Metric.Value,
                Past6HourMin = forecast.TemperatureSummary.Past6HourRange.Minimum.Metric.Value,
                Location = "Santa Cruz de Tenerife"
            };
        }
    }
}
