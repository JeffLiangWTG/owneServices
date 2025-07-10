namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion6Test : USReferenceDbUpgraderVersionTest
	{
		protected override void AssertUpgradeResult()
		{
			AssertEquals("Column 'UR_Prefix' should exists in table USCRegionDistrictPort", 1, (int)testConnection.ExecuteScalar(string.Format("SELECT COUNT(*) FROM [{0}].information_Schema.columns where column_Name = 'UR_Prefix'", refDbUpgrader.DbName)));
			AssertEquals("Index 'NR_IX__UR_Code' should exists in table USCRegionDistrictPort", 1, (int)testConnection.ExecuteScalar(string.Format("SELECT COUNT(*) FROM [{0}].sys.indexes i INNER JOIN [{0}].sys.tables t ON i.object_id = t.object_id WHERE i.name = 'NR_IX__UR_Code' AND t.name = 'USCRegionDistrictPort'", refDbUpgrader.DbName)));

			string sqlText = string.Format(@"
				SELECT count(*) FROM [{0}].INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = 'USCRegionDistrictPort'
				AND COLUMN_NAME = 'UR_Code'
				AND DATA_TYPE = 'varchar'
				AND CHARACTER_MAXIMUM_LENGTH = '4'",
			refDbUpgrader.DbName);

			AssertEquals("Column 'UR_Code' should exists and length should be '4'", 1, (int)testConnection.ExecuteScalar(sqlText));
		}

		protected override int LatestVersionNumber
		{
			get { return 6; }
		}
	}
}
