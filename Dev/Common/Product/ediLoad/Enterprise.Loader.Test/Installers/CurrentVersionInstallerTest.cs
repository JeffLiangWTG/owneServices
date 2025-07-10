using System;
using CargoWise.Loader.Client;
using CargoWise.Loader.Common;
using Enterprise.Upgrades;
using Moq;
using NUnit.Framework;

namespace Enterprise.Loader.Testing.Installers
{
	class CurrentVersionInstallerTest : TestCase
	{
		public void TestNoDialogsInNoUiMode()
		{
			// Arrange
			config.UILevel = UILevel.AutomatedWithNoUI;
			var currentVersionInstaller = new CurrentVersionInstallerForTest(new Installation(config), null);

			// Act
			var result = currentVersionInstaller.InstallExcludingDependencies();

			// Assert
			AssertEquals(InstallationResultStatus.Error, result.Status);
			AssertEquals("No current software version configured.", result.Message);
		}

		public void TestSkipUpgradePackageWhenCurrentVerisonIsEqualToCachedAndPackageIsValid()
		{
			// Arrange
			config.TargetVersion = new Version(18, 1, 1, 1);
			config.UILevel = UILevel.AutomatedWithNoUI;
			config.MockUgradeManager = new MockUpgradeManager() { CurrentVersionForTest = new UpgradeInfo(Guid.NewGuid(), config.TargetVersion) };

			var versionInfoInitializer = new VersionInfoInitializer(new Installation(config), null);
			versionInfoInitializer.Install(new InstallationResultCollection());

			var currentVersionInstaller = new CurrentVersionInstallerForTest(new Installation(config), null);
			currentVersionInstaller.ValidateInstallationShouldSuccessForTest = true;

			// Act
			var result = currentVersionInstaller.InstallExcludingDependencies();

			// Assert
			AssertEquals(InstallationResultStatus.OK, result.Status);
			AssertEquals(false, ((MockUpgradeManager)config.MockUgradeManager).InstallUpgradePackageCalledForTest);
		}

		public void TestUpgradePackageWhenCurrentVerisonIsDifferentWithCached()
		{
			// Arrange
			config.TargetVersion = new Version(18, 1, 1, 1);
			config.UILevel = UILevel.AutomatedWithNoUI;
			config.MockUgradeManager = new MockUpgradeManager() { CurrentVersionForTest = new UpgradeInfo(Guid.NewGuid(), new Version(config.TargetVersion.Major, config.TargetVersion.Minor + 1, config.TargetVersion.Build, config.TargetVersion.Revision)) };

			var versionInfoInitializer = new VersionInfoInitializer(new Installation(config), null);
			versionInfoInitializer.Install(new InstallationResultCollection());

			var currentVersionInstaller = new CurrentVersionInstallerForTest(new Installation(config), null);

			// Act
			currentVersionInstaller.InstallExcludingDependencies();

			// Assert
			AssertEquals(true, ((MockUpgradeManager)config.MockUgradeManager).InstallUpgradePackageCalledForTest);
		}

		public void TestUpgradePackageWhenCurrentVerisonIsEqualToCachedButPackageIsInvalid()
		{
			// Arrange
			config.TargetVersion = new Version(18, 1, 1, 1);
			config.UILevel = UILevel.AutomatedWithNoUI;
			config.MockUgradeManager = new MockUpgradeManager() { CurrentVersionForTest = new UpgradeInfo(Guid.NewGuid(), config.TargetVersion) };

			var versionInfoInitializer = new VersionInfoInitializer(new Installation(config), null);
			versionInfoInitializer.Install(new InstallationResultCollection());

			var currentVersionInstaller = new CurrentVersionInstallerForTest(new Installation(config), null);

			AssertExceptionThrown(typeof(InvalidPackageException), () => UpgradeManager.ValidateInstallation(config.TargetVersion, config.CurrentPackage));

			// Act
			currentVersionInstaller.InstallExcludingDependencies();

			// Assert
			AssertEquals(true, ((MockUpgradeManager)config.MockUgradeManager).InstallUpgradePackageCalledForTest);
		}

		public void TestNoUpgradePackageError()
		{
			// Arrange
			config.UILevel = UILevel.Normal;
			var upgradeManagerMock = new MockUpgradeManager();
			config.MockUgradeManager = upgradeManagerMock;
			upgradeManagerMock.RunnablePackages = new UpgradeInfoExtendedCollection()
			{
				//Empty
			};

			var currentVersionInstaller = new CurrentVersionInstallerForTest(new Installation(config), null);

			// Act
			var result = currentVersionInstaller.InstallExcludingDependencies();

			// Assert
			AssertEquals(InstallationResultStatus.Error, result.Status);
			AssertEquals("No compatible software version is available to install.", result.Message);
		}

		public void TestShouldLogFullMessageWhenNonCriticalExceptionWasCaught()
		{
			// Arrange
			config.UILevel = UILevel.Normal;
			var mockUpgradeManager = new Mock<UpgradeManager>("NoServer", "NoDatabase");
			mockUpgradeManager.Setup(m => m.QueryRunnablePackages()).Throws(new Exception("Outer Exception", new Exception("Inner Exception")));
			config.MockUgradeManager = mockUpgradeManager.Object;
			var currentVersionInstaller = new CurrentVersionInstallerForTest(new Installation(config), null);

			// Act
			var result = currentVersionInstaller.InstallExcludingDependencies();

			// Assert
			AssertEquals(InstallationResultStatus.Error, result.Status);
			AssertContains("Outer Exception", result.Message);
			AssertContains("Inner Exception", result.Message);
		}

		protected override void SetUp()
		{
			config = new MockEnterpriseConfiguration();
			base.SetUp();
		}

		MockEnterpriseConfiguration config;

		class CurrentVersionInstallerForTest : CurrentVersionInstaller
		{
			public CurrentVersionInstallerForTest(Installation installation, InstallationProgramFromVersionedFile enterpriseLaunch)
				: base(installation, enterpriseLaunch)
			{
			}

			public bool ValidateInstallationShouldSuccessForTest { get; set; }

			public new InstallationResult InstallExcludingDependencies()
			{
				return base.InstallExcludingDependencies();
			}

			protected override void ValidateInstallation(Version targetVersion, string installationPath)
			{
				try
				{
					base.ValidateInstallation(targetVersion, installationPath);
				}
				catch (Exception)
				{
					if (!ValidateInstallationShouldSuccessForTest)
					{
						throw;
					}
				}
			}
		}
	}
}
