using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion125_AddA99TariffRuleFor9903Test : USReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber => 125;

		protected override void PrepareTestData(DbConnection conn)
		{
			TestHelper.CreateTables(conn, new ITableScript[] { new USCTariffRule() });
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals("990301", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM dbo.USCTariffRule WHERE U1_RuleCode = 'A99' AND U1_Tariff = '990301' AND U1_PK = 'CD836551-1989-4CC4-8AC4-E131768398E6'"));
			AssertEquals("990381", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM dbo.USCTariffRule WHERE U1_RuleCode = 'A99' AND U1_Tariff = '990381' AND U1_PK = '0D88E7F3-63B5-446D-8E31-50F4DF59200B'"));
			AssertEquals("990389", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM dbo.USCTariffRule WHERE U1_RuleCode = 'A99' AND U1_Tariff = '990389' AND U1_PK = 'BFECB802-731C-45CD-A3E4-9677F4CC3061'"));
		}
	}
}
