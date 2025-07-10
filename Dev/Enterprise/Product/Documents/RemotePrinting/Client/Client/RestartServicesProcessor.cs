using System;
using System.IO;

namespace Enterprise.RemotePrinting.Client
{
	public class RestartServicesProcessor
	{
		public RestartServicesProcessor(IWindowsServicesHelper servicesHelper)
		{
			ServicesHelper = servicesHelper;
		}

		readonly IWindowsServicesHelper ServicesHelper;
		const string RestartLogFileWriterPrefix = "RestartLog";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "logWriter append")]
		public void RestartService(string[] args)
		{
			try
			{
				var configName = ServicesHelper.GetConfigNameArgumentValue(args);
				if (string.IsNullOrEmpty(configName))
				{
					RegisterFileTarget(LogWriter.GetOutputDirectory().FullName, RestartLogFileWriterPrefix);
					LogWriter.Append(WebPrintEventLogEntryType.Error, "No Service configuration name!");
					return;
				}

				var fileName = Path.Combine(LogWriter.GetOutputDirectory().FullName, configName);
				RegisterFileTarget(fileName, RestartLogFileWriterPrefix);

				var serviceName = ServicesHelper.GetServiceNameFromConfigName(configName);
				if (ServicesHelper.CheckServiceControllerStatus(configName) == WindowsServicesHelper.ServiceNotFound)
				{
					LogWriter.Append(WebPrintEventLogEntryType.Error, "No Service installed for this configuration! Please use Configurator and install a service.", RestartLogFileWriterPrefix);
					return;
				}

				if (ServicesHelper.StopProcess(serviceName, out var stopErrorMessage))
				{
					if (ServicesHelper.CheckServiceControllerStatusWithRetry(serviceName, 3000, s => s != WindowsServicesHelper.ServiceControllerStatusStopped) == WindowsServicesHelper.ServiceControllerStatusStopped)
					{
						LogWriter.Append(WebPrintEventLogEntryType.Information, "The Service has been stopped.", RestartLogFileWriterPrefix);
					}
					else
					{
						LogWriter.Append(WebPrintEventLogEntryType.Information, "The Service has been marked to stop.", RestartLogFileWriterPrefix);
					}
				}
				else
				{
					LogWriter.Append(WebPrintEventLogEntryType.Error, "Failed to stop the WebPrint Client Service, error message:\r\n" + stopErrorMessage, RestartLogFileWriterPrefix);
				}

				if (ServicesHelper.StartProcess(serviceName, out var startErrorMessage))
				{
					LogWriter.Append(WebPrintEventLogEntryType.Information, "The Service has been started.", RestartLogFileWriterPrefix);
				}
				else
				{
					LogWriter.Append(WebPrintEventLogEntryType.Error, "Failed to start the WebPrint Client Service, error message:\r\n" + stopErrorMessage, RestartLogFileWriterPrefix);
				}
			}
			catch (Exception ex)
			{
				LogWriter.Append(WebPrintEventLogEntryType.Error, "Restart WebPrint Client Service Error\r\n" + ex.ToString(), RestartLogFileWriterPrefix);
			}
		}

		protected virtual void RegisterFileTarget(string filePath, string filePrefix)
		{
			LogWriter.RegisterFileTarget(filePath, filePrefix, WebPrintEventLogEntryType.Information, WebPrintEventLogEntryType.Error, filePrefix);
		}
	}
}
