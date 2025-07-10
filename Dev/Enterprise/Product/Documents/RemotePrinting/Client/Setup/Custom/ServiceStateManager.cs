using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.ServiceProcess;
using Enterprise.RemotePrinting.Client.CustomAction;

namespace Enterprise.RemotePrinting.Client.Setup.Custom
{
	public static class ServiceStateManager
	{
		public static string TempFolder => Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);

		public static string GetStateFilePath(string stateFileName)
		{
			return Path.Combine(TempFolder, stateFileName);
		}

		public static void SaveCurrentState(string serviceName, string stateFileName, IWindowsServicesHelper servicesHelper, Action<string, bool> log)
		{
			var stateFile = GetStateFilePath(stateFileName);

			if (File.Exists(stateFile))
			{
				log.Invoke($"Service Name: {serviceName}. The old state file needs to be deleted. File path: {stateFile}", false);
				File.Delete(stateFile);

				if (File.Exists(stateFile))
				{
					log.Invoke($"Service Name: {serviceName}. The old state file has been deleted.", false);
				}
				else
				{
					log.Invoke($"Service Name: {serviceName}. The old state file was deleted failed.", false);
				}
			}

			var sc = servicesHelper.GetServiceController(serviceName);
			try
			{
				if (sc == null)
				{
					log.Invoke($"Service Name: {serviceName}. Could not get service controller.", false);
					return;
				}

				var startMode = sc.StartType;
				var currentStatus = sc.Status;

				var state = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", currentStatus, startMode);
				log.Invoke($"Service Name: {serviceName}. State info: {state}", true);
				File.WriteAllText(stateFile, state);
				log.Invoke($"Service Name: {serviceName}. State file saved successfully, and the file exists is {File.Exists(stateFile)}", false);
			}
			catch (Exception ex)
			{
				// Exception should be recorded in the log file
				log.Invoke($"Service Name: {serviceName}. An error occurred saving the state file. Exception: {ex}", false);

				// Ignore
				if (ex is InvalidOperationException)
				{
					return;
				}

				throw;
			}
			finally
			{
				sc?.Dispose();
			}
		}

		public static void StopServicesIfNeeded(List<string> serviceNames, IWindowsServicesHelper servicesHelper, Action<string, bool> log)
		{
			var runningServices = new List<IServiceController>();

			log.Invoke("Stop running service.", false);

			// Initiate stop
			foreach (var serviceName in serviceNames)
			{
				var sc = servicesHelper.GetServiceController(serviceName);
				try
				{
					if (sc == null)
					{
						log.Invoke($"Service Name: {serviceName}. Could not get service controller.", false);
					}
					else
					{
						log.Invoke($"Service Name: {serviceName}. Status is {sc.Status}.", false);
					}

					if (sc != null && sc.Status == ServiceControllerStatus.Running)
					{
						runningServices.Add(sc);
						sc.Stop();
					}
					else
					{
						sc?.Dispose();
					}
				}
				catch (Exception ex)
				{
					// Exception should be recorded in the log file
					log.Invoke($"Service Name: {serviceName}. An error occurred stopping the service name. Exception: {ex}.", false);

					// Can happen reading service Status if it was removed
					if (ex is InvalidOperationException)
					{
						if (sc != null)
						{
							sc.Dispose();
							runningServices.Remove(sc);
						}

						return;
					}

					throw;
				}
			}

			// Wait till all services are stopped
			var sw = new Stopwatch();
			sw.Start();
			foreach (var sc in runningServices)
			{
				var timeout = 15_000 - (int)sw.Elapsed.TotalMilliseconds; // Do not exceed total 15,000 ms
				if (timeout <= 10)
				{
					timeout = 10; // Wait at least 10 ms
				}

				try
				{
					sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(timeout));
				}
				catch (InvalidOperationException) // Can happen reading service Status if it was removed
				{
				}
				finally
				{
					sc.Dispose();
				}
			}
			sw.Stop();

			log.Invoke("All running services have been stopped.", false);
		}

		public static void RestoreCurrentState(string serviceName, string stateFileName, IWindowsServicesHelper servicesHelper, Action<string> log)
		{
			if (serviceName == Constants.RegistryManager.DefaultWindowsServiceName)
			{
				RenameDefaultWindowsService(servicesHelper, log);
			}

			var stateFile = GetStateFilePath(stateFileName);
			if (!File.Exists(stateFile))
			{
				log.Invoke($"Service Name: {serviceName}. Could not find the state file.");
				return;
			}

			var sc = servicesHelper.GetServiceController(serviceName);
			try
			{
				if (sc == null)
				{
					log.Invoke($"Service Name: {serviceName}. Could not get the service controller.");
					return;
				}

				var stateContent = File.ReadAllText(stateFile);
				var state = stateContent.Split('|');
				var logs = $@"Service Name: {serviceName}.
Saved state file location: {stateFile}
Saved state: {stateContent}

Service current status: {sc.Status}";
				log.Invoke(logs);

				var expectedStatus = (ServiceControllerStatus)Enum.Parse(typeof(ServiceControllerStatus), state[0]);

				var shouldRestart = expectedStatus == ServiceControllerStatus.Running && sc.Status != ServiceControllerStatus.Running;
				if (shouldRestart)
				{
					log.Invoke("Running restore action.");
					sc.Start();
					log.Invoke("Restore action succeeded.");

					if (sc.Status != ServiceControllerStatus.Running)
					{
						// If it is still Stopped, wait for 5 seconds and check again.
						sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(5));
					}

					log.Invoke($"Service new status: {sc.Status}.");
				}
			}
			catch (Exception ex)
			{
				// Exception should be recorded in the log file
				log.Invoke($"Service Name: {serviceName}. An error occurred restoring the service. Exception: {ex}.");

				//ignore
				if (ex is InvalidOperationException)
				{
					return;
				}

				throw;
			}
			finally
			{
				sc?.Dispose();
				File.Delete(stateFile);
			}
		}

		static void RenameDefaultWindowsService(IWindowsServicesHelper servicesHelper, Action<string> log)
		{
			if (!string.IsNullOrEmpty(servicesHelper.GetConfigName(Constants.RegistryManager.DefaultWindowsServiceName)))
			{
				log.Invoke($"Service Name: {Constants.RegistryManager.DefaultWindowsServiceName}. Could not get config name.");
				return;
			}

			var configSetForWinService = RegistryHelper.GetCurrentlySelectedConfigForWindowsService(RegistryHelper.DefaultRegistryRoot, Constants.RegistryManager.CargoWiseWebPrintTopKeyName);

#if DEBUG
			if (!string.IsNullOrEmpty(SelectedConfigForWindowsServiceForTest))
			{
				configSetForWinService = SelectedConfigForWindowsServiceForTest;
			}
#endif

			if (!string.IsNullOrEmpty(configSetForWinService))
			{
				var displayName = servicesHelper.GetDisplayName(configSetForWinService);

				log.Invoke($"Service Name: {Constants.RegistryManager.DefaultWindowsServiceName}. Try to change service name to {configSetForWinService}.");
				servicesHelper.ChangeConfigName(Constants.RegistryManager.DefaultWindowsServiceName, configSetForWinService, displayName);
				log.Invoke($"Service Name: {Constants.RegistryManager.DefaultWindowsServiceName}. Service name has been changed to {configSetForWinService}.");
			}
		}

#if DEBUG
		public static string SelectedConfigForWindowsServiceForTest { get; set; }
#endif
	}
}
