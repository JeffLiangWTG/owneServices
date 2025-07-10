namespace Enterprise.DbUpgrader.ReferenceDatabases.CA
{
	abstract class CATariffReferenceDbUpgraderVersionTest : ReferenceDbUpgraderVersionTest<CATariffReferenceDbUpgraderForVersionTesting>
	{
		protected override void DoExtraAssertions()
		{
			base.DoExtraAssertions();

			int latestVersion = LatestVersionNumber;
			int actualLatestVersion = refDbUpgrader.GetActualLatestVersion(testConnection);
			Assert(string.Format("You have not updated CATariffReferenceDbUpgrader.LatestVersion to {0}. It is currently {1}.", latestVersion, actualLatestVersion), latestVersion <= actualLatestVersion);
		}
	}
}
