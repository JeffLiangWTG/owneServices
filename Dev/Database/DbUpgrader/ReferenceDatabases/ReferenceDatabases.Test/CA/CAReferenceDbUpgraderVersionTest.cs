namespace Enterprise.DbUpgrader.ReferenceDatabases.CA.Testing
{
	abstract class CAReferenceDbUpgraderVersionTest : ReferenceDbUpgraderVersionTest<CAReferenceDbUpgraderForVersionTesting>
	{
		protected override void DoExtraAssertions()
		{
			base.DoExtraAssertions();

			int latestVersion = LatestVersionNumber;
			int actualLatestVersion = refDbUpgrader.GetActualLatestVersion(testConnection);
			Assert(string.Format("You have not updated CAReferenceDbUpgrader.LatestVersion to {0}. It is currently {1}.", latestVersion, actualLatestVersion), latestVersion <= actualLatestVersion);
		}
	}
}
