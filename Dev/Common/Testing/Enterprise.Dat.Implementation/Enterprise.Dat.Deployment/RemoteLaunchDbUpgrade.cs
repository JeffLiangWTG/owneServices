using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using Dat.Integration;

namespace Enterprise.Dat.Implementation
{
	using Environment = System.Environment;

	public static class RemoteLaunchDbUpgrade
	{
		public const string CWStartExeArgumentFormat = "{0} {1} -ScheduledDbUpgrader -NoUI -RemoveOldVersions -ForceRemoveOldVersionsInFrontOfInstallation";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public static void Invoke(ITaskLogger logger, string targetHost, string serverName, string databaseName)
		{
			var exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WiseTech Global", "CargoWise", "CargoWise.Start.exe");
			var arguments = string.Format(CWStartExeArgumentFormat, serverName, databaseName);
			var taskGuid = Guid.NewGuid().ToString();

			var psScript = $"Start-LaunchDbUpgradeTask -executablePath '{exePath}' -arguments '{arguments}' -taskGuid '{taskGuid}'";

			using var remoteRunspace = NewRemoteRunspace(targetHost);
			remoteRunspace.Open();

			var retryCount = 10;
			do
			{
				using var powershell = PowerShell.Create();
				powershell.Runspace = remoteRunspace;
				powershell.Commands.Clear();
				powershell.Commands.AddScript(functionScript);
				powershell.Commands.AddScript(psScript);
				var psResult = powershell.Invoke();

				if (!powershell.HadErrors)
				{
					logger?.RecordInfo($"Remote command started successfully. Task GUID: {taskGuid}");
					return;
				}
				else
				{
					logger?.RecordInfo($"Failed to start remote command. Task GUID: {taskGuid}. Return value: {string.Join(", ", powershell.Streams.Error)}");
				}

				Thread.Sleep(TimeSpan.FromSeconds(10));
			} while (--retryCount >= 0);
		}

		static Runspace NewRemoteRunspace(string targetHost)
		{
			var connectionUri = new Uri($"https://{targetHost}:5986/wsman");
			var connectionInfo = new WSManConnectionInfo()
			{
				SkipCACheck = true,
				SkipCNCheck = true,
				SkipRevocationCheck = true,
				AuthenticationMechanism = AuthenticationMechanism.Default,
				ConnectionUri = connectionUri,
			};

			var runspace = RunspaceFactory.CreateRunspace(connectionInfo);
			return runspace;
		}

		static readonly string functionScript = @"
		function Start-LaunchDbUpgradeTask {
			param (
				[string]$executablePath,
				[string]$arguments,
				[string]$taskGuid
			)

			# Create a unique task name
			$taskName = 'LaunchDbUpgradeTask_' + $taskGuid

			# Define the task action
			$action = New-ScheduledTaskAction -Execute $executablePath -Argument $arguments

			# Define the task trigger to run once in 5 seconds and expire in 1 minute
			$trigger = New-ScheduledTaskTrigger -Once -At (Get-Date).AddSeconds(5)
			$trigger.EndBoundary = (Get-Date).AddMinutes(1).ToString('s')

			# Define the task settings to delete the task after it expires in 1 day
			$settings = New-ScheduledTaskSettingsSet  -DeleteExpiredTaskAfter (New-TimeSpan -Days 1)

			# Register the scheduled task with a unique name
			Register-ScheduledTask -Action $action -TaskName $taskName -Description 'Run LaunchDbUpgrade with parameters' -User 'SYSTEM' -Trigger $trigger -Settings $settings
		}";
	}
}
