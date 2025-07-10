using System;
using System.Windows.Forms;
using Microsoft.Win32;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
	class PrintClientFormTest : TestCase
	{
		public void TestStartRunningShouldBeCalledOnceDuringAutoStart()
		{
			SettingsHelper.SaveAutoStart(true);
			using (var form = new PrintClientFormForTest())
			{
				form.Show();

				AssertEquals("StartRunning should be called once", 1, form.RunningTimes);
			}
		}

		public void TestRestartMessage()
		{
			SettingsHelper.SaveAutoStart(true);
			using (var form = new PrintClientFormForTest())
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Restart should not be requested initially", false, form.restartRequested);

				var sent = ApplicationRestartHelper.PostMessage(
					(IntPtr)ApplicationRestartHelper.HWND_BROADCAST,
					ApplicationRestartHelper.WM_WPR_RESTART,
					new IntPtr(ApplicationRestartHelper.wParam),
					new IntPtr(ApplicationRestartHelper.lParam)
				);

				Application.DoEvents();

				AssertEquals("PostMessage should return true when message is sent successfully", true, sent);
				AssertEquals("Restart should be requested after calling RestartApplication", true, form.restartRequested);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			configName = "ConfigTest1";
			var webClientConfig = new WebClientConfiguration("url1", "user1", "pwd1", 1, "machine1", false, "proxy1", 43, "proxyuser1", "proxypwd1", false, false, 5000, false, 0, 0, 100, 100, false, false, 0, 0, 0, false, 4, 60, true, 0, ConnectionRegistryManagerTest.GetWebClientUpdateConfigurationForTest(), false, 100);
			registryManager = new ConnectionRegistryManagerForPrintClientFormTest();
			registryManager.LoadFromRegistry(configName);
			registryManager.SaveRemotePrintingRegistryValues(webClientConfig);
		}

		protected override void TearDown()
		{
			base.TearDown();

			registryManager.DeleteFromRegistry(configName);
			registryManager.DeleteTestRootKey();
		}

		string configName;
		ConnectionRegistryManagerForPrintClientFormTest registryManager;

		class PrintClientFormForTest : PrintClientForm
		{
			public PrintClientFormForTest() : base() { }

			ConnectionRegistryManagerForPrintClientFormTest managerForTest;
			protected override ConnectionRegistryManager RegistryManager => managerForTest ?? (managerForTest = new ConnectionRegistryManagerForPrintClientFormTest());

			RegistryKey autoStartData;
			protected override RegistryKey GetWebPrintAutoStartData()
			{
				if (autoStartData == null)
				{
					autoStartData = RegistryManager.FindKey(Constants.RegistryManager.WebPrintAutoStartData);
					autoStartData.SetValue("Jerry Test", "Jerry Test", RegistryValueKind.String);
				}
				return autoStartData;
			}

			protected override void StartRunning()
			{
				runningTimes++;
			}

			int runningTimes;
			public int RunningTimes => runningTimes;

			public bool restartRequested;
			protected override void RestartApplicationCore()
			{
				restartRequested = true;
			}
		}

		class ConnectionRegistryManagerForPrintClientFormTest : ConnectionRegistryManager
		{
			const string RootKeyForTest = "TestPrintClientForm";

			RegistryKey testRoot;
			protected override RegistryKey RegistryRoot
			{
				get
				{
					if (testRoot == null)
					{
						testRoot = Registry.CurrentUser.OpenSubKey(RootKeyForTest, true) ?? Registry.CurrentUser.CreateSubKey(RootKeyForTest);
					}
					return testRoot;
				}
			}

			public void DeleteTestRootKey()
			{
				Registry.CurrentUser.DeleteSubKeyTree(RootKeyForTest);
			}
		}
	}
}
