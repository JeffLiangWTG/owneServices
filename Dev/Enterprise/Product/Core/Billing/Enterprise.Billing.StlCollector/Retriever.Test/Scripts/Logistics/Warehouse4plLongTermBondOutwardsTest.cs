using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(Warehouse4plLongTermBondOutwards))]
	sealed class Warehouse4plLongTermBondOutwardsTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			var nowUTC = DateTime.UtcNow;
			var year = nowUTC.Year;
			var month = nowUTC.Month;
			var day = nowUTC.Day;
			var beforeQueryPeriod = nowUTC.AddMonths(-2);
			var afterQueryPeriod = nowUTC.AddMonths(1);

			var company = GlbCompany.ShallowLoadFromDB(TestConnection, c => c.GC_Code == "DEM").First();
			var branch = GlbBranch.ShallowLoadFromDB(TestConnection, b => b.GB_GC == company.PK).First();
			var orgAddress = OrgAddress.ShallowLoadFromDB(TestConnection).First();
			var orgHeader = OrgHeader.ShallowLoadFromDB(TestConnection).First();

			var product = new OrgSupplierPart("P1").InsertAndReturnObject(TestConnection);

			var whs1 = new WhsWarehouse("WH1", branch.PK, orgAddress.PK) { WW_WarehouseName = "Warehouse 1", WW_IsVirtualWarehouse = false }.WithDockDoor(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch.PK, orgAddress.PK) { WW_WarehouseName = "Warehouse 2", WW_IsVirtualWarehouse = true }.WithDockDoor(TestConnection);

			var pick1 = new WhsPick(whs1, "P1", "").InsertAndReturnObject(TestConnection);
			var pick2 = new WhsPick(whs2, "P2", "").InsertAndReturnObject(TestConnection);

			var order1 = CreateDocketForPick(orgHeader.PK, whs1.PK, pick1.PK, new DateTime(2012, 12, 20, 21, 00, 00), "ATP", "WD01", "US1"); // 2 docket lines; Expected 1 result
			var order2 = CreateDocketForPick(orgHeader.PK, whs1.PK, pick1.PK, new DateTime(2012, 12, 20, 22, 00, 00), "ATP", "WD02", "US2"); // Expected 1 result
			var order3 = CreateDocketForPick(orgHeader.PK, whs1.PK, pick1.PK, new DateTime(2012, 12, 19, 23, 00, 00), "ATP", "WD03", "US3"); // Wrong date
			var order4 = CreateDocketForPick(orgHeader.PK, whs1.PK, pick1.PK, new DateTime(2012, 12, 20, 22, 00, 00), "ATP", "WD04", "US4"); // No Bonded Warehouse Attribute
			var order5 = CreateDocketForPick(orgHeader.PK, whs1.PK, pick1.PK, new DateTime(2012, 12, 20, 21, 00, 00), "ATP", "WD05", "US5"); // No Docket Line

			var order6 = CreateDocketForPick(orgHeader.PK, whs1.PK, null, new DateTime(2012, 12, 20, 21, 00, 00), "CAN", "WD06", "US6", "~BP", new DateTime(2012, 12, 20, 21, 00, 00), isFinalised: false); // Expected 1 result
			var order7 = CreateDocketForPick(orgHeader.PK, whs1.PK, null, new DateTime(2012, 12, 20, 21, 00, 00), "CAN", "WD07", "US7", "~BP", new DateTime(2012, 12, 20, 21, 00, 00), isFinalised: false); // Correction 1
			var order8 = CreateDocketForPick(orgHeader.PK, whs1.PK, pick1.PK, new DateTime(2012, 12, 20, 20, 03, 00), "ATP", "WD08", "US8"); // Correction 2

			var order9 = CreateDocketForPick(orgHeader.PK, whs2.PK, pick2.PK, new DateTime(2012, 12, 20, 21, 00, 00), "ATP", "WD09", "US9"); // virtual order; Expected 1 result

			var docketLine1 = CreateDocketLineForDocket(order1, "FIN", 1, 1);
			var docketLine2 = CreateDocketLineForDocket(order1, "FIN", 1, 2);
			var docketLine3 = CreateDocketLineForDocket(order2, "FIN", 1, 1);
			var docketLine4 = CreateDocketLineForDocket(order3, "FIN", 1, 1);
			var docketLine5 = CreateDocketLineForDocket(order4, "FIN", 1, 1);
			var docketLine6 = CreateDocketLineForDocket(order6, "CAN", 1, 1, isFinalised: false);
			var docketLine7 = CreateDocketLineForDocket(order7, "CAN", 1, 1, isFinalised: false);
			var docketLine8 = CreateDocketLineForDocket(order8, "FIN", 1, 1);
			var docketLine9 = CreateDocketLineForDocket(order9, "FIN", 1, 1);

			new WhsBondedWarehouseAttribute(docketLine1.PK, "WE") { WB_DeclarationReference = "REF000A", WB_EntryKey = "ENT000A", WB_EntryDate = new DateTime(2012, 12, 01, 11, 00, 00) }.InsertAndReturnObject(TestConnection);
			new WhsBondedWarehouseAttribute(docketLine2.PK, "WE") { WB_DeclarationReference = "REF000A", WB_EntryKey = "ENT000A", WB_EntryDate = new DateTime(2012, 12, 01, 12, 00, 00) }.InsertAndReturnObject(TestConnection);
			new WhsBondedWarehouseAttribute(docketLine3.PK, "WE") { WB_DeclarationReference = "REF000B", WB_EntryKey = "ENT000B", WB_EntryDate = new DateTime(2012, 12, 01, 13, 00, 00) }.InsertAndReturnObject(TestConnection);
			new WhsBondedWarehouseAttribute(docketLine4.PK, "WE") { WB_DeclarationReference = "REF000C", WB_EntryKey = "ENT000C", WB_EntryDate = new DateTime(2012, 12, 01, 14, 00, 00) }.InsertAndReturnObject(TestConnection);

			new WhsBondedWarehouseAttribute(docketLine6.PK, "WE") { WB_DeclarationReference = "REF000D", WB_EntryKey = "", WB_EntryDate = new DateTime(2012, 12, 01, 14, 01, 00) }.InsertAndReturnObject(TestConnection);
			new WhsBondedWarehouseAttribute(docketLine7.PK, "WE") { WB_DeclarationReference = "", WB_EntryKey = "", WB_EntryDate = new DateTime(2012, 12, 01, 14, 02, 00) }.InsertAndReturnObject(TestConnection);
			new WhsBondedWarehouseAttribute(docketLine8.PK, "WE") { WB_DeclarationReference = "REF000D", WB_EntryKey = "ENT000D", WB_EntryDate = new DateTime(2012, 12, 01, 14, 03, 00) }.InsertAndReturnObject(TestConnection);

			new WhsBondedWarehouseAttribute(docketLine9.PK, "WE") { WB_DeclarationReference = "REF000E", WB_EntryKey = "ENT000E", WB_EntryDate = new DateTime(2012, 12, 01, 11, 00, 00) }.InsertAndReturnObject(TestConnection);

			WhsDocket CreateDocketForPick(
				Guid client,
				Guid whs,
				Guid? pick,
				DateTime createTime,
				string status,
				string docketID,
				string creatingUser,
				string canceledBy = "",
				DateTime? canceledTime = null,
				bool isFinalised = true)
			{
				return new WhsDocket(client, whs, "ORD", "CUS", status, docketID)
				{
					WD_CustomerReference = "REFFF " + docketID,
					WD_WP = pick,
					WD_FinalisedDate = isFinalised ? nowUTC : (DateTime?)null,
					WD_GS_NKFinalizedBy = isFinalised ? "A" : string.Empty,
					WD_BookingDate = nowUTC,
					WD_GS_NKCanceledBy = canceledBy,
					WD_CanceledTimeUtc = canceledTime,
					WD_SystemCreateTimeUtc = createTime,
					WD_SystemCreateUser = creatingUser,
				}.InsertAndReturnObject(TestConnection);
			}

			WhsDocketLine CreateDocketLineForDocket(
				WhsDocket docket,
				string status,
				short lineNumber,
				short subLineNumber,
				bool isFinalised = true)
			{
				return new WhsDocketLine(docket, product.PK, 0m)
				{
					WE_DocketLineStatus = status,
					WE_LineNo = lineNumber,
					WE_SubLineNo = subLineNumber,
					WE_F3_NKPackType = "UNT",
					WE_FinalisedDate = isFinalised ? nowUTC : (DateTime?)null,
				}.InsertAndReturnObject(TestConnection);
			}
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 4, transactions.Count());

			var transaction1 = FindRowByRef2(transactions, "WD08");
			AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 12, 20, 20, 03, 00), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "US8", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "WH1 - N", transaction1.Reference1);
			AssertEquals("TransactionReference03", "REFFF WD08", transaction1.Reference3);
			AssertEquals("TransactionReference04", "REF000D", transaction1.Reference4);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction1.AdditionalRefs);

			var transaction2 = FindRowByRef2(transactions, "WD01");
			AssertEquals("CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 12, 20, 21, 00, 00), transaction2.ServiceOccuredUTC);
			AssertEquals("UserCode", "US1", transaction2.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction2.BillableCount);
			AssertEquals("TransactionReference01", "WH1 - N", transaction2.Reference1);
			AssertEquals("TransactionReference03", "REFFF WD01", transaction2.Reference3);
			AssertEquals("TransactionReference04", "REF000A", transaction2.Reference4);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction2.AdditionalRefs);

			var transaction3 = FindRowByRef2(transactions, "WD02");
			AssertEquals("CompanyCode", "DEM", transaction3.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction3.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 12, 20, 22, 00, 00), transaction3.ServiceOccuredUTC);
			AssertEquals("UserCode", "US2", transaction3.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction3.BillableCount);
			AssertEquals("TransactionReference01", "WH1 - N", transaction3.Reference1);
			AssertEquals("TransactionReference03", "REFFF WD02", transaction3.Reference3);
			AssertEquals("TransactionReference04", "REF000B", transaction3.Reference4);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction3.AdditionalRefs);

			var transaction4 = FindRowByRef2(transactions, "WD09");
			AssertEquals("CompanyCode", "DEM", transaction4.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction4.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 12, 20, 21, 00, 00), transaction4.ServiceOccuredUTC);
			AssertEquals("UserCode", "US9", transaction4.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction4.BillableCount);
			AssertEquals("TransactionReference01", "WH2 - Y", transaction4.Reference1);
			AssertEquals("TransactionReference03", "REFFF WD09", transaction4.Reference3);
			AssertEquals("TransactionReference04", "REF000E", transaction4.Reference4);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 2\"}"
				, transaction4.AdditionalRefs);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				DateTime startDate = new DateTime(2012, 12, 20);
				return new RecurringRange(startDate, startDate.AddDays(1));
			}
		}
	}
}
