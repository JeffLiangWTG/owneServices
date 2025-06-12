using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CargoWise.eServices.Encryption.Server.Decryptor;
using Elasticsearch.Net;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Nest;
using Nest.JsonNetSerializer;
using Serilog;
using XH.Framework.GrpcClient;
using XH.XT.Monitoring.HealthCheckService.HealthChecks;
using XH.XT.Monitoring.HealthCheckService.XTRESTAPI;

namespace XH.XT.Monitoring.HealthCheckService
{
	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposable objects are used to setup services.")]
	public static class Startup
	{
		public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
		{
			services.AddHttpContextAccessor();
			services.AddMemoryCache();
			services.AddHealthChecks().AddTypeActivatedCheck<ServiceHealthCheck>("ServiceHealthCheck");
			services.AddHealthChecks().AddTypeActivatedCheck<CTMHealthCheck>($"ClickToMonitorHealthCheck{XTRestSettings.DirectxT}", XTRestSettings.DirectxT);
			services.AddHealthChecks().AddTypeActivatedCheck<CTMHealthCheck>($"ClickToMonitorHealthCheck{XTRestSettings.XHub}", XTRestSettings.XHub);
			services.AddHealthChecks().AddTypeActivatedCheck<GrpcHealthCheck>("gRPCHealthCheck");
			services.AddHealthChecks().AddTypeActivatedCheck<TrackingMessageHealthCheck>("TrackingMessageHealthCheck");
			services.AddHealthChecks().AddTypeActivatedCheck<CustomsServersHealthCheck>(nameof(CustomsServersHealthCheck));
			services.AddSingleton<ILogger>(Log.Logger);
			var xTClient = new XtClient(context?.Configuration["GrpcConfigurationPath"]);
			services.AddSingleton<IXtClient>(xTClient);
			services.AddOptions<HealthCheckSettings>()
				.Bind(context.Configuration.GetSection("HealthCheckSettings"))
				.ValidateDataAnnotations()
				.ValidateOnStart();
			ValidateTrackingMessages(context, services);
			ValidateCustomsWebServers(context, services);
			ValidateCustomsMailServers(context, services);

			services.Configure<XTRestSettings>(XTRestSettings.DirectxT, context.Configuration.GetSection($"XTRestSettings:{XTRestSettings.DirectxT}"));
			services.Configure<XTRestSettings>(XTRestSettings.XHub, context.Configuration.GetSection($"XTRestSettings:{XTRestSettings.XHub}"));
			services.AddSingleton(GetElasticClient(context.Configuration));
			services.AddSingleton<XTRestClient>();

			services.Configure<HttpClientSettings>(context.Configuration.GetSection("HttpClient"));
			var httpClientSettings = context.Configuration.GetSection("HttpClient").Get<HttpClientSettings>();
			var httpClient = new HttpClient
			{
				Timeout = TimeSpan.FromSeconds(httpClientSettings.TimeoutInSeconds)
			};
			services.AddSingleton(httpClientSettings);
			services.AddSingleton<HttpMessageInvoker>(httpClient);

			services.AddSingleton<IDateTimeProvider>(new DateTimeProvider());
			services.AddHttpClient();
			services.AddSingleton<ISmtpClient, SmtpClient>();
		}

		public static void ValidateTrackingMessages(HostBuilderContext context, IServiceCollection services)
		{
			var trackingMessagesSection = context?.Configuration.GetSection("HealthCheckSettings:TrackingMessages");
			foreach (var trackingMessage in trackingMessagesSection.GetChildren())
			{
				services.AddOptions<TrackingMessage>(trackingMessage.Key)
					.Bind(trackingMessage)
					.ValidateDataAnnotations()
					.ValidateOnStart();
			}
		}

		public static void ValidateCustomsWebServers(HostBuilderContext context, IServiceCollection services)
		{
			var customWebServersSection = context?.Configuration.GetSection("HealthCheckSettings:CustomsWebServers");
			foreach (var server in customWebServersSection.GetChildren())
			{
				services.AddOptions<CustomsWebServer>(server.Key)
					.Bind(server)
					.ValidateDataAnnotations()
					.ValidateOnStart();
			}
		}

		public static void ValidateCustomsMailServers(HostBuilderContext context, IServiceCollection services)
		{
			var customMailServersSection = context?.Configuration.GetSection("HealthCheckSettings:CustomsMailServers");
			foreach (var server in customMailServersSection.GetChildren())
			{
				services.AddOptions<CustomsMailServer>(server.Key)
					.Bind(server)
					.ValidateDataAnnotations()
					.ValidateOnStart();
			}
		}

		public static void Configure(IApplicationBuilder app)
		{
			var settings = app?.ApplicationServices.GetRequiredService<IConfiguration>().GetSection("Options").Get<IDictionary<string, string[]>>();
			foreach (var pair in settings)
			{
				app.MapWhen((context) =>
					{
						var pathBase = context.Request.PathBase.Value;
						var path = context.Request.Path.Value;
						return $"{pathBase}{path}".Equals(pair.Key, StringComparison.OrdinalIgnoreCase);
					},
					(a) =>
					{
						a.UseMiddleware<HealthCheckMiddleware>(Options.Create(new HealthCheckOptions()
						{
							ResultStatusCodes =
							{
								[HealthStatus.Healthy] = StatusCodes.Status200OK,
								[HealthStatus.Degraded] = StatusCodes.Status200OK,
								[HealthStatus.Unhealthy] = StatusCodes.Status200OK
							},
							Predicate = healthCheck => pair.Value.Contains(healthCheck.Name),
							ResponseWriter = HealthCheckHelper.WriteResponse,
						}));
					});
			}
			app.MapWhen(context =>
			{
				var pathBase = context.Request.PathBase.Value;
				var path = context.Request.Path.Value;
				var match = XTRestSettings.ServerNameRegex.Match($"{pathBase}{path}");

				if (match.Success)
				{
					context.Items["ServerName"] = match.Groups[1].Value;
					return true;
				}
				return false;
			},
			appBuilder =>
			{
				appBuilder.UseMiddleware<HealthCheckMiddleware>(Options.Create(new HealthCheckOptions()
				{
					ResultStatusCodes =
			{
				[HealthStatus.Healthy] = StatusCodes.Status200OK,
				[HealthStatus.Degraded] = StatusCodes.Status200OK,
				[HealthStatus.Unhealthy] = StatusCodes.Status200OK
			},
					Predicate = healthCheck =>
					{
						var serverName = appBuilder.ApplicationServices
							.GetRequiredService<IHttpContextAccessor>().HttpContext?.Items["ServerName"]?.ToString();
						return healthCheck.Name.Equals($"ClickToMonitorHealthCheck{serverName}", StringComparison.OrdinalIgnoreCase);
					},
					ResponseWriter = HealthCheckHelper.WriteResponse,
				}));
			});
		}

		static IElasticClient GetElasticClient(IConfiguration configuration)
		{
			var pool = new SingleNodeConnectionPool(new Uri(configuration["ElasticEndpoint"]));
			var connectionSettings = new ConnectionSettings(pool, sourceSerializer: (builtin, settings) => new JsonNetSerializer(builtin, settings)).DisablePing();
			connectionSettings.ApiKeyAuthentication(new ApiKeyAuthenticationCredentials(EhubServerDecryptor.Decrypt(configuration["ElasticApiKey"])));
			connectionSettings.DisableDirectStreaming();
			connectionSettings.DefaultFieldNameInferrer(p => p);
			connectionSettings.ThrowExceptions();
			connectionSettings.RequestTimeout(TimeSpan.FromSeconds(30));
			return new ElasticClient(connectionSettings);
		}
	}
}
