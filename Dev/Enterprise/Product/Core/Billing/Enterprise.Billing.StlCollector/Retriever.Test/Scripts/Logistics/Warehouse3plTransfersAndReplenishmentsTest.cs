using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(Warehouse3plTransfersAndReplenishments))]
	sealed class Warehouse3plTransfersAndReplenishmentsTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			var company = GlbCompany.ShallowLoadFromDB(TestConnection, c => c.GC_Code == "DEM").First();
			var branch = GlbBranch.ShallowLoadFromDB(TestConnection, b => b.GB_GC == company.PK).First();
			var orgAddress = OrgAddress.ShallowLoadFromDB(TestConnection).First();
			var orgHeader = OrgHeader.ShallowLoadFromDB(TestConnection).First();

			var whs1 = new WhsWarehouse("WH1", branch.PK, orgAddress.PK) { WW_WarehouseName = "Warehouse 1", WW_IsVirtualWarehouse = false }.WithDockDoor(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch.PK, orgAddress.PK) { WW_WarehouseName = "Warehouse 2", WW_IsVirtualWarehouse = true }.WithDockDoor(TestConnection);

			var transfer1 = new WhsDocket(orgHeader.PK, whs1.PK, "TFR", "TFR", "ENT", "WD01") { WD_SystemCreateTimeUtc = new DateTime(2012, 11, 11), WD_SystemCreateUser = "US1" }.InsertAndReturnObject(TestConnection);
			var transfer2 = new WhsDocket(orgHeader.PK, whs1.PK, "TFR", "TFR", "ENT", "WD02") { WD_SystemCreateTimeUtc = new DateTime(2012, 10, 12), WD_SystemCreateUser = "US2" }.InsertAndReturnObject(TestConnection);
			var transfer3 = new WhsDocket(orgHeader.PK, whs1.PK, "TFR", "TFR", "ENT", "WD03") { WD_SystemCreateTimeUtc = new DateTime(2012, 11, 14), WD_SystemCreateUser = "US3" }.InsertAndReturnObject(TestConnection);
			var receive1 = new WhsDocket(orgHeader.PK, whs1.PK, "INW", "REC", "ENT", "WD04") { WD_SystemCreateTimeUtc = new DateTime(2012, 11, 13), WD_SystemCreateUser = "US4" }.InsertAndReturnObject(TestConnection);
			var transfer4 = new WhsDocket(orgHeader.PK, whs2.PK, "TFR", "TFR", "ENT", "WD05") { WD_SystemCreateTimeUtc = new DateTime(2012, 11, 11), WD_SystemCreateUser = "US5" }.InsertAndReturnObject(TestConnection);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());

			var transaction1 = FindRowByRef2(transactions, "WD01");
			AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 11, 11), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "US1", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "WH1 - N", transaction1.Reference1);
			AssertEquals("TransactionReference02", "WD01", transaction1.Reference2);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction1.AdditionalRefs);

			var transaction2 = FindRowByRef2(transactions, "WD03");
			AssertEquals("CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 11, 14), transaction2.ServiceOccuredUTC);
			AssertEquals("UserCode", "US3", transaction2.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction2.BillableCount);
			AssertEquals("TransactionReference01", "WH1 - N", transaction2.Reference1);
			AssertEquals("TransactionReference02", "WD03", transaction2.Reference2);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction2.AdditionalRefs);

			var transaction3 = FindRowByRef2(transactions, "WD05");
			AssertEquals("CompanyCode", "DEM", transaction3.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction3.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 11, 11), transaction3.ServiceOccuredUTC);
			AssertEquals("UserCode", "US5", transaction3.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction3.BillableCount);
			AssertEquals("TransactionReference01", "WH2 - Y", transaction3.Reference1);
			AssertEquals("TransactionReference02", "WD05", transaction3.Reference2);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 2\"}"
				, transaction3.AdditionalRefs);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2012, 11);
			}
		}
	}
}
