using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion62AdjustUSCTariffSchemaForUE_OGACodesTest : USReferenceDbUpgraderVersionTest
	{
		protected override void AssertUpgradeResult()
		{
			string script = @"SELECT CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS
					WHERE TABLE_NAME = 'USCTariff'
					AND COLUMN_NAME = 'UE_OGACodes'";

			AssertEquals(30, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, script));
		}

		protected override int LatestVersionNumber
		{
			get { return 62; }
		}
	}
}
