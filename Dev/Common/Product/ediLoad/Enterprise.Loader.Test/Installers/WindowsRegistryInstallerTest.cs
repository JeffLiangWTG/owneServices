using CargoWise.ApplicationManager.Common;
using CargoWise.Loader.Client;
using CargoWise.Loader.Common;
using CargoWise.Loader.Common.Testing;
using Enterprise.Upgrades;
using Microsoft.Win32;
using NUnit.Framework;

namespace Enterprise.Loader.Testing
{
	[TestRequiresAdministrativePrivileges("This test modifies the registry.")]
	class WindowsRegistryInstallerTest : TestCase
	{
		MockEnterpriseConfiguration configuration;
		MockWindowsRegistryInstaller mockInstaller;
		MockClientSetupHelper setupHelper;

		protected override void SetUp()
		{
			configuration = new MockEnterpriseConfiguration();
			configuration.ServerName = "myserver";
			configuration.DatabaseName = "mydb";

			setupHelper = new MockClientSetupHelper();
			mockInstaller = new MockWindowsRegistryInstaller(new ClientInstallation(configuration), setupHelper);
			configuration.SetAppManagerClient(new AppManagerForTesting(mockInstaller));

			setupHelper.DeleteWebBrowserEmulationKey();
			setupHelper.CreateWebBrowserEmulationKey();
		}

		protected override void TearDown()
		{
			setupHelper.DeleteWebBrowserEmulationKey();
			base.TearDown();
		}

		public void TestInstallIfInstanceRegistrySubKeyDoesNotExist()
		{
			setupHelper.DeleteWebBrowserEmulationKey();
			Assert(mockInstaller.NeedsToInstall());
		}

		public void TestInstallIfInstanceRegistrySubKeyHasValue()
		{
			setupHelper.DeleteWebBrowserEmulationKey();
			CreateSubKeyValue(0, RegistryView.Registry32);
			CreateSubKeyValue(0, RegistryView.Registry64);

			Assert(!mockInstaller.NeedsToInstall());

			setupHelper.DeleteWebBrowserEmulationKey();
			CreateSubKeyValue(10000, RegistryView.Registry32);
			CreateSubKeyValue(10000, RegistryView.Registry64);

			Assert(mockInstaller.NeedsToInstall());
		}

		public void TestInstallSubKeyValue()
		{
			setupHelper.DeleteWebBrowserEmulationKey();
			Assert(mockInstaller.InstallExcludingDependencies().IsOK);

			AssertSubKeyValue(0, RegistryView.Registry32);
			AssertSubKeyValue(0, RegistryView.Registry64);
		}

		public void TestReplaceSubKeyValue()
		{
			setupHelper.DeleteWebBrowserEmulationKey();
			CreateSubKeyValue(10000, RegistryView.Registry32);
			CreateSubKeyValue(10000, RegistryView.Registry64);

			AssertSubKeyValue(10000, RegistryView.Registry32);
			AssertSubKeyValue(10000, RegistryView.Registry64);

			Assert(mockInstaller.InstallExcludingDependencies().IsOK);

			AssertSubKeyValue(0, RegistryView.Registry32);
			AssertSubKeyValue(0, RegistryView.Registry64);
		}

		void AssertSubKeyValue(int value, RegistryView view)
		{
			using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
			{
				using (var subKey = key.OpenSubKey(setupHelper.WebBrowserEmulationKeyName))
				{
					AssertEquals(value, (int)subKey.GetValue(ExeFileNames.CargoWiseWindowsDesktopExe));
				}
			}
		}

		void CreateSubKeyValue(int value, RegistryView view)
		{
			using (var rootKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
			{
				var subKey = rootKey.CreateSubKey(setupHelper.WebBrowserEmulationKeyName);

				subKey.SetValue(ExeFileNames.CargoWiseWindowsDesktopExe, value, RegistryValueKind.DWord);
			}
		}

		public void TestInstallWithNoUI()
		{
			configuration.UILevel = UILevel.AutomatedWithNoUI;
			Assert("Should run when no UI", mockInstaller.NeedsToInstall());
		}

		#region MockAppManager

		class AppManagerForTesting : MockAppManager
		{
			readonly MockWindowsRegistryInstaller host;

			public AppManagerForTesting(MockWindowsRegistryInstaller host)
			{
				this.host = host;
			}

			public override AppManagerResult Invoke(string assemblyPath, string typeName, object state, MutexRequest request)
			{
				return ((IAppManagerInvocable)host).Invoke(false, state);
			}
		}

		#endregion

		#region MockInstaller

		class MockWindowsRegistryInstaller : WindowsRegistryInstaller
		{
			readonly MockClientSetupHelper setupHelper;

			public MockWindowsRegistryInstaller(ClientInstallation installation, MockClientSetupHelper setupHelper)
				: base(installation)
			{
				this.setupHelper = setupHelper;
			}

			protected override string WebBrowserEmulationKeyName => setupHelper.WebBrowserEmulationKeyName;
			public new InstallationResult InstallExcludingDependencies() => base.InstallExcludingDependencies();
		}

		class MockClientSetupHelper
		{
			public string WebBrowserEmulationKeyName => @"SOFTWARE\WiseTech Global\CargoWise One (Test)";

			public void DeleteWebBrowserEmulationKey()
			{
				var subKey = Registry.LocalMachine.OpenSubKey(WebBrowserEmulationKeyName, true);
				if (subKey != null)
				{
					subKey.Dispose();
					Registry.LocalMachine.DeleteSubKeyTree(WebBrowserEmulationKeyName);
				}
			}

			public void CreateWebBrowserEmulationKey()
			{
				Registry.LocalMachine.CreateSubKey(WebBrowserEmulationKeyName);
			}
		}

		#endregion
	}
}
