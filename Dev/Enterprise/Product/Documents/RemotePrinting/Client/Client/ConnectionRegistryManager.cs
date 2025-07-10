using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using Microsoft.Win32;

namespace Enterprise.RemotePrinting.Client
{
	[ImmutableObject(true)]
	public class ConnectionRegistryManager
	{
		protected ConnectionRegistryManager()
		{
		}

		public void LoadFromRegistry(string configurationName)
		{
			keyName = GetFullKeyName(configurationName);
			LoadFromRegistryCore(keyName);
		}

		protected virtual void LoadFromRegistryCore(string keyName)
		{
			ediWebPrintKey?.Dispose();
			ediWebPrintKey = FindKey(keyName);
		}

		internal string GetFullKeyName(string configurationName)
		{
			if (string.IsNullOrEmpty(configurationName) || configurationName == Constants.RegistryManager.WebPrintKeyName)
			{
				return WebPrintKeyName;
			}
			else
			{
				return WebPrintKeyName + "-" + configurationName;
			}
		}

		public RegistryKey FindKey(string keyName)
		{
			var res = RegistryRoot.OpenSubKey(keyName, true);
			if (res == null)
			{
				res = RegistryRoot.CreateSubKey(keyName);
				CopyFromOldRegistrySettings();
			}
			return res;
		}

		public string[] GetConfigurationNamesInRegistry(string keyName)
		{
			string[] subKeyNames;
			using (var key = FindKey(keyName))
			{
				subKeyNames = key.GetSubKeyNames();
			}
			return subKeyNames.Where(name => name.StartsWith(Constants.RegistryManager.WebPrintKeyName, StringComparison.Ordinal)).Select(x => x == Constants.RegistryManager.WebPrintKeyName ? x : RegistryHelper.GetConfigurationShortName(x)).ToArray();
		}

		public string GetCurrentlySelectedConfigForWindowsService(string topName)
		{
			return RegistryHelper.GetCurrentlySelectedConfigForWindowsService(RegistryRoot, topName);
		}

		public static readonly ConnectionRegistryManager Instance = new ConnectionRegistryManager();

		public WebClientConfiguration GetWebClientConfiguration(string configurationName)
		{
			lock (ediWebPrintKeyMutex)
			{
				LoadFromRegistry(configurationName);
				var config = new WebClientConfiguration
				{
					WebServiceUrl = GetRemotePrintingWebServiceUrl(),
					WebServiceUser = GetRemotePrintingWebServiceUser(),
					WebServicePwd = GetRemotePrintingWebServicePwd(),
					RequestPauseInSeconds = GetRemotePrintingRequestPauseInSeconds(),
					LocalMachineName = GetLocalMachineName(),
					ProxyEnabled = GetRemotePrintingProxyEnabled(),
					ProxyAddress = GetRemotePrintingProxyAddress(),
					ProxyPort = GetRemotePrintingProxyPort(),
					ProxyUser = GetRemotePrintingProxyUser(),
					ProxyPwd = GetRemotePrintingProxyPwd(),
					ProxyUseDefaultSystemSettings = GetRemotePrintingProxyUseDefaultSystemSettings(),
					WindowsServiceConfigurationSelected = GetWindowsServiceConfigurationSelected(),
					SecondsBetweenScanForNewPrinters = GetRefreshTimeForNewPrintersScanInSeconds(),
					KeepAliveEnabled = GetKeepAliveEnabled(),
					KeepAliveTime = GetKeepAliveTime(),
					KeepAliveInterval = GetKeepAliveInterval(),
					RemotePrintingServiceTimeoutInSeconds = GetRemotePrintingServiceTimeoutInSeconds(),
					NumberOfLoopsToCheckForUpdate = GetNumberOfLoopsToCheckForUpdate(),
					EnableSignalR = GetEnableSignalR(),
					EnableSignalRPause = GetEnableSignalRPause(),
					ReconnectionAttempts = GetReconnectionAttempts(),
					ReconnectionLimitMinutes = GetReconnectionLimitMinutes(),
					PauseSignalRMinutes = GetPauseSignalRMinutes(),
					Expect100Continue = GetExpect100Continue(),
					ConnectionRetryAttempts = GetConnectionRetryAttemps(),
					RetryDelay = GetRetryDelay(),
					EnableVerboseLogging = GetEnableVerboseLogging(),
					JobPrintingTimeout = GetJobPrintingTimeout(),
					UpdateConfiguration = GetUpdateConfiguration(),
					EnableMemoryUsageMonitoring = GetEnableMemoryUsageMonitoring(),
					MemoryUsageMonitoringLimit = GetMemoryUsageMonitoringLimit(),
				};
				return config;
			}
		}

		public string GetLocalMachineName()
		{
			string fullMachineName = (string)ediWebPrintKey.GetValue(ValueNames.LocalMachine, System.Environment.MachineName.Trim());
			return (fullMachineName.Length > 128) ? fullMachineName.Substring(0, 128) : fullMachineName;
		}

		public string GetRemotePrintingWebServiceUrl()
		{
			return (string)ediWebPrintKey.GetValue(ValueNames.WebServiceUrl, "");
		}

		public string GetRemotePrintingWebServiceUser()
		{
			return (string)ediWebPrintKey.GetValue(ValueNames.WebServiceUser, "");
		}

		public string GetRemotePrintingWebServicePwd()
		{
			return (string)ediWebPrintKey.GetValue(ValueNames.WebServicePwd, "");
		}

		public int GetIntRegistryItem(string registryItemName, int defaultValue, RegistryKey key = null)
		{
			object rawValue;
			if (key == null)
			{
				rawValue = GetRawValueWithRetry(ediWebPrintKey, registryItemName, defaultValue.ToString(CultureInfo.InvariantCulture));
			}
			else
			{
				rawValue = key.GetValue(registryItemName, defaultValue.ToString(CultureInfo.InvariantCulture));
			}

			if (rawValue is int intValue)
			{
				return intValue;
			}

			if (rawValue == null || !int.TryParse(rawValue.ToString(), out var result))
			{
				result = defaultValue;
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Retry logic implemented to handle potential ObjectDisposedException when accessing closed registry keys.")]
		public object GetRawValueWithRetry(RegistryKey registrykey, string name, object value)
		{
			var retryCount = 0;
			while (retryCount < 3)
			{
				try
				{
					return registrykey.GetValue(name, value);
				}
				catch (ObjectDisposedException ex) when (ex.Message.Contains("Cannot access a closed registry key"))
				{
					registrykey = FindKey(keyName);
					retryCount++;
				}
			}
			return null;
		}

		bool GetBoolRegistryItem(string registryItemName, bool defaultValue)
		{
			return Convert.ToBoolean(ediWebPrintKey.GetValue(registryItemName, defaultValue), CultureInfo.InvariantCulture);
		}

		DateTime GetDateTimeRegistryItem(string registryItemName, DateTime defaultValue, string format)
		{
			var rawValue = (string)ediWebPrintKey.GetValue(registryItemName, defaultValue.ToString(format, CultureInfo.InvariantCulture));

			if (!DateTime.TryParseExact(rawValue, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var result))
			{
				result = defaultValue;
			}

			if (result < DateTime.MinValue)
			{
				result = DateTime.Today + result.TimeOfDay;
			}

			return result;
		}

		public RegistryKey GetLastConfigurationRegistryValue() => FindKey(Constants.RegistryManager.CargoWiseWebPrintTopKeyName);

		public void SetLastConfigurationRegistryValue(RegistryKey registryKey, string valueName, RegistryValueKind valueKind) => registryKey.SetValue(valueName, false, valueKind);

		public int GetRemotePrintingRequestPauseInSeconds() => GetIntRegistryItem(ValueNames.Pause, DefaultValues.RequestPause);

		public int GetRefreshTimeForNewPrintersScanInSeconds() => GetIntRegistryItem(ValueNames.RefreshPrintersScan, DefaultValues.RefreshPrintersScan);

		public bool GetWindowsServiceConfigurationSelected() => GetBoolRegistryItem(ValueNames.WindowsServiceConfiguration, false);

		public bool GetRemotePrintingProxyEnabled() => GetBoolRegistryItem(ValueNames.ProxyEnabled, false);

		public int GetRemotePrintingProxyPort() => GetIntRegistryItem(ValueNames.ProxyPort, DefaultValues.ProxyPort);

		public string GetRemotePrintingProxyAddress() => (string)ediWebPrintKey.GetValue(ValueNames.ProxyAddress, "");

		public string GetRemotePrintingProxyUser() => (string)ediWebPrintKey.GetValue(ValueNames.ProxyUser, "");

		public string GetRemotePrintingProxyPwd() => (string)ediWebPrintKey.GetValue(ValueNames.ProxyPwd, "");

		public bool GetRemotePrintingProxyUseDefaultSystemSettings() => GetBoolRegistryItem(ValueNames.ProxyUseDefaultSystemSettings, false);

		public bool GetKeepAliveEnabled() => GetBoolRegistryItem(ValueNames.KeepAliveEnabled, false);

		public int GetKeepAliveTime() => GetIntRegistryItem(ValueNames.KeepAliveTime, DefaultValues.KeepAliveTime);

		public int GetKeepAliveInterval() => GetIntRegistryItem(ValueNames.KeepAliveInterval, DefaultValues.KeepAliveInterval);

		public bool GetExpect100Continue() => Convert.ToBoolean(ediWebPrintKey.GetValue(ValueNames.EnableExpect100ContinueName, false), CultureInfo.CurrentCulture);

		public int GetRemotePrintingServiceTimeoutInSeconds() => GetIntRegistryItem(ValueNames.RemotePrintingServiceTimeoutInSeconds, DefaultValues.RemotePrintingServiceTimeoutInSeconds);

		public int GetNumberOfLoopsToCheckForUpdate() => GetIntRegistryItem(ValueNames.NumberOfLoopsToCheckForUpdate, DefaultValues.NumberOfLoopsToCheckForUpdate);

		public bool GetEnableSignalR() => GetBoolRegistryItem(ValueNames.EnableSignalR, true);

		public bool GetEnableSignalRPause() => GetBoolRegistryItem(ValueNames.EnableSignalRPause, true);

		public int GetReconnectionAttempts() => GetIntRegistryItem(ValueNames.ReconnectionAttempts, DefaultValues.ReconnectionAttempts);

		public int GetReconnectionLimitMinutes() => GetIntRegistryItem(ValueNames.ReconnectionLimitMinutes, DefaultValues.ReconnectionLimitMinutes);

		public int GetPauseSignalRMinutes() => GetIntRegistryItem(ValueNames.PauseSignalRMinutes, DefaultValues.PauseSignalRMinutes);

		public bool GetEnableMemoryUsageMonitoring() => GetBoolRegistryItem(ValueNames.EnableMemoryUsageMonitoring, false);

		public int GetMemoryUsageMonitoringLimit() => GetIntRegistryItem(ValueNames.MemoryUsageMonitoringLimit, DefaultValues.MemoryUsageMonitoringLimit);

		public int GetConnectionRetryAttemps() => GetIntRegistryItem(ValueNames.ConnectionRetryAttemps, DefaultValues.ConnectionRetryAttemps);

		public int GetRetryDelay() => GetIntRegistryItem(ValueNames.RetryDelay, DefaultValues.RetryDelay);

		public bool GetEnableVerboseLogging() => GetBoolRegistryItem(ValueNames.EnableVerboseLogging, false);

		public int GetJobPrintingTimeout() => GetIntRegistryItem(ValueNames.JobPrintingTimeout, DefaultValues.JobPrintingTimeout);

		public WebClientUpdateConfiguration GetUpdateConfiguration()
		{
			return new WebClientUpdateConfiguration
			{
				Mode = GetUpdateMode(),
				SendDailyNotificationAboutNewVersion = GetSendDailyNotificationAboutNewVersion(),
				AutomaticUpdateToMajorVersion = GetAutomaticUpdateToMajorVersion(),
				AutomaticUpdateToMinorVersion = GetAutomaticUpdateToMinorVersion(),
				ForceAutomaticUpdateAfterNDays = GetForceAutomaticUpdateAfterNDays(),
				UpdateAllowedTimeFromSafe = GetUpdateAllowedTimeFrom(),
				UpdateAllowedTimeToSafe = GetUpdateAllowedTimeTo(),
				UpdateAllowedDaysOfWeek = GetUpdateAllowedDaysOfWeek(),
				NotifyBeforeUpdate = GetNotifyBeforeUpdate(),
				NotifyAfterUpdate = GetNotifyAfterUpdate(),
				VersionBeforeUpdate = GetVersionBeforeUpdate(),
				UpdateRunningDate = GetUpdateRunningDate(),
				PauseAutomaticUpdateHours = GetPauseAutomaticUpdateHours(),
				NewUpdateAppearedDate = GetNewUpdateAppearedDate(),
				NewUpdateLastNotificationDate = GetNewUpdateLastNotificationDate(),
				UpdateWithNotMatchingLastNotificationDate = GetUpdateWithNotMatchingLastNotificationDate()
			};
		}

		public WebClientUpdateConfiguration.UpdateMode GetUpdateMode() => (WebClientUpdateConfiguration.UpdateMode)GetIntRegistryItem(ValueNames.UpdateMode, (int)DefaultValues.UpdateMode);

		public bool GetSendDailyNotificationAboutNewVersion() => GetBoolRegistryItem(ValueNames.SendDailyNotificationAboutNewVersion, DefaultValues.SendDailyNotificationAboutNewVersion);

		public bool GetAutomaticUpdateToMajorVersion() => GetBoolRegistryItem(ValueNames.AutomaticUpdateToMajorVersion, DefaultValues.AutomaticUpdateToMajorVersion);

		public bool GetAutomaticUpdateToMinorVersion() => GetBoolRegistryItem(ValueNames.AutomaticUpdateToMinorVersion, DefaultValues.AutomaticUpdateToMinorVersion);

		public int GetForceAutomaticUpdateAfterNDays() => GetIntRegistryItem(ValueNames.ForceAutomaticUpdateAfterNDays, DefaultValues.ForceAutomaticUpdateAfterNDays);

		public DateTime GetUpdateAllowedTimeFrom() => GetDateTimeRegistryItem(ValueNames.UpdateAllowedTimeFrom, DefaultUpdateAllowedTimeFrom, TimeFormat);

		public DateTime GetUpdateAllowedTimeTo() => GetDateTimeRegistryItem(ValueNames.UpdateAllowedTimeTo, DefaultUpdateAllowedTimeTo, TimeFormat);

		public WebClientUpdateConfiguration.DaysOfWeek GetUpdateAllowedDaysOfWeek() => (WebClientUpdateConfiguration.DaysOfWeek)GetIntRegistryItem(ValueNames.UpdateAllowedDaysOfWeek, (int)DefaultValues.UpdateAllowedDaysOfWeek);

		public bool GetNotifyBeforeUpdate() => GetBoolRegistryItem(ValueNames.NotifyBeforeUpdate, DefaultValues.NotifyBeforeUpdate);

		public bool GetNotifyAfterUpdate() => GetBoolRegistryItem(ValueNames.NotifyAfterUpdate, DefaultValues.NotifyAfterUpdate);

		public string GetVersionBeforeUpdate() => (string)ediWebPrintKey.GetValue(ValueNames.VersionBeforeUpdate, string.Empty);

		public DateTime GetUpdateRunningDate() => GetDateTimeRegistryItem(ValueNames.UpdateRunningDate, WebClientUpdateConfiguration.EmptyDate, DateTimeFormat);

		public int GetPauseAutomaticUpdateHours() => GetIntRegistryItem(ValueNames.PauseAutomaticUpdateHours, DefaultValues.PauseAutomaticUpdateHours);

		public DateTime GetNewUpdateAppearedDate() => GetDateTimeRegistryItem(ValueNames.NewUpdateAppearedDate, WebClientUpdateConfiguration.EmptyDate, DateFormat);

		public DateTime GetNewUpdateLastNotificationDate() => GetDateTimeRegistryItem(ValueNames.NewUpdateLastNotificationDate, WebClientUpdateConfiguration.EmptyDate, DateTimeFormat);

		public DateTime GetUpdateWithNotMatchingLastNotificationDate() => GetDateTimeRegistryItem(ValueNames.UpdateWithNotMatchingLastNotificationDate, WebClientUpdateConfiguration.EmptyDate, DateFormat);

		public void SaveRemotePrintingRegistryValues(WebClientConfiguration config)
		{
			ediWebPrintKey.SetValue(ValueNames.WebServiceUrl, config.WebServiceUrl, RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.WebServiceUser, config.WebServiceUser, RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.WebServicePwd, config.WebServicePwd, RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.Pause, config.RequestPauseInSeconds, RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.ProxyEnabled, config.ProxyEnabled, RegistryValueKind.DWord);
			ediWebPrintKey.SetValue(ValueNames.ProxyAddress, config.ProxyAddress, RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.ProxyPort, config.ProxyPort, RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.ProxyUser, config.ProxyUser, RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.ProxyPwd, config.ProxyPwd, RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.ProxyUseDefaultSystemSettings, config.ProxyUseDefaultSystemSettings, RegistryValueKind.DWord);
			ediWebPrintKey.SetValue(ValueNames.WindowsServiceConfiguration, config.WindowsServiceConfigurationSelected, RegistryValueKind.DWord);
			ediWebPrintKey.SetValue(ValueNames.RefreshPrintersScan, config.SecondsBetweenScanForNewPrinters, RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.KeepAliveEnabled, config.KeepAliveEnabled, RegistryValueKind.DWord);
			ediWebPrintKey.SetValue(ValueNames.KeepAliveTime, config.KeepAliveTime, RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.KeepAliveInterval, config.KeepAliveInterval, RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.NumberOfLoopsToCheckForUpdate, config.NumberOfLoopsToCheckForUpdate, RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.RemotePrintingServiceTimeoutInSeconds, config.RemotePrintingServiceTimeoutInSeconds, RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.EnableSignalR, config.EnableSignalR, RegistryValueKind.DWord);
			ediWebPrintKey.SetValue(ValueNames.EnableSignalRPause, config.EnableSignalRPause, RegistryValueKind.DWord);
			ediWebPrintKey.SetValue(ValueNames.ReconnectionAttempts, config.ReconnectionAttempts, RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.ReconnectionLimitMinutes, config.ReconnectionLimitMinutes, RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.PauseSignalRMinutes, config.PauseSignalRMinutes, RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.EnableExpect100ContinueName, config.Expect100Continue, RegistryValueKind.DWord);
			ediWebPrintKey.SetValue(ValueNames.ConnectionRetryAttemps, config.ConnectionRetryAttempts, RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.RetryDelay, config.RetryDelay, RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.EnableVerboseLogging, config.EnableVerboseLogging, RegistryValueKind.DWord);
			ediWebPrintKey.SetValue(ValueNames.JobPrintingTimeout, config.JobPrintingTimeout, RegistryValueKind.DWord);
			ediWebPrintKey.SetValue(ValueNames.EnableMemoryUsageMonitoring, config.EnableMemoryUsageMonitoring, RegistryValueKind.DWord);
			ediWebPrintKey.SetValue(ValueNames.MemoryUsageMonitoringLimit, config.MemoryUsageMonitoringLimit, RegistryValueKind.String);

			if (string.IsNullOrEmpty(config.LocalMachineName) || config.LocalMachineName.Equals(System.Environment.MachineName.Trim(), StringComparison.OrdinalIgnoreCase))
			{
				ediWebPrintKey.DeleteValue(ValueNames.LocalMachine, false);
			}
			else
			{
				ediWebPrintKey.SetValue(ValueNames.LocalMachine, config.LocalMachineName, RegistryValueKind.String);
			}

			SaveRemotePrintingUpdateConfigRegistryValues(config.UpdateConfiguration);
		}

		public void SaveRemotePrintingUpdateConfigRegistryValues(WebClientUpdateConfiguration updateConfiguration)
		{
			ediWebPrintKey.SetValue(ValueNames.UpdateMode, (int)updateConfiguration.Mode, RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.SendDailyNotificationAboutNewVersion, updateConfiguration.SendDailyNotificationAboutNewVersion, RegistryValueKind.DWord);
			ediWebPrintKey.SetValue(ValueNames.AutomaticUpdateToMajorVersion, updateConfiguration.AutomaticUpdateToMajorVersion, RegistryValueKind.DWord);
			ediWebPrintKey.SetValue(ValueNames.AutomaticUpdateToMinorVersion, updateConfiguration.AutomaticUpdateToMinorVersion, RegistryValueKind.DWord);
			ediWebPrintKey.SetValue(ValueNames.ForceAutomaticUpdateAfterNDays, updateConfiguration.ForceAutomaticUpdateAfterNDays, RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.UpdateAllowedTimeFrom, updateConfiguration.UpdateAllowedTimeFromSafe.ToString(TimeFormat, CultureInfo.InvariantCulture), RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.UpdateAllowedTimeTo, updateConfiguration.UpdateAllowedTimeToSafe.ToString(TimeFormat, CultureInfo.InvariantCulture), RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.UpdateAllowedDaysOfWeek, (int)updateConfiguration.UpdateAllowedDaysOfWeek, RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.NotifyBeforeUpdate, updateConfiguration.NotifyBeforeUpdate, RegistryValueKind.DWord);
			ediWebPrintKey.SetValue(ValueNames.NotifyAfterUpdate, updateConfiguration.NotifyAfterUpdate, RegistryValueKind.DWord);
			ediWebPrintKey.SetValue(ValueNames.PauseAutomaticUpdateHours, updateConfiguration.PauseAutomaticUpdateHours, RegistryValueKind.String);

			SaveRemotePrintingUpdateTrackingConfigRegistryValuesCore(updateConfiguration);
			SaveRemotePrintingUpdateWithNotMatchingConfigRegistryValuesCore(updateConfiguration);
		}

		public void SaveRemotePrintingUpdateTrackingConfigRegistryValues(string configurationName, WebClientUpdateConfiguration updateConfiguration)
		{
			var oldEdiWebPrintKey = ediWebPrintKey;
			try
			{
				LoadFromRegistry(configurationName);
				SaveRemotePrintingUpdateTrackingConfigRegistryValuesCore(updateConfiguration);
			}
			finally
			{
				if (oldEdiWebPrintKey != null)
				{
					ediWebPrintKey = oldEdiWebPrintKey;
				}
			}
		}

		public void SaveRemotePrintingUpdateTrackingConfigRegistryValuesCore(WebClientUpdateConfiguration updateConfiguration)
		{
			ediWebPrintKey.SetValue(ValueNames.VersionBeforeUpdate, updateConfiguration.VersionBeforeUpdate ?? string.Empty, RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.UpdateRunningDate, updateConfiguration.UpdateRunningDate.ToString(DateTimeFormat, CultureInfo.InvariantCulture), RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.NewUpdateAppearedDate, updateConfiguration.NewUpdateAppearedDate.ToString(DateFormat, CultureInfo.InvariantCulture), RegistryValueKind.String);
			ediWebPrintKey.SetValue(ValueNames.NewUpdateLastNotificationDate, updateConfiguration.NewUpdateLastNotificationDate.ToString(DateTimeFormat, CultureInfo.InvariantCulture), RegistryValueKind.String);
		}

		public void SaveRemotePrintingUpdateWithNotMatchingConfigRegistryValuesCore(WebClientUpdateConfiguration updateConfiguration)
		{
			ediWebPrintKey.SetValue(ValueNames.UpdateWithNotMatchingLastNotificationDate, updateConfiguration.UpdateWithNotMatchingLastNotificationDate.ToString(DateFormat, CultureInfo.InvariantCulture), RegistryValueKind.String);
		}

		public string GetLastConfigurationValue()
		{
			using (var key = FindKey(Constants.RegistryManager.CargoWiseWebPrintTopKeyName))
			{
				return (string)key.GetValue(ValueNames.LastUsedConfiguration);
			}
		}

		public void SetLastUsedConfigurationValue(string lastConfiguration)
		{
			if (lastConfiguration != null)
			{
				using (var key = FindKey(Constants.RegistryManager.CargoWiseWebPrintTopKeyName))
				{
					key.SetValue(ValueNames.LastUsedConfiguration, lastConfiguration);
				}
			}
		}

		void CopyFromOldRegistrySettings()
		{
			if (Registry64.RegOpenKeyEx(Registry64.HKEY_LOCAL_MACHINE, EdiKeyName, 0, RegSam.KEY_READ | RegSam.KEY_WOW64_64KEY, out var key) == WinError.ERROR_SUCCESS)
			{
				try
				{
					var config = new WebClientConfiguration();
					config.WebServiceUrl = Registry64.RegQueryString(key, ValueNames.WebServiceUrl) ?? "";
					config.WebServiceUser = Registry64.RegQueryString(key, ValueNames.WebServiceUser) ?? "";
					config.WebServicePwd = Registry64.RegQueryString(key, ValueNames.WebServicePwd) ?? "";
					if (!int.TryParse(Registry64.RegQueryString(key, ValueNames.Pause), out var requestPauseInSeconds))
					{
						requestPauseInSeconds = DefaultValues.RequestPause;
					}
					config.RequestPauseInSeconds = requestPauseInSeconds;
					config.LocalMachineName = Registry64.RegQueryString(key, ValueNames.LocalMachine) ?? "";
					if (!int.TryParse(Registry64.RegQueryString(key, ValueNames.RefreshPrintersScan), out var refreshScanForNewPrinters))
					{
						refreshScanForNewPrinters = DefaultValues.RefreshPrintersScan;
					}
					config.SecondsBetweenScanForNewPrinters = refreshScanForNewPrinters;
					SaveRemotePrintingRegistryValues(config);
				}
				finally
				{
					Registry64.RegCloseKey(key);
				}
			}
		}

		public void UnmarkWindowsServiceConfigurationSelectedForKey(string keyName)
		{
			using (var key = RegistryRoot.OpenSubKey(GetFullKeyName(keyName), true))
			{
				key?.SetValue(ValueNames.WindowsServiceConfiguration, false, RegistryValueKind.DWord);
			}
			MarkWindowsServiceConfigurationSelectedForKey();
		}

		public void MarkWindowsServiceConfigurationSelectedForKey()
		{
			ediWebPrintKey.SetValue(ValueNames.WindowsServiceConfiguration, true, RegistryValueKind.DWord);
		}

		public void DeleteFromRegistry(string configName)
		{
			RegistryRoot.DeleteSubKey(GetFullKeyName(configName), false);
		}

		const string ProtectedAllPasswordVersion = "1.0"; //Just change this version if need to re-run Protect password

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Encrypts passwords for all configurations and manages registry operations safely.")]
		public void ProtectWebServicePasswordForAllConfigurations(Action<string> onShowInformation)
		{
			onShowInformation?.Invoke("Encrypt password");
			try
			{
				using (var key = FindKey(WebPrintTopKeyName))
				{
					var protectedStatusKey = ValueNames.ProtectedAllPassword + ProtectedAllPasswordVersion;
					var protectedStatus = Convert.ToBoolean(key.GetValue(protectedStatusKey), CultureInfo.InvariantCulture);
					if (protectedStatus)
					{
						return;
					}

					var configNames = GetConfigurationNamesInRegistry(WebPrintTopKeyName);
					if (configNames.Length == 0)
					{
						return;
					}

					foreach (var configName in configNames)
					{
						var keyName = GetFullKeyName(configName);
						var rootKey = RegistryRoot.OpenSubKey(keyName, true);
						if (rootKey != null)
						{
							ProtectPassword(rootKey, ValueNames.WebServicePwd);
							ProtectPassword(rootKey, ValueNames.ProxyPwd);
						}
					}

					key.SetValue(protectedStatusKey, true, RegistryValueKind.DWord);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				onShowInformation?.Invoke($"Encrypt password failed, the error message is {ex.Message}");
			}
		}

		void ProtectPassword(RegistryKey rootKey, string keyName)
		{
			var pwd = (string)rootKey.GetValue(keyName, "");
			var unprotectedPwd = ProtectedDataHelper.Unprotect(pwd);
			if (!string.IsNullOrEmpty(pwd) && pwd == unprotectedPwd)
			{
				var protectedPwd = ProtectedDataHelper.Protect(pwd);
				rootKey.SetValue(keyName, protectedPwd);
			}
		}

		public static class ValueNames
		{
			public const string WebServiceUrl = "WebServiceUrl";
			public const string WebServiceUser = "WebServiceUser";
			public const string WebServicePwd = "WebServicePwd";
			public const string Pause = "RequestPauseInSeconds";
			public const string LocalMachine = "LocalMachineName";
			public const string ProxyEnabled = "ProxyEnabled";
			public const string ProxyAddress = "ProxyAddress";
			public const string ProxyPort = "ProxyPort";
			public const string ProxyUser = "ProxyUser";
			public const string ProxyPwd = "ProxyPwd";
			public const string ProxyUseDefaultSystemSettings = "ProxyUseDefaultSystemSettings";
			public const string WindowsServiceConfiguration = RegistryHelper.WindowsServiceConfiguration;
			public const string RefreshPrintersScan = "RefreshPrintersScanInSeconds";
			public const string KeepAliveEnabled = "KeepAliveEnabled";
			public const string KeepAliveTime = "KeepAliveTime";
			public const string KeepAliveInterval = "KeepAliveInterval";
			public const string NumberOfLoopsToCheckForUpdate = "NumberOfLoopsToCheckForUpdate";
			public const string RemotePrintingServiceTimeoutInSeconds = "RemotePrintingServiceTimeoutInSeconds";
			public const string EnableSignalR = "EnableSignalR";
			public const string EnableSignalRPause = "EnableSignalRPause";
			public const string ReconnectionAttempts = "ReconnectionAttempts";
			public const string ReconnectionLimitMinutes = "ReconnectionLimitMinutes";
			public const string PauseSignalRMinutes = "PauseSignalRMinutes";
			public const string EnableExpect100ContinueName = "Expect100Continue";
			public const string EnableVerboseLogging = "EnableVerboseLogging";
			public const string JobPrintingTimeout = "JobPrintingTimeoutInMinutes";

			public const string UpdateMode = "UpdateMode";
			public const string SendDailyNotificationAboutNewVersion = "SendDailyNotificationAboutNewVersion";
			public const string AutomaticUpdateToMajorVersion = "AutomaticUpdateToMajorVersion";
			public const string AutomaticUpdateToMinorVersion = "AutomaticUpdateToMinorVersion";
			public const string ForceAutomaticUpdateAfterNDays = "ForceAutomaticUpdateAfterNDays";
			public const string UpdateAllowedTimeFrom = "UpdateAllowedTimeFrom";
			public const string UpdateAllowedTimeTo = "UpdateAllowedTimeTo";
			public const string UpdateAllowedDaysOfWeek = "UpdateAllowedDaysOfWeek";
			public const string NotifyBeforeUpdate = "NotifyBeforeUpdate";
			public const string NotifyAfterUpdate = "NotifyAfterUpdate";
			public const string VersionBeforeUpdate = "VersionBeforeUpdate";
			public const string UpdateRunningDate = "UpdateRunningDate";
			public const string PauseAutomaticUpdateHours = "PauseAutomaticUpdateHours";
			public const string NewUpdateAppearedDate = "NewUpdateAppearedDate";
			public const string NewUpdateLastNotificationDate = "NewUpdateLastNotificationDate";
			public const string ConnectionRetryAttemps = "ConnectionRetryAttemps";
			public const string RetryDelay = "RetryDelay";
			public const string UpdateWithNotMatchingLastNotificationDate = "UpdateWithNotMatchingLastNotificationDate";

			public const string LastUsedConfiguration = "LastUsedConfiguration";
			public const string EnableMemoryUsageMonitoring = "EnableMemoryUsageMonitoring";
			public const string MemoryUsageMonitoringLimit = "MemoryUsageMonitoringLimit";

			public const string ProtectedAllPassword = "ProtectedAllPassword";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "BaseLine")]
		public static class DefaultValues
		{
			public const int RequestPause = 30;
			public const int ProxyPort = 0;
			public const int RefreshPrintersScan = 600;
			public const int KeepAliveTime = 10;
			public const int KeepAliveInterval = 5;
			public const int NumberOfLoopsToCheckForUpdate = 100;
			public const int RemotePrintingServiceTimeoutInSeconds = 100;
			public const int ConnectionRetryAttemps = 0;
			public const int RetryDelay = 0;
			public const int JobPrintingTimeout = 0;
			public const int MemoryUsageMonitoringLimit = 100;
			public const int ReconnectionAttempts = 3;
			public const int ReconnectionLimitMinutes = 5;
			public const int PauseSignalRMinutes = 20;

			public const WebClientUpdateConfiguration.UpdateMode UpdateMode = WebClientUpdateConfiguration.UpdateMode.Automatic;
			public const int PauseAutomaticUpdateHours = 24;
			public const bool SendDailyNotificationAboutNewVersion = true;
			public const bool AutomaticUpdateToMajorVersion = true;
			public const bool AutomaticUpdateToMinorVersion = false;
			public const int ForceAutomaticUpdateAfterNDays = 0;
			public const WebClientUpdateConfiguration.DaysOfWeek UpdateAllowedDaysOfWeek = WebClientUpdateConfiguration.DaysOfWeek.All;
			public const bool NotifyBeforeUpdate = false;
			public const bool NotifyAfterUpdate = false;
		}

		public readonly DateTime DefaultUpdateAllowedTimeFrom = new DateTime(2020, 8, 18, 0, 0, 0, DateTimeKind.Local);
		public readonly DateTime DefaultUpdateAllowedTimeTo = new DateTime(2020, 8, 18, 0, 0, 0, DateTimeKind.Local);

		public const string TimeFormat = "HH:mm:ss";
		public const string DateFormat = "yyyy-MM-dd";
		public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

		protected virtual string WebPrintKeyName => Constants.RegistryManager.WebPrintConfigKeyName;
		protected virtual string EdiKeyName => Constants.RegistryManager.EdiConfigKeyName;
		protected virtual RegistryKey RegistryRoot => RegistryHelper.DefaultRegistryRoot;
		protected virtual string WebPrintTopKeyName => Constants.RegistryManager.CargoWiseWebPrintTopKeyName;

		RegistryKey ediWebPrintKey;
		string keyName;
		readonly object ediWebPrintKeyMutex = new object();

		internal bool EdiWebPrintKeyIsLoaded => ediWebPrintKey != null;
	}
}
