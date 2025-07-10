using System;
#if NET
using System.Runtime.CompilerServices;
using CargoWise;
#endif
using Microsoft.Extensions.DependencyInjection;
using ServiceManager.Host.CW;
using ServiceManager.Logging.CW;

namespace Enterprise.ServiceManager.Host
{
	class Program
	{
#if NET
		[ModuleInitializer]
		public static void InitializeModule()
		{
			NetCoreAssemblyResolver.Setup();
		}
#endif

		[STAThread]
		public static int Main(string[] args)
		{
			ConfigureLogging();

			using var serviceProvider = CreateServiceProvider(args);

			return serviceProvider.GetRequiredService<ServiceManagerHost>().Run();

			static void ConfigureLogging()
			{
				LoggerConfiguration.InitializeLoggingConsole();
			}

			static ServiceProvider CreateServiceProvider(string[] args)
			{
				return new ServiceCollection()
					.AddRegistrations(args)
					.BuildServiceProvider();
			}
		}
	}
}
