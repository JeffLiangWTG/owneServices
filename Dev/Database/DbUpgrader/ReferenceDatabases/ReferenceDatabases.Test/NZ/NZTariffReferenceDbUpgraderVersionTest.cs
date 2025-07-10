namespace Enterprise.DbUpgrader.ReferenceDatabases.NZ.Testing
{
	abstract class NZTariffReferenceDbUpgraderVersionTest : ReferenceDbUpgraderVersionTest<NZTariffReferenceDbUpgraderForVersionTesting>
	{
		protected override void DoExtraAssertions()
		{
			base.DoExtraAssertions();

			int latestVersion = LatestVersionNumber;
			int actualLatestVersion = refDbUpgrader.GetActualLatestVersion(testConnection);
			Assert(string.Format("You have not updated NZTariffReferenceDbUpgrader.LatestVersion to {0}. It is currently {1}.", latestVersion, actualLatestVersion), latestVersion <= actualLatestVersion);
		}
	}
}
