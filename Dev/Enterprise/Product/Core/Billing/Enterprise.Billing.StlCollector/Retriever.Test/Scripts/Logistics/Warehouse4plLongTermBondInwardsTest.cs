using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(Warehouse4plLongTermBondInwards))]
	sealed class Warehouse4plLongTermBondInwardsTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			var company = GlbCompany.ShallowLoadFromDB(TestConnection, c => c.GC_Code == "DEM").First();
			var branch = GlbBranch.ShallowLoadFromDB(TestConnection, b => b.GB_GC == company.PK).First();
			var orgAddress = OrgAddress.ShallowLoadFromDB(TestConnection).First();
			var orgHeader = OrgHeader.ShallowLoadFromDB(TestConnection).First();

			var whs1 = new WhsWarehouse("WH1", branch.PK, orgAddress.PK)
			{
				WW_WarehouseName = "WW01",
				WW_IsVirtualWarehouse = false,
			}.WithDockDoor(TestConnection);

			var area = new WhsArea(whs1.PK, "AREA01") { WA_AreaType = "FRE" }.InsertAndReturnObject(TestConnection);
			var row = new WhsRow(whs1, "ROW01").InsertAndReturnObject(TestConnection);
			var location = new WhsLocation(row.PK, area.PK, area.PK).InsertAndReturnObject(TestConnection);

			var product = new OrgSupplierPart("P1").InsertAndReturnObject(TestConnection);

			var docket1 = PrepareReceive(orgHeader, whs1, 1).InsertAndReturnObject(TestConnection); // 2 docket lines; Expected 1 result
			var docket2 = PrepareReceive(orgHeader, whs1, 2).InsertAndReturnObject(TestConnection); // Expected 1 result

			var docket3 = PrepareReceive(orgHeader, whs1, 3);
			docket3.WD_SystemCreateTimeUtc = new DateTime(2012, 12, 19); // Out of Range for collector
			docket3.InsertAndReturnObject(TestConnection);

			var docket4 = PrepareReceive(orgHeader, whs1, 4).InsertAndReturnObject(TestConnection); // No Bonded Warehouse Attribute
			var docket5 = PrepareReceive(orgHeader, whs1, 5).InsertAndReturnObject(TestConnection); // No Docket Line

			var docket6 = PrepareReceive(orgHeader, whs1, 6);
			docket6.WD_ExternalReferenceSplit = 0;
			docket6.InsertAndReturnObject(TestConnection); // Expected 1 result

			var docket7 = new WhsDocket(orgHeader.PK, whs1.PK, "ADJ", "CUS", "ENT", "WD07")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 12, 20),
				WD_SystemCreateUser = "US7",
			}.InsertAndReturnObject(TestConnection); // Adjustment

			var docket8 = PrepareReceive(orgHeader, whs1, 8);
			docket8.WD_ExternalReferenceSplit = 1;
			docket8.InsertAndReturnObject(TestConnection); // Correction

			var docketLine11 = PrepareReceiveLine(docket1, product).InsertAndReturnObject(TestConnection);
			var docketLine12 = PrepareReceiveLine(docket1, product).InsertAndReturnObject(TestConnection);
			var docketLine2 = PrepareReceiveLine(docket2, product).InsertAndReturnObject(TestConnection);
			var docketLine3 = PrepareReceiveLine(docket3, product).InsertAndReturnObject(TestConnection);
			var docketLine4 = PrepareReceiveLine(docket4, product).InsertAndReturnObject(TestConnection);
			var docketLine6 = PrepareReceiveLine(docket6, product).InsertAndReturnObject(TestConnection);
			var docketLine7 = new WhsDocketLine(docket7, product.PK, 1m)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_WL = location.PK,
			}.InsertAndReturnObject(TestConnection);
			var docketLine8 = PrepareReceiveLine(docket8, product).InsertAndReturnObject(TestConnection);

			PrepareBondedWarehouseAttribute(docketLine11, "A").InsertAndReturnObject(TestConnection);
			PrepareBondedWarehouseAttribute(docketLine12, "A").InsertAndReturnObject(TestConnection);
			PrepareBondedWarehouseAttribute(docketLine2, "B").InsertAndReturnObject(TestConnection);
			PrepareBondedWarehouseAttribute(docketLine3, "C").InsertAndReturnObject(TestConnection);
			PrepareBondedWarehouseAttribute(docketLine6, "D").InsertAndReturnObject(TestConnection);
			PrepareBondedWarehouseAttribute(docketLine7, "D").InsertAndReturnObject(TestConnection);
			PrepareBondedWarehouseAttribute(docketLine8, "D").InsertAndReturnObject(TestConnection);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());

			var transaction1 = FindRowByRef2(transactions, "WD06");
			AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 12, 20), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "US6", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "WH1 - N", transaction1.Reference1);
			AssertEquals("TransactionReference03", "ENT000D", transaction1.Reference3);
			AssertEquals("TransactionReference04", "REF000D", transaction1.Reference4);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"WW01\"}"
				, transaction1.AdditionalRefs);

			var transaction2 = FindRowByRef2(transactions, "WD01");
			AssertEquals("CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 12, 20), transaction2.ServiceOccuredUTC);
			AssertEquals("UserCode", "US1", transaction2.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction2.BillableCount);
			AssertEquals("TransactionReference01", "WH1 - N", transaction2.Reference1);
			AssertEquals("TransactionReference03", "ENT000A", transaction2.Reference3);
			AssertEquals("TransactionReference04", "REF000A", transaction2.Reference4);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"WW01\"}"
				, transaction2.AdditionalRefs);

			var transaction3 = FindRowByRef2(transactions, "WD02");
			AssertEquals("CompanyCode", "DEM", transaction3.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction3.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 12, 20), transaction3.ServiceOccuredUTC);
			AssertEquals("UserCode", "US2", transaction3.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction3.BillableCount);
			AssertEquals("TransactionReference01", "WH1 - N", transaction3.Reference1);
			AssertEquals("TransactionReference03", "ENT000B", transaction3.Reference3);
			AssertEquals("TransactionReference04", "REF000B", transaction3.Reference4);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"WW01\"}"
				, transaction3.AdditionalRefs);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				DateTime startDate = new DateTime(2012, 12, 20);
				return new RecurringRange(startDate, startDate.AddDays(1));
			}
		}

		public void TestCollector_IgnoresReceivesCreatedByWhsComponentOrder()
		{
			var company = GlbCompany.ShallowLoadFromDB(TestConnection, c => c.GC_Code == "DEM").First();
			var branch = GlbBranch.ShallowLoadFromDB(TestConnection, b => b.GB_GC == company.PK).First();
			var orgAddress = OrgAddress.ShallowLoadFromDB(TestConnection).First();
			var orgHeader = OrgHeader.ShallowLoadFromDB(TestConnection).First();

			var whs = new WhsWarehouse("W1", branch.PK, orgAddress.PK)
			{
				WW_WarehouseName = "WW01",
				WW_IsVirtualWarehouse = true,
			}.InsertAndReturnObject(TestConnection);

			var area = new WhsArea(whs.PK, "AREA01") { WA_AreaType = "FRE" }.InsertAndReturnObject(TestConnection);
			var row = new WhsRow(whs, "ROW01").InsertAndReturnObject(TestConnection);
			var location = new WhsLocation(row.PK, area.PK, area.PK).InsertAndReturnObject(TestConnection);

			var product = new OrgSupplierPart("P1").InsertAndReturnObject(TestConnection);

			var dynamicWorkOrder = new WhsDocket(orgHeader.PK, whs.PK, "DWO", "ASS", "ENT", "WD01")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 12, 20),
				WD_SystemCreateUser = "US1",
			}.InsertAndReturnObject(TestConnection);

			var workOrder = new WhsDocket(orgHeader.PK, whs.PK, "WOR", "ASS", "ENT", "WD02")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 12, 20),
				WD_SystemCreateUser = "US2",
			}.InsertAndReturnObject(TestConnection);

			var docket1 = PrepareReceive(orgHeader, whs, 3).InsertAndReturnObject(TestConnection);
			var docket2 = PrepareReceive(orgHeader, whs, 4);
			docket2.WD_WD_ParentDocket = dynamicWorkOrder;
			docket2.InsertAndReturnObject(TestConnection);

			var docket3 = PrepareReceive(orgHeader, whs, 5);
			docket3.WD_WD_ParentDocket = workOrder;
			docket3.InsertAndReturnObject(TestConnection);

			var receiveLine1 = PrepareReceiveLine(docket1, product).InsertAndReturnObject(TestConnection);
			var receiveLine2 = PrepareReceiveLine(docket2, product).InsertAndReturnObject(TestConnection);
			var receiveLine3 = PrepareReceiveLine(docket3, product).InsertAndReturnObject(TestConnection);

			PrepareBondedWarehouseAttribute(receiveLine1, "A").InsertAndReturnObject(TestConnection);
			PrepareBondedWarehouseAttribute(receiveLine2, "B").InsertAndReturnObject(TestConnection);
			PrepareBondedWarehouseAttribute(receiveLine3, "C").InsertAndReturnObject(TestConnection);

			var transactions = ScriptToTest.Run(TestDateTimeRange).Select(t => (IBillingTransaction)t);

			AssertEquals("Number of Transactions", 1, transactions.Count());

			var transaction = FindRowByRef2(transactions, "WD03");
			AssertEquals("CompanyCode", "DEM", transaction.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 12, 20), transaction.ServiceOccuredUTC);
			AssertEquals("UserCode", "US3", transaction.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction.BillableCount);
			AssertEquals("TransactionReference01", "W1 - Y", transaction.Reference1);
			AssertEquals("TransactionReference03", "ENT000A", transaction.Reference3);
			AssertEquals("TransactionReference04", "REF000A", transaction.Reference4);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"WW01\"}"
				, transaction.AdditionalRefs);
		}

		WhsDocket PrepareReceive(OrgHeader orgHeader, WhsWarehouse whs, int postFix)
			=> new WhsDocket(orgHeader.PK, whs.PK, "INW", "CUS", "ENT", "WD0" + postFix)
				{
					WD_SystemCreateTimeUtc = new DateTime(2012, 12, 20),
					WD_SystemCreateUser = "US" + postFix,
				};

		WhsDocketLine PrepareReceiveLine(WhsDocket docket, OrgSupplierPart product)
			=> new WhsDocketLine(docket, product.PK, 1m)
			{
				WE_StockOnHand = 1m,
				WE_AdjustmentArrivalDate = DateTime.Now,
				WE_OriginalInventoryStatus = "ARV",
				WE_CurrentInventoryStatus = "ARV",
			};

		WhsBondedWarehouseAttribute PrepareBondedWarehouseAttribute(WhsDocketLine docketLine, string postFix)
			=>
			new WhsBondedWarehouseAttribute(docketLine.PK, "WE")
			{
				WB_DeclarationReference = "REF000" + postFix,
				WB_EntryKey = "ENT000" + postFix,
			};
	}
}
