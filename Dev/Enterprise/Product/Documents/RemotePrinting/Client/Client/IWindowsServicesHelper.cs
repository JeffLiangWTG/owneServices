using System;
using System.Collections.Generic;

namespace Enterprise.RemotePrinting.Client
#if WixCustomAction
.CustomAction // Different namespace for file copy in Setup project (to prevent error 'same symbol name defined in multiple project').
#endif
{
	public interface IWindowsServicesHelper
	{
		bool InstallNewService(string serviceName, string configName, string startMode, string login, string password, out string errorMessage);

		string CheckServiceControllerStatusWithRetry(string serviceName, int timeoutMs, Func<string, bool> retryFilter);

		string CheckServiceControllerStatus(string serviceName);

		bool DeleteService(string serviceName, out string errorMessage);

		bool StopProcess(string serviceName, out string errorMessage);

		bool StartProcess(string serviceName, out string errorMessage);

		string GetServiceNameFromConfigName(string configName);

		string GetConfigNameArgumentValue(string[] args);

		string GetConfigName(string serviceName);

		void ChangeConfigName(string serviceName, string newConfigName, string newDisplayName = null);

		string GetDisplayName(string configName);

		IServiceController GetServiceController(string serviceName);

		List<string> GetAllServicesRunning();
	}

	public enum RemotePrintServiceMode
	{
		Start = 1,
		Stop = 2,
		Delete = 3,
		Create = 4,
	}
}
