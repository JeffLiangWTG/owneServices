using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.CA.Testing
{
	sealed class UpgradeToVersion153Test : CAReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber => 153;
		protected override void AssertUpgradeResult()
		{
			AssertEquals("CQ_RelatedUSPortOfExit - 3301", "3301",
				UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT CQ_RelatedUSPortOfExit FROM CACCBSAOfficeCodes WHERE CQ_Code in ('0607')"));
			AssertEquals("CQ_RelatedUSPortOfExit - 3023", "3023",
				UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT CQ_RelatedUSPortOfExit FROM CACCBSAOfficeCodes WHERE CQ_Code in ('0841')"));
		}
	}
}
