using System;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration;
using CargoWise.DataProtection.Administration.SqlServer;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.Server.Setup
{
	internal static class Application
	{
		internal static ServiceProvider ServiceProvider { get; private set; }

		internal static void ConfigureApplicationServices(Action<IServiceCollection> configureServices = null)
		{
			var services = new ServiceCollection();
			services.ConfigureProtectedDataFactoryServices();
			services.ConfigureProtectedDataSqlExtensions(ApplicationType.Default);
			services.ConfigureProtectedDataAdministrationServices();
			services.ConfigureProtectedDataSqlServerAdministrationServices();
			services.AddSingleton<IProtectedDataAdministrationSqlExecutionContextManager, CargowiseSetupSqlContextManager>();

			configureServices?.Invoke(services);

			ServiceProvider = services.BuildServiceProvider();
		}
	}
}
