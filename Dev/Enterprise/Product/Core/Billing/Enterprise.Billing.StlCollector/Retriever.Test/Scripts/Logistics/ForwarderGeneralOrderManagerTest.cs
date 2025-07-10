using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ForwarderGeneralOrderManager))]
	sealed class ForwarderGeneralOrderManagerTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @JdPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @JdPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPk01 UNIQUEIDENTIFIER = NEWID();
				DECLARE @GcPk02 UNIQUEIDENTIFIER = NEWID();
				DECLARE @GbPk01 UNIQUEIDENTIFIER = NEWID();
				DECLARE @GbPk02 UNIQUEIDENTIFIER = NEWID();

				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk01, 'DAU', 'AU company', 'AUD', 'AU');
				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk02, 'SHA', 'CN company', 'CNY', 'CN');
				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk01, 'GB1', @GcPk01, NULL);
				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk02, 'GB2', @GcPk02, NULL);
				DECLARE @OaPk UNIQUEIDENTIFIER = (SELECT TOP(1) OA_PK FROM dbo.OrgAddress);
				INSERT dbo.JobOrderHeader  (JD_PK, JD_OrderNumber, JD_SystemCreateTimeUtc, JD_OA_BuyerAddress, JD_SystemCreateUser) VALUES
					(@JdPk01, 'JOH01', '2014-04-30', @OaPk, 'US1'),
					(@JdPk02, 'JOH02', '2014-05-01', @OaPk, 'US2');
				INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsCancelled) VALUES
					(newid(), 'JobOrderHeader', getdate(), '2014-04-30 12:01:00', @JdPk01, 'ADD', '', 'GB1', 'N'),
					(newid(), 'JobOrderHeader', getdate(), '2014-04-30 12:02:00', @JdPk01, 'EDT', '', 'GB1', 'N')
				INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsCancelled) VALUES
					(newid(), 'JobOrderHeader', getdate(), '2014-05-01 12:01:00', @JdPk02, 'ADD', '', 'GB2', 'N'),
					(newid(), 'JobOrderHeader', getdate(), '2014-05-01 12:02:00', @JdPk02, 'EDT', '', 'GB2', 'N')";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());
			var transaction1 = transactions.First();
			AssertEquals("CompanyCode", "DAU", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "GB1", transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2014, 4, 30), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "US1", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "JOH01", transaction1.Reference1);
			AssertEquals("TransactionReference02", null, transaction1.Reference2);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 4);
			}
		}
	}
}
