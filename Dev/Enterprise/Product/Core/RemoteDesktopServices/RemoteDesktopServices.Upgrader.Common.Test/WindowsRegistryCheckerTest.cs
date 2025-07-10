using CargoWise.Loader.Common;
using Microsoft.Win32;
using NUnit.Framework;

namespace CargoWise.RemoteDesktopServices.Upgrader.Common.Test
{
	sealed class WindowsRegistryCheckerTest : TestCase
	{
		[TestRequiresAdministrativePrivileges("Windows registry access")]
		public void TestInstall()
		{
			TestInstallInRegistry(Registry.CurrentUser);

			// Tests LocalMachine with CurrentUser Sub-key exists but value not existing
			SetRegistryEntry(SubRegKey1, null, Registry.CurrentUser);
			SetRegistryEntry(SubRegKey2, null, Registry.CurrentUser);
			TestInstallInRegistry(Registry.LocalMachine);

			// Tests LocalMachine with CurrentUser Sub-key not existing
			DeleteRegistryKey(SubRegKey1, Registry.CurrentUser);
			DeleteRegistryKey(SubRegKey2, Registry.CurrentUser);
			TestInstallInRegistry(Registry.LocalMachine);

			void TestInstallInRegistry(RegistryKey registry)
			{
				const string ErrorMessage = "[ProductName] Auto Update is turned off. Users may not be able to use all features. If an updated feature is needed, please contact your system administrator to manually install the new client.";

				Test(registry, null, "false", InstallationResultStatus.OK, string.Empty);
				Test(registry, null, "true", InstallationResultStatus.Error, ErrorMessage);

				Test(registry, "false", null, InstallationResultStatus.OK, string.Empty);
				Test(registry, "false", "false", InstallationResultStatus.OK, string.Empty);
				Test(registry, "false", "true", InstallationResultStatus.Error, ErrorMessage);

				Test(registry, "true", null, InstallationResultStatus.Error, ErrorMessage);
				Test(registry, "true", "false", InstallationResultStatus.Error, ErrorMessage);
				Test(registry, "true", "true", InstallationResultStatus.Error, ErrorMessage);

				if (registry == Registry.LocalMachine)
				{
					Test(registry, null, null, InstallationResultStatus.OK, string.Empty);
				}
			}

			void Test(RegistryKey registry, string disableValueForRegKey1, string disableValueForRegKey2, InstallationResultStatus expectedStatus, string expectedMessage)
			{
				// Arrange
				SetRegistryEntry(SubRegKey1, disableValueForRegKey1, registry);
				SetRegistryEntry(SubRegKey2, disableValueForRegKey2, registry);

				var results = new InstallationResultCollection();

				var installation = new Installation(new Configuration());
				var checker = new WindowsRegistryChecker(installation, [SubRegKey1, SubRegKey2], "[ProductName]");

				// Act
				checker.Install(results);

				// Assert
				AssertEquals(1, results.Count);
				AssertEquals(expectedStatus, results[0].Status);
				AssertEquals(expectedMessage, results[0].Message);
			}
		}

		public void TestInstallSucceedsWhenRegKeyIsMissing()
		{
			// Arrange
			DeleteRegistryKey(SubRegKey1, Registry.LocalMachine);
			DeleteRegistryKey(SubRegKey1, Registry.CurrentUser);
			DeleteRegistryKey(SubRegKey2, Registry.LocalMachine);
			DeleteRegistryKey(SubRegKey2, Registry.CurrentUser);

			var results = new InstallationResultCollection();

			var installation = new Installation(new Configuration());
			var checker = new WindowsRegistryChecker(installation, [SubRegKey1, SubRegKey2], "[ProductName]");

			// Act
			checker.Install(results);

			// Assert
			AssertEquals(1, results.Count);
			AssertEquals(InstallationResultStatus.OK, results[0].Status);
		}

		const string BaseKeyPath = @"SOFTWARE\WiseTech Global";
		const string SubRegKey1 = nameof(SubRegKey1);
		const string SubRegKey2 = nameof(SubRegKey2);

		protected override void TearDown()
		{
			DeleteRegistryKey(SubRegKey1, Registry.LocalMachine);
			DeleteRegistryKey(SubRegKey1, Registry.CurrentUser);
			DeleteRegistryKey(SubRegKey2, Registry.LocalMachine);
			DeleteRegistryKey(SubRegKey2, Registry.CurrentUser);

			base.TearDown();
		}

		static void SetRegistryEntry(string regKey, string disableValue, RegistryKey registry)
		{
			var regPath = $@"{BaseKeyPath}\{regKey}";
			using (var key = registry.OpenSubKey(regPath, true) ?? registry.CreateSubKey(regPath))
			{
				if (disableValue == null)
				{
					key.DeleteValue("DisableAutoUpgrade", false);
				}
				else
				{
					key.SetValue("DisableAutoUpgrade", disableValue, RegistryValueKind.String);
				}
			}
		}

		static void DeleteRegistryKey(string regKey, RegistryKey registry)
		{
			var baseKey = registry.OpenSubKey(BaseKeyPath, true);
			baseKey?.DeleteSubKeyTree(regKey, false);
		}
	}
}
