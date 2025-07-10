using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(OtherPartiesForwarderSeaCargoImport))]
	sealed class OtherPartiesForwarderSeaCargoImportTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @CbPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @CbPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @CbPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @CbPk04 UNIQUEIDENTIFIER = newid();
				DECLARE @CbPk05 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkSg UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'SIN');
				DECLARE @GcPkAu UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPkSg UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkSg);
				DECLARE @GbPkAu UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkAu);

				INSERT dbo.CusScaOceanBill (CB_PK, CB_GB, CB_ApplicationCode, CB_OceanBill, CB_SystemCreateTimeUtc, CB_SystemCreateUser, CB_MasterHouseBill, CB_SystemLastEditTimeUtc, CB_SystemLastEditUser) VALUES
					(@CbPk01, @GbPkAu, 'XXX', 'CB01', '2012-03-21', 'US1', 'MH01', GetUtcDate(), '~BP'),
					(@CbPk02, @GbPkAu, 'CMR', 'CB02', '2012-03-22', 'US2', 'MH02', GetUtcDate(), '~BP'),
					(@CbPk03, @GbPkSg, 'CMR', 'CB04', '2012-03-23', 'US3', 'MH03', GetUtcDate(), '~BP'),
					(@CbPk04, @GbPkAu, 'CMR', 'CB05', '2012-04-04', 'US4', 'MH04', GetUtcDate(), '~BP'),
					(@CbPk05, @GbPkAu, 'CMR', 'CB06', '2012-03-25', 'US5', 'MH05', GetUtcDate(), '~BP');
				INSERT dbo.CusScaHouse (CA_PK, CA_CB, CA_HouseBill, CA_IsHVLV, CA_SystemCreateTimeUtc, CA_SystemCreateUser, CA_SystemLastEditTimeUtc, CA_SystemLastEditUser) VALUES
					(newid(), @CbPk01, 'CA11', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), @CbPk02, 'CA21', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), @CbPk02, 'CA22', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), @CbPk03, 'CA31', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), @CbPk03, 'CA32', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), @CbPk04, 'CA41', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), @CbPk05, 'CA51', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByRef2(transactions, "CA21");
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2012, 3, 22), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US2", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference01", "OBL: CB02", transaction1.Reference1);
			AssertEquals("[T1] TransactionReference03", null, transaction1.Reference3);
			AssertEquals("[T1] TransactionReference04", null, transaction1.Reference4);

			var transaction2 = FindRowByRef2(transactions, "CA22");
			AssertEquals("[T2] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2012, 3, 22), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "US2", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference01", "OBL: CB02", transaction2.Reference1);
			AssertEquals("[T2] TransactionReference03", null, transaction2.Reference3);
			AssertEquals("[T2] TransactionReference04", null, transaction2.Reference4);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2012, 3);
			}
		}
	}
}
