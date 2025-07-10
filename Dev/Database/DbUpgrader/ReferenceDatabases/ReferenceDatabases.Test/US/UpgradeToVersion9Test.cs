using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion9Test : USReferenceDbUpgraderVersionTest
	{
		protected override void AssertUpgradeResult()
		{
			string columnSizeCheck =
					"SELECT count(*) " +
					"FROM sys.tables AS tbl " +
					"INNER JOIN sys.all_columns AS clmns ON clmns.object_id=tbl.object_id " +
					"WHERE (tbl.name=N'USCCarrier' and clmns.name=N'UI_Address' and SCHEMA_NAME(tbl.schema_id)=N'dbo' and clmns.max_length=105)";
			AssertEquals("USCCarrier.UI_Address column size is NOT equal 105", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, columnSizeCheck));
		}

		protected override int LatestVersionNumber
		{
			get { return 9; }
		}
	}
}
