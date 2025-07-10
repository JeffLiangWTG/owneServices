using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion104_RemoveEmptyCodeOnUSCFIRMS : USReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber => 104;

		Guid PK;

		protected override void PrepareTestData(DbConnection conn)
		{
			TestHelper.CreateTables(conn, new ITableScript[] {
				new USCFIRMS()
				});

			PK = Guid.NewGuid();
			var insertSql = @"INSERT INTO [dbo].[USCFIRMS] ([US_PK] ,[US_Code] ,[US_DistrictPortCode],[US_IsActive],[US_FacilityType],[US_Name],[US_LastUpdate],[US_Address],[US_City],[US_State],[US_ZipCode],[US_Country])
VALUES('{0}', '', '1101', '1', '01', 'FTSA', '2018-10-25', '', '', '', '', '')
";
			conn.ExecuteNonQuery(string.Format(System.Globalization.CultureInfo.InvariantCulture, insertSql, PK));
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals(0, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, string.Format(System.Globalization.CultureInfo.InvariantCulture, "SELECT count(1) FROM USCFIRMS where US_PK = '{0}'", PK)));
		}
	}
}
