using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using CargoWise.Types;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(CommunicationSpecialEntryInbond))]
	sealed class CommunicationSpecialEntryInbondTest : RefStlScriptWithDefaultsTest
	{
		internal readonly ZGuid transactionGuidReference = ZGuid.NewZGuid();
		internal readonly ZGuid transactionGuidReference2 = ZGuid.NewZGuid();

		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @BhPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @BhPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkSg UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'SIN');
				DECLARE @GcPkUs UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPkSg UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkSg);
				DECLARE @GbPkUs UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkUs);
				UPDATE dbo.GlbCompany SET GC_RN_NKCountryCode = 'US', GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_PK = @GcPkUs;

				INSERT dbo.CusInbondHeader (BH_PK, BH_GB, BH_JobReference, BH_ApplicationCode, BH_SystemCreateTimeUtc, BH_SystemCreateUser, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser) VALUES
					(@BhPk01, @GbPkUs, 'BH01', 'INB', '2012-12-03', 'US1', GetUtcDate(), '~BP'),
					(@BhPk02, @GbPkSg, 'BH02', 'INB', '2012-12-02', 'US2', GetUtcDate(), '~BP'),
					(newid(), @GbPkUs, 'BH03', 'INB', '2012-12-10', 'US2', GetUtcDate(), '~BP');
				INSERT dbo.CusInbondMoveHeader (BM_PK, BM_BH,BM_SystemCreateTimeUtc,BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser) VALUES
					('{0}', @BhPk01, '2017-01-17', 'TU', GetUtcDate(), '~BP'),
					('{1}', @BhPk02, '2017-02-02', 'TU', GetUtcDate(), '~BP');
INSERT dbo.CusEntryNum (CE_PK, CE_EntryNum,CE_ParentTable,CE_ParentID,CE_EntryType, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) VALUES
					(newid(), '00001', 'CusInbondMoveHeader', '{0}', 'INB', getutcdate(), '~BP', getutcdate(), '~BP'),
					(newid(), '00002', 'CusInbondMoveHeader', '{1}', 'INB', getutcdate(), '~BP', getutcdate(), '~BP');";
			TestConnection.ExecuteNonQuery(string.Format(sqlText, transactionGuidReference, transactionGuidReference2));
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());
			var transaction1 = transactions.First();
			AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2017, 01, 17), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "TU", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "BH01", transaction1.Reference1);
			AssertEquals("TransactionReference02", "00001", transaction1.Reference2);
			AssertEquals("TransactionGuidReference", transactionGuidReference.ToString().ToUpper(), transaction1.Reference5.ToUpper());
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2017, 1);
			}
		}
	}
}
