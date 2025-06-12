using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;

namespace XH.XT.Monitoring.HealthCheckService.Tests
{
	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	internal class CustomWebApplicationFactory : WebApplicationFactory<Program>
	{
		readonly IEnumerable<(Type, object)> serviceMocks;
		readonly Mock<IWebHostEnvironment> envMock;
		readonly Action<IApplicationBuilder> customAppConfigureAction;
		private readonly Dictionary<string, string> healthCheckOptions;
		public string ExtraConfigurationJsonFile { get; init; } = string.Empty;
		public CustomWebApplicationFactory(IEnumerable<(Type Type, object Object)> serviceMocks, Action<IApplicationBuilder> customAppConfigureAction, Dictionary<string, string> healthCheckOptions)
		{
			this.serviceMocks = serviceMocks;
			this.customAppConfigureAction = customAppConfigureAction;
			envMock = new Mock<IWebHostEnvironment>();
			envMock.Setup(x => x.EnvironmentName).Returns("IntegrationTest");
			this.healthCheckOptions = healthCheckOptions;
		}

		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			base.ConfigureWebHost(builder);
			if (!string.IsNullOrEmpty(ExtraConfigurationJsonFile))
			{
				var config = new ConfigurationBuilder().AddJsonStream(LoadFile(ExtraConfigurationJsonFile)).Build();
				builder.UseConfiguration(config);
			}
			builder.ConfigureAppConfiguration((hostingContext, config) =>
			{
				var env = hostingContext.HostingEnvironment;
				config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
				config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
				config.AddInMemoryCollection(healthCheckOptions.ToArray());
			});
			builder.UseEnvironment("IntegrationTest");
			builder.UseContentRoot(Path.GetDirectoryName(GetType().Assembly.Location)!);
			builder.ConfigureServices(services =>
			{
				if (serviceMocks != null && serviceMocks.Any())
				{
					foreach (var (serviceType, instance) in serviceMocks)
					{
						var currentInstance = services.SingleOrDefault(s => s.ServiceType == serviceType && (s.ImplementationType == null || $"{s.ImplementationType?.Name}Proxy" == instance.GetType().Name));
						if (currentInstance != null)
						{
							services.Remove(currentInstance);
						}

						if (serviceType.GetInterface(typeof(IHealthCheck).FullName!) != null)
						{
							services.AddHealthChecks().AddCheck(serviceType.Name + "Test", (IHealthCheck)instance);
						}
						else
						{
							services.AddSingleton(serviceType, instance);
						}
					}
				}
			}).Configure(app =>
			{
				customAppConfigureAction?.Invoke(app);
				Startup.Configure(app);
			});
		}

		public static Stream LoadFile(string fileName)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream($"XH.XT.Monitoring.HealthCheckService.IntegrationTests.xTHealthCheck.{fileName}");
		}
	}
}
