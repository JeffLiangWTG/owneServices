using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.eHub.Products.TWCustoms.TWCustomsGatewayAdapter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CargoWise.eHub.Products.TWCustoms.TWCustomsGateway
{
	public class Startup
	{
		public Startup(IWebHostEnvironment env)
		{
			var configurationBuilder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json")
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", false, true);
			Configuration = configurationBuilder.Build();
			Log.Logger = new LoggerConfiguration()
				.ReadFrom.Configuration(Configuration)
				.CreateLogger();
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton(Log.Logger);
			services.AddControllers();
			services.AddMvc()
				.AddXmlSerializerFormatters()
				.AddXmlDataContractSerializerFormatters();
			var fileManager = new FileManager();
			services.TryAddSingleton(Configuration);
			services.TryAddSingleton<IFileManager>(fileManager);
			services.TryAddSingleton<IHttpClient>(new HttpClientWrapper());
			services.TryAddSingleton<IPluginManager>(new PluginManager(fileManager, Log.Logger));
			services.AddHealthChecks();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseHsts();
			}

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});

			app.UseHealthChecks("/wtg/status", new HealthCheckOptions()
			{
				ResponseWriter = CustomResponseWriter
			});
		}

		private static Task CustomResponseWriter(HttpContext context, HealthReport healthReport)
		{
			var healthStatusName = healthReport.Status switch
			{
				HealthStatus.Unhealthy => "ERROR",
				HealthStatus.Degraded => "WARNING",
				HealthStatus.Healthy => "INFO",
				_ => throw new ArgumentOutOfRangeException(nameof(healthReport.Status),
					$"Unknown healthStatus with value '{healthReport.Status}' was provided")
			};

			var description = "";
			description = healthStatusName == "INFO" ? "Service is alive." : healthReport.Entries.Aggregate(description, (current, entry) => current + (entry.Value.Description + " "));

			var result = $"{healthStatusName}(TWCustomsGateway): {description}";
			context.Response.ContentType = "text/plain; charset=utf-8";
			return context.Response.WriteAsync(result);
		}
	}
}