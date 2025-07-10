using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(GlobalManifestCountTransactionForGMB))]
	sealed class GlobalManifestCountTransactionForGMBTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			var sqlText = @"

				DECLARE @GcPkSg UNIQUEIDENTIFIER = NEWID();
				DECLARE @GcPkTr UNIQUEIDENTIFIER = NEWID();

				DECLARE @GbPkSg UNIQUEIDENTIFIER = NEWID();
				DECLARE @GbPkTr UNIQUEIDENTIFIER = NEWID();

				DECLARE @AmaPk1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @AmaPk2 UNIQUEIDENTIFIER = NEWID();
				DECLARE @AmaPk3 UNIQUEIDENTIFIER = NEWID();
				DECLARE @AmaPk4 UNIQUEIDENTIFIER = NEWID();
				DECLARE @AmaPk5 UNIQUEIDENTIFIER = NEWID();
				DECLARE @AmaPk6 UNIQUEIDENTIFIER = NEWID();

				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES
					(@GcPkSg, '~SG', 'SG company', 'SG'),
					(@GcPkTr, '~TR', 'TR company', 'TR');

				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES
					(@GbPkSg, '^SG', @GcPkSg),
					(@GbPkTr, '^TR', @GcPkTr);

				INSERT dbo.AsycudaManifestHeader (AMA_PK, AMA_JobReference, AMA_ManifestType, AMA_ApplicationCode, AMA_SystemCreateTimeUtc, AMA_SystemLastEditTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditUser, AMA_ClusterKey, AMA_RN_NKCountry, AMA_GB) VALUES
				(@AmaPk1, 'C0000ABC1', 'MAN', 'ETR', 2022-08-01, 2022-08-01, 'YGT', 'KNZ', 1, 'SG', @GbPkSg),
				(@AmaPk2, 'C0000ABC2', 'MAN', 'NVC', 2022-08-01, 2022-08-01, 'YGT', 'KNZ', 2, 'TR', @GbPkTr),
				(@AmaPk3, 'C0000ABC3', 'MAN', 'ETR', 2022-08-01, 2022-08-01, 'YGT', 'KNZ', 3, 'SG', @GbPkSg),
				(@AmaPk4, 'C0000ABC4', 'MAN', 'VOC', 2022-08-01, 2022-08-01, 'YGT', 'KNZ', 4, 'TR', @GbPkTr),
				(@AmaPk5, 'C0000ABC5', 'MAN', 'ETR', 2022-08-01, 2022-08-01, 'YGT', 'KNZ', 5, 'TR', @GbPkTr),
				(@AmaPk6, 'C0000ABC6', 'MAN', 'OUT', 2022-08-01, 2022-08-01, 'YGT', 'KNZ', 6, 'SG', @GbPkSg);

				INSERT dbo.AsycudaBill (ABL_PK, ABL_AMA, ABL_BolType, ABL_BillNumber, ABL_SystemCreateTimeUtc, ABL_SystemLastEditTimeUtc, ABL_SystemCreateUser, ABL_SystemLastEditUser, ABL_ClusterKey) VALUES
				(0x1, @AmaPk1, 'STD', 'BN001', 2022-08-01, 2022-08-01, 'YGT', 'KNZ', 1),
				(0x2, @AmaPk2, 'BOL', 'BN002', 2022-08-01, 2022-08-01, 'YGT', 'KNZ', 2),
				(0x3, @AmaPk3, 'BOL', 'BN003', 2022-08-01, 2022-08-01, 'YGT', 'KNZ', 3),
				(0x4, @AmaPk4, 'STD', 'BN004', 2022-08-01, 2022-08-01, 'YGT', 'KNZ', 4),
				(0x5, @AmaPk5, 'STD', 'BN005', 2022-08-01, 2022-08-01, 'YGT', 'KNZ', 5),
				(0x6, @AmaPk6, 'BOL', 'BN006', 2022-08-01, 2022-08-01, 'YGT', 'KNZ', 6);

				INSERT dbo.CusEntryNum(CE_PK, CE_ParentID, CE_ParentTable, CE_EntryNum, CE_RN_NKCountryCode, CE_EntryType, CE_Category, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) VALUES
				(NEWID(), @AmaPk2, 'AsycudaManifestHeader', 'CEN02', 'TR', 'ASY', 'CUS', '2022-08-01', 'YGT', getutcdate(), 'KNZ'),
				(NEWID(), @AmaPk3, 'AsycudaManifestHeader', 'CEN03', 'TR', 'ASY', 'CUS', '2022-08-01', 'YGT', getutcdate(), 'KNZ'),
				(NEWID(), @AmaPk4, 'AsycudaManifestHeader', 'CEN04', 'TR', 'ASY', 'CUS', '2022-08-01', 'YGT', getutcdate(), 'KNZ'),
				(NEWID(), @AmaPk5, 'AsycudaManifestHeader', 'CEN05', 'TR', 'ASY', 'CUS', '2022-08-01', 'YGT', getutcdate(), 'KNZ');
			";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 8);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions("Below test is for the number of transactions and the macthing row as expected", () =>
			{
				AssertEquals("Number of Transactions", 1, transactions.Count());
				AssertRowMatchingRef1(transactions, "TR-Correct for first record", "~TR", "^TR", new DateTime(2022, 08, 01), "YGT", 1, "CEN04", "C0000ABC4", "BN004", null);
			});
		}
	}
}
