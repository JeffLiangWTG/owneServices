using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(Warehouse3plVasOrder))]
	sealed class Warehouse3plVasOrdersTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			var company = GlbCompany.ShallowLoadFromDB(TestConnection, c => c.GC_Code == "DEM").First();
			var branch = GlbBranch.ShallowLoadFromDB(TestConnection, b => b.GB_GC == company.PK).First();
			var orgAddress = OrgAddress.ShallowLoadFromDB(TestConnection).First();
			var orgHeader = OrgHeader.ShallowLoadFromDB(TestConnection).First();

			var whs1 = new WhsWarehouse("WH1", branch.PK, orgAddress.PK) { WW_WarehouseName = "Warehouse 1", WW_IsVirtualWarehouse = false }.WithDockDoor(TestConnection);
			var serviceArea1 = new WhsArea(whs1.PK, "SV1").InsertAndReturnObject(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch.PK, orgAddress.PK) { WW_WarehouseName = "Warehouse 2", WW_IsVirtualWarehouse = true }.WithDockDoor(TestConnection);
			var serviceArea2 = new WhsArea(whs2.PK, "SV2").InsertAndReturnObject(TestConnection);

			var adjustment1 = new WhsVASOrder(orgHeader, serviceArea1, "WV01", "REF-01") { WVO_SystemCreateTimeUtc = new DateTime(2012, 06, 01) }.InsertAndReturnObject(TestConnection);
			var adjustment2 = new WhsVASOrder(orgHeader, serviceArea1, "WV02", "REF-02") { WVO_SystemCreateTimeUtc = new DateTime(2012, 08, 02) }.InsertAndReturnObject(TestConnection);
			var adjustment3 = new WhsVASOrder(orgHeader, serviceArea1, "WV03", "REF-03") { WVO_SystemCreateTimeUtc = new DateTime(2012, 07, 03) }.InsertAndReturnObject(TestConnection);

			var adjustment4 = new WhsVASOrder(orgHeader, serviceArea2, "WV04", "REF-04") { WVO_SystemCreateTimeUtc = new DateTime(2012, 06, 01) }.InsertAndReturnObject(TestConnection);
			var adjustment5 = new WhsVASOrder(orgHeader, serviceArea2, "WV05", "REF-05") { WVO_SystemCreateTimeUtc = new DateTime(2012, 08, 02) }.InsertAndReturnObject(TestConnection);
			var adjustment6 = new WhsVASOrder(orgHeader, serviceArea2, "WV06", "REF-06") { WVO_SystemCreateTimeUtc = new DateTime(2012, 07, 03) }.InsertAndReturnObject(TestConnection);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "WH1 - N");
			AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 7, 3), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "A", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "WH1 - N", transaction1.Reference1);
			AssertEquals("TransactionReference02", "WV03", transaction1.Reference2);
			AssertEquals("TransactionReference03", "REF-03", transaction1.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction1.AdditionalRefs);

			var transaction2 = FindRowByRef1(transactions, "WH2 - Y");
			AssertEquals("CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 7, 3), transaction2.ServiceOccuredUTC);
			AssertEquals("UserCode", "A", transaction2.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction2.BillableCount);
			AssertEquals("TransactionReference01", "WH2 - Y", transaction2.Reference1);
			AssertEquals("TransactionReference02", "WV06", transaction2.Reference2);
			AssertEquals("TransactionReference03", "REF-06", transaction2.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 2\"}"
				, transaction2.AdditionalRefs);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2012, 7);
			}
		}
	}
}
