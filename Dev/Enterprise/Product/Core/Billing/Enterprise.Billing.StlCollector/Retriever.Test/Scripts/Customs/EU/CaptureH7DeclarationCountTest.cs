using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Scripts.Customs.EU;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Testing.Scripts.Customs.EU
{
	[TestedType(typeof(CaptureH7DeclarationCount))]
	sealed class CaptureH7DeclarationCountTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2024, 11);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Transactions", 5, transactions.Count());
				AssertResultRow(transactions, "ABL1110001", "GB", "00000002-0000-0000-0000-000000000000", "~C1", "~B1", "~S1", new DateTime(2024, 11, 11), "NoStmALog", "Should return H7 job without StmALog");
				AssertResultRow(transactions, "ABL2220001", "GB", "00000004-0000-0000-0000-000000000000", "~C1", "~B1", "~S1", new DateTime(2024, 11, 12), "StmALog:NotConvertedFromHVL", "Should return H7 job if StmALog.SL_Reference does not contain TYP=HVL");
				AssertResultRow(transactions, "ABL2220002", "GB", "00000005-0000-0000-0000-000000000000", "~C1", "~B1", "~S1", new DateTime(2024, 11, 13), "StmALog:NotConvertedFromHVL", "Should return all bills with ABL_BolType <> 'BOL'");
				AssertResultRow(transactions, "ABL3330001", "GB", "00000006-0000-0000-0000-000000000000", "~C1", "~B1", "~S1", new DateTime(2024, 11, 14), "StmALog:EventNotTRF", "Should return H7 job if StmALog.Event <> TRF");
				AssertResultRow(transactions, "ABL4440001", "GB", "00000007-0000-0000-0000-000000000000", "~C1", "~B1", "~S1", new DateTime(2024, 11, 15), "StmALog:ParentNotAMA", "Should return H7 job if StmALog.ParentTableCode <> AsycudaManifestHeader");
			});
		}

		protected override void PrepareTestData()
		{
			var sqlText = $@"
				DECLARE @CompanyPk UNIQUEIDENTIFIER = newid();
				DECLARE @BranchPk UNIQUEIDENTIFIER = newid();
				DECLARE @JobConsolPk UNIQUEIDENTIFIER = newid();
				DECLARE @Manifest1Pk UNIQUEIDENTIFIER = newid();
				DECLARE @Manifest2Pk UNIQUEIDENTIFIER = newid();
				DECLARE @Manifest3Pk UNIQUEIDENTIFIER = newid();
				DECLARE @Manifest4Pk UNIQUEIDENTIFIER = newid();
				DECLARE @Manifest5Pk UNIQUEIDENTIFIER = newid();
				DECLARE @Manifest6Pk UNIQUEIDENTIFIER = newid();
				DECLARE @Manifest7Pk UNIQUEIDENTIFIER = newid();

				INSERT INTO dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_Code, GC_Name) VALUES (@CompanyPk, 'GB', '~C1', 'GB company');
				INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_Code) VALUES (@BranchPk, @CompanyPk, '~B1');

				INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_FullName, GS_LoginName, GS_GB_HomeBranch, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) 
				VALUES (newid(), '~S1', 'Test User', 'test.staff', NULL, GetUtcDate(), 'E', GetUtcDate(), '~BP');

				INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_MasterBillNum) VALUES (@JobConsolPk, 'JK00000001', 'B00000001')

				INSERT INTO dbo.AsycudaManifestHeader(AMA_PK, AMA_RN_NKCountry, AMA_GB, AMA_ManifestType, AMA_ClusterKey, AMA_JobReference, AMA_ParentTableCode, AMA_ParentId, AMA_SystemCreateTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditTimeUtc, AMA_SystemLastEditUser) 
				VALUES
					(@Manifest1Pk, 'GB', @BranchPk, 'EH7', 1, 'NoStmALog', 'JK', @JobConsolPk, '2024-10-01 00:00:00.000', 'E', '2024-11-16 00:00:00.000', 'E'),
					(@Manifest2Pk, 'GB', @BranchPk, 'EH7', 2, 'StmALog:NotConvertedFromHVL', 'JK', @JobConsolPk, '2024-11-11 00:00:00.000', 'E', '2024-12-16 00:00:00.000', 'E'),
					(@Manifest3Pk, 'GB', @BranchPk, 'EH7', 3, 'StmALog:EventNotTRF', '', NULL, '2024-11-01 00:00:00.000', 'E', '2024-11-16 00:00:00.000', 'E'),
					(@Manifest4Pk, 'GB', @BranchPk, 'EH7', 4, 'StmALog:ParentNotAMA', '', NULL, '2024-11-01 00:00:00.000', 'E', '2024-11-16 00:00:00.000', 'E'),
					(@Manifest5Pk, 'GB', @BranchPk, 'MAN', 5, 'NotEH7', '', NULL, '2024-11-01 00:00:00.000', 'E', '2024-12-16 00:00:00.000', 'E'),
					(@Manifest6Pk, 'GB', @BranchPk, 'EH7', 6, 'ConvertedFromHVL', '', NULL, '2024-11-01 00:00:00.000', 'E', '2024-11-16 00:00:00.000', 'E');

				INSERT INTO dbo.AsycudaBill(ABL_PK, ABL_AMA, ABL_ClusterKey, ABL_BillNumber, ABL_BolType, ABL_SystemCreateTimeUtc, ABL_SystemCreateUser, ABL_SystemLastEditTimeUtc, ABL_SystemLastEditUser) 
				VALUES
					('00000001-0000-0000-0000-000000000000', @Manifest1Pk, 1, 'ABL1110000', 'BOL', '2024-11-10 00:00:00.000', '~S1', '2024-11-16 00:00:00.000', 'E'),
					('00000002-0000-0000-0000-000000000000', @Manifest1Pk, 1, 'ABL1110001', '', '2024-11-11 00:00:00.000', '~S1', '2024-11-16 00:00:00.000', 'E'),
					('00000003-0000-0000-0000-000000000000', @Manifest2Pk, 2, 'ABL2220000', 'BOL', '2024-11-10 00:00:00.000', '~S1', '2024-11-16 00:00:00.000', 'E'),
					('00000004-0000-0000-0000-000000000000', @Manifest2Pk, 2, 'ABL2220001', '', '2024-11-12 00:00:00.000', '~S1', '2024-11-16 00:00:00.000', 'E'),
					('00000005-0000-0000-0000-000000000000', @Manifest2Pk, 2, 'ABL2220002', '', '2024-11-13 00:00:00.000', '~S1', '2024-11-16 00:00:00.000', 'E'),
					('00000006-0000-0000-0000-000000000000', @Manifest3Pk, 3, 'ABL3330001', '', '2024-11-14 00:00:00.000', '~S1', '2024-11-16 00:00:00.000', 'E'),
					('00000007-0000-0000-0000-000000000000', @Manifest4Pk, 4, 'ABL4440001', '', '2024-11-15 00:00:00.000', '~S1', '2024-11-16 00:00:00.000', 'E'),
					('00000008-0000-0000-0000-000000000000', @Manifest5Pk, 5, 'ABL5550001', '', '2024-11-16 00:00:00.000', '~S1', '2024-11-16 00:00:00.000', 'E'),
					('00000009-0000-0000-0000-000000000000', @Manifest6Pk, 6, 'ABL6660001', '', '2024-11-17 00:00:00.000', '~S1', '2024-11-16 00:00:00.000', 'E');	

				INSERT dbo.StmALog (SL_PK, SL_Parent, SL_Table, SL_Reference, SL_IsCancelled, SL_SE_NKEvent, SL_PostedTimeUtc, SL_EventTime)
				VALUES
					(newid(), @Manifest2Pk, 'AsycudaManifestHeader', '|TYP=OTH', 'N', 'TRF', '2023-09-21', '2023-09-21'),
					(newid(), @Manifest3Pk, 'AsycudaManifestHeader', '|TYP=HVL', 'N', 'NAN', '2023-09-21', '2023-09-21'),	
					(newid(), @Manifest5Pk, 'Non-AsycudaManifestHeader', '|TYP=HVL', 'N', 'TRF', '2023-09-21', '2023-09-21'),
					(newid(), @Manifest6Pk, 'AsycudaManifestHeader', '|TYP=HVL', 'N', 'TRF', '2023-09-20', '2023-09-20')";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		void AssertResultRow(IEnumerable<IStlTransaction> transactions, string reference2, string reference3, string reference5, string companyCode, string branchCode, string staffCode, DateTime transactionDate, string reference1, string message)
		{
			IStlTransaction transaction = null;
			AssertNoExceptionThrown(message, () => { transaction = FindRowByRef2(transactions, reference2); });
			AssertEquals("TransactionReference01", reference1, transaction.Reference1);
			AssertEquals("TransactionReference03", reference3, transaction.Reference3);
			AssertEquals("TransactionReference04", null, transaction.Reference4);
			AssertEquals("TransactionGuidReference", reference5, transaction.Reference5);
			AssertEquals("CompanyCode", companyCode, transaction.GetCompanyCode());
			AssertEquals("BranchCode", branchCode, transaction.GetBranchCode());
			AssertEquals("UserCode", staffCode, transaction.ClientStaffCode);
			AssertEquals("TransactionDate", transactionDate, transaction.ServiceOccuredUTC);
		}
	}
}
