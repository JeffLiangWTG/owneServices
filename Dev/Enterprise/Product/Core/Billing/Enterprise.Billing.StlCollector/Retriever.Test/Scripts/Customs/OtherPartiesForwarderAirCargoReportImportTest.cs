using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(OtherPartiesForwarderAirCargoReportImport))]
	sealed class OtherPartiesForwarderAirCargoReportImportTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @CmPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @CmPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @CmPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @CmPk04 UNIQUEIDENTIFIER = newid();
				DECLARE @CmPk05 UNIQUEIDENTIFIER = newid();
				DECLARE @jkPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @jkPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @jkPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @jsPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @jsPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkSg UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'SIN');
				DECLARE @GcPkAu UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPkSg UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkSg);
				DECLARE @GbPkAu UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkAu);
				INSERT dbo.CusMawb (CM_PK, CM_GB, CM_ApplicationCode, CM_IsCtoMawb, CM_MAWB, CM_MasterHouseBill) VALUES
					(@CmPk01, @GbPkAu, 'CMR', 1, 'CM01', 'MS01'),
					(@CmPk02, @GbPkAu, 'XXX', 0, 'CM02', 'MS02'),
					(@CmPk03, @GbPkAu, 'CMR', 0, 'CM03', 'MS03'),
					(@CmPk04, @GbPkSg, 'CMR', 0, 'CM04', 'MS04'),
					(@CmPk05, @GbPkAu, 'CMR', 0, 'CM05', 'MS05'),
					(newid(), @GbPkAu, 'CMR', 0, 'CM06', 'MS06');
				INSERT dbo.CusHawb (CS_PK, CS_CM, CS_HAWB, CS_SystemCreateTimeUtc, CS_SystemCreateUser) VALUES
					(newid(), @CmPk01, 'CS11', '2013-05-11', 'US1'),
					(newid(), @CmPk02, 'CS21', '2013-05-12', 'US2'),
					(newid(), @CmPk03, 'CS31', '2013-05-13', 'US3'),
					(newid(), @CmPk03, 'CS32', '2013-05-13', 'US3'),
					(newid(), @CmPk04, 'CS41', '2013-05-14', 'US4'),
					(newid(), @CmPk05, 'CS51', '2013-04-15', 'US5'),
					(newid(), @CmPk05, 'CS52', '2013-05-13', 'US6');
				INSERT dbo.JobConsol (JK_PK, JK_MasterBillNum, JK_UniqueConsignRef) VALUES
					(@jkPk01, 'CM05', 'AAA'),
					(@jkPk02, 'CM03', 'BBB'),
					(@jkPk03, 'CM03', 'CCC');
				INSERT dbo.JobShipment (JS_PK, JS_ShipmentType, JS_HouseBill, JS_UniqueConsignRef) VALUES
					(@jsPk01, 'HLS', 'MS05', 'JS001'),			
					(@jsPk02, 'STD', 'MS03', 'JS002');
				INSERT dbo.JobConShipLink (JN_PK, JN_JK, JN_JS) VALUES
					(newid(), @jkPK01, @jsPk01),
					(newid(), @jkPK02, @jsPk02),
					(newid(), @jkPK03, @jsPk02);
";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());

			var transaction1 = transactions.Single(t => t.Reference1 == "MAWB: CM03" && t.Reference2 == "HAWB: CS31");
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2013, 5, 13), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US3", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference03", "MS03", transaction1.Reference3);
			AssertEquals("[T1] TransactionReference04", null, transaction1.Reference4);

			var transaction2 = transactions.Single(t => t.Reference1 == "MAWB: CM03" && t.Reference2 == "HAWB: CS32");
			AssertEquals("[T2] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2013, 5, 13), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "US3", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference03", "MS03", transaction2.Reference3);
			AssertEquals("[T2] TransactionReference04", null, transaction2.Reference4);

			var transaction3 = transactions.Single(t => t.Reference1 == "MAWB: CM05" && t.Reference2 == "HAWB: CS52");
			AssertEquals("[T3] CompanyCode", "DEM", transaction3.GetCompanyCode());
			AssertEquals("[T3] BranchCode", "DEM", transaction3.GetBranchCode());
			AssertEquals("[T3] TransactionDateUtc", new DateTime(2013, 5, 13), transaction3.ServiceOccuredUTC);
			AssertEquals("[T3] UserCode", "US6", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);
			AssertEquals("[T3] TransactionReference03", "MS05", transaction3.Reference3);
			AssertEquals("[T3] TransactionReference04", null, transaction3.Reference4);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2013, 5);
			}
		}
	}
}
