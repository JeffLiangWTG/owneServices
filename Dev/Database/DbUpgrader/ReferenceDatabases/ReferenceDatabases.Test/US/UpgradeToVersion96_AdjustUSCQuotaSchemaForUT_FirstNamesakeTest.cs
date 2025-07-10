using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion96_AdjustUSCQuotaSchemaForUT_FirstNamesakeTest : USReferenceDbUpgraderVersionTest
	{
		protected override void AssertUpgradeResult()
		{
			string script = @"SELECT CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS
					WHERE TABLE_NAME = 'USCQuota'
					AND COLUMN_NAME = 'UT_FirstNamesake'";

			AssertEquals(15, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, script));
		}

		protected override int LatestVersionNumber
		{
			get { return 96; }
		}
	}
}
