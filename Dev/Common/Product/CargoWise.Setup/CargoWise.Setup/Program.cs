using System.Runtime.CompilerServices;
using CargoWise.Setup.InstallComponents;
using CargoWise.Setup.Services;
using Enterprise.Upgrades;
using Enterprise.Upgrades.Installers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CargoWise.Setup
{
	class Program
	{
		internal const string UsageMessage = "Usage ./CargoWise.Setup.exe [install|uninstall]  [--configFile=<configFile>] [--<configKey>=<value>]* --components=<component1>,<component2> [--help]\n";

		internal enum ExitCodes
		{
			Success = 0,
			Error = -1,
			BadUsage = -2,
		}

		[ModuleInitializer]
		public static void InitializeModule()
		{
			NetCoreAssemblyResolver.Setup();
		}

		public static async Task<int> Main(string[] args)
		{
			return await new Program().Run(args);
		}

		public Program()
		{
			var services = new ServiceCollection();
			ConfigureServices(services);
			ConfiguredComponentsInOrder(services);
			ServiceProvider = services.BuildServiceProvider();
		}

		internal ServiceProvider ServiceProvider { get; set; }

		internal async Task<int> Run(string[] args)
		{
			try
			{
				var applicationArgumentParser = ServiceProvider.GetRequiredService<ApplicationArguments>();
				if (applicationArgumentParser.HasHelpArgument(args))
				{
					Console.WriteLine(UsageMessage);
					Console.WriteLine($"Available Components:\n\t{string.Join("\n\t", ListComponents(ServiceProvider))}");
					return (int)ExitCodes.Success;
				}

				var config = applicationArgumentParser.ReadConfigFromArgs(args);
				var installDirector = ServiceProvider.GetRequiredService<IInstallDirector>();
				var tokenSource = new CancellationTokenSource();
				Console.CancelKeyPress += (s, e) =>
				{
					Console.WriteLine("Cancellation requested");
					tokenSource.Cancel();
					e.Cancel = true;
				};

				await installDirector.Run(config, tokenSource.Token);
				ServiceProvider.GetRequiredService<ILoggerFactory>().Dispose();
				return (int)ExitCodes.Success;
			}
			catch (BadUsageException ex)
			{
				Console.Error.WriteLine($"{ex.Message}\n{UsageMessage}");
				return (int)ExitCodes.BadUsage;
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine($"{ex}");
				return (int)ExitCodes.Error;
			}
		}

		static IEnumerable<string> ListComponents(IServiceProvider serviceProvider) => serviceProvider.GetRequiredService<IEnumerable<IInstallationComponent>>().Select(x => x.Name);

		internal static void ConfigureServices(ServiceCollection services)
		{
			services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(MinimumLogLevel))
				.AddSingleton<IInstallDirector, InstallDirector>()
				.AddSingleton<IConfigurationDefaultsProvider, ConfigurationDefaultsProvider>()
				.AddSingleton<ApplicationArguments>()
				.AddSingleton<IBrowserCompatibilityConfigurator, BrowserCompatibilityConfigurator>()
				.AddSingleton<ICargoWiseStartInstaller, CargoWiseStartInstaller>()
				.AddSingleton<IEdiUrlRegistration, EdiUrlRegistration>()
				.AddSingleton<IEventSourceCreator, EventSourceCreator>()
				.AddSingleton<IInstallPathProvider, InstallPathProvider>()
				.AddSingleton<IProcessRunner, ProcessRunnerProxy>()
				.AddSingleton<IWindowsRegistryProxy, WindowsRegistryProxy>()
				.AddSingleton<IFontInstaller, FontInstaller>()
				.AddSingleton<ICargoWiseStartExeUpdater, CargoWiseStartExeUpdater>()
				;
		}

		internal static void ConfiguredComponentsInOrder(ServiceCollection services)
		{
			services.AddSingleton<IInstallationComponent, EnvironmentComponent>();
			services.AddSingleton<IInstallationComponent, NGenInstaller>();
		}

		static LogLevel MinimumLogLevel =>
#if DEBUG
			LogLevel.Debug
#else
			LogLevel.Information
#endif
		;
	}
}
