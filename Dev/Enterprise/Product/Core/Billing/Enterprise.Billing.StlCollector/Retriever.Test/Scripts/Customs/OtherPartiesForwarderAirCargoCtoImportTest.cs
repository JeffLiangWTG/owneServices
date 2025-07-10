using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(OtherPartiesForwarderAirCargoCtoImport))]
	sealed class OtherPartiesForwarderAirCargoCtoImportTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @CmPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @CmPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @CmPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @CmPk04 UNIQUEIDENTIFIER = newid();
				DECLARE @CmPk05 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkSg UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'SIN');
				DECLARE @GcPkAu UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPkSg UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkSg);
				DECLARE @GbPkAu UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkAu);
				INSERT dbo.CusMawb (CM_PK, CM_GB, CM_ApplicationCode, CM_IsCtoMawb, CM_MAWB) VALUES
					(@CmPk01, @GbPkAu, 'CMR', 0, 'CM01'),
					(@CmPk02, @GbPkAu, 'XXX', 1, 'CM02'),
					(@CmPk03, @GbPkAu, 'CMR', 1, 'CM03'),
					(@CmPk04, @GbPkSg, 'CMR', 1, 'CM04'),
					(@CmPk05, @GbPkAu, 'CMR', 1, 'CM05'),
					(newid(), @GbPkAu, 'CMR', 1, 'CM06');
				INSERT dbo.CusHawb (CS_PK, CS_CM, CS_HAWB, CS_SystemCreateTimeUtc, CS_SystemCreateUser) VALUES
					(newid(), @CmPk01, 'CS11', '2013-09-21', 'U11'),
					(newid(), @CmPk02, 'CS21', '2013-09-22', 'U21'),
					(newid(), @CmPk02, 'CS22', '2013-09-22', 'U22'),
					(newid(), @CmPk03, 'CS31', '2013-09-23', 'U31'),
					(newid(), @CmPk03, 'CS32', '2013-09-23', 'U32'),
					(newid(), @CmPk04, 'CS41', '2013-09-24', 'U41'),
					(newid(), @CmPk05, 'CS51', '2013-10-01', 'U51');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = transactions.Single(t => t.Reference1 == "MAWB: CM03" && t.Reference2 == "HAWB: CS31");
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2013, 9, 23), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "U31", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);

			var transaction2 = transactions.Single(t => t.Reference1 == "MAWB: CM03" && t.Reference2 == "HAWB: CS32");
			AssertEquals("[T2] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2013, 9, 23), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "U32", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2013, 9);
			}
		}
	}
}
