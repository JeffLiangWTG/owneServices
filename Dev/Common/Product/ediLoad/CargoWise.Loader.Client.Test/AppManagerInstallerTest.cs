using System;
using System.IO;
using System.Reflection;
using CargoWise.ApplicationManager.Common;
using CargoWise.Loader.Common;
using CargoWise.Loader.Common.Testing;
using Moq;
using NUnit.Framework;

namespace CargoWise.Loader.Client.Testing
{
	class AppManagerInstallerTest : PrerequisiteInstallerTest
	{
		protected override InstallationItem GetItemToTest(Installation installation)
		{
			return new AppManagerInstaller(installation);
		}

		[TestRequiresAdministrativePrivileges("Registry write")]
		public void TestNeedsToInstall()
		{
			var backupValue = Configuration.ProgramFilesOverrideRegistry;
			RegistryInstallationLocation.SetRegistryInstallationLocationOverride(null);
			try
			{
				var mocker = new MockRepository(MockBehavior.Default);
				var services = new MoqMockServiceContainer(mocker);
				Installation installation = new Installation(new MockConfiguration(TemplateCDPath));
				installation.Configuration.Services = services;

				services.RegistryForTest.Setup(m => m.GetValue(AppManagerInstaller.RegistryKeyName, AppManagerInstaller.RegistryValueName, null)).Returns(null);
				SetupMock(services);
				AssertEquals("NeedsToInstall", true, GetItemToTest(installation).NeedsToInstall());
				mocker.VerifyAll();

				services.RegistryForTest.Setup(m => m.GetValue(AppManagerInstaller.RegistryKeyName, AppManagerInstaller.RegistryValueName, null)).Returns("1.0.1");
				AssertEquals("NeedsToInstall", true, GetItemToTest(installation).NeedsToInstall());
				mocker.VerifyAll();

				services.RegistryForTest.Setup(m => m.GetValue(AppManagerInstaller.RegistryKeyName, AppManagerInstaller.RegistryValueName, null)).Returns("1.0.2");
				AssertEquals("NeedsToInstall", true, GetItemToTest(installation).NeedsToInstall());
				mocker.VerifyAll();

				services.RegistryForTest.Setup(m => m.GetValue(AppManagerInstaller.RegistryKeyName, AppManagerInstaller.RegistryValueName, null)).Returns("1.0.3");
				AssertEquals("NeedsToInstall", true, GetItemToTest(installation).NeedsToInstall());
				mocker.VerifyAll();

				services.RegistryForTest.Setup(m => m.GetValue(AppManagerInstaller.RegistryKeyName, AppManagerInstaller.RegistryValueName, null)).Returns("1.0.4");
				AssertEquals("NeedsToInstall", true, GetItemToTest(installation).NeedsToInstall());
				mocker.VerifyAll();

				services.RegistryForTest.Setup(m => m.GetValue(AppManagerInstaller.RegistryKeyName, AppManagerInstaller.RegistryValueName, null)).Returns("2.0.0");
				AssertEquals("NeedsToInstall", true, GetItemToTest(installation).NeedsToInstall());
				mocker.VerifyAll();

				services.RegistryForTest.Setup(m => m.GetValue(AppManagerInstaller.RegistryKeyName, AppManagerInstaller.RegistryValueName, null)).Returns("2.0.1");
				AssertEquals("NeedsToInstall", true, GetItemToTest(installation).NeedsToInstall());
				mocker.VerifyAll();

				services.RegistryForTest.Setup(m => m.GetValue(AppManagerInstaller.RegistryKeyName, AppManagerInstaller.RegistryValueName, null)).Returns("2.0.2");
				AssertEquals("NeedsToInstall", true, GetItemToTest(installation).NeedsToInstall());
				mocker.VerifyAll();

				services.RegistryForTest.Setup(m => m.GetValue(AppManagerInstaller.RegistryKeyName, AppManagerInstaller.RegistryValueName, null)).Returns("2.0.3");
				AssertEquals("NeedsToInstall", true, GetItemToTest(installation).NeedsToInstall());
				mocker.VerifyAll();

				services.RegistryForTest.Setup(m => m.GetValue(AppManagerInstaller.RegistryKeyName, AppManagerInstaller.RegistryValueName, null)).Returns("2.0.4");
				AssertEquals("NeedsToInstall", true, GetItemToTest(installation).NeedsToInstall());
				mocker.VerifyAll();

				services.RegistryForTest.Setup(m => m.GetValue(AppManagerInstaller.RegistryKeyName, AppManagerInstaller.RegistryValueName, null)).Returns("2.0.12");
				AssertEquals("NeedsToInstall", true, GetItemToTest(installation).NeedsToInstall());
				mocker.VerifyAll();

				services.RegistryForTest.Setup(m => m.GetValue(AppManagerInstaller.RegistryKeyName, AppManagerInstaller.RegistryValueName, null)).Returns("2.0.13");
				AssertEquals("NeedsToInstall", true, GetItemToTest(installation).NeedsToInstall());
				mocker.VerifyAll();

				services.RegistryForTest.Setup(m => m.GetValue(AppManagerInstaller.RegistryKeyName, AppManagerInstaller.RegistryValueName, null)).Returns("2.1.0");
				AssertEquals("NeedsToInstall", true, GetItemToTest(installation).NeedsToInstall());
				mocker.VerifyAll();

				services.RegistryForTest.Setup(m => m.GetValue(AppManagerInstaller.RegistryKeyName, AppManagerInstaller.RegistryValueName, null)).Returns("2.1.1");
				AssertEquals("NeedsToInstall", true, GetItemToTest(installation).NeedsToInstall());
				mocker.VerifyAll();

				services.RegistryForTest.Setup(m => m.GetValue(AppManagerInstaller.RegistryKeyName, AppManagerInstaller.RegistryValueName, null)).Returns("2.2.0");
				AssertEquals("NeedsToInstall", true, GetItemToTest(installation).NeedsToInstall());
				mocker.VerifyAll();

				services.RegistryForTest.Setup(m => m.GetValue(AppManagerInstaller.RegistryKeyName, AppManagerInstaller.RegistryValueName, null)).Returns("2.3.0");
				AssertEquals("NeedsToInstall", false, GetItemToTest(installation).NeedsToInstall());
				mocker.VerifyAll();

				services.EnvironmentForTest.Verify(m => m.GetEnvironmentVariable("Enterprise_Program_Files"), Times.Exactly(2));
			}
			finally
			{
				RegistryInstallationLocation.SetRegistryInstallationLocationOverride(backupValue);
			}
		}

		public override void TestRequiredFilesExistInServerInstall()
		{
			WindowsInstallerProgram installProgram = null;
			var mocker = new MockRepository(MockBehavior.Default);
			var services = new MoqMockServiceContainer(mocker);
			var installation = new Installation(new MockConfiguration(TemplateCDPath));
			installation.Configuration.Services = services;
			services.RegistryForTest.Setup(m => m.GetValue(AppManagerInstaller.RegistryKeyName, AppManagerInstaller.RegistryValueName, null)).Returns(null);
			SetupMock(services);

			foreach (InstallationItem item in GetItemToTest(installation).GetDepthFirstEnumerable())
			{
				installProgram = item as WindowsInstallerProgram;
				if (installProgram != null)
				{
					break;
				}
			}
			AssertNotNull(installProgram);
			string fileName = Path.GetFileName(installProgram.FullPathOfProgramToRun);
			Assert(fileName, File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) + "\\", fileName)));
			services.EnvironmentForTest.Verify(m => m.GetEnvironmentVariable("Enterprise_Program_Files"), Times.Exactly(2));
		}

		public override void TestDependsOnTerminalServerInstallMode()
		{
			var mocker = new MockRepository(MockBehavior.Default);
			var services = new MoqMockServiceContainer(mocker);
			var installation = new Installation(new MockConfiguration(TemplateCDPath));
			installation.Configuration.Services = services;
			services.RegistryForTest.Setup(m => m.GetValue(AppManagerInstaller.RegistryKeyName, AppManagerInstaller.RegistryValueName, null)).Returns(null);
			SetupMock(services);
			TerminalServerInstallModeTest.AssertDependsOnInstallMode(GetItemToTest(installation));
			services.EnvironmentForTest.Verify(m => m.GetEnvironmentVariable("Enterprise_Program_Files"), Times.Exactly(2));
		}

		void SetupMock(MoqMockServiceContainer services)
		{
			services.FileForTest.Setup(m => m.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "CHANGE.EXE"))).Returns(true);
			services.EnvironmentForTest.Setup(m => m.GetEnvironmentVariable("Enterprise_Program_Files")).Returns(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
		public void TestVersionsMatch()
		{
			string msiFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CargoWiseAppManagerSetup.msi");
			var type = Type.GetTypeFromProgID("WindowsInstaller.Installer");
			dynamic installer = Activator.CreateInstance(type);
			dynamic installerDatabase = installer.OpenDatabase(msiFile, 0);
			dynamic view = installerDatabase.OpenView("SELECT `Value` FROM `Property` WHERE `Property`='ProductVersion'");
			dynamic record = null;
			view.Execute(record);
			record = view.Fetch();
			var actualMsiVersion = new Version(record.StringData(1).ToString());

			AssertEquals("AppManagerInstaller.CurrentVersion", actualMsiVersion, AppManagerInstaller.CurrentVersion);

			var details = (IAppManagerUpgradeDetailsProvider)Activator.CreateInstance(Assembly.Load("CargoWise.AppManagerUpgrade").GetType("CargoWise.AppManagerUpgrade.AppManagerUpgradeDetailsProvider"));
			AssertEquals("CargoWise.AppManagerUpgrade.AppManagerUpgradeDetailsProvider", actualMsiVersion, details.GetDetails(null).Version);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
		public void TestAppManagerPackageGUIDRemainsConstant()
		{
			string msiFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CargoWiseAppManagerSetup.msi");
			var type = Type.GetTypeFromProgID("WindowsInstaller.Installer");
			dynamic installer = Activator.CreateInstance(type);
			dynamic installerDatabase = installer.OpenDatabase(msiFile, 0);
			dynamic view = installerDatabase.OpenView("SELECT `Directory` FROM `Directory` WHERE `Directory_Parent`='INSTALLLOCATION'");
			dynamic record = null;
			view.Execute(record);
			record = view.Fetch();
			var packageGuid = ((string)record.StringData(1).ToString()).Split('.')[1];

			AssertEquals("If package guid is intentionally changed, the constant must be updated too", packageGuid, AppManagerInstaller.MSIPackageGuid);
		}
	}
}
