using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(PNTSManifestScript))]
	sealed class PNTSManifestScriptTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 8);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Transactions", 3, transactions.Count());
				AssertRowMatchingRef1(transactions, "LV-08", "~LV", "^LV", new DateTime(2022, 8, 01), string.Empty, 1, "Job Reference:C0000ABC2", "Master Bill:BN002", null, null);
				AssertRowMatchingRef1(transactions, "LV-08", "~LV", "^LV", new DateTime(2022, 8, 01), string.Empty, 1, "Job Reference:C0000ABC4", null, null, null);
				AssertRowMatchingRef1(transactions, "FR-08", "~FR", "^FR", new DateTime(2022, 8, 01), string.Empty, 1, "Job Reference:C0000ABC6", "Master Bill:BN006", null, null);
			});
		}

		protected override void PrepareTestData()
		{
			var sqlText = @"

				DECLARE @GcPkFr UNIQUEIDENTIFIER = NEWID();
				DECLARE @GcPkLv UNIQUEIDENTIFIER = NEWID();

				DECLARE @GbPkFr UNIQUEIDENTIFIER = NEWID();
				DECLARE @GbPkLv UNIQUEIDENTIFIER = NEWID();

				DECLARE @AmaPk1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @AmaPk2 UNIQUEIDENTIFIER = NEWID();
				DECLARE @AmaPk3 UNIQUEIDENTIFIER = NEWID();
				DECLARE @AmaPk4 UNIQUEIDENTIFIER = NEWID();
				DECLARE @AmaPk5 UNIQUEIDENTIFIER = NEWID();
				DECLARE @AmaPk6 UNIQUEIDENTIFIER = NEWID();

				INSERT GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES
					(@GcPkFr, '~FR', 'FR company', 'FR'),
					(@GcPkLv, '~LV', 'LV company', 'LV');

				INSERT GlbBranch (GB_PK, GB_Code, GB_GC) VALUES
					(@GbPkFr, '^FR', @GcPkFr),
					(@GbPkLv, '^LV', @GcPkLv);

				INSERT AsycudaManifestHeader (AMA_PK, AMA_JobReference, AMA_ManifestType, AMA_ApplicationCode, AMA_SystemCreateTimeUtc, AMA_SystemLastEditTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditUser, AMA_ClusterKey, AMA_RN_NKCountry, AMA_GB) VALUES
				(@AmaPk1, 'C0000ABC1', 'MAN', 'ETR', '2022-08-01', '2022-08-01', 'YGT', 'KNZ', 1, 'FR', @GbPkFr),
				(@AmaPk2, 'C0000ABC2', 'MAN', 'STO', '2022-08-01', '2022-08-01', 'YGT', 'KNZ', 2, 'LV', @GbPkLv),
				(@AmaPk3, 'C0000ABC3', 'MAN', 'ETR', '2022-08-01', '2022-08-01', 'YGT', 'KNZ', 3, 'FR', @GbPkFr),
				(@AmaPk4, 'C0000ABC4', 'MAN', 'STO', '2022-08-01', '2022-08-01', 'YGT', 'KNZ', 4, 'LV', @GbPkLv),
				(@AmaPk5, 'C0000ABC5', 'MAN', 'ETR', '2022-08-01', '2022-08-01', 'YGT', 'KNZ', 5, 'LV', @GbPkLv),
				(@AmaPk6, 'C0000ABC6', 'MAN', 'STO', '2022-08-01', '2022-08-01', 'YGT', 'KNZ', 6, 'FR', @GbPkFr);

				INSERT AsycudaBill (ABL_PK, ABL_AMA, ABL_BolType, ABL_BillNumber, ABL_SystemCreateTimeUtc, ABL_SystemLastEditTimeUtc, ABL_SystemCreateUser, ABL_SystemLastEditUser, ABL_ClusterKey) VALUES
				(0x1, @AmaPk1, 'STD', 'BN001', '2022-08-01', '2022-08-01', 'YGT', 'KNZ', 1),
				(0x2, @AmaPk2, 'BOL', 'BN002', '2022-08-01', '2022-08-01', 'YGT', 'KNZ', 2),
				(0x3, @AmaPk3, 'BOL', 'BN003', '2022-08-01', '2022-08-01', 'YGT', 'KNZ', 3),
				(0x4, @AmaPk4, 'STD', 'BN004', '2022-08-01', '2022-08-01', 'YGT', 'KNZ', 4),
				(0x5, @AmaPk5, 'STD', 'BN005', '2022-08-01', '2022-08-01', 'YGT', 'KNZ', 5),
				(0x6, @AmaPk6, 'BOL', 'BN006', '2022-08-01', '2022-08-01', 'YGT', 'KNZ', 6);
			";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
