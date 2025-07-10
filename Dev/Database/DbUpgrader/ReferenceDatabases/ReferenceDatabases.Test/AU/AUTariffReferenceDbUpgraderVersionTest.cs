namespace Enterprise.DbUpgrader.ReferenceDatabases.AU
{
	abstract class AUTariffReferenceDbUpgraderVersionTest : ReferenceDbUpgraderVersionTest<AUTariffReferenceDbUpgraderForVersionTesting>
	{
		protected override void DoExtraAssertions()
		{
			base.DoExtraAssertions();

			int latestVersion = LatestVersionNumber;
			int actualLatestVersion = refDbUpgrader.GetActualLatestVersion(testConnection);
			Assert(string.Format("You have not updated AUTariffReferenceDbUpgrader.LatestVersion to {0}. It is currently {1}.", latestVersion, actualLatestVersion), latestVersion <= actualLatestVersion);
		}
	}
}
