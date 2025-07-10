using System;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration;
using CargoWise.DataProtection.Administration.SqlServer;
using Enterprise.Builder.Generator.RestoreDatabase;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.Builder.Generator
{
	public static class Application
	{
#pragma warning disable CW1021
		static IServiceProvider serviceProvider = GetServiceProvider();

		public static IServiceProvider ServiceProvider
		{
			get => serviceProvider;
			set => serviceProvider = value;
		}
#pragma warning restore CW1021

		static IServiceProvider GetServiceProvider()
		{
			var services = new ServiceCollection();
			services.ConfigureProtectedDataFactoryServices();
			services.ConfigureProtectedDataSqlExtensions(ApplicationType.Default);
			services.ConfigureProtectedDataAdministrationServices();
			services.ConfigureProtectedDataSqlServerAdministrationServices();
			services.AddSingleton<IProtectedDataAdministrationSqlExecutionContextManager, SqlExecutionContextManager>();
			return services.BuildServiceProvider();
		}
	}
}
