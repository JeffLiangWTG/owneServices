using CargoWise.Loader.Common;
using NUnit.Framework;

namespace Enterprise.Server.Setup.Testing
{
	class ServerInstallationTopLevelItemTest : TestCase
	{
		SetupConfiguration config;

		protected override void SetUp()
		{
			config = new SetupConfiguration();
			config.InstallationSettings.SQLServerMachineName = "theserver";
			config.InstallationSettings.DbName = "thedatabase";
			base.SetUp();
		}

		public void TestDefaultInstanceNameIsEliminatedFromArguments()
		{
			// Arrange
			var testDbChoice = new DatabaseChoice("MSSQLSERVER");
			config.InstallationSettings.SelectedDatabase = testDbChoice;
			var expectedArguments = $"{config.InstallationSettings.SQLServerMachineName} {config.InstallationSettings.DbName}";

			// Act
			var serverInstallationTopLevelItem = new ServerInstallationTopLevelItem(new Installation(config), config.InstallationSettings);

			// Assert
			AssertEquals(expectedArguments, serverInstallationTopLevelItem.Arguments);
		}

		public void TestNonDefaultInstanceNameIsNotEliminatedFromArguments()
		{
			// Arrange
			var testDbChoice = new DatabaseChoice("theinstance");
			config.InstallationSettings.SelectedDatabase = testDbChoice;
			var expectedArguments = @$"{config.InstallationSettings.SQLServerMachineName}\{config.InstallationSettings.SelectedDatabase.InstanceName} {config.InstallationSettings.DbName}";

			// Act
			var serverInstallationTopLevelItem = new ServerInstallationTopLevelItem(new Installation(config), config.InstallationSettings);

			// Assert
			AssertEquals(expectedArguments, serverInstallationTopLevelItem.Arguments);
		}
	}
}
