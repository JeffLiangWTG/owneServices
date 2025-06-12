using System;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Serilog;

namespace CargoWise.eHub.Products.TWCustoms.TWCustomsGateway
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.Build();

			var bootstrapLogger = new LoggerConfiguration()
				.ReadFrom.Configuration(configuration)
				.CreateLogger();

			try
			{
				var host = CreateHostBuilder(args);
				host.UseSerilog();
				host.Build().Run();
			}
			catch (Exception ex)
			{
				bootstrapLogger.Fatal(ex, "Host terminated unexpectedly");
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}