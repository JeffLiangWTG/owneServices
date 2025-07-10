using CargoWise.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using WTG.ApplicationLogging.Abstractions;
using WTG.ApplicationLogging.Extensions;

namespace Enterprise.Loader
{
	static class Application
	{
		internal static ServiceProvider ServiceProvider { get; private set; }
		internal static IApplicationLoggerFactory LoggerFactory => ServiceProvider.GetService<IApplicationLoggerFactory>();

		public static void ConfigureApplicationServices()
		{
			var services = new ServiceCollection()
				.AddApplicationLogging(o => o
					.Configure(Product.CargoWise)
					.WithTracing())
				.ConfigureProtectedDataFactoryServices()
				.ConfigureProtectedDataSqlExtensions(ApplicationType.Default);

			ServiceProvider = services.BuildServiceProvider();
		}

		public static void ShutdownApplicationServices()
		{
			ServiceProvider?.Dispose();
			ServiceProvider = null;
		}
	}
}
