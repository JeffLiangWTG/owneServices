using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(ExtensionsGreatBritainImportAirAgent))]
	sealed class ExtensionsGreatBritainImportAirAgentTest : RefStlScriptWithDefaultsTest
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
					(newid(), @CmPk01, 1, 'CUKFFW9800', 'CS11', '2013-09-11', 'U11'),
					(newid(), @CmPk01, 1, 'CUKFFW9800', 'CS12', '2013-10-12', 'U12'),
					(newid(), @CmPk02, 0, 'CUKFFW9800', 'CS21', '2013-09-02', 'U21'),
					(newid(), @CmPk02, 0, 'CUKFFWXXXX', 'CS22', '2013-09-22', 'U22'),
					(newid(), @CmPk02, 0, 'CUKFFW9800', 'CS23', '2013-09-23', 'U23'),
					(newid(), @CmPk03, 1, 'CUKFFW9800', 'CS31', '2013-09-30', 'U31'),
					(newid(), @CmPk03, 0, 'CUKFFWXXXX', 'CS32', '2013-08-30', 'U32');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());

			var transaction1 = FindRowByOccured(transactions, new DateTime(2013, 9, 2));
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("[T1] UserCode", "U21", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference01", "MAWB: CM02", transaction1.Reference1);
			AssertEquals("[T1] TransactionReference02", "HAWB: CS21", transaction1.Reference2);

			var transaction2 = FindRowByOccured(transactions, new DateTime(2013, 9, 11));
			AssertEquals("[T2] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("[T2] UserCode", "U11", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference01", "MAWB: CM01", transaction2.Reference1);
			AssertEquals("[T2] TransactionReference02", "HAWB: CS11", transaction2.Reference2);

			var transaction3 = FindRowByOccured(transactions, new DateTime(2013, 9, 23));
			AssertEquals("[T3] CompanyCode", "DEM", transaction3.GetCompanyCode());
			AssertEquals("[T3] BranchCode", "DEM", transaction3.GetBranchCode());
			AssertEquals("[T3] UserCode", "U23", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);
			AssertEquals("[T3] TransactionReference01", "MAWB: CM02", transaction3.Reference1);
			AssertEquals("[T3] TransactionReference02", "HAWB: CS23", transaction3.Reference2);
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
