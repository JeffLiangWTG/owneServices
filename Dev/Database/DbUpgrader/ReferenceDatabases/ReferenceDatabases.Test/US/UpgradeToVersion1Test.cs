using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion1Test : USReferenceDbUpgraderVersionTest
	{
		protected override void AssertUpgradeResult()
		{
			AssertEquals("A tariff '2517100015' exists", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from USCTariff where UE_Tariff = '2517100015'"));
			AssertEquals("A tariff '9802009000' exists", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from USCTariff where UE_Tariff = '9802009000'"));
			AssertEquals("A tariff '98030050' exists", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from USCTariff where UE_Tariff = '98030050'"));
			AssertEquals("A tariff '98040005' exists", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from USCTariff where UE_Tariff = '98040005'"));
			AssertEquals("A tariff '98060005' exists", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from USCTariff where UE_Tariff = '98060005'"));
			AssertEquals("A tariff '98070040' exists", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from USCTariff where UE_Tariff = '98070040'"));
			AssertEquals("A tariff '9808001000' exists", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from USCTariff where UE_Tariff = '9808001000'"));
			AssertEquals("A tariff '98090010' exists", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from USCTariff where UE_Tariff = '98090010'"));
			AssertEquals("A tariff '9810006000' exists", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from USCTariff where UE_Tariff = '9810006000'"));
			AssertEquals("A tariff '9817009800' exists", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from USCTariff where UE_Tariff = '9817009800'"));
		}

		protected override int LatestVersionNumber
		{
			get { return 1; }
		}
	}
}
