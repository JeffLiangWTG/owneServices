using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(WarehouseTransitReceiptIntoDepot))]
	sealed class WarehouseTransitReceiptIntoDepotTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPk1 UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPk);
				DECLARE @GbPk2 UNIQUEIDENTIFIER = NewID();
				DECLARE @OaPk UNIQUEIDENTIFIER = (SELECT TOP (1) OA_PK FROM dbo.OrgAddress);
				DECLARE @WwPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @WwPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @WrcPk11 UNIQUEIDENTIFIER = newid();
				DECLARE @WrcPk12 UNIQUEIDENTIFIER = newid();
				DECLARE @WrcPk21 UNIQUEIDENTIFIER = newid();

				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES
					(@GbPk2, 'ABC', @GcPk)

				INSERT dbo.WhsWarehouse (WW_PK, WW_WarehouseCode, WW_GB_RelatedCompanyBranch, WW_OA_WarehouseAddress, WW_IsVirtualWarehouse, WW_WLT_DefaultLocationType, WW_SystemCreateTimeUtc, WW_SystemCreateUser, WW_SystemLastEditTimeUtc, WW_SystemLastEditUser) VALUES
					(@WwPk01, 'WH1', @GbPk1, @OaPk, 1, '16C9FD62-730A-42ED-A20E-699606FFF360', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@WwPk02, 'WH2', @GbPk2, @OaPk, 1, '16C9FD62-730A-42ED-A20E-699606FFF360', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.WhsItemReceiveConsignment (WRC_PK, WRC_WW_IntendedWarehouse, WRC_ConsignmentID, WRC_SystemCreateTimeUtc, WRC_SystemLastEditTimeUtc, WRC_SystemCreateUser, WRC_SystemLastEditUser, WRC_JobID) VALUES
					(@WrcPk11, @WwPk01, 'WRC11', '2014-12-11', '2014-12-11', 'US1', 'US1', 'RC011'),
					(@WrcPk12, @WwPk01, 'WRC12', '2014-11-12', '2014-11-12', 'US2', 'US2', 'RC012'),
					(@WrcPk21, @WwPk02, 'WRC21', '2014-12-21', '2014-12-21', 'US3', 'US3', 'RC021');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByRef2(transactions, "WH1");
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode().Trim());
			AssertEquals("[T1] BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 12, 11), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US1", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference01", "RC011", transaction1.Reference1);

			var transaction2 = FindRowByRef2(transactions, "WH2");
			AssertEquals("[T2] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", "ABC", transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2014, 12, 21), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "US3", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference01", "RC021", transaction2.Reference1);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 12);
			}
		}
	}
}
