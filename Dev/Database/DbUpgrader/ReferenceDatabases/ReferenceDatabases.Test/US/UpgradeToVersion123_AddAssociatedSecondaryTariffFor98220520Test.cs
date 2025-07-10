using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion123_AddAssociatedSecondaryTariffFor98220520Test : USReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber => 123;

		protected override void PrepareTestData(DbConnection conn)
		{
			TestHelper.CreateTables(conn, new ITableScript[] {
				new USCTariffRule(),
				new USCRuleSecondaryTariff()
				});

			conn.ExecuteNonQuery("INSERT INTO USCTariffRule(U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom) VALUES ('F2E4CD86-C0E9-4A6B-96D8-A0640C618AC3', 'STN', '98220520', '2000-01-01')");
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM dbo.USCRuleSecondaryTariff WHERE U3_TariffFrom = '17011450' AND U3_U1 = 'F2E4CD86-C0E9-4A6B-96D8-A0640C618AC3'"));
		}
	}
}
