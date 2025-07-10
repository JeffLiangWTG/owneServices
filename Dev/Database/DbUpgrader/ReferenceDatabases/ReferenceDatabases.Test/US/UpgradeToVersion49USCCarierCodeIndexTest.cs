using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion49USCCarierCodeIndexTest : USReferenceDbUpgraderVersionTest
	{
		protected override void AssertUpgradeResult()
		{
			AssertEquals("Db has NR_IX__USCCarrier_UI_Code", true, DbObjectCreator.IndexExists(testConnection, "USCCarrier", "NR_IX__USCCarrier_UI_Code"));
		}

		protected override int LatestVersionNumber
		{
			get { return 49; }
		}
	}
}
