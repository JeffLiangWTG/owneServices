using System.Diagnostics.CodeAnalysis;

namespace Enterprise.RemotePrinting.Client
{
	public struct WebClientConfiguration
	{
		public string WebServiceUrl;
		public string WebServiceUser;
		public string WebServicePwd;
		public int RequestPauseInSeconds;
		public string LocalMachineName;
		public bool ProxyEnabled;
		public string ProxyAddress;
		public int ProxyPort;
		public string ProxyUser;
		public string ProxyPwd;
		public bool ProxyUseDefaultSystemSettings;
		public bool WindowsServiceConfigurationSelected;
		public int SecondsBetweenScanForNewPrinters;
		public bool KeepAliveEnabled;
		public bool Expect100Continue;
		public int KeepAliveTime;
		public int KeepAliveInterval;
		public int NumberOfLoopsToCheckForUpdate;
		public int RemotePrintingServiceTimeoutInSeconds;
		public bool EnableSignalR;
		public bool EnableSignalRPause;
		public int ReconnectionAttempts;
		public int ReconnectionLimitMinutes;
		public int PauseSignalRMinutes;
		public int ConnectionRetryAttempts;
		public int RetryDelay;
		public bool EnableVerboseLogging;
		public int JobPrintingTimeout;
		public int MemoryUsageMonitoringLimit;
		public bool EnableMemoryUsageMonitoring;

		public WebClientUpdateConfiguration UpdateConfiguration;

		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
		[SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "BaseLine")]
		public WebClientConfiguration(string webServiceUrl, string webServiceUser, string webServicePwd, int requestPauseInSeconds, string localMachineName, bool proxyEnabled, string proxyAddress, int proxyPort,
			string proxyUser, string proxyPwd, bool proxyUseDefaultSystemSettings, bool windowsServiceConfigurationSelected, int secondsBetweenScanForNewPrinters,
			bool keepAliveEnabled, int keepAliveTime, int keepAliveInterval, int numberOfLoopsToCheckForUpdate, int remotePrintingServiceTimeoutInSeconds, bool enableSignalR, bool nnableSignalRPause, int reconnectionAttempts, int reconnectionLimitMinutes, int pauseSignalRMinutes, bool expect100Continue,
			int connectionRetryAttempts, int retryDelay, bool enableVerboseLogging, int jobPrintingTimeout,
			WebClientUpdateConfiguration updateConfiguration, bool enableMemoryUsageMonitoring, int memoryUsageMonitoringLimit)
		{
			WebServiceUrl = webServiceUrl;
			WebServiceUser = webServiceUser;
			WebServicePwd = webServicePwd;
			RequestPauseInSeconds = requestPauseInSeconds;
			LocalMachineName = localMachineName;
			ProxyEnabled = proxyEnabled;
			ProxyAddress = proxyAddress;
			ProxyPort = proxyPort;
			ProxyUser = proxyUser;
			ProxyPwd = proxyPwd;
			ProxyUseDefaultSystemSettings = proxyUseDefaultSystemSettings;
			WindowsServiceConfigurationSelected = windowsServiceConfigurationSelected;
			SecondsBetweenScanForNewPrinters = secondsBetweenScanForNewPrinters;
			KeepAliveEnabled = keepAliveEnabled;
			KeepAliveTime = keepAliveTime;
			KeepAliveInterval = keepAliveInterval;
			Expect100Continue = expect100Continue;
			NumberOfLoopsToCheckForUpdate = numberOfLoopsToCheckForUpdate;
			RemotePrintingServiceTimeoutInSeconds = remotePrintingServiceTimeoutInSeconds;
			EnableSignalR = enableSignalR;
			EnableSignalRPause = nnableSignalRPause;
			ReconnectionAttempts = reconnectionAttempts;
			ReconnectionLimitMinutes = reconnectionLimitMinutes;
			PauseSignalRMinutes = pauseSignalRMinutes;
			ConnectionRetryAttempts = connectionRetryAttempts;
			RetryDelay = retryDelay;
			EnableVerboseLogging = enableVerboseLogging;
			JobPrintingTimeout = jobPrintingTimeout;
			UpdateConfiguration = updateConfiguration;
			EnableMemoryUsageMonitoring = enableMemoryUsageMonitoring;
			MemoryUsageMonitoringLimit = memoryUsageMonitoringLimit;
		}
	}
}
