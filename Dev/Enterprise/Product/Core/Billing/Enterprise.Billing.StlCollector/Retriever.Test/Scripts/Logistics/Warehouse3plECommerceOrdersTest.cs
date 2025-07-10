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
	[TestedType(typeof(Warehouse3plECommerceOrders))]
	sealed class Warehouse3plECommerceOrdersTest : RefStlScriptWithDefaultsTest
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
	
			var physicalPick1 = new WhsPick(whs1, "1", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 01, 23, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var physicalPick2 = new WhsPick(whs1, "2", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 02, 24, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var physicalPick3 = new WhsPick(whs1, "3", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 01, 25, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var physicalPick4 = new WhsPick(whs1, "4", "FIN") { WP_FinalizedDateUtc = new DateTime(beforeQueryPeriod.Year, beforeQueryPeriod.Month, 01, 01, 43, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var physicalPick5 = new WhsPick(whs1, "5", "FIN") { WP_FinalizedDateUtc = new DateTime(afterQueryPeriod.Year, afterQueryPeriod.Month, 01, 01, 53, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var physicalPick6 = new WhsPick(whs1, "6", "ENT") { WP_FinalizedDateUtc = null }.InsertAndReturnObject(TestConnection);
			var physicalPick7 = new WhsPick(whs1, "7", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 04, 33, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var virtualPick1 = new WhsPick(whs2, "8", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 02, 24, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);

			var physicalOrder1 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick1.PK, null, "WOR", "ASS", "", "FIN", "WD01", "US1"); // not order
			var physicalOrder2 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick2.PK, null, "ORD", "ORD", "ECO", "DEP", "WD02", "US2"); // pick finalised ecommerce order
			var physicalOrder3 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick3.PK, null, "ORD", "CUS", "ECO", "DEP", "WD03", "US3"); // pick finalised customs ecomm
			var physicalOrder4 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick7.PK, null, "ORD", "ORD", "UNK", "DEP", "WD04", "US4"); // pick finalised unknown order
			var physicalOrder5 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick4.PK, null, "WOR", "ASS", "", "FIN", "WD05", "US5"); // pick finalised ecommerce order, pick finalisation date out of bounds - before query period
			var physicalOrder6 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick5.PK, null, "ORD", "ORD", "ECO", "DEP", "WD06", "US6"); // pick finalised ecommerce order, pick finalisation date out of bounds - after query period
			var physicalOrder7 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick2.PK, nowUTC, "ORD", "ORD", "ECO", "DEP", "WD07", "US7"); // pick finalised booked ecommerce order
			var physicalOrder8 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick2.PK, null, "ORD", "ORD", "BLK", "DEP", "WD08", "US8"); // pick finalised  bulk order
			var physicalOrder9 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick6.PK, null, "ORD", "ORD", "ECO", "ATP", "WD09", "US9"); // ecommerce order, pick not finalised
			var physicalOrder10 = CreateDocketForPick(orgHeader.PK, whs1.PK, null, null, "ORD", "ORD", "", "ENT", "WD10", "US10", isFinalised: false); // unpicked order
			var physicalOrder11 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick2.PK, nowUTC, "ORD", "ORD", "UNK", "DEP", "WD11", "US11"); // pick finalised booked unknown order
			var virtualOrder1 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick1.PK, null, "ORD", "ORD", "ECO", "DEP", "WD12", "U12"); // pick finalised ecommerce order

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
			AssertEquals("Number of Transactions", 3, transactions.Count());

			var transaction1 = FindRowByRef2(transactions, "WD02");
			AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("UserCode", "US2", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "WH1 - N", transaction1.Reference1);
			AssertEquals("TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 2, 24, 0), transaction1.ServiceOccuredUTC);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction1.AdditionalRefs);

			var transaction2 = FindRowByRef2(transactions, "WD04");
			AssertEquals("CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("UserCode", "US4", transaction2.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction2.BillableCount);
			AssertEquals("TransactionReference01", "WH1 - N", transaction1.Reference1);
			AssertEquals("TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 4, 33, 0), transaction2.ServiceOccuredUTC);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction2.AdditionalRefs);

			var transaction3 = FindRowByRef2(transactions, "WD12");
			AssertEquals("CompanyCode", "DEM", transaction3.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction3.GetBranchCode());
			AssertEquals("UserCode", "U12", transaction3.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction3.BillableCount);
			AssertEquals("TransactionReference01", "WH2 - Y", transaction3.Reference1);
			AssertEquals("TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 2, 24, 0), transaction3.ServiceOccuredUTC);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 2\"}"
				, transaction3.AdditionalRefs);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(Now.Year, Now.Month);
		ZDateTime Now => (now ?? (now = DateTime.Now)).Value;
		ZDateTime? now;
	}
}
