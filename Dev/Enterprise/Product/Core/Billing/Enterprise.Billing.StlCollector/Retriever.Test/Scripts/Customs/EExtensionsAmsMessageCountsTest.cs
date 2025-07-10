using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(ExtensionsAmsMessageCounts))]
	sealed class EExtensionsAmsMessageCountsTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @BhPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @BhPk02 UNIQUEIDENTIFIER = newid();

				DECLARE @BbPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @BbPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @BbPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @BbPk04 UNIQUEIDENTIFIER = newid();

				DECLARE @BmPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @BmPk02 UNIQUEIDENTIFIER = newid();

				DECLARE @GcPkGb UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPkGb UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkGb);
				DECLARE @GdPK01 UNIQUEIDENTIFIER = (SELECT TOP (1) GE_PK FROM dbo.GlbDepartment);

				UPDATE dbo.GlbCompany SET GC_RN_NKCountryCode = 'GB', GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_PK = @GcPkGb;

				INSERT dbo.CusInbondHeader (BH_PK, BH_GB, BH_JobReference, BH_ApplicationCode, BH_SystemCreateTimeUtc, BH_SystemCreateUser, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser) VALUES
					(@BhPk01, @GbPkGb, 'AMS0000001', 'AMS', '2012-12-03', 'US1', GetUtcDate(), '~BP');

				INSERT dbo.CusInbondHeader (BH_PK, BH_GB, BH_JobReference, BH_ApplicationCode, BH_SystemCreateTimeUtc, BH_SystemCreateUser, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser) VALUES
					(@BhPk02, @GbPkGb, 'AMS0000002', 'AMS', '2012-12-03', 'US1', GetUtcDate(), '~BP');

				INSERT dbo.CusInBondMoveHeader (BM_PK, BM_SubApplicationCode, BM_BH, BM_SystemCreateTimeUtc, BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser) VALUES
					(@BmPk01, 'AMS', @BhPk01, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@BmPk02, 'AMS', @BhPk02, GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.CusInbondBill (B0_PK, B0_BH, B0_IssuerCode, B0_MasterbillNumber, B0_ShipmentType, B0_SystemCreateTimeUtc, B0_SystemCreateUser, B0_SystemLastEditTimeUtc, B0_SystemLastEditUser) VALUES
					(@BbPk01, @BhPk01, 'OTT1', 'Bill01', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@BbPk02, @BhPk01, 'OTT1', 'Bill02', 'OBT', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@BbPk03, @BhPk02, 'OTT1', 'Bill03', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@BbPk04, @BhPk02, 'OTT1', 'Bill04', 'OBT', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.CusInBondMoveDetail (B9_PK, B9_BM, B9_B0, B9_FirstAcceptedTime, B9_SystemCreateTimeUtc, B9_SystemCreateUser, B9_SystemLastEditTimeUtc, B9_SystemLastEditUser) VALUES
					(newid(), @BmPk01, @BbPk01, '2012-12-03 01:10:50', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), @BmPk02, @BbPk03, '2012-12-03 01:11:50', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference) VALUES
					(newid(), 'CusInbondHeader', '2012-12-03', '2012-12-03', @BhPk02, 'TRF', '|TYP=HVL|JOB=S00000001');
";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions("results", () =>
			{
				AssertEquals("Number of Transactions", 2, transactions.Count());

				var transaction1 = FindRowByRef1(transactions, "AMS0000001");
				AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
				AssertEquals("BranchCode", "DEM", transaction1.GetBranchCode());
				AssertEquals("TransactionDateUtc", new DateTime(2012, 12, 3, 01, 10, 50), transaction1.ServiceOccuredUTC);
				AssertEquals("UserCode", "~BP", transaction1.ClientStaffCode);
				AssertEquals("ItemCount", 1, transaction1.BillableCount);
				AssertEquals("TransactionReference02", "Bill02", transaction1.Reference2);
				AssertEquals("TransactionReference03", "Bill01", transaction1.Reference3);
				AssertEquals("TransactionReference04", "OTT1", transaction1.Reference4);

				var transaction2 = FindRowByRef1(transactions, "AMS0000002");
				AssertEquals("CompanyCode", "DEM", transaction2.GetCompanyCode());
				AssertEquals("BranchCode", "DEM", transaction2.GetBranchCode());
				AssertEquals("TransactionDateUtc", new DateTime(2012, 12, 3, 01, 11, 50), transaction2.ServiceOccuredUTC);
				AssertEquals("UserCode", "~BP", transaction2.ClientStaffCode);
				AssertEquals("ItemCount", 1, transaction2.BillableCount);
				AssertEquals("TransactionReference02", "Bill04", transaction2.Reference2);
				AssertEquals("TransactionReference03", "Bill03", transaction2.Reference3);
				AssertEquals("TransactionReference04", "HVL", transaction2.Reference4);
			});
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2012, 12);
			}
		}
	}
}
