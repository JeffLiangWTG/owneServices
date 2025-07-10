using System;
using System.IO;
using CargoWise.IO;
using Enterprise.Client.Common;
using Enterprise.Upgrades;
using Microsoft.Win32;
using NUnit.Framework;

namespace Enterprise.Loader.Testing
{
	class EnterpriseConfigurationTest : TestCase
	{
		EnterpriseConfiguration configuration;
		TempDirectory tempDirectory;
		string tempPath;

		protected override void TearDown()
		{
			if (tempDirectory != null)
			{
				tempDirectory.Dispose();
			}
			base.TearDown();
		}

		public void TestConfigurationFileDefaultsWhenSettingIsNotPresent()
		{
			WriteConfigFile(string.Empty);
			AssertDefaults();
		}

		public void TestConfigurationFileDefaultsWhenSettingIsPresentAndValueIsBlank()
		{
			WriteConfigFile(@"[CONFIGURATION]
SERVER=
INSTANCE=
DATABASE=
OTHERPARAMETERS=
MAINTENANCEMESSAGE=
");
			AssertDefaults();
		}

		public void TestConfigurationFileParsedUsingRelaxedSyntax()
		{
			WriteConfigFile(@"SERVER=SomeServer
INSTANCE = SomeInstance 
; Some comment line INSTANCE = this is just a comment
  DATABASE = SomeDatabase
OTHERPARAMETERS =Some other parameters
; The following line has a trailing space but should be parsed as empty
MAINTENANCEMESSAGE= 
APPMANAGEROverride =AppManager directory
ENTErpRISEINSTANCE=Some instance
");
			var configuration = GetNewConfiguration(TempPath);
			AssertTestSettingsExceptMaintenanceMessage(configuration);
			AssertEquals("MaintenanceMessage", string.Empty, configuration.MaintenanceMessage);
		}

		public void TestConfigurationFileParsedUsingStrictLegacySyntax()
		{
			WriteConfigFile(@"[CONFIGURATION]
SERVER=SomeServer
INSTANCE=SomeInstance
DATABASE=SomeDatabase
OTHERPARAMETERS=Some other parameters
MAINTENANCEMESSAGE=A maintenance = message.
APPMANAGEROVERRIDE=AppManager directory
ENTERPRISEINSTANCE=Some instance
");
			var configuration = GetNewConfiguration(TempPath);
			AssertTestSettingsExceptMaintenanceMessage(configuration);
			AssertEquals("MaintenanceMessage", "A maintenance = message.", configuration.MaintenanceMessage);
		}

		public void TestDefaultsWhenFileIsNotPresent()
		{
			AssertDefaults();
		}

		public void TestInstanceName()
		{
			MockEnterpriseConfiguration configuration = new MockEnterpriseConfiguration();
			AssertEquals("CargoWise", configuration.LegacyInstanceName);
			configuration.TargetDirectoryName = "Moo";
			AssertEquals("Moo", configuration.LegacyInstanceName);
			configuration.TargetDirectoryName = string.Empty;
			AssertEquals("ediEnterprise", configuration.LegacyInstanceName);
		}

		public void TestLaunchEnterprise()
		{
			WriteConfigFile(@"[CONFIGURATION]
SERVER=server
INSTANCE=Enterprise
DATABASE=OdysseyTest
OTHERPARAMETERS=-SomeFlag -OtherFlag ""-Foo:Bar bar bar""
MAINTENANCEMESSAGE=
LOCALDIROVERRIDE=Some local directory
");
			var config = GetNewConfiguration(TempPath);
			AssertEquals(@"server\Enterprise OdysseyTest -IAmDoingTheWrongThingByRunningEnterpriseWithoutLoader -ShowLogin -SDir:""" + TempPath + @""" -SomeFlag -OtherFlag ""-Foo:Bar bar bar""", config.ProgramArguments);
			AssertEquals(@"server\Enterprise", config.ServerName);
			AssertEquals("OdysseyTest", config.DatabaseName);

			WriteConfigFile(@"[CONFIGURATION]
SERVER=server
INSTANCE=
DATABASE=OdysseyTest
OTHERPARAMETERS=-SomeFlag -OtherFlag ""-Foo:Bar bar bar""
MAINTENANCEMESSAGE=
LOCALDIROVERRIDE=Some local directory
");
			config = GetNewConfiguration(TempPath);
			AssertEquals(@"server OdysseyTest -IAmDoingTheWrongThingByRunningEnterpriseWithoutLoader -ShowLogin -SDir:""" + TempPath + @""" -SomeFlag -OtherFlag ""-Foo:Bar bar bar""", config.ProgramArguments);
			AssertEquals("server", config.ServerName);
			AssertEquals("OdysseyTest", config.DatabaseName);
		}

		[TestRequiresAdministrativePrivileges("Cleanup registry")]
		public void TestProgramFileName()
		{
			var configuration = new EnterpriseConfiguration();
			configuration.ServerName = @"InstanceNameForTest\ServerNameForTest";
			configuration.DatabaseName = "DatabasenameForTest";
			var rootKey = @"SOFTWARE\WiseTech Global\CargoWise One\InstanceNameForTest-ServerNameForTest";

			var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			var testPathCargoWiseWindowsDesktopExe = Path.Combine(tempDir, ExeFileNames.CargoWiseWindowsDesktopExe);

			Directory.CreateDirectory(tempDir);
			try
			{
				using (File.Create(testPathCargoWiseWindowsDesktopExe))
				{
					// Test with path that shouldn't find the newer exe.
#pragma warning disable CS0618 // Type or member is obsolete - Testing backwards compatibility
					AssertEquals(ExeFileNames.CargoWiseOneAnyCpuExe, configuration.ProgramFileName);
#pragma warning restore CS0618 // Type or member is obsolete - Testing backwards compatibility

					// Test with path that should find the newer exe.
					configuration.BaseTargetPath = tempDir;
					AssertEquals(ExeFileNames.CargoWiseWindowsDesktopExe, configuration.ProgramFileName);
				}
			}
			finally
			{
				using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
				{
					baseKey.DeleteSubKeyTree(rootKey, false);
				}
				Directory.Delete(tempDir, recursive: true);
			}
		}

		public void TestLaunchEnterpriseWithoutConfigFile()
		{
			var config = GetNewConfiguration(TempPath, "CW1Server", "CW1Database");
			AssertEquals(@"CW1Server CW1Database -IAmDoingTheWrongThingByRunningEnterpriseWithoutLoader -ShowLogin", config.ProgramArguments);
		}

		public void TestPathOnCommandLine()
		{
			Configuration.Initialize(new string[] { @"-path=""X:\Special directory\special""" });
			AssertEquals(@"X:\Special directory\special", Configuration.TargetPath);
		}

		public void TestUnrecognisedCommandLineParametersArePassedToEnterprise()
		{
			WriteConfigFile("OtherParameters=-Foo -Bar:\"Baz baz\"");
			var configuration = GetNewConfiguration(TempPath, "Pass this on", "This too", "ThisToo", "AndThis", "-And:also this");
			AssertEquals(DefautProgramParameters + " -SDir:\"" + TempPath + "\" -Foo -Bar:\"Baz baz\" \"Pass this on\" \"This too\" ThisToo AndThis \"-And:also this\"", configuration.ProgramArguments);
		}

		public void TestSelectCurrentVersion()
		{
			Configuration.Initialize(new string[] { @"-SelectCurrentVersion" });
			AssertEquals("SelectCurrentVersion", true, Configuration.SelectCurrentVersion);
		}

		public void TestUpdateUsageLog()
		{
			Configuration.Initialize(new string[] { @"-UpdateUsageLog" });
			AssertEquals("UpdateUsageLog", true, Configuration.UpdateUsageLog);
		}

		public void TestRemoveOldVersions()
		{
			Configuration.Initialize(new string[] { @"-RemoveOldVersions" });
			AssertEquals("RemoveOldVersions", true, Configuration.RemoveOldVersions);
		}

		public void TestForceRemoveOldVersionsInFrontOfInstallationByDefaultIfNotSpecified()
		{
			Configuration.Initialize(new string[] { "-RemoveOldVersions" });
			AssertEquals(false, Configuration.ForceRemoveOldVersionsInFrontOfInstallation);
		}

		public void TestForceRemoveOldVersionsInFrontOfInstallationIfSpecified()
		{
			Configuration.Initialize(new string[] { "-ForceRemoveOldVersionsInFrontOfInstallation" });
			AssertEquals(true, Configuration.ForceRemoveOldVersionsInFrontOfInstallation);
		}

		public void TestCommandArgument()
		{
			WriteConfigFile(@"[CONFIGURATION]
SERVER=MyServer
DATABASE=OdysseyTest
");
			var config = GetNewConfiguration(TempPath, "-Cmd", "RunMe.exe");
			AssertEquals("RunMe.exe", config.ProgramFileName);
			AssertEquals("MyServer OdysseyTest", config.ProgramArguments);
		}

		public void TestCommandArgumentWithOption()
		{
			Configuration.Initialize(new string[] { @"-Cmd", "RunMe.exe", "withoption" });
			AssertEquals("RunMe.exe", Configuration.ProgramFileName);
			AssertEquals("withoption", Configuration.ProgramArguments);
		}

		public void TestStartupPathOverride()
		{
			Configuration.Initialize(new string[] { @"-StartupPath:\\somewhere\out\there" });
			AssertEquals(@"\\somewhere\out\there", Configuration.StartupPath);
			AssertEquals(DefautProgramParameters, Configuration.ProgramArguments);
		}

		public void TestConfigFileFromStartupPathOverrideIsUsed()
		{
			WriteConfigFile(@"[CONFIGURATION]
SERVER=MyServer
DATABASE=OdysseyTest
");
			var config = GetNewConfiguration(@"C:\Some\Other\Path");
			config.Initialize(new string[] { "-StartupPath:" + TempPath });
			AssertEquals("MyServer", config.ServerName);
			AssertEquals("OdysseyTest", config.DatabaseName);
		}

		public void TestIntanceArgument()
		{
			Configuration.Initialize(new string[] { "-Instance:theone" });
			AssertEquals("theone", configuration.InstanceName);
		}

		public void TestInstanceArgumentNotPassedToCargoWiseOne()
		{
			Configuration.Initialize(new string[] { "-Instance:theone" });
			Configuration.ServerName = "theserver";
			Configuration.DatabaseName = "thedatabase";
			Configuration.TargetVersion = new Version(15, 12, 31, 1);
			Assert("Expected no -Instance:theone actual arguments: " + Configuration.ProgramArguments, Configuration.ProgramArguments.IndexOf("-Instance:theone") == -1);
			Configuration.TargetVersion = new Version(15, 10, 11, 527);
			Assert("Expected no -Instance: argument, actual arguments: " + Configuration.ProgramArguments, Configuration.ProgramArguments.IndexOf("-Instance:theone") == -1);
		}

		public void TestStartBrandingArgumentNotPassedToCargoWiseOne()
		{
			Configuration.Initialize(new string[] { "-StartBranding:CWNext" });
			Configuration.ServerName = "theserver";
			Configuration.DatabaseName = "thedatabase";
			Configuration.TargetVersion = new Version(15, 12, 31, 1);
			Assert("Expected no -StartBranding:CWNext actual arguments: " + Configuration.ProgramArguments, Configuration.ProgramArguments.IndexOf("-StartBranding:CWNext") == -1);
		}

		EnterpriseConfiguration Configuration
		{
			get { return configuration ?? (configuration = new EnterpriseConfiguration()); }
		}

		string DefautProgramParameters
		{
			get { return "-IAmDoingTheWrongThingByRunningEnterpriseWithoutLoader -ShowLogin"; }
		}

		string TempPath
		{
			get
			{
				if (tempPath == null)
				{
					tempDirectory = new TempDirectory();
					tempPath = Path.Combine(tempDirectory, "Eagle Datamation International\\ediEnterprise"); // Stick some spaces in the path to make sure the code can handle long file names.
					Directory.CreateDirectory(tempPath);
				}
				return tempPath;
			}
		}

		void AssertDefaults()
		{
			Configuration.Initialize(Array.Empty<string>());
			AssertEquals("InstanceName", "CargoWise", Configuration.LegacyInstanceName);
			AssertEquals("ServerName", string.Empty, Configuration.ServerName);
			AssertEquals("ServerName", string.Empty, Configuration.DatabaseName);
			AssertEquals("MaintenanceMessage", string.Empty, Configuration.MaintenanceMessage);
			AssertEquals("TargetDirectoryName", "CargoWise", Configuration.TargetDirectoryName);
		}

		void AssertTestSettingsExceptMaintenanceMessage(EnterpriseConfiguration configuration)
		{
			Configuration.Initialize(Array.Empty<string>());
			AssertEquals("AppManagerDirectoryName", "AppManager directory", configuration.AppManagerDirectoryName);
			AssertEquals("InstanceName", "Some instance", configuration.LegacyInstanceName);
			AssertEquals("ProgramParameters", "SomeServer\\SomeInstance SomeDatabase -IAmDoingTheWrongThingByRunningEnterpriseWithoutLoader -ShowLogin -SDir:\"" + TempPath + "\" Some other parameters", configuration.ProgramArguments);
			AssertEquals("TargetDirectoryName", "CargoWise", configuration.TargetDirectoryName);
		}

		static EnterpriseConfiguration GetNewConfiguration(string startupPathOverride, params string[] args)
		{
			EnterpriseConfiguration result = new EnterpriseConfiguration();
			result.StartupPathOverride = startupPathOverride;
			result.Initialize(args);
			return result;
		}

		void WriteConfigFile(string fileContents)
		{
			File.WriteAllText(Path.Combine(TempPath, ConfigFile.FileName), fileContents);
		}
	}
}
