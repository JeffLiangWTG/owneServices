using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.Core.Testing;
using Microsoft.Win32;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class ConnectionRegistryManagerTest : TestCase
	{
		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		public void TestProtectPasswordForAllConfigurations()
		{
			manager.ProtectWebServicePasswordForAllConfigurations(null);
			var config1 = manager.GetWebClientConfiguration(configName1);
			var config2 = manager.GetWebClientConfiguration(configName2);

			CombineAssertions(() =>
			{
				AssertNotEquals("pwd1", config1.WebServicePwd);
				AssertEquals("pwd1", ProtectedDataHelper.Unprotect(config1.WebServicePwd));

				AssertNotEquals("pwd2", config2.WebServicePwd);
				AssertEquals("pwd2", ProtectedDataHelper.Unprotect(config2.WebServicePwd));

				AssertNotEquals("proxypwd1", config1.ProxyPwd);
				AssertEquals("proxypwd1", ProtectedDataHelper.Unprotect(config1.ProxyPwd));

				AssertNotEquals("proxypwd2", config2.ProxyPwd);
				AssertEquals("proxypwd2", ProtectedDataHelper.Unprotect(config2.ProxyPwd));
			});
		}

		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		[ExpectNoExceptions]
		public void TestCorrectSettingsAreSavedAndLoaded()
		{
			// Assert WebClientConfiguration Are loaded correctly
			// Setup() leaves manager with webConfig2 as current
			CombineAssertions(() =>
			{
				AssertArrayEqualsByElements("Should find both keys in Registry", new string[] { configName1, configName2 }, manager.GetConfigurationNamesInRegistry(manager.ediKeyName_Exposed));
				AssertEquals("Web Service URL should be ok", "url2", manager.GetRemotePrintingWebServiceUrl());
				AssertEquals("Web Service Username should be ok", "user2", manager.GetRemotePrintingWebServiceUser());
				AssertEquals("Web Service Password should be ok", "pwd2", manager.GetRemotePrintingWebServicePwd());
				AssertEquals("Remote Printing Request Pause in secs should be ok", 2, manager.GetRemotePrintingRequestPauseInSeconds());
				AssertEquals("Local machine name should be ok", "machine2", manager.GetLocalMachineName());
				AssertEquals("Proxy enabled should be ok", true, manager.GetRemotePrintingProxyEnabled());
				AssertEquals("Proxy Address should be ok", "proxy2", manager.GetRemotePrintingProxyAddress());
				AssertEquals("Proxy Port should be ok", 43, manager.GetRemotePrintingProxyPort());
				AssertEquals("Proxy User should be ok", "proxyuser2", manager.GetRemotePrintingProxyUser());
				AssertEquals("Proxy Password should be ok", "proxypwd2", manager.GetRemotePrintingProxyPwd());
				AssertEquals("Proxy Use Default System Settings should be ok", true, manager.GetRemotePrintingProxyUseDefaultSystemSettings());
				AssertEquals("Web Service Configuration should be ok", false, manager.GetWindowsServiceConfigurationSelected());
				AssertEquals("Refresh time for New Printers scan in secs should be ok", 7000, manager.GetRefreshTimeForNewPrintersScanInSeconds());
				AssertEquals("Keep alive enabled should be ok", true, manager.GetKeepAliveEnabled());
				AssertEquals("Keep alive time in secs should be ok", 300, manager.GetKeepAliveTime());
				AssertEquals("Keep alive interval in secs should be ok", 5, manager.GetKeepAliveInterval());
				AssertEquals("Enable 100-Continue behavior for HTTP requests", false, manager.GetExpect100Continue());
				AssertEquals("Last configuration should be ok", LastConfigurationName, manager.GetLastConfigurationValue());
				AssertEquals("Connection Retry attempts", 0, manager.GetConnectionRetryAttemps());
				AssertEquals("Retry Delay", 0, manager.GetRetryDelay());
				AssertEquals("Enable Verbose Logging", false, manager.GetEnableVerboseLogging());
				AssertEquals("Job printing timeout", 15, manager.GetJobPrintingTimeout());
				AssertEquals("Enable Pause", true, manager.GetEnableSignalRPause());
				AssertEquals("Reconnection Attempts", 3, manager.GetReconnectionAttempts());
				AssertEquals("Reconnection Limit Minutes", 5, manager.GetReconnectionLimitMinutes());
				AssertEquals("Pause SignalR Minutes", 20, manager.GetPauseSignalRMinutes());
			});

			// Save Again and check
			webConfig2.WebServiceUser = "secondUser2";
			manager.SaveRemotePrintingRegistryValues(webConfig2);
			AssertEquals("secondUser2", manager.GetRemotePrintingWebServiceUser());

			// Check load of different config
			manager.LoadFromRegistry(configName1);
			CombineAssertions(() =>
			{
				AssertEquals("Web Service URL should be ok", "url1", manager.GetRemotePrintingWebServiceUrl());
				AssertEquals("Web Service Username should be ok", "user1", manager.GetRemotePrintingWebServiceUser());
				AssertEquals("Web Service Password should be ok", "pwd1", manager.GetRemotePrintingWebServicePwd());
				AssertEquals("Remote Printing Request Pause in secs should be ok", 1, manager.GetRemotePrintingRequestPauseInSeconds());
				AssertEquals("Local machine name should be ok", "machine1", manager.GetLocalMachineName());
				AssertEquals("Proxy enabled should be ok", false, manager.GetRemotePrintingProxyEnabled());
				AssertEquals("Proxy Address should be ok", "proxy1", manager.GetRemotePrintingProxyAddress());
				AssertEquals("Proxy Port should be ok", 43, manager.GetRemotePrintingProxyPort());
				AssertEquals("Proxy User should be ok", "proxyuser1", manager.GetRemotePrintingProxyUser());
				AssertEquals("Proxy Password should be ok", "proxypwd1", manager.GetRemotePrintingProxyPwd());
				AssertEquals("Proxy Use Default System Settings should be ok", false, manager.GetRemotePrintingProxyUseDefaultSystemSettings());
				AssertEquals("Web Service Configuration should be ok", false, manager.GetWindowsServiceConfigurationSelected());
				AssertEquals("Refresh time for New Printers scan in secs should be ok", 5000, manager.GetRefreshTimeForNewPrintersScanInSeconds());
				AssertEquals("Keep alive enabled should be ok", false, manager.GetKeepAliveEnabled());
				AssertEquals("Keep alive time in secs should be ok", 0, manager.GetKeepAliveTime());
				AssertEquals("Keep alive interval in secs should be ok", 0, manager.GetKeepAliveInterval());
				AssertEquals("Enable 100-Continue behavior for HTTP requests", false, manager.GetExpect100Continue());
				AssertEquals("Connection Retry attempts", 4, manager.GetConnectionRetryAttemps());
				AssertEquals("Retry Delay", 60, manager.GetRetryDelay());
				AssertEquals("Enable Verbose Logging", true, manager.GetEnableVerboseLogging());
				AssertEquals("Job printing timeout", 0, manager.GetJobPrintingTimeout());
				AssertEquals("Last configuration should be ok", LastConfigurationName, manager.GetLastConfigurationValue());
				AssertEquals("Enable Pause", false, manager.GetEnableSignalRPause());
				AssertEquals("Reconnection Attempts", 0, manager.GetReconnectionAttempts());
				AssertEquals("Reconnection Limit Minutes", 0, manager.GetReconnectionLimitMinutes());
				AssertEquals("Pause SignalR Minutes", 0, manager.GetPauseSignalRMinutes());
			});
		}

		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		public void TestDefaultValues()
		{
			var newConfigName = "new_config";

			manager.LoadFromRegistry(newConfigName);

			CombineAssertions(() =>
			{
				AssertEquals("Web Service URL should be ok", "", manager.GetRemotePrintingWebServiceUrl());
				AssertEquals("Web Service Username should be ok", "", manager.GetRemotePrintingWebServiceUser());
				AssertEquals("Web Service Password should be ok", "", manager.GetRemotePrintingWebServicePwd());
				AssertEquals("Remote Printing Request Pause in secs should be ok", ConnectionRegistryManager.DefaultValues.RequestPause, manager.GetRemotePrintingRequestPauseInSeconds());
				AssertEquals("Local machine name should be ok", System.Environment.MachineName.Trim(), manager.GetLocalMachineName());
				AssertEquals("Proxy enabled should be ok", false, manager.GetRemotePrintingProxyEnabled());
				AssertEquals("Proxy Address should be ok", "", manager.GetRemotePrintingProxyAddress());
				AssertEquals("Proxy Port should be ok", ConnectionRegistryManager.DefaultValues.ProxyPort, manager.GetRemotePrintingProxyPort());
				AssertEquals("Proxy User should be ok", "", manager.GetRemotePrintingProxyUser());
				AssertEquals("Proxy Password should be ok", "", manager.GetRemotePrintingProxyPwd());
				AssertEquals("Proxy Use Default System Settings should be ok", false, manager.GetRemotePrintingProxyUseDefaultSystemSettings());
				AssertEquals("Web Service Configuration should be ok", false, manager.GetWindowsServiceConfigurationSelected());
				AssertEquals("Refresh time for New Printers scan in secs should be ok", ConnectionRegistryManager.DefaultValues.RefreshPrintersScan, manager.GetRefreshTimeForNewPrintersScanInSeconds());
				AssertEquals("Keep alive enabled should be ok", false, manager.GetKeepAliveEnabled());
				AssertEquals("Keep alive time in secs should be ok", ConnectionRegistryManager.DefaultValues.KeepAliveTime, manager.GetKeepAliveTime());
				AssertEquals("Keep alive interval in secs should be ok", ConnectionRegistryManager.DefaultValues.KeepAliveInterval, manager.GetKeepAliveInterval());
				AssertEquals("Enable 100-Continue behavior for HTTP requests", false, manager.GetExpect100Continue());
				AssertEquals("Connection Retry attempts", 0, manager.GetConnectionRetryAttemps());
				AssertEquals("Retry Delay", 0, manager.GetRetryDelay());
				AssertEquals("Enable Verbose Logging", false, manager.GetEnableVerboseLogging());
				AssertEquals("Job printing timeout", 0, manager.GetJobPrintingTimeout());
				AssertEquals("Enable Pause", true, manager.GetEnableSignalRPause());
				AssertEquals("Reconnection Attempts", 3, manager.GetReconnectionAttempts());
				AssertEquals("Reconnection Limit Minutes", 5, manager.GetReconnectionLimitMinutes());
				AssertEquals("Pause SignalR Minutes", 20, manager.GetPauseSignalRMinutes());

				manager.DeleteFromRegistry(newConfigName);
			});
		}

		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		public void TestUpdateConfigDefaultValues()
		{
			const string configName = "new_config";
			manager.LoadFromRegistry(configName);
			try
			{
				CombineAssertions(() =>
				{
					AssertEquals(WebClientUpdateConfiguration.UpdateMode.Automatic, manager.GetUpdateMode());
					AssertEquals(true, manager.GetSendDailyNotificationAboutNewVersion());
					AssertEquals(true, manager.GetAutomaticUpdateToMajorVersion());
					AssertEquals(false, manager.GetAutomaticUpdateToMinorVersion());
					AssertEquals(0, manager.GetForceAutomaticUpdateAfterNDays());
					AssertEquals(DateTime.Today + manager.DefaultUpdateAllowedTimeFrom.TimeOfDay, manager.GetUpdateAllowedTimeFrom());
					AssertEquals(DateTime.Today + manager.DefaultUpdateAllowedTimeTo.TimeOfDay, manager.GetUpdateAllowedTimeTo());
					AssertEquals(WebClientUpdateConfiguration.DaysOfWeek.All, manager.GetUpdateAllowedDaysOfWeek());
					AssertEquals(false, manager.GetNotifyBeforeUpdate());
					AssertEquals(false, manager.GetNotifyAfterUpdate());
					AssertEquals(string.Empty, manager.GetVersionBeforeUpdate());
					AssertEquals(WebClientUpdateConfiguration.EmptyDate, manager.GetNewUpdateAppearedDate());
					AssertEquals(WebClientUpdateConfiguration.EmptyDate, manager.GetNewUpdateLastNotificationDate());
					AssertEquals(WebClientUpdateConfiguration.EmptyDate, manager.GetUpdateWithNotMatchingLastNotificationDate());
					AssertEquals(24, manager.GetPauseAutomaticUpdateHours());
					AssertEquals(WebClientUpdateConfiguration.EmptyDate, manager.GetUpdateRunningDate());
				});
			}
			finally
			{
				manager.DeleteFromRegistry(configName);
			}
		}

		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		[ExpectNoExceptions]
		public void TestGetWebClientConfiguration_MultiThreading()
		{
			var managerForTest = new ConnectionRegistryManagerForTest();

			managerForTest.LoadFromRegistry(configName1);
			managerForTest.EdiWebPrintKeyDisposed = () =>
			{
				managerForTest.EdiWebPrintKeyDisposed = () => Thread.Sleep(1000);
			};

			var thread1 = new Task(() => { managerForTest.GetWebClientConfiguration(configName1); });
			var thread2 = new Task(() => { managerForTest.GetWebClientConfiguration(configName1); });

			thread1.Start();
			thread2.Start();
			Task.WaitAll(thread1, thread2);
		}

		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		public void TestUpdateConfigSaveAndLoad()
		{
			const string configName = "new_config";
			manager.LoadFromRegistry(configName);
			try
			{
				var updateConfig = new WebClientUpdateConfiguration
				{
					Mode = WebClientUpdateConfiguration.UpdateMode.Custom,
					SendDailyNotificationAboutNewVersion = false,
					AutomaticUpdateToMajorVersion = false,
					AutomaticUpdateToMinorVersion = true,
					ForceAutomaticUpdateAfterNDays = 3,
					UpdateAllowedTimeFrom = new DateTime(2020, 1, 1, 13, 5, 33),
					UpdateAllowedTimeTo = new DateTime(2020, 1, 1, 13, 5, 34),
					UpdateAllowedDaysOfWeek = WebClientUpdateConfiguration.DaysOfWeek.Saturday | WebClientUpdateConfiguration.DaysOfWeek.Sunday,
					NotifyBeforeUpdate = true,
					NotifyAfterUpdate = true,

					VersionBeforeUpdate = "1.2.3.4",
					NewUpdateAppearedDate = new DateTime(2020, 9, 2, 13, 5, 35),
					NewUpdateLastNotificationDate = new DateTime(2020, 9, 2, 13, 5, 36),
					UpdateWithNotMatchingLastNotificationDate = new DateTime(2021, 2, 6, 14, 46, 35),
					PauseAutomaticUpdateHours = 20,
					UpdateRunningDate = new DateTime(2025, 2, 21, 7, 5, 37)
				};

				manager.SaveRemotePrintingUpdateConfigRegistryValues(updateConfig);

				var newManager = new ConnectionRegistryManagerForTest();
				newManager.LoadFromRegistry(configName);

				CombineAssertions(() =>
				{
					AssertEquals(WebClientUpdateConfiguration.UpdateMode.Custom, newManager.GetUpdateMode());
					AssertEquals(false, newManager.GetSendDailyNotificationAboutNewVersion());
					AssertEquals(false, newManager.GetAutomaticUpdateToMajorVersion());
					AssertEquals(true, newManager.GetAutomaticUpdateToMinorVersion());
					AssertEquals(3, newManager.GetForceAutomaticUpdateAfterNDays());
					AssertEquals(DateTime.Today + new DateTime(2020, 1, 1, 13, 5, 33).TimeOfDay, newManager.GetUpdateAllowedTimeFrom());
					AssertEquals(DateTime.Today + new DateTime(2020, 1, 1, 13, 5, 34).TimeOfDay, newManager.GetUpdateAllowedTimeTo());
					AssertEquals(WebClientUpdateConfiguration.DaysOfWeek.Saturday | WebClientUpdateConfiguration.DaysOfWeek.Sunday, newManager.GetUpdateAllowedDaysOfWeek());
					AssertEquals(true, newManager.GetNotifyBeforeUpdate());
					AssertEquals(true, newManager.GetNotifyAfterUpdate());
					AssertEquals("1.2.3.4", newManager.GetVersionBeforeUpdate());
					AssertEquals(new DateTime(2020, 9, 2, 0, 0, 0), newManager.GetNewUpdateAppearedDate());
					AssertEquals(new DateTime(2020, 9, 2, 13, 5, 36), newManager.GetNewUpdateLastNotificationDate());
					AssertEquals(new DateTime(2021, 2, 6, 0, 0, 0), newManager.GetUpdateWithNotMatchingLastNotificationDate());
					AssertEquals(20, newManager.GetPauseAutomaticUpdateHours());
					AssertEquals(new DateTime(2025, 2, 21, 7, 5, 37), newManager.GetUpdateRunningDate());
				});
			}
			finally
			{
				manager.DeleteFromRegistry(configName);
			}
		}

		public void TestExpected100Continue_SaveAndLoad()
		{
			AssertArrayEqualsByElements("Should find both keys in Registry", new string[] { configName1, configName2 }, manager.GetConfigurationNamesInRegistry(manager.ediKeyName_Exposed));
			AssertEquals("Enable 100-Continue behavior for HTTP requests", false, manager.GetExpect100Continue());

			// Change value and Save Again and check
			webConfig2.WebServiceUser = "secondUser2";
			webConfig2.Expect100Continue = true;
			manager.SaveRemotePrintingRegistryValues(webConfig2);
			AssertEquals("secondUser2", manager.GetRemotePrintingWebServiceUser());

			AssertEquals("Enable 100-Continue behavior for HTTP requests - has changed", true, manager.GetExpect100Continue());
		}

		public void TestKeepAliveDefaultValues()
		{
			AssertEquals(10, ConnectionRegistryManager.DefaultValues.KeepAliveTime);
			AssertEquals(5, ConnectionRegistryManager.DefaultValues.KeepAliveInterval);
		}

		public void TestReturnsCorrectWindowsServiceConfiguration()
		{
			// Set WebConfig1 as Configuration For Windows Service
			webConfig1.WindowsServiceConfigurationSelected = true;
			manager.LoadFromRegistry(configName1);
			manager.SaveRemotePrintingRegistryValues(webConfig1);

			AssertEquals("Should return the correct selected configuration for Windows Service", "configForTest1", manager.GetCurrentlySelectedConfigForWindowsService(manager.ediKeyName_Exposed));
		}

		public void TestReturnsCorrectWindowsServiceConfiguration_WhenNoSelectionMade()
		{
			AssertEquals("Should return 'empty' when looking for selected configuration for Windows Service with no selection", string.Empty, manager.GetCurrentlySelectedConfigForWindowsService(manager.ediKeyName_Exposed));
		}

		public void TestReturnsCorrectWindowsServiceConfiguration_WhenOnlyDefaultExists()
		{
			AssertEquals("Should return the default key when only the legacy default key exists", Constants.RegistryManager.WebPrintKeyName, legacyManager.GetCurrentlySelectedConfigForWindowsService(legacyManager.ediKeyName_Exposed));
		}

		public void TestReturnsCorrectWindowsServiceConfiguration_WhenMixtureOfOldAndNew()
		{
			webConfig1.WindowsServiceConfigurationSelected = true;
			manager.LoadFromRegistry(configName1);
			manager.SaveRemotePrintingRegistryValues(webConfig1);

			var webConfigLegacy = new WebClientConfiguration("url2", "user2", "pwd2", 2, "machine2", true,
				"proxy2", 43, "proxyuser2", "proxypwd2", true,
				windowsServiceConfigurationSelected: true,
				7000, false, 0, 0, 100, 100, false, false, 0, 0, 0, false, 0, 0, false, 0,
				GetWebClientUpdateConfigurationForTest(), false, 100);
			manager.LoadFromRegistry(string.Empty);
			manager.SaveRemotePrintingRegistryValues(webConfigLegacy);

			AssertEquals("Should return the correct selected configuration for Windows Service", "configForTest1", manager.GetCurrentlySelectedConfigForWindowsService(manager.ediKeyName_Exposed));
		}

		public void TestReturnsCorrectWindowsServiceConfiguration_WhenOnlyDefaultIsMarked()
		{
			webConfig1.WindowsServiceConfigurationSelected = false;
			manager.LoadFromRegistry(configName1);
			manager.SaveRemotePrintingRegistryValues(webConfig1);

			var webConfigLegacy = new WebClientConfiguration("url2", "user2", "pwd2", 2, "machine2",
				true, "proxy2", 43, "proxyuser2", "proxypwd2", true,
				windowsServiceConfigurationSelected: true,
				600, false, 10, 5, 100, 100, false, false, 0, 0, 0, false, 0, 0, false, 0,
				GetWebClientUpdateConfigurationForTest(), false, 100);
			manager.LoadFromRegistry(string.Empty);
			manager.SaveRemotePrintingRegistryValues(webConfigLegacy);

			AssertEquals("Should return the correct selected configuration for Windows Service", Constants.RegistryManager.WebPrintKeyName, manager.GetCurrentlySelectedConfigForWindowsService(manager.ediKeyName_Exposed));
		}

		public void TestBaseKeyCreatedOnStartupIfNotPresent()
		{
			Registry.CurrentUser.DeleteSubKeyTree(manager.ediKeyName_Exposed);
			var configurations = manager.GetConfigurationNamesInRegistry(manager.ediKeyName_Exposed);
			AssertEquals(0, configurations.Length);
		}

		protected override void SetUp()
		{
			base.SetUp();

			webConfig1 = new WebClientConfiguration("url1", "user1", "pwd1", 1, "machine1", false, "proxy1", 43, "proxyuser1", "proxypwd1", false, false, 5000, false, 0, 0, 100, 100, false, false, 0, 0, 0, false, 4, 60, true, 0, GetWebClientUpdateConfigurationForTest(), false, 100);
			webConfig2 = new WebClientConfiguration("url2", "user2", "pwd2", 2, "machine2", true, "proxy2", 43, "proxyuser2", "proxypwd2", true, false, 7000, true, 300, 5, 100, 100, false, true, 3, 5, 20, false, 0, 0, false, 15, GetWebClientUpdateConfigurationForTest(), false, 100);
			manager = new ConnectionRegistryManagerForTest();
			configName1 = "configForTest1";
			configName2 = "configForTest2";

			// Create test Registry Keys
			manager.LoadFromRegistry(configName1);
			manager.SaveRemotePrintingRegistryValues(webConfig1);
			manager.LoadFromRegistry(configName2);
			manager.SaveRemotePrintingRegistryValues(webConfig2);
			manager.registryRoot_Exposed.DeleteSubKey(manager.webPrintKeyName_Exposed, false);

			// Create Legacy test Registry Keys
			legacyManager = new ConnectionRegistryManagerForLegacyTest();
			legacyManager.LoadFromRegistry(string.Empty);
			legacyManager.SaveRemotePrintingRegistryValues(webConfig1);

			manager.SetLastUsedConfigurationValue(LastConfigurationName);
		}

		protected override void TearDown()
		{
			base.TearDown();

			// Cleanup test Registry Keys
			manager.DeleteFromRegistry(string.Empty); // There is a unit test where there are both legacy and new keys
			manager.DeleteFromRegistry(configName1);
			manager.DeleteFromRegistry(configName2);
			manager.DeleteLastConfigurationRegistry();
			AssertNull("Should have deleted config #1", manager.registryRoot_Exposed.OpenSubKey(manager.webPrintKeyName_Exposed + "-" + configName1));
			AssertNull("Should have deleted config #2", manager.registryRoot_Exposed.OpenSubKey(manager.webPrintKeyName_Exposed + "-" + configName2));
			Registry.CurrentUser.DeleteSubKey(TestEdiKeyName, false);
			AssertNull("Should have deleted config folder", manager.registryRoot_Exposed.OpenSubKey(manager.webPrintKeyName_Exposed));

			legacyManager.DeleteFromRegistry(string.Empty);
			AssertNull("Should have deleted default config", legacyManager.registryRoot_Exposed.OpenSubKey(manager.webPrintKeyName_Exposed));
			Registry.CurrentUser.DeleteSubKey("SOFTWARE\\CargoWise Legacy tst", false);
		}

		WebClientConfiguration webConfig1;
		WebClientConfiguration webConfig2;
		ConnectionRegistryManagerForTest manager;
		ConnectionRegistryManagerForLegacyTest legacyManager;
		string configName1;
		string configName2;

		public const string TestEdiKeyName = "SOFTWARE\\CargoWise tst";

		public class ConnectionRegistryManagerForTest : ConnectionRegistryManager
		{
			protected override string EdiKeyName => TestEdiKeyName;
			protected override string WebPrintKeyName => TestEdiKeyName + "\\" + Constants.RegistryManager.WebPrintKeyName;
			protected override RegistryKey RegistryRoot => Registry.CurrentUser;
			protected override string WebPrintTopKeyName => TestEdiKeyName;

			protected override void LoadFromRegistryCore(string keyName)
			{
				RegisteredResetTestListener.Instance.RegisterResetAction(() =>
				{
					AssertNull($"Should deleted config [{keyName}] ", RegistryRoot.OpenSubKey(keyName));
				});

				EdiWebPrintKeyDisposed?.Invoke();
				base.LoadFromRegistryCore(keyName);
			}

			public Action EdiWebPrintKeyDisposed;

			public string ediKeyName_Exposed => EdiKeyName;
			public string webPrintKeyName_Exposed => WebPrintKeyName;
			public RegistryKey registryRoot_Exposed => RegistryRoot;

			public void DeleteLastConfigurationRegistry()
			{
				using (var key = FindKey(Constants.RegistryManager.CargoWiseWebPrintTopKeyName))
				{
					key.DeleteValue(ValueNames.LastUsedConfiguration);
				}
			}

			public static void DeleteWebPrintConfig()
			{
				var manager = new ConnectionRegistryManagerForTest();
				if (manager.registryRoot_Exposed.OpenSubKey(manager.webPrintKeyName_Exposed) != null)
				{
					manager.DeleteFromRegistry(string.Empty);
				}
			}
		}

		public static WebClientUpdateConfiguration GetWebClientUpdateConfigurationForTest()
		{
			return new WebClientUpdateConfiguration(WebClientUpdateConfiguration.UpdateMode.Automatic, true, true, false, 0,
				new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Local), new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Local), WebClientUpdateConfiguration.DaysOfWeek.All,
				false, false, 24);
		}

		public class ConnectionRegistryManagerForLegacyTest : ConnectionRegistryManager
		{
			protected override string EdiKeyName => "SOFTWARE\\CargoWise Legacy tst";
			protected override string WebPrintKeyName => "SOFTWARE\\CargoWise Legacy tst\\" + Constants.RegistryManager.WebPrintKeyName;
			protected override RegistryKey RegistryRoot => Registry.CurrentUser;

			public string ediKeyName_Exposed => EdiKeyName;
			public string webPrintKeyName_Exposed => WebPrintKeyName;
			public RegistryKey registryRoot_Exposed => RegistryRoot;
		}

		const string LastConfigurationName = "CargoWise";
	}
}
