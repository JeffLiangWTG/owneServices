using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.CA.Testing
{
	sealed class UpgradeToVersion149Test : CAReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber => 149;

		protected override void AssertUpgradeResult()
		{
			AssertEquals("ValidTariffTreatments of BY", "03", UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT CA_ValidTariffTreatments FROM CACountryPreference WHERE CA_CountryCode = 'BY'"));
			AssertEquals("ValidTariffTreatments of RU", "03", UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT CA_ValidTariffTreatments FROM CACountryPreference WHERE CA_CountryCode = 'RU'"));
		}
	}
}
