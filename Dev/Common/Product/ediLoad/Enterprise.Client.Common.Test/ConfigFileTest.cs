using System.IO;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.Client.Common.Testing
{
	class ConfigFileTest : TestCase
	{
		TempDirectory tempDirectory;

		protected override void SetUp()
		{
			base.SetUp();
			tempDirectory = new TempDirectory();
		}

		protected override void TearDown()
		{
			tempDirectory.Dispose();
			base.TearDown();
		}

		public void TestParseUsingRelaxedSyntax()
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
			AssertConfigFile(string.Empty);
		}

		public void TestParseUsingStrictLegacySyntax()
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
			AssertConfigFile("A maintenance = message.");
		}

		public void TestFileDoesNotExist()
		{
			AssertEquals(false, new ConfigFile(tempDirectory).Exists);
		}

		void AssertConfigFile(string maintenanceMessage)
		{
			ConfigFile configFile = new ConfigFile(tempDirectory);
			AssertEquals("AppManagerDirectoryOverride", "AppManager directory", configFile.AppManagerDirectoryOverride);
			AssertEquals("DbInstance", "SomeInstance", configFile.DbInstance);
			AssertEquals("DbName", "SomeDatabase", configFile.DbName);
			AssertEquals("DbServer", "SomeServer", configFile.DbServer);
			AssertEquals("EnterpriseInstance", "Some instance", configFile.EnterpriseInstance);
			AssertEquals("MaintenanceMessage", maintenanceMessage, configFile.MaintenanceMessage);
			AssertEquals("OtherParameters", "Some other parameters", configFile.OtherParameters);
		}

		void WriteConfigFile(string contents)
		{
			File.WriteAllText(Path.Combine(tempDirectory, ConfigFile.FileName), contents);
		}
	}
}