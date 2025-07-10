using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Marketing;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Marketing
{
	[TestedType(typeof(SalesCampaignEmail))]
	sealed class SalesCampaignEmailTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @G0Pk01 UNIQUEIDENTIFIER = newid();
				DECLARE @G0Pk02 UNIQUEIDENTIFIER = newid();
				DECLARE @G0Pk03 UNIQUEIDENTIFIER = newid();
				DECLARE @G0Pk04 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPk01 UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GcPk02 UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'EDI');
				INSERT dbo.GlbCompanyCampaign (G0_PK, G0_GC, G0_CampaignName, G0_CampaignID, G0_BroadcastVoteSurveyExam, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser) VALUES
					(@G0Pk01, @GcPk01, 'CPG01', 'TST00001000', 'BRD', GetUtcDate(), 'E', GetUtcDate(), 'E'),
					(@G0Pk02, @GcPk02, 'CPG02', 'TST00001001', 'BRD', GetUtcDate(), 'E', GetUtcDate(), 'E'),
					(@G0Pk03, @GcPk01, 'CPG03', 'TST00001002', 'BRD', GetUtcDate(), 'E', GetUtcDate(), 'E'),
					(@G0Pk04, @GcPk01, 'Exam01', 'TST00001003', 'LCT', GetUtcDate(), 'E', GetUtcDate(), 'E');
				INSERT dbo.GlbCompanyCampaignItem (G8_PK, G8_G0, G8_DeliveryMethod, G8_RecipientTableCode, G8_RecipientID, G8_SystemCreateTimeUtc, G8_SystemCreateUser, G8_SystemLastEditTimeUtc, G8_SystemLastEditUser) VALUES
					(newid(), @G0Pk01, 'EML', 'GS', newid(), '2014-09-05', 'US1', GetUtcDate(), 'E'),
					(newid(), @G0Pk01, 'EML', 'GS', newid(), '2014-09-15', 'US2', GetUtcDate(), 'E'),
					(newid(), @G0Pk02, 'EML', 'GS', newid(), '2013-09-01', 'US3', GetUtcDate(), 'E'),
					(newid(), @G0Pk02, 'EML', 'GS', newid(), '2014-09-30', 'US4', GetUtcDate(), 'E'),
					(newid(), @G0Pk02, 'EML', 'GS', newid(), '2014-09-30', 'US5', GetUtcDate(), 'E'),
					(newid(), @G0Pk03, 'EML', 'GS', newid(), '2014-10-31', 'US6', GetUtcDate(), 'E'),
					(newid(), @G0Pk03, 'TGL', 'GS', newid(), '2014-10-31', 'US6', GetUtcDate(), 'E'),
					(newid(), @G0Pk04, 'LCT', 'GS', newid(), '2017-07-01', 'US7', GetUtcDate(), 'E');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 4, transactions.Count());

			var transaction1 = FindRowByRef2(transactions, "US1");
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", null, transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 9, 5), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US1", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference01", "CPG01", transaction1.Reference1);

			var transaction2 = FindRowByRef2(transactions, "US2");
			AssertEquals("[T2] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", null, transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2014, 9, 15), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "US2", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference01", "CPG01", transaction2.Reference1);

			var transaction3 = FindRowByRef2(transactions, "US4");
			AssertEquals("[T3] CompanyCode", "EDI", transaction3.GetCompanyCode());
			AssertEquals("[T3] BranchCode", null, transaction3.GetBranchCode());
			AssertEquals("[T3] TransactionDateUtc", new DateTime(2014, 9, 30), transaction3.ServiceOccuredUTC);
			AssertEquals("[T3] UserCode", "US4", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);
			AssertEquals("[T3] TransactionReference01", "CPG02", transaction3.Reference1);

			var transaction4 = FindRowByRef2(transactions, "US5");
			AssertEquals("[T4] CompanyCode", "EDI", transaction4.GetCompanyCode());
			AssertEquals("[T4] BranchCode", null, transaction4.GetBranchCode());
			AssertEquals("[T4] TransactionDateUtc", new DateTime(2014, 9, 30), Convert.ToDateTime(transaction4.ServiceOccuredUTC));
			AssertEquals("[T4] UserCode", "US5", transaction4.ClientStaffCode);
			AssertEquals("[T4] ItemCount", 1, Convert.ToInt32(transaction4.BillableCount));
			AssertEquals("[T4] TransactionReference01", "CPG02", transaction4.Reference1);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 9);
			}
		}
	}
}
