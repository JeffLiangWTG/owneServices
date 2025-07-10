using System.Diagnostics;
using CargoWise.Setup.Services;
using Microsoft.Extensions.Logging;

namespace CargoWise.Setup.InstallComponents;

class NGenInstaller(IInstallPathProvider installPathProvider, IWindowsRegistryProxy registry, IProcessRunner processRunner, ILogger<NGenInstaller> logger) : IInstallationComponent
{
	public string Name => "NGen";

	public Task Install(ConfigurationModel config, CancellationToken token)
	{
		RunNGenProcess(Action.Install);
		return Task.CompletedTask;
	}

	public Task Remove(ConfigurationModel config, CancellationToken token)
	{
		RunNGenProcess(Action.Uninstall);
		return Task.CompletedTask;
	}

	void RunNGenProcess(Action action)
	{
		if (!NGenEnabled && action == Action.Install)
		{
			return;
		}

		var timeoutArg = ExecutionTimeout.ToString() ?? string.Empty;
		var installPath = installPathProvider.GetInstallPath();
		var targetPathArg = Path.Combine(installPath, "CargoWise.NGenRoot.dll");
		var ngenInstallerPath = Path.Combine(installPath, "CargoWise.NGenInstaller.exe");
		var startInfo = new ProcessStartInfo(ngenInstallerPath)
		{
			WorkingDirectory = installPathProvider.GetInstallPath(),
			Arguments = $"{action} \"{targetPathArg}\" \"{timeoutArg}\"",
			UseShellExecute = false,
			CreateNoWindow = true,
		};
		logger.LogInformation("Starting NGenInstaller process.");
		var process = processRunner.Start(startInfo);
		if (!process.Present)
		{
			logger.LogError($"Failed to start the NGenInstaller process at '{ngenInstallerPath}'");
			return;
		}

		process.WaitForExit();
		if (process.ExitCode != 0)
		{
			logger.LogError($"NGenInstaller process exited with error code {process.ExitCode}");
		}
	}

	bool NGenEnabled => registry.GetValue(SoftwareCargoWiseRegistryKey, "nongen", null) == null;
	int? ExecutionTimeout => (int?)registry.GetValue(SoftwareCargoWiseRegistryKey, "NGen_TimeoutSecondsPerExecution", null);

	const string SoftwareCargoWiseRegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\WiseTech Global\CargoWise";

	public enum Action
	{
		Install,
		Uninstall
	}
}
