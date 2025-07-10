using System;
using System.Linq;
using CargoWise.BrandManager;
using CargoWise.Loader.Common;
using CargoWise.RemoteDesktopServices.Upgrader.Common;
using Enterprise.RemoteDesktopServices;
using NUnit.Framework;

namespace CargoWise.CitrixServices.Upgrader.Test
{
	sealed class UpgraderStartupDirectorTest : TestCase
	{
		public void TestSetupResourceCanBeLocated()
		{
			// Arrange
			var install = installEnvironmentContext.TopLevelInstallationItem.Dependencies.OfType<UpgradeInstallerAsync>().First();

			// Act
			var resource = install.GetInstallerResourceForTest(typeof(UpgraderStartupDirector).Assembly);

			// Assert
			AssertEquals(resource.Name, ClientCitrixVersion.InstallerExeName);
			AssertNotNull(resource.Stream);
		}

		public void TestConfigurationBrandingType()
		{
			AssertType<CargoWiseOneBranding>(installEnvironmentContext.Branding);
		}

		public void TestProcessNameUsedBySetUpProcessRunningInstanceCheckerIsCorrect()
		{
			// Arrange
			var allInstallItems = installEnvironmentContext.TopLevelInstallationItem.Dependencies;

			var install = allInstallItems.OfType<UpgradeInstallerAsync>().First();
			var processChecker = allInstallItems.OfType<SetUpProcessRunningInstanceChecker>().First();

			// Act
			var resource = install.GetInstallerResourceForTest(typeof(UpgraderStartupDirector).Assembly);
			var processName = processChecker.processName;

			// Assert
			AssertEquals(resource.Name, $"{processName}.exe");
		}

		InstallEnvironmentContext installEnvironmentContext;
		protected override void SetUp()
		{
			base.SetUp();

			installEnvironmentContext = new InstallEnvironmentContext();

			var startupDirector = new UpgraderStartupDirectorForTest(installEnvironmentContext);
			startupDirector.StartApplication(Array.Empty<string>());

			AssertEquals("InitializeInstallationItems should return true", true, installEnvironmentContext.ActualInitializationResult);
		}

		sealed class InstallEnvironmentContext
		{
			public bool ActualInitializationResult { get; set; }
			public IBranding Branding { get; set; }
			public InstallationItem TopLevelInstallationItem { get; set; }
		}

		sealed class UpgraderStartupDirectorForTest : UpgraderStartupDirector
		{
			readonly InstallEnvironmentContext installEnvironmentContext;
			public UpgraderStartupDirectorForTest(InstallEnvironmentContext installEnvironmentContext) : base()
			{
				this.installEnvironmentContext = installEnvironmentContext;
			}

			protected override bool InitializeInstallationItems()
			{
				installEnvironmentContext.ActualInitializationResult = base.InitializeInstallationItems();
				installEnvironmentContext.Branding = BrandingFactory.Instance;
				installEnvironmentContext.TopLevelInstallationItem = TopLevelItem;

				// Fail the install intentionally
				Configuration.ReturnCode = ReturnCode.Failure;
				return false;
			}

			protected override void StartUserInterface()
			{
				throw new NotSupportedException("Failed initialization should not start UI at all");
			}
		}
	}
}
