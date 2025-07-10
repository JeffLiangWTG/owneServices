using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion116_UpdateUSCTariffRule : USReferenceDbUpgraderVersionTest
	{
		protected override void AssertUpgradeResult()
		{
			AssertEquals(0, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName,
				"SELECT Count(U1_PK) FROM USCTariffRule WHERE U1_Tariff = '9817006000' AND U1_RuleCode = 'STN' AND U1_PK = '7E8717A6-245F-4AF0-B016-CF1473FD72C9'"));
			AssertEquals(0, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName,
				"SELECT Count(U3_PK) FROM USCRuleSecondaryTariff WHERE U3_U1 = '7E8717A6-245F-4AF0-B016-CF1473FD72C9'"));
		}

		protected override int LatestVersionNumber => 116;
	}
}
