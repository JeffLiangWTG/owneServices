using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace Enterprise.RemotePrinting.Client
#if WixCustomAction
.CustomAction // Different namespace for file copy in Setup project (to prevent error 'same symbol name defined in multiple project').
#endif
{
	public class WindowsServicesHelper : IWindowsServicesHelper
	{
		public WindowsServicesHelper(string binPathOverride = null)
		{
			ServiceExeFileName = GetServiceExeFileName(binPathOverride);
		}

		public static string GetServiceExeFileName(string binPathOverride = null)
		{
			var binPath = string.IsNullOrEmpty(binPathOverride) ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) : binPathOverride;
			return Path.Combine(binPath, ServiceExecutableFileName);
		}

		public string ServiceExeFileName { get; }

		public string GetServiceNameFromConfigName(string configName)
		{
			if (configName.Equals(Constants.RegistryManager.DefaultWindowsServiceName, StringComparison.OrdinalIgnoreCase))
			{
				return Constants.RegistryManager.DefaultWindowsServiceName;
			}
			else if (configName.StartsWith(ServiceNamePrefix, StringComparison.OrdinalIgnoreCase))
			{
				return configName;
			}
			else
			{
				List<string> subKeyNames;
				var fallback = ServiceNamePrefix + new string(configName.Where(char.IsLetterOrDigit).ToArray());

				using (var topKey = RegistryHelper.DefaultRegistryRoot.OpenSubKey(ServicesRegistryPath))
				{
					if (topKey == null)
					{
						return fallback;
					}

					subKeyNames = topKey.GetSubKeyNames().ToList();
				}

				foreach (var keyName in subKeyNames)
				{
					if (GetConfigName(keyName) == configName)
					{
						return keyName;
					}
				}

				return fallback;
			}
		}

		public string GetConfigNameArgumentValue(string[] args)
		{
			var configNameArg = CommandLineHelper.GetArgument(args, ConfigNameArg);
			return string.IsNullOrEmpty(configNameArg) ? string.Empty : configNameArg.Substring(ConfigNameArg.Length);
		}

		public string GetConfigName(string serviceName)
		{
			try
			{
				using (var key = RegistryHelper.DefaultRegistryRoot.OpenSubKey(ServicesRegistryPath + serviceName, true))
				{
					if (key == null)
					{
						return string.Empty;
					}

					var imagePath = key.GetValue("ImagePath")?.ToString();
					if (string.IsNullOrEmpty(imagePath))
					{
						return string.Empty;
					}

					var configNameArg = CommandLineHelper.GetArgument(imagePath, ConfigNameArg);
					return string.IsNullOrEmpty(configNameArg) ? string.Empty : configNameArg.Substring(ConfigNameArg.Length);
				}
			}
			catch
			{
				return string.Empty;
			}
		}

		public void ChangeConfigName(string serviceName, string newConfigName, string newDisplayName = null)
		{
			using (var key = RegistryHelper.DefaultRegistryRoot.OpenSubKey(ServicesRegistryPath + serviceName, true))
			{
				if (key == null)
				{
					return;
				}

				var imagePath = key.GetValue("ImagePath")?.ToString();
				if (string.IsNullOrEmpty(imagePath))
				{
					return;
				}

				var configNameArg = CommandLineHelper.GetArgument(imagePath, ConfigNameArg);
				if (!string.IsNullOrEmpty(configNameArg))
				{
					var newConfigNameArg = ConfigNameArg + newConfigName;
					if (string.Equals(newConfigNameArg, configNameArg))
					{
						return; // Already same configuration specified
					}

					imagePath = imagePath.Replace(configNameArg, newConfigNameArg);
				}
				else
				{
					imagePath = imagePath + " \"" + ConfigNameArg + newConfigName + "\"";
				}

				key.SetValue("ImagePath", imagePath, RegistryValueKind.String);

				if (!string.IsNullOrEmpty(newDisplayName))
				{
					key.SetValue("DisplayName", newDisplayName, RegistryValueKind.String);
				}
			}
		}

		// For some reason this suppression does not work in project RemotePrinting.Setup (perhaps FxCop cannot parse conditional namespace name) - added it to Baseline.
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "CargoWise One WebPrint Client does not have access to CargoWise.BrandManager.dll or Resources.dll.")]
		public string GetDisplayName(string configName) => Constants.ApplicationName + " (" + configName + ")";

		public IServiceController GetServiceController(string serviceName)
		{
			try
			{
				return new PrintServiceController(new ServiceController(serviceName));
			}
			catch (ArgumentException)
			{
				return null;
			}
		}

		public string CheckServiceControllerStatusWithRetry(string serviceName, int timeoutMs, Func<string, bool> retryFilter)
		{
			var status = CheckServiceControllerStatus(serviceName);
			if (timeoutMs > 0 && retryFilter != null && retryFilter(status))
			{
				Thread.Sleep(timeoutMs);
				status = CheckServiceControllerStatus(serviceName);
			}
			return status;
		}

		public string CheckServiceControllerStatus(string serviceName)
		{
			var innerServiceName = GetServiceNameFromConfigName(serviceName);
			using (var sc = GetServiceController(innerServiceName))
			{
				if (sc == null)
				{
					return ServiceNotFound;
				}

				try
				{
					switch (sc.Status)
					{
						case ServiceControllerStatus.Running:
							return ServiceControllerStatusRunning;
						case ServiceControllerStatus.Stopped:
							return ServiceControllerStatusStopped;
						case ServiceControllerStatus.Paused:
							return ServiceControllerStatusPaused;
						case ServiceControllerStatus.StopPending:
							return ServiceControllerStatusStopPending;
						case ServiceControllerStatus.StartPending:
							return ServiceControllerStatusStartPending;
						default:
							return ServiceControllerStatusStatusChanging;
					}
				}
				catch (InvalidOperationException) // Can happen reading service Status if it was removed
				{
					return ServiceNotFound;
				}
			}
		}

		public string ServicesRegistryPath => ServicesRegistryPathCore;

		protected virtual string ServicesRegistryPathCore => "SYSTEM\\CurrentControlSet\\Services\\";

		public const string ServiceNamePrefix = "RemotePrintingService_";
		public const string ServiceExecutableFileName = "RemotePrinting.Client.Service.exe";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const string")]
		public const string ServiceNotFound = "The service was not found";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const string")]
		public const string ServiceControllerStatusRunning = "Running";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const string")]
		public const string ServiceControllerStatusStopped = "Stopped";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const string")]
		public const string ServiceControllerStatusPaused = "Paused";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const string")]
		public const string ServiceControllerStatusStopPending = "Stopping";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const string")]
		public const string ServiceControllerStatusStartPending = "Starting";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const string")]
		public const string ServiceControllerStatusStatusChanging = "Status Unkown";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const string")]
		public const string ServiceStartModeAuto = "auto";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const string")]
		public const string ServiceStartModeDemand = "demand";
		public const string ConfigNameArg = "ConfigName:";
		const string RunningFailedFlag = "FAILED";

		public bool InstallNewService(string serviceName, string configName, string startMode, string login, string password, out string errorMessage)
		{
			var installServiceArguments = $" binpath= \"\\\"{ServiceExeFileName}\\\" \\\"{ConfigNameArg}{configName}\\\"\" displayName= \"{GetDisplayName(configName)}\" start= {startMode}";
			if (!string.IsNullOrEmpty(login) || !string.IsNullOrEmpty(password))
			{
				installServiceArguments += $" obj= \"{login}\" password= \"{password}\"";
			}

			return ExecuteProcess(serviceName, RemotePrintServiceMode.Create, out errorMessage, installServiceArguments);
		}

		public bool DeleteService(string serviceName, out string errorMessage)
		{
			var stopErrorMessage = string.Empty;
			var status = CheckServiceControllerStatus(serviceName);
			if (status == ServiceControllerStatusRunning)
			{
				StopProcess(serviceName, out stopErrorMessage);
			}
			var result = ExecuteProcess(serviceName, RemotePrintServiceMode.Delete, out var deletedErrorMessage);
			errorMessage = string.IsNullOrEmpty(stopErrorMessage) ? deletedErrorMessage : $"{stopErrorMessage}\r\n{deletedErrorMessage}";
			return result;
		}

		public bool StopProcess(string serviceName, out string errorMessage) => ExecuteProcess(serviceName, RemotePrintServiceMode.Stop, out errorMessage);

		public bool StartProcess(string serviceName, out string errorMessage) => ExecuteProcess(serviceName, RemotePrintServiceMode.Start, out errorMessage);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Uses ProcessStartInfo")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "runas")]
		public bool ExecuteProcess(string serviceName, RemotePrintServiceMode mode, out string errorMessage, string customArguments = null)
		{
			var output = new StringBuilder();
			try
			{
				using (var process = new Process())
				{
					var startInfo = new ProcessStartInfo();
					startInfo.WindowStyle = ProcessWindowStyle.Normal;
					startInfo.FileName = "cmd.exe";
					startInfo.Verb = "runas";
					startInfo.UseShellExecute = false;
					startInfo.RedirectStandardError = true;
					startInfo.RedirectStandardOutput = true;

					startInfo.Arguments = $"/C sc.exe {mode.ToString()} \"{serviceName}\"";
					if (!string.IsNullOrEmpty(customArguments))
					{
						startInfo.Arguments += customArguments;
					}

					process.StartInfo = startInfo;
					process.Start();

					process.OutputDataReceived += (sender, e) =>
					{
						if (!string.IsNullOrEmpty(e.Data))
						{
							output.AppendLine(e.Data);
						}
					};
					process.BeginOutputReadLine();

					process.ErrorDataReceived += (sender, e) =>
					{
						if (e.Data != null)
						{
							output.AppendLine(e.Data);
						}
					};
					process.BeginErrorReadLine();
					process.WaitForExit();

					errorMessage = output.ToString();
					if (errorMessage.IndexOf(RunningFailedFlag, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						return false;
					}

					errorMessage = string.Empty;
					return true;
				}
			}
			catch (Exception ex)
			{
				CombineExceptionMessages(ex, output);
				errorMessage = output.ToString();
				return false;
			}
		}

		void CombineExceptionMessages(Exception ex, StringBuilder error)
		{
			while (ex != null)
			{
				error.AppendLine(ex.GetType().FullName + ": " + ex.Message);
				ex = ex.InnerException;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Uses ProcessStartInfo")]
		public List<string> GetAllServicesRunning()
		{
			Exception firstException = null;
			try
			{
				return GetAllServicesRunningFromServicesRegistry();
			}
			catch (Exception ex)
			{
				firstException = ex;
			}

			try
			{
				return GetAllServicesRunningFromServiceController();
			}
			catch (Exception ex)
			{
				throw new AggregateException(firstException, ex);
			}
		}

		List<string> GetAllServicesRunningFromServicesRegistry()
		{
			string[] subKeyNames;

			using (var topKey = DefaultRegistryRoot.OpenSubKey(ServicesRegistryPath))
			{
				if (topKey == null)
				{
					return new List<string>();
				}

				subKeyNames = topKey.GetSubKeyNames();
			}

			var servicesNames = new List<string>();

			foreach (var keyName in subKeyNames)
			{
				using (var key = DefaultRegistryRoot.OpenSubKey(ServicesRegistryPath + keyName))
				{
					if (key == null)
					{
						continue;
					}

					var imagePath = key?.GetValue("ImagePath")?.ToString();
					if (imagePath != null && CommandLineHelper.MatchesExeFileName(imagePath, ServiceExeFileName))
					{
						servicesNames.Add(keyName);
					}
				}
			}

			return servicesNames;
		}

		List<string> GetAllServicesRunningFromServiceController()
		{
			return RunningServices.Where(IsMatchingConfig).ToList();
		}

		protected virtual IEnumerable<string> RunningServices => ServiceController.GetServices().Select(s => s.ServiceName);

		bool IsMatchingConfig(string serviceName)
		{
			if (serviceName == Constants.RegistryManager.DefaultWindowsServiceName)
			{
				return true;
			}

			try
			{
				if (serviceName.StartsWith(ServiceNamePrefix, StringComparison.OrdinalIgnoreCase))
				{
					var keyName = Constants.RegistryManager.WebPrintConfigKeyName + "-" + serviceName.Substring(ServiceNamePrefix.Length);
					using (var key = DefaultRegistryRoot.OpenSubKey(keyName))
					{
						return key != null;
					}
				}

				return false;
			}
			catch
			{
				return false;
			}
		}

		protected virtual RegistryKey DefaultRegistryRoot => RegistryHelper.DefaultRegistryRoot;
	}
}
