using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion65AdjustUSCTariffSchemaForUE_OGACodes_2_Test : USReferenceDbUpgraderVersionTest
	{
		protected override void AssertUpgradeResult()
		{
			string script = @"SELECT CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS
					WHERE TABLE_NAME = 'USCTariff'
					AND COLUMN_NAME = 'UE_OGACodes'";

			AssertEquals(75, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, script));
		}

		protected override int LatestVersionNumber
		{
			get { return 65; }
		}
	}
}
