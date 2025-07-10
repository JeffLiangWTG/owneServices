using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion82_AddAMSProductNumber : USReferenceDbUpgraderVersionTest
	{
		protected override void PrepareTestData(DbConnection conn)
		{
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals(1348, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCAMSProductNumber"));
		}

		protected override int LatestVersionNumber
		{
			get { return 82; }
		}
	}
}
