using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.CA.Testing
{
	sealed class UpgradeToVersion146Test : CAReferenceDbUpgraderVersionTest
	{
		protected override void PrepareTestData(DbConnection conn)
		{
			base.PrepareTestData(conn);

			var addTariff = @"
INSERT INTO CACClassHeader (ZA_PK, ZA_ClassificationNumber, ZA_EffectiveDate, ZA_ExpiryDate, ZA_AreaCode, ZA_QuotaInd, ZA_PermitInd, ZA_InactiveInd, ZA_ExchangeDateDeterminationFlag, ZA_ClassAuthorityNumber, ZA_TariffAuthorityNumber, ZA_StatisticalUOMCode) VALUES (NEWID(), '1234567890', '2019-01-01 00:00:00', '2019-06-05 00:00:00', '900', 'N', 'N', 'N', 'N', '19-TARIF-KORE', '19-TARIF-KORE', 'KGM');
INSERT INTO CACClassHeader (ZA_PK, ZA_ClassificationNumber, ZA_EffectiveDate, ZA_ExpiryDate, ZA_AreaCode, ZA_QuotaInd, ZA_PermitInd, ZA_InactiveInd, ZA_ExchangeDateDeterminationFlag, ZA_ClassAuthorityNumber, ZA_TariffAuthorityNumber, ZA_StatisticalUOMCode) VALUES (NEWID(), '1234567890', '2020-01-01 00:00:00', '2079-06-05 00:00:00', '900', 'N', 'N', 'N', 'N', '20-TARIF-KORE', '20-TARIF-KORE', 'KGM');
INSERT INTO CACClassHeader (ZA_PK, ZA_ClassificationNumber, ZA_EffectiveDate, ZA_ExpiryDate, ZA_AreaCode, ZA_QuotaInd, ZA_PermitInd, ZA_InactiveInd, ZA_ExchangeDateDeterminationFlag, ZA_ClassAuthorityNumber, ZA_TariffAuthorityNumber, ZA_StatisticalUOMCode) VALUES (NEWID(), '1234567890', '2021-04-01 00:00:00', '2022-04-30 00:00:00', '900', 'N', 'N', 'N', 'N', '21-TARIF-KORE', '21-TARIF-KORE', 'KGM');
INSERT INTO CACClassHeader (ZA_PK, ZA_ClassificationNumber, ZA_EffectiveDate, ZA_ExpiryDate, ZA_AreaCode, ZA_QuotaInd, ZA_PermitInd, ZA_InactiveInd, ZA_ExchangeDateDeterminationFlag, ZA_ClassAuthorityNumber, ZA_TariffAuthorityNumber, ZA_StatisticalUOMCode) VALUES (NEWID(), '1234567890', '2022-01-01 00:00:00', '2022-12-31 00:00:00', '900', 'N', 'N', 'N', 'N', '22-TARIF-KORE', '22-TARIF-KORE', 'KGM');
";
			conn.ExecuteNonQuery(addTariff);
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals("Expired tariff", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACClassHeader WHERE ZA_ClassificationNumber = '1234567890' AND ZA_ExpiryDate = '2019-06-05 00:00:00'"));
			AssertEquals("Expired tariff", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACClassHeader WHERE ZA_ClassificationNumber = '1234567890' AND ZA_ExpiryDate = '2021-03-31 00:00:00'"));
			AssertEquals("Expired tariff", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACClassHeader WHERE ZA_ClassificationNumber = '1234567890' AND ZA_ExpiryDate = '2021-12-31 00:00:00'"));
			AssertEquals("Expired tariff", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACClassHeader WHERE ZA_ClassificationNumber = '1234567890' AND ZA_ExpiryDate = '2022-12-31 00:00:00'"));
		}

		protected override int LatestVersionNumber
		{
			get { return 146; }
		}
	}
}
