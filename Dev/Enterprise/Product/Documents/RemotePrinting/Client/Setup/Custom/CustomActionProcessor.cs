using System;
using System.Collections.Generic;
using System.Text;
using Enterprise.RemotePrinting.Client.CustomAction;

namespace Enterprise.RemotePrinting.Client.Setup.Custom
{
	public class CustomActionProcessor
	{
		public CustomActionProcessor(string action, string installLocation)
		{
			this.action = action;
			this.installLocation = installLocation;

			// If error happens during restoring services statuses - log it and continue upgrade process.
			// Services would not be restarted regardless if we rollback upgrade or not.
			shouldRethrow = action != CommandRestore;

			InitializeLogWriter();
		}

		protected readonly string action;
		readonly string installLocation;
		bool shouldRethrow;
		protected LogWriterForSetup logWriter;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logs service processing and handles null checks.")]
		public void Process()
		{
			var servicesHelper = GetWindowsServicesHelper();
			if (servicesHelper == null)
			{
				return;
			}

			var serviceNames = GetServiceNames(servicesHelper);
			if (serviceNames == null)
			{
				WriteLog("ServiceHelper returned <null> list of installed Service Clients.");
				return;
			}

			if (serviceNames.Count > 0)
			{
				try
				{
					WriteLog(string.Empty, serviceNames);
					WriteLog($"Command: {action}");

					switch (action)
					{
						case CommandSave:
							SaveServicesStateAndStop(serviceNames, servicesHelper);
							break;

						case CommandRestore:
							RestoreServices(serviceNames, servicesHelper);
							break;

						case CommandDeleteService:
							DeleteServices(serviceNames, servicesHelper);
							break;

						default:
							// Do not throw exception here, just log an error.
							WriteLog("Unknown custom action.");
							return;
					}

					WriteLog("Action succeeded.");
				}
				catch (Exception ex)
				{
					WriteLog(ex.Message, serviceNames, ex);

					if (shouldRethrow)
					{
						throw;
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Windows Service Helper")]
		protected IWindowsServicesHelper GetWindowsServicesHelper()
		{
			try
			{
				return GetWindowsServicesHelperCore();
			}
			catch (Exception ex)
			{
				WriteLog("Error while creating Windows Service Helper.", exception: ex);

				if (shouldRethrow)
				{
					throw;
				}
				else
				{
					return null;
				}
			}
		}

		protected virtual IWindowsServicesHelper GetWindowsServicesHelperCore()
		{
			return new WindowsServicesHelper(installLocation);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "installed Service Clients")]
		protected List<string> GetServiceNames(IWindowsServicesHelper servicesHelper)
		{
			try
			{
				return servicesHelper.GetAllServicesRunning();
			}
			catch (Exception ex)
			{
				WriteLog("Error while retrieving list of installed Service Clients.", exception: ex);

				if (shouldRethrow)
				{
					throw;
				}
				else
				{
					return null;
				}
			}
		}

		protected virtual void SaveServicesStateAndStop(List<string> serviceNames, IWindowsServicesHelper servicesHelper)
		{
			foreach (var serviceName in serviceNames)
			{
				var fileName = serviceName + StateFileSuffix;
				ServiceStateManager.SaveCurrentState(serviceName, fileName, servicesHelper, (message, shouldAddCommandInfo) => WriteLog(message, shouldAddCommandInfo: shouldAddCommandInfo));
			}

			ServiceStateManager.StopServicesIfNeeded(serviceNames, servicesHelper, (message, shouldAddCommandInfo) => WriteLog(message, shouldAddCommandInfo: shouldAddCommandInfo));
		}

		protected virtual void RestoreServices(List<string> serviceNames, IWindowsServicesHelper servicesHelper)
		{
			foreach (var serviceName in serviceNames)
			{
				var fileName = serviceName + StateFileSuffix;
				ServiceStateManager.RestoreCurrentState(serviceName, fileName, servicesHelper, message => WriteLog(message));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "WriteLog")]
		protected virtual void DeleteServices(List<string> serviceNames, IWindowsServicesHelper servicesHelper)
		{
			foreach (var serviceName in serviceNames)
			{
				if (!servicesHelper.DeleteService(serviceName, out var errorMessage))
				{
					WriteLog("Error while deleting service " + serviceName + ": " + errorMessage);
				}
			}
		}

		void InitializeLogWriter()
		{
			try
			{
				logWriter = CreateLogWriter();
			}
			catch
			{
				// No logging possible, rethrow all exceptions
				shouldRethrow = true;
			}
		}

		protected virtual LogWriterForSetup CreateLogWriter() => new LogWriterForSetup(action);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "logBuilder")]
		protected void WriteLog(string message, IList<string> serviceNames = null, Exception exception = null, bool shouldAddCommandInfo = false)
		{
			if (logWriter == null)
			{
				return;
			}

			var logBuilder = new StringBuilder(message);
			if (shouldAddCommandInfo)
			{
				logBuilder.AppendLine().Append("Command: ").Append(action ?? string.Empty);
				logBuilder.AppendLine().Append("Location: ").Append(installLocation ?? string.Empty);
			}

			if (serviceNames != null)
			{
				logBuilder.AppendLine().Append("Number of services found: ").Append(serviceNames.Count);
				foreach (var serviceName in serviceNames)
				{
					logBuilder.AppendLine().Append("  ").Append(serviceName);
				}
			}

			if (exception != null)
			{
				logBuilder.AppendLine().AppendLine().Append(exception);
			}

			logBuilder.AppendLine().AppendLine();

			logWriter.WriteLog(logBuilder.ToString());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const string")]
		public const string CommandSave = "Save";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const string")]
		public const string CommandRestore = "Restore";
		public const string CommandDeleteService = "DeleteService";
		const string StateFileSuffix = "_state.txt";
	}
}
