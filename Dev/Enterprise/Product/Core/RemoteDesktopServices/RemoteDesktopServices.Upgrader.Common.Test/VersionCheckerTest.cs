using System;
using CargoWise.Loader.Common;
using Microsoft.Win32;
using Moq;
using NUnit.Framework;

namespace CargoWise.RemoteDesktopServices.Upgrader.Common.Test
{
	[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
	sealed class VersionCheckerTest : TestCase
	{
		readonly string encryptedUpgradeCode = Guid.NewGuid().ToString("N");
		readonly string encryptedProductCode = Guid.NewGuid().ToString("N");
		readonly Version localVersion = new Version(4, 14, 30);

		protected override void SetUp()
		{
			base.SetUp();
			using (var registry = Registry.LocalMachine)
			{
				var productCode = VersionChecker.DecryptProductCode(encryptedProductCode);
				var upgradeCodeNode = registry.CreateSubKey(VersionChecker.UpgradeCodePath + encryptedUpgradeCode);
				upgradeCodeNode.SetValue(encryptedProductCode, "");

				var productNode = registry.CreateSubKey(VersionChecker.ProductCodePath + productCode);
				productNode.SetValue("DisplayVersion", localVersion.ToString());
				productNode.Flush();
			}
		}

		public void TestInstallExcludingDependencies_WhenLatestVersionMatchesLocalVersion_ReturnsOK()
		{
			// Arrange
			var latestVersion = new Version(4, 14, 30);
			var services = new Mock<IServiceContainer>();
			services
				.SetupGet(s => s.MessageBox)
				.Returns(Mock.Of<IMessageBoxProxy>());
			var config = new Configuration()
			{
				Services = services.Object,
			};
			var installation = new Installation(config);
			var installationResult = new InstallationResultCollection();
			var versionCheckerForTest = new VersionCheckerForTest(installation, latestVersion.ToString(), encryptedUpgradeCode, TimeSpan.FromSeconds(10), null, null);

			// Act
			var result = versionCheckerForTest.InstallExcludingDependenciesForTest();

			// Assert
			AssertEquals(InstallationResultStatus.OK, result.Status);
		}

		public void TestInstallExcludingDependencies_WhenLatestVersionNotFoundInLocalRegistry_ReturnsOutOfTime()
		{
			// Arrange
			var latestVersion = new Version(4, 14, 10);
			var services = new Mock<IServiceContainer>();
			services
				.SetupGet(s => s.MessageBox)
				.Returns(Mock.Of<IMessageBoxProxy>());
			var config = new Configuration()
			{
				Services = services.Object,
			};
			var installation = new Installation(config);
			var pluginProductName = "CargoWise Services";
			var manualFixLink = "https://www.cargowise.com/";
			var versionCheckerForTest = new VersionCheckerForTest(installation, latestVersion.ToString(), encryptedUpgradeCode, TimeSpan.FromSeconds(10), pluginProductName, manualFixLink);

			// Act
			var result = versionCheckerForTest.InstallExcludingDependenciesForTest();

			// Assert
			AssertEquals(InstallationResultStatus.Error, result.Status);
			AssertEquals($"The {pluginProductName} has timed out installing. It may be blocked or in a stuck state. For further information please refer to: {manualFixLink}", result.Message);
		}

		public void TestDecryptProductCode()
		{
			// Arrange
			var encryptedProductCode = "B48FEC29A38B50B4B9F5310A04467E45";
			var expectedProductCode = "{92CEF84B-B83A-4B05-9B5F-13A04064E754}";

			// Act
			var productCode = VersionChecker.DecryptProductCode(encryptedProductCode);

			// Assert
			AssertEquals(expectedProductCode, productCode);
		}

		protected override void TearDown()
		{
			base.TearDown();
			using (var registry = Registry.LocalMachine)
			{
				var productCode = VersionChecker.DecryptProductCode(encryptedProductCode);
				registry.DeleteSubKey(VersionChecker.UpgradeCodePath + encryptedUpgradeCode);
				registry.DeleteSubKey(VersionChecker.ProductCodePath + productCode);
				registry.Flush();
			}
		}
	}

	public class VersionCheckerForTest : VersionChecker
	{
		public VersionCheckerForTest(Installation installation, string latestVersion, string upgradeCodePath, TimeSpan waitUpgradeTime, string pluginProductName, string manualFixLink)
			: base(installation, latestVersion, upgradeCodePath, waitUpgradeTime, pluginProductName, manualFixLink)
		{
		}

		public InstallationResult InstallExcludingDependenciesForTest()
		{
			return base.InstallExcludingDependencies();
		}
	}
}
