using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion8Test : USReferenceDbUpgraderVersionTest
	{
		protected override void AssertUpgradeResult()
		{
			string constraintCheck = "SELECT count(*) FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND parent_object_id = OBJECT_ID(N'[dbo].[USCCountry]')";
			AssertEquals("Db constraints removed USCVersion", 0, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, string.Format(constraintCheck, "UC_LesserDevelopedCountry")));
		}

		protected override int LatestVersionNumber
		{
			get { return 8; }
		}
	}
}
