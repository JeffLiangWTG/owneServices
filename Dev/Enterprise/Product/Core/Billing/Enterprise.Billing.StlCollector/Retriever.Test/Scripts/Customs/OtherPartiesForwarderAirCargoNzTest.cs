using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(OtherPartiesForwarderAirCargoNz))]
	sealed class OtherPartiesForwarderAirCargoNzTest : RefStlScriptWithDefaultsTest
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
				DECLARE @GcPkNz UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPkSg UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkSg);
				DECLARE @GbPkNz UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkNz);
				UPDATE dbo.GlbCompany SET GC_RN_NKCountryCode = 'NZ', GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_PK = @GcPkNz;
				INSERT dbo.CusMawb (CM_PK, CM_GB, CM_ApplicationCode, CM_MAWB) VALUES
					(@CmPk01, @GbPkNz, 'NZE', 'CM01'),
					(@CmPk02, @GbPkNz, 'XXX', 'CM02'),
					(@CmPk03, @GbPkSg, 'NZE', 'CM03'),
					(@CmPk04, @GbPkNz, 'NZE', 'CM04'),
					(@CmPk05, @GbPkNz, 'TSW', 'CM06');
				INSERT dbo.CusHawb (CS_PK, CS_CM, CS_HAWB, CS_SystemCreateTimeUtc, CS_SystemCreateUser, CS_IsHVLV) VALUES
					(newid(), @CmPk01, 'CS11', '2014-04-21', 'US1', 0),
					(newid(), @CmPk02, 'CS21', '2014-04-22', 'US2', 0),
					(newid(), @CmPk03, 'CS31', '2014-04-23', 'US3', 0),
					(newid(), @CmPk04, 'CS41', '2014-05-24', 'US4', 0),
					(newid(), @CmPk05, 'CS51', '2014-04-25', 'US5', 0),
					(newid(), @CmPk05, 'CS52', '2014-04-25', 'US5', 1);";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "MAWB: CM01");
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 4, 21), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US1", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference02", "HAWB: CS11", transaction1.Reference2);

			var transaction2 = FindRowByRef1(transactions, "MAWB: CM06");
			AssertEquals("[T1] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 4, 25), transaction2.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US5", transaction2.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T1] TransactionReference02", "HAWB: CS51", transaction2.Reference2);
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
