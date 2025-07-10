using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion63_USCCountry_AddUcCodeWhenUserHaveColumnAlready : USReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber
		{
			get { return 63; }
		}

		protected override void PrepareTestData(DbConnection conn)
		{
			base.PrepareTestData(conn);

			conn.ExecuteNonQuery(DbSchemaChange.GetAddColumnIfNotExistsScript("USCCountry", "UC_Code", "char(2) NOT NULL DEFAULT('')"));
			conn.ExecuteNonQuery(DbSchemaChange.GetCreateIndexIfNotExistsScript("USCCountry", "UC_Code", "CREATE UNIQUE INDEX UC_Code ON USCCountry(UC_Code)"));
			conn.ExecuteNonQuery(DbSchemaChange.GetCreateIndexIfNotExistsScript("USCCountry", "NR_IX__UC_IsoCountryCode", "CREATE UNIQUE INDEX NR_IX__UC_IsoCountryCode ON USCCountry(UC_ISOCountryCode)"));
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select Count(*) from sys.columns  where name = 'uc_IsoCountryCode'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from sys.columns  where name = 'uc_Code'"));
			AssertEquals(0, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from USCCountry where uc_code != uc_IsoCOuntryCode"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from sys.indexes where name = 'NR_IX__UC_Code'"));
			AssertEquals(0, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from sys.indexes where name = 'NR_IX__UC_IsoCountryCode'"));
			AssertEquals(0, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from sys.indexes where name = 'UC_Code'"));
		}
	}
}
