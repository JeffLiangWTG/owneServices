using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(Warehouse3plBillOfMaterials))]
	sealed class Warehouse3plBillOfMaterialsTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			var company = GlbCompany.ShallowLoadFromDB(TestConnection, c => c.GC_Code == "DEM").First();
			var branch = GlbBranch.ShallowLoadFromDB(TestConnection, b => b.GB_GC == company.PK).First();
			var orgAddress = OrgAddress.ShallowLoadFromDB(TestConnection).First();
			var orgHeader = OrgHeader.ShallowLoadFromDB(TestConnection).First();

			var whs1 = new WhsWarehouse("WH1", branch.PK, orgAddress.PK) { WW_WarehouseName = "Warehouse 1", WW_IsVirtualWarehouse = false }.WithDockDoor(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch.PK, orgAddress.PK) { WW_WarehouseName = "Warehouse 2", WW_IsVirtualWarehouse = true }.WithDockDoor(TestConnection);

			var workOrder1 = new WhsDocket(orgHeader.PK, whs1.PK, "WOR", "ASS", "ENT", "WD01") { WD_SystemCreateTimeUtc = new DateTime(2012, 07, 01), WD_SystemCreateUser = "US1" }.InsertAndReturnObject(TestConnection);
			var workOrder2 = new WhsDocket(orgHeader.PK, whs1.PK, "WOR", "DIS", "ENT", "WD02") { WD_SystemCreateTimeUtc = new DateTime(2012, 08, 02), WD_SystemCreateUser = "US2" }.InsertAndReturnObject(TestConnection);
			var workOrder3 = new WhsDocket(orgHeader.PK, whs1.PK, "WOR", "DIS", "ENT", "WD03") { WD_SystemCreateTimeUtc = new DateTime(2012, 08, 03), WD_SystemCreateUser = "US3" }.InsertAndReturnObject(TestConnection);
			var receive1 = new WhsDocket(orgHeader.PK, whs1.PK, "INW", "CUS", "ENT", "WD04") { WD_SystemCreateTimeUtc = new DateTime(2012, 08, 04), WD_SystemCreateUser = "US4" }.InsertAndReturnObject(TestConnection);

			var workOrder4 = new WhsDocket(orgHeader.PK, whs2.PK, "WOR", "ASS", "ENT", "WD05") { WD_SystemCreateTimeUtc = new DateTime(2012, 07, 01), WD_SystemCreateUser = "US5" }.InsertAndReturnObject(TestConnection);
			var workOrder5 = new WhsDocket(orgHeader.PK, whs2.PK, "WOR", "DIS", "ENT", "WD06") { WD_SystemCreateTimeUtc = new DateTime(2012, 08, 02), WD_SystemCreateUser = "US6" }.InsertAndReturnObject(TestConnection);
			var workOrder6 = new WhsDocket(orgHeader.PK, whs2.PK, "WOR", "DIS", "ENT", "WD07") { WD_SystemCreateTimeUtc = new DateTime(2012, 08, 03), WD_SystemCreateUser = "US7" }.InsertAndReturnObject(TestConnection);
			var receive2 = new WhsDocket(orgHeader.PK, whs2.PK, "INW", "CUS", "ENT", "WD08") { WD_SystemCreateTimeUtc = new DateTime(2012, 08, 04), WD_SystemCreateUser = "US8" }.InsertAndReturnObject(TestConnection);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 4, transactions.Count());

			var transaction1 = FindRowByRef2(transactions, "WD02");
			AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 8, 2), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "US2", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "WH1 - N", transaction1.Reference1);
			AssertEquals("TransactionReference02", "WD02", transaction1.Reference2);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction1.AdditionalRefs);

			var transaction2 = FindRowByRef2(transactions, "WD03");
			AssertEquals("CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 8, 3), transaction2.ServiceOccuredUTC);
			AssertEquals("UserCode", "US3", transaction2.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction2.BillableCount);
			AssertEquals("TransactionReference01", "WH1 - N", transaction2.Reference1);
			AssertEquals("TransactionReference02", "WD03", transaction2.Reference2);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction2.AdditionalRefs);

			var transaction3 = FindRowByRef2(transactions, "WD06");
			AssertEquals("CompanyCode", "DEM", transaction3.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction3.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 8, 2), transaction3.ServiceOccuredUTC);
			AssertEquals("UserCode", "US6", transaction3.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction3.BillableCount);
			AssertEquals("TransactionReference01", "WH2 - Y", transaction3.Reference1);
			AssertEquals("TransactionReference02", "WD06", transaction3.Reference2);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 2\"}"
				, transaction3.AdditionalRefs);

			var transaction4 = FindRowByRef2(transactions, "WD07");
			AssertEquals("CompanyCode", "DEM", transaction4.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction4.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 8, 3), transaction4.ServiceOccuredUTC);
			AssertEquals("UserCode", "US7", transaction4.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction4.BillableCount);
			AssertEquals("TransactionReference01", "WH2 - Y", transaction4.Reference1);
			AssertEquals("TransactionReference02", "WD07", transaction4.Reference2);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 2\"}"
				, transaction4.AdditionalRefs);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2012, 8);
			}
		}
	}
}
