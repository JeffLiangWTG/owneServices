using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion83_UpdateUSCZipCodeAndAddPGAGDAFD0TariffsRule : USReferenceDbUpgraderVersionTest
	{
		protected override void PrepareTestData(DbConnection conn)
		{
			TestHelper.CreateTables(conn, new ITableScript[] {
				new USCZipCode(),
				new USCRule(),
				new USCTariffRule(),
				new USCTariffRuleException(),
				new USCRuleSecondaryTariff(),
				new USCRuleSecondaryTariffException(),
				});
			conn.ExecuteNonQuery("insert into USCZipCode values(newid(),'123','567','ST')");
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals("There is no exception", 64, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCZipCode"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM sys.tables tab  INNER JOIN sys.indexes ind ON tab.object_id = ind.object_id WHERE tab.name = 'USCZipCode' AND ind.name = 'NR_IX__USCZipCode'"));

			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRule where U0_Code = 'FD0'"));
			AssertEquals(150, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule WHERE U1_RuleCode =  'FD0'"));
		}

		protected override int LatestVersionNumber
		{
			get { return 83; }
		}
	}
}
