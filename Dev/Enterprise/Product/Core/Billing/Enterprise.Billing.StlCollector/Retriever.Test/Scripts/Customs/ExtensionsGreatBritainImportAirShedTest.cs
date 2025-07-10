using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(ExtensionsGreatBritainImportAirShed))]
	sealed class ExtensionsGreatBritainImportAirShedTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @CmPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @CmPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @CmPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkSg UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'SIN');
				DECLARE @GcPkGb UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPkSg UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkSg);
				DECLARE @GbPkGb UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkGb);
				UPDATE dbo.GlbCompany SET GC_RN_NKCountryCode = 'GB', GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_PK = @GcPkGb;
				INSERT dbo.CusMawb (CM_PK, CM_GB, CM_ApplicationCode, CM_IsCtoMawb, CM_MAWB) VALUES
					(@CmPk01, @GbPkGb, 'CUK', 0, 'CM01'),
					(@CmPk02, @GbPkGb, 'CUK', 0, 'CM02'),
					(@CmPk03, @GbPkGb, 'CUK', 0, 'CM03'),
					(newid(), @GbPkGb, 'CUK', 0, 'CM04'),
					(newid(), @GbPkGb, 'CUK', 0, 'CM05'),
					(newid(), @GbPkGb, 'CUK', 1, 'CM06'),
					(newid(), @GbPkSg, 'CUK', 0, 'CM07');
				INSERT dbo.CusHawb (CS_PK, CS_CM, CS_IsMasterHouse, CS_FolioReference, CS_HAWB, CS_SystemCreateTimeUtc, CS_SystemCreateUser) VALUES
					(newid(), @CmPk01, 1, 'CUKAIR9800', 'CS11', '2014-01-11', 'U11'),
					(newid(), @CmPk01, 1, 'CUKAIR9800', 'CS12', '2014-02-12', 'U12'),
					(newid(), @CmPk02, 0, 'CUKAIR9800', 'CS21', '2014-01-02', 'U21'),
					(newid(), @CmPk02, 0, 'CUKAIR9800', 'CS22', '2014-01-02', 'U22'),
					(newid(), @CmPk02, 0, 'CUKAIRXXXX', 'CS23', '2014-01-02', 'U23'),
					(newid(), @CmPk03, 1, 'CUKAIR9800', 'CS31', '2014-01-30', 'U31'),
					(newid(), @CmPk03, 0, 'CUKAIRXXXX', 'CS32', '2014-01-30', 'U32');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());

			var transaction1 = transactions.Single(t => t.Reference1 == "MAWB: CM02" && t.Reference2 == "HAWB: CS21");
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 1, 2), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "U21", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);

			var transaction2 = transactions.Single(t => t.Reference1 == "MAWB: CM02" && t.Reference2 == "HAWB: CS22");
			AssertEquals("[T2] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2014, 1, 2), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "U22", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);

			var transaction3 = transactions.Single(t => t.Reference1 == "MAWB: CM01" && t.Reference2 == "HAWB: CS11");
			AssertEquals("[T3] CompanyCode", "DEM", transaction3.GetCompanyCode());
			AssertEquals("[T3] BranchCode", "DEM", transaction3.GetBranchCode());
			AssertEquals("[T3] TransactionDateUtc", new DateTime(2014, 1, 11), transaction3.ServiceOccuredUTC);
			AssertEquals("[T3] UserCode", "U11", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 1);
			}
		}
	}
}
