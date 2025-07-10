using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(Warehouse3plAdjustments))]
	sealed class Warehouse3plAdjustmentsTest : RefStlScriptWithDefaultsTest
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

			var adjustment1 = new WhsDocket(orgHeader.PK, whs1.PK, "ADJ", "CUS", "ENT", "WD01") { WD_SystemCreateTimeUtc = new DateTime(2012, 07, 01) }.InsertAndReturnObject(TestConnection);
			var adjustment2 = new WhsDocket(orgHeader.PK, whs1.PK, "ADJ", "NEA", "ENT", "WD02") { WD_SystemCreateTimeUtc = new DateTime(2012, 08, 02) }.InsertAndReturnObject(TestConnection);
			var adjustment3 = new WhsDocket(orgHeader.PK, whs1.PK, "ADJ", "NEA", "ENT", "WD03") { WD_SystemCreateTimeUtc = new DateTime(2012, 07, 03) }.InsertAndReturnObject(TestConnection);
			var adjustment4 = new WhsDocket(orgHeader.PK, whs1.PK, "INW", "REC", "ENT", "WD04") { WD_SystemCreateTimeUtc = new DateTime(2012, 07, 04) }.InsertAndReturnObject(TestConnection);

			var adjustment5 = new WhsDocket(orgHeader.PK, whs2.PK, "ADJ", "CUS", "ENT", "WD05") { WD_SystemCreateTimeUtc = new DateTime(2012, 07, 01) }.InsertAndReturnObject(TestConnection);
			var adjustment6 = new WhsDocket(orgHeader.PK, whs2.PK, "ADJ", "NEA", "ENT", "WD06") { WD_SystemCreateTimeUtc = new DateTime(2012, 08, 02) }.InsertAndReturnObject(TestConnection);
			var adjustment7 = new WhsDocket(orgHeader.PK, whs2.PK, "ADJ", "NEA", "ENT", "WD07") { WD_SystemCreateTimeUtc = new DateTime(2012, 07, 03) }.InsertAndReturnObject(TestConnection);
			var adjustment8 = new WhsDocket(orgHeader.PK, whs2.PK, "INW", "REC", "ENT", "WD08") { WD_SystemCreateTimeUtc = new DateTime(2012, 07, 04) }.InsertAndReturnObject(TestConnection);
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
			AssertEquals("TransactionReference02", "WD03", transaction1.Reference2);
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
			AssertEquals("TransactionReference02", "WD07", transaction2.Reference2);
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
