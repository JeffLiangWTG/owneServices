using System.Diagnostics;
using System.Security;
using Enterprise.Upgrades;
using Microsoft.Extensions.Logging;

namespace CargoWise.Setup.Services;

public interface ICargoWiseStartInstaller
{
	void Install();
}

internal class CargoWiseStartInstaller(IWindowsRegistryProxy registry, IProcessRunner processRunner, IInstallPathProvider pathProvider, ICargoWiseStartExeUpdater exeUpdater, ILogger<CargoWiseStartInstaller> logger) : ICargoWiseStartInstaller
{
	public void Install()
	{
		if (IsInstalled())
		{
			logger.LogInformation($"CargoWise.Start.exe is already installed, updating file if needed.");
			exeUpdater.UpdateCargoWiseStartExe(pathProvider.GetCurrentVersionFilePath(), pathProvider.GetVersionInfoFromDirectory());
			return;
		}

		logger.LogInformation($"Installing CargoWise.Start.exe using {installerName}.");

		var startInfo = new ProcessStartInfo
		{
			FileName = InstallerPath,
			WorkingDirectory = Path.GetDirectoryName(InstallerPath),
			Arguments = Arguments,
			CreateNoWindow = true,
			UseShellExecute = true,
		};

		var process = processRunner.Start(startInfo);
		process.WaitForExit();
		var exitCode = process.ExitCode;
		switch (exitCode)
		{
			case 0:
				break;
			case -1:
				logger.LogError("Failed to launch the CargoWise.Start.exe installer.");
				break;
			default:
				logger.LogError($"Unable to install CargoWise.Start.exe. Installer exit code: {exitCode}");
				break;
		}
	}

	bool IsInstalled()
	{
		try
		{
			return registry.GetValue(RegistryName, "ClientInstalled") != null;
		}
		catch (SecurityException ex)
		{
			logger.LogWarning(ex, $"Could not read registry '{RegistryName}\\ClientInstalled', running CargoWise.Start.exe installer anyway");
			return false;
		}
	}

	const string RegistryName = @"HKEY_LOCAL_MACHINE\SOFTWARE\WiseTech Global\CargoWise";

	string InstallerPath => Path.Combine(pathProvider.GetInstallPath(), installerName);
	const string installerName = "CargoWiseSetup.msi";

	string Arguments => "/qn";
}
