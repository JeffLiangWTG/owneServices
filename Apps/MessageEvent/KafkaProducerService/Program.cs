using System;
using System.IO;
using System.Threading.Tasks;
using CargoWise.eHub.MessageEvent.KafkaProducerService.DI;
using CargoWise.eHub.MessageEvent.KafkaProducerService.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CargoWise.eHub.MessageEvent.KafkaProducerService
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

			var configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.Build();

			Log.Logger = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.Enrich.WithMachineName()
				.Enrich.WithProcessId()
				.Enrich.WithThreadId()
				.Enrich.With<LogLevelEnricher>()
				.Enrich.With<ContextSourceReplaceEnricher>()
				.ReadFrom.Configuration(configuration)
				.CreateBootstrapLogger();

			var loggerFactory = LoggerFactory.Create(builder =>
			{
				builder.AddSerilog();
			});
			var logger = loggerFactory.CreateLogger<Program>();

			logger.eHubLog(LogLevel.Information, "", "", "", "Starting up");
	
			try
			{
				await Host.CreateDefaultBuilder(args)
					.ConfigureAppConfiguration(builder =>
					{
						builder.AddEnvironmentVariables(prefix: Constants.EnvironmentVariablePrefix);
					})
					.ConfigureServices(services =>
					{
						services.AddAppOptions();
						services.AddAppServices();
						services.AddHostedService<HostedService.KafkaProducerService>();
					})
					.UseSerilog((ctx, lc) => lc
						.Enrich.FromLogContext()
						.Enrich.WithMachineName()
						.Enrich.WithProcessId()
						.Enrich.WithThreadId()
						.Enrich.With<LogLevelEnricher>()
						.Enrich.With<ContextSourceReplaceEnricher>()
						.ReadFrom.Configuration(ctx.Configuration))
					.UseWindowsService()
					.Build()
					.RunAsync()
					.ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				logger.eHubLog(LogLevel.Critical, "", "", "", ex, "Unhandled exception");
			}
			finally
			{
				logger.eHubLog(LogLevel.Information, "", "", "", "Shutting down");
				Log.CloseAndFlush();
			}
		}
	}
}
