using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.Types;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(Warehouse3plBookedOrders))]
	sealed class Warehouse3plBookedOrdersTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			var nowUTC = DateTime.UtcNow;
			var year = Now.Year;
			var month = Now.Month;
			var day = Now.Day;
			var beforeQueryPeriod = Now.AddMonths(-2);
			var afterQueryPeriod = Now.AddMonths(1);

			// In order to test filter non custom order (WhereClause), need to drop Constraint
			var sqlText = "ALTER TABLE [WhsDocket] DROP CONSTRAINT [Constraint_WD_BookedWithCBADateTimeUtc]";
			TestConnection.ExecuteNonQuery(sqlText);

			var company = GlbCompany.ShallowLoadFromDB(TestConnection, c => c.GC_Code == "DEM").First();
			var branch = GlbBranch.ShallowLoadFromDB(TestConnection, b => b.GB_GC == company.PK).First();
			var orgAddress = OrgAddress.ShallowLoadFromDB(TestConnection).First();
			var orgHeader = OrgHeader.ShallowLoadFromDB(TestConnection).First();

			var whs1 = new WhsWarehouse("WH1", branch.PK, orgAddress.PK) { WW_WarehouseName = "Warehouse 1", WW_IsVirtualWarehouse = false }.WithDockDoor(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch.PK, orgAddress.PK) { WW_WarehouseName = "Warehouse 2", WW_IsVirtualWarehouse = true }.WithDockDoor(TestConnection);

			var product = new OrgSupplierPart("P1").InsertAndReturnObject(TestConnection);

			var physicalPick1 = new WhsPick(whs1, "PP1", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 01, 23, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var physicalPick2 = new WhsPick(whs1, "PP2", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 02, 24, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var physicalPick3 = new WhsPick(whs1, "PP3", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 01, 25, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var physicalPick4 = new WhsPick(whs1, "PP4", "FIN") { WP_FinalizedDateUtc = new DateTime(beforeQueryPeriod.Year, beforeQueryPeriod.Month, 01), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var physicalPick5 = new WhsPick(whs1, "PP5", "FIN") { WP_FinalizedDateUtc = new DateTime(afterQueryPeriod.Year, afterQueryPeriod.Month, 01), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var physicalPick6 = new WhsPick(whs1, "PP6", "ENT") { WP_FinalizedDateUtc = null }.InsertAndReturnObject(TestConnection);
			var physicalPick7 = new WhsPick(whs1, "PP7", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 04, 33, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);

			var virtualPick1 = new WhsPick(whs2, "VP1", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 01, 23, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var virtualPick2 = new WhsPick(whs2, "VP2", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 02, 24, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var virtualPick3 = new WhsPick(whs2, "VP3", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 01, 25, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var virtualPick4 = new WhsPick(whs2, "VP4", "FIN") { WP_FinalizedDateUtc = new DateTime(beforeQueryPeriod.Year, beforeQueryPeriod.Month, 01), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var virtualPick5 = new WhsPick(whs2, "VP5", "FIN") { WP_FinalizedDateUtc = new DateTime(afterQueryPeriod.Year, afterQueryPeriod.Month, 01), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var virtualPick6 = new WhsPick(whs2, "VP6", "ENT") { WP_FinalizedDateUtc = null }.InsertAndReturnObject(TestConnection);
			var virtualPick7 = new WhsPick(whs2, "VP7", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 04, 33, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);

			var physicalOrder1 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick1.PK, nowUTC, "WOR", "ASS", "", "FIN", "WDP01", "US1"); // not order
			var physicalOrder2 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick2.PK, nowUTC, "ORD", "ORD", "ECO", "DEP", "WDP02", "US2"); // booked ecommerce order
			var physicalOrder3 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick3.PK, nowUTC, "ORD", "CUS", "ECO", "DEP", "WDP03", "US3"); // booked customs ecommerce order
			var physicalOrder4 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick7.PK, nowUTC, "ORD", "ORD", "UNK", "DEP", "WDP04", "US4"); // booked unknown order
			var physicalOrder5 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick4.PK, nowUTC, "WOR", "ASS", "", "FIN", "WDP05", "US5"); // booked ecommerce order, pick finalisation date out of bounds - before query period
			var physicalOrder6 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick5.PK, nowUTC, "ORD", "ORD", "ECO", "DEP", "WDP06", "US6"); // booked ecommerce order, pick finalisation date out of bounds - after query period
			var physicalOrder7 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick2.PK, null, "ORD", "ORD", "ECO", "DEP", "WDP07", "US7"); // not booked ecommerce order
			var physicalOrder8 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick2.PK, nowUTC, "ORD", "ORD", "BLK", "DEP", "WDP08", "US8"); // booked bulk order
			var physicalOrder9 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick6.PK, nowUTC, "ORD", "ORD", "ECO", "ATP", "WDP09", "US9"); // booked ecommerce order, pick not finalised
			var physicalOrder10 = CreateDocketForPick(orgHeader.PK, whs1.PK, null, nowUTC, "ORD", "ORD", "", "ENT", "WDP10", "US10", isFinalised: false); // unpicked order
			var physicalOrder11 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick2.PK, null, "ORD", "ORD", "UNK", "DEP", "WDP11", "US11"); // not booked unknown order

			var virtualOrder1 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick1.PK, nowUTC, "WOR", "ASS", "", "FIN", "WDV01", "US1"); // not order
			var virtualOrder2 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick2.PK, nowUTC, "ORD", "ORD", "ECO", "DEP", "WDV02", "US2"); // booked ecommerce order
			var virtualOrder3 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick3.PK, nowUTC, "ORD", "CUS", "ECO", "DEP", "WDV03", "US3"); // booked customs ecommerce order
			var virtualOrder4 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick7.PK, nowUTC, "ORD", "ORD", "UNK", "DEP", "WDV04", "US4"); // booked unknown order
			var virtualOrder5 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick4.PK, nowUTC, "WOR", "ASS", "", "FIN", "WDV05", "US5"); // booked ecommerce order, pick finalisation date out of bounds - before query period
			var virtualOrder6 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick5.PK, nowUTC, "ORD", "ORD", "ECO", "DEP", "WDV06", "US6"); // booked ecommerce order, pick finalisation date out of bounds - after query period
			var virtualOrder7 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick2.PK, null, "ORD", "ORD", "ECO", "DEP", "WDV07", "US7"); // not booked ecommerce order
			var virtualOrder8 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick2.PK, nowUTC, "ORD", "ORD", "BLK", "DEP", "WDV08", "US8"); // booked bulk order
			var virtualOrder9 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick6.PK, nowUTC, "ORD", "ORD", "ECO", "ATP", "WDV09", "US9"); // booked ecommerce order, pick not finalised
			var virtualOrder10 = CreateDocketForPick(orgHeader.PK, whs2.PK, null, nowUTC, "ORD", "ORD", "", "ENT", "WDV10", "US10", isFinalised: false); // unpicked order
			var virtualOrder11 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick2.PK, null, "ORD", "ORD", "UNK", "DEP", "WDV11", "US11"); // not booked unknown order

			WhsDocket CreateDocketForPick(
				Guid client,
				Guid whs,
				Guid? pick,
				DateTime? bookedWithCBADateTime,
				string docketType,
				string docketSubType,
				string orderClassification,
				string status,
				string docketID,
				string user,
				bool isFinalised = true)
			{
				return new WhsDocket(client, whs, docketType, docketSubType, status, docketID)
				{
					WD_WP = pick,
					WD_OrderClassification = orderClassification,
					WD_FinalisedDate = isFinalised ? Now.ToDateTime() : (DateTime?)null,
					WD_GS_NKFinalizedBy = isFinalised ? "A" : string.Empty,
					WD_BookingDate = nowUTC,
					WD_BookedWithCBADateTimeUtc = bookedWithCBADateTime,
					WD_SystemCreateUser = user,
				}.InsertAndReturnObject(TestConnection);
			}
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 4, transactions.Count());

			var transaction1 = FindRowByRef2(transactions, "WDP02");
			AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("UserCode", "US2", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "WH1 - N", transaction1.Reference1);
			AssertEquals("TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 2, 24, 0), transaction1.ServiceOccuredUTC);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction1.AdditionalRefs);

			var transaction2 = FindRowByRef2(transactions, "WDP04");
			AssertEquals("CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("UserCode", "US4", transaction2.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction2.BillableCount);
			AssertEquals("TransactionReference01", "WH1 - N", transaction2.Reference1);
			AssertEquals("TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 4, 33, 0), transaction2.ServiceOccuredUTC);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction2.AdditionalRefs);

			var transaction3 = FindRowByRef2(transactions, "WDV02");
			AssertEquals("CompanyCode", "DEM", transaction3.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction3.GetBranchCode());
			AssertEquals("UserCode", "US2", transaction3.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction3.BillableCount);
			AssertEquals("TransactionReference01", "WH2 - Y", transaction3.Reference1);
			AssertEquals("TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 2, 24, 0), transaction3.ServiceOccuredUTC);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 2\"}"
				, transaction3.AdditionalRefs);

			var transaction4 = FindRowByRef2(transactions, "WDV04");
			AssertEquals("CompanyCode", "DEM", transaction4.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction4.GetBranchCode());
			AssertEquals("UserCode", "US4", transaction4.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction4.BillableCount);
			AssertEquals("TransactionReference01", "WH2 - Y", transaction4.Reference1);
			AssertEquals("TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 4, 33, 0), transaction4.ServiceOccuredUTC);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 2\"}"
				, transaction4.AdditionalRefs);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(Now.Year, Now.Month);
		ZDateTime Now => (now ?? (now = DateTime.Now)).Value;
		ZDateTime? now;
	}
}
