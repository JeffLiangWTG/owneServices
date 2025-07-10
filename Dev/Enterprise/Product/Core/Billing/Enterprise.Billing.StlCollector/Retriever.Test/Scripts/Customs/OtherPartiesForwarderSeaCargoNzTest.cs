using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(OtherPartiesForwarderSeaCargoNz))]
	sealed class OtherPartiesForwarderSeaCargoNzTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @CbPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @CbPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @CbPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @CbPk04 UNIQUEIDENTIFIER = newid();
				DECLARE @CbPk05 UNIQUEIDENTIFIER = newid();
				DECLARE @CbPk06 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkSg UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'SIN');
				DECLARE @GcPkNz UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPkSg UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkSg);
				DECLARE @GbPkNz UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkNz);
				UPDATE dbo.GlbCompany SET GC_RN_NKCountryCode = 'NZ', GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_PK = @GcPkNz;
				INSERT dbo.CusSCAOceanBill (CB_PK, CB_GB, CB_ApplicationCode, CB_OceanBill, CB_SystemCreateTimeUtc, CB_SystemCreateUser, CB_SystemLastEditTimeUtc, CB_SystemLastEditUser) VALUES
					(@CbPk01, @GbPkSg, 'TSW', 'OB01', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@CbPk02, @GbPkNz, 'NZE', 'OB02', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@CbPk03, @GbPkNz, 'XXX', 'OB03', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@CbPk04, @GbPkNz, 'NZE', 'OB04', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@CbPk05, @GbPkNz, 'TSW', 'OB05', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@CbPk06, @GbPkNz, 'TSW', 'OB06', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				INSERT dbo.CusSCAHouse (CA_PK, CA_CB, CA_HouseBill, CA_SystemCreateTimeUtc, CA_SystemCreateUser, CA_IsHVLV, CA_SystemLastEditTimeUtc, CA_SystemLastEditUser) VALUES
					(newid(), @CbPk01, 'HB11', '2014-04-10', 'US1', 1, GetUtcDate(), '~BP'), --SG
					(newid(), @CbPk02, 'HB21', '2014-04-10', 'US2', 1, GetUtcDate(), '~BP'), --NZE
					(newid(), @CbPk03, 'HB31', '2014-04-10', 'US3', 1, GetUtcDate(), '~BP'), --XXX
					(newid(), @CbPk04, 'HB41', '2014-05-10', 'US4', 1, GetUtcDate(), '~BP'), --May
					(newid(), @CbPk05, 'HB51', '2014-04-10', 'US5', 1, GetUtcDate(), '~BP'), --HVLV=0,
					(newid(), @CbPk05, 'HB52', '2014-04-10', 'US5', 0, GetUtcDate(), '~BP'),
					(newid(), @CbPk06, 'HB61', '2014-04-10', 'US6', 0, GetUtcDate(), '~BP');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "MOBL: OB05");
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 4, 10), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US5", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference02", "HOBL: HB52", transaction1.Reference2);

			var transaction2 = FindRowByRef1(transactions, "MOBL: OB06");
			AssertEquals("[T1] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 4, 10), transaction2.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US6", transaction2.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T1] TransactionReference02", "HOBL: HB61", transaction2.Reference2);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2014, 4);
	}
}
