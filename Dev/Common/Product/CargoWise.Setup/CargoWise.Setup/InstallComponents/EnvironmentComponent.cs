using CargoWise.Setup.Services;
using Enterprise.Upgrades.Installers;
using Microsoft.Extensions.Logging;

namespace CargoWise.Setup.InstallComponents;

class EnvironmentComponent(
	IEventSourceCreator eventSourceCreator,
	IBrowserCompatibilityConfigurator browserCompatibilityConfigurator,
	IEdiUrlRegistration urlRegistration,
	ICargoWiseStartInstaller cargoWiseStartInstaller,
	IFontInstaller fontInstaller,
	IInstallPathProvider pathProvider,
	ILogger<EnvironmentComponent> logger) : IInstallationComponent
{
	public static readonly string ComponentName = "Environment";
	public string Name => ComponentName;
	public Task Install(ConfigurationModel config, CancellationToken token)
	{
		var acquiredMutex = false;
		using var mutex = new Mutex(false, MutexName);
		try
		{
			try
			{
				if (!mutex.WaitOne(0))
				{
					logger.LogInformation("Waiting to acquire environment setup mutex");
					if (!mutex.WaitOne(mutexTimeout))
					{
						throw new TimeoutException("Could not acquire environment setup mutex");
					}
				}
			}
			catch (AbandonedMutexException)
			{
			}

			var packageFolder = pathProvider.GetInstallPath();

			acquiredMutex = true;
			eventSourceCreator.CreateEventSource();
			browserCompatibilityConfigurator.ConfigureCompatibilityMode();
			urlRegistration.RegisterUrlHandlers();
			cargoWiseStartInstaller.Install();
			fontInstaller.InstallAllFonts(packageFolder);
		}
		finally
		{
			if (acquiredMutex)
			{
				mutex.ReleaseMutex();
			}
		}

		return Task.CompletedTask;
	}

	internal static string MutexName => "CargoWiseEnvironmentSetupMutex";
	internal TimeSpan mutexTimeout = TimeSpan.FromMinutes(1);

	public Task Remove(ConfigurationModel config, CancellationToken token)
	{
		return Task.CompletedTask;
	}
}
