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
	[TestedType(typeof(Warehouse3plECommerceOrderLine_Pre_24_2_10_70))]
	sealed class Warehouse3plECommerceOrderLine_Pre_24_2_10_70Test : RefStlScriptWithDefaultsTest
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

			var physicalPick1 = new WhsPick(whs1, "P1", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 01, 23, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var physicalPick2 = new WhsPick(whs1, "P2", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 02, 24, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var physicalPick3 = new WhsPick(whs1, "P3", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 01, 25, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var physicalPick4 = new WhsPick(whs1, "P4", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 02, 26, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var physicalPick5 = new WhsPick(whs1, "P5", "FIN") { WP_FinalizedDateUtc = new DateTime(beforeQueryPeriod.Year, beforeQueryPeriod.Month, 01), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var physicalPick6 = new WhsPick(whs1, "P6", "FIN") { WP_FinalizedDateUtc = new DateTime(afterQueryPeriod.Year, afterQueryPeriod.Month, 01), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var physicalPick7 = new WhsPick(whs1, "P7", "ENT") { WP_FinalizedDateUtc = null }.InsertAndReturnObject(TestConnection);
			var virtualPick1 = new WhsPick(whs2, "VP1", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 01, 23, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);

			var physicalOrder1 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick1.PK, null, "ORD", "ORD", "ECO", "DEP", "WD01"); // pick finalised eCommerce order
			var physicalOrder2 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick2.PK, null, "ORD", "CUS", "ECO", "DEP", "WD02"); // pick finalised eCommerce customs order
			var physicalOrder3 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick3.PK, null, "ORD", "ORD", "ECO", "DEP", "WD03"); // pick finalised eCommerce order
			var physicalOrder4 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick5.PK, null, "ORD", "ORD", "ECO", "DEP", "WD04"); // pick finalised eCommerce order, pick finalisation date out of bounds - before query
			var physicalOrder5 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick4.PK, null, "WOR", "ASS", "", "FIN", "WD05"); // not order
			var physicalOrder6 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick6.PK, null, "ORD", "ORD", "ECO", "DEP", "WD06"); // pick finalised eCommerce order, pick finalisation date out of bounds - after query
			var physicalOrder7 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick1.PK, null, "ORD", "ORD", "BLK", "DEP", "WD07"); // pick finalised bulk order
			var physicalOrder8 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick1.PK, null, "ORD", "ORD", "UNK", "DEP", "WD08"); // pick finalised unknown order
			var physicalOrder9 = CreateDocketForPick(orgHeader.PK, whs1.PK, null, nowUTC, "ORD", "ORD", "", "ENT", "WD09", isFinalised: false); // unpicked order
			var physicalOrder10 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick2.PK, nowUTC, "ORD", "ORD", "ECO", "DEP", "WD10"); // pick finalised booked ecommerce order
			var physicalOrder11 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick2.PK, nowUTC, "ORD", "ORD", "UNK", "DEP", "WD11"); // pick finalised booked unknown order
			var physicalOrder12 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick7.PK, null, "ORD", "ORD", "ECO", "ATP", "WD12"); // finalised eCommerce order, pick not finalised
			var virtualOrder1 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick1.PK, null, "ORD", "ORD", "ECO", "DEP", "WD13"); // virtual warehouse pick finalised eCommerce order
			var pickedOnSaleOrder1 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick1.PK, null, "ORD", "ORD", "ECO", "DEP", "WD14"); // pick finalised eCommerce order, picked on sale
			var pickedOnSaleOrder2 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick3.PK, null, "ORD", "ORD", "ECO", "DEP", "WD15"); // pick finalised ECO picked on sale order, without receive on pick

			CreateDocketLineForDocket(physicalOrder1, "U01", "FIN", 1, 1);
			CreateDocketLineForDocket(physicalOrder1, "U02", "FIN", 1, 2);
			CreateDocketLineForDocket(physicalOrder2, "U03", "FIN", 2, 1);
			CreateDocketLineForDocket(physicalOrder3, "U04", "FIN", 3, 1);
			CreateDocketLineForDocket(physicalOrder4, "U05", "FIN", 4, 1);
			CreateDocketLineForDocket(physicalOrder5, "U06", "FIN", 5, 1);
			CreateDocketLineForDocket(physicalOrder6, "U07", "FIN", 6, 1);
			CreateDocketLineForDocket(physicalOrder7, "U08", "FIN", 7, 1);
			CreateDocketLineForDocket(physicalOrder8, "U09", "FIN", 8, 1);
			CreateDocketLineForDocket(physicalOrder9, "U10", "", 9, 1, isFinalised: false);
			CreateDocketLineForDocket(physicalOrder10, "U11", "FIN", 10, 1);
			CreateDocketLineForDocket(physicalOrder11, "U12", "FIN", 11, 1);
			CreateDocketLineForDocket(physicalOrder12, "U13", "FIN", 12, 1);
			CreateDocketLineForDocket(virtualOrder1, "U14", "FIN", 13, 1);

			var parentLine = CreateDocketLineForDocket(pickedOnSaleOrder1, "U15", "FIN", 14, 1);
			var componentLine = CreateDocketLineForDocket(pickedOnSaleOrder1, "U15", "FIN", 14, 2, parentLine: parentLine);
			CreateReceiveFromPickByBOM(orgHeader.PK, whs1, "REC02", physicalPick1);

			var parentLine2 = CreateDocketLineForDocket(pickedOnSaleOrder2, "U16", "FIN", 15, 1);
			var componentLine2 = CreateDocketLineForDocket(pickedOnSaleOrder2, "U16", "FIN", 15, 2, parentLine: parentLine2);

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
				}.InsertAndReturnObject(TestConnection);
			}

			WhsDocketLine CreateDocketLineForDocket(
				WhsDocket docket,
				string creatingUser,
				string status,
				short lineNumber,
				short subLineNumber,
				bool isFinalised = true,
				WhsDocketLine parentLine = null)
			{
				return new WhsDocketLine(docket, product.PK, 0m)
				{
					WE_DocketLineStatus = status,
					WE_SystemCreateUser = creatingUser,
					WE_LineNo = lineNumber,
					WE_SubLineNo = subLineNumber,
					WE_F3_NKPackType = "UNT",
					WE_FinalisedDate = isFinalised ? Now.ToDateTime() : (DateTime?)null,
					WE_WE_ParentDocketLine = parentLine,
				}.InsertAndReturnObject(TestConnection);
			}

			void CreateReceiveFromPickByBOM(Guid clientPK, WhsWarehouse whs, string reference, WhsPick pick)
			{
				new WhsDocket(clientPK, whs.PK, "INW", "REC", "", reference)
				{
					WD_SystemCreateTimeUtc = new DateTime(2012, 09, 01),
					WD_SystemCreateUser = "US8",
					WD_WP_ParentPickForReceive = pick
				}.InsertAndReturnObject(TestConnection);
			}
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 9, transactions.Count());

			var transaction1 = FindRowByRef2(transactions, "#1.1");
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 23, 0), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "U01", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference01", "WH1 - N", transaction1.Reference1);
			AssertEquals("[T1] TransactionReference03", "WD01", transaction1.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction1.AdditionalRefs);

			var transaction2 = FindRowByRef2(transactions, "#1.2");
			AssertEquals("[T2] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 23, 0), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "U02", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T1] TransactionReference01", "WH1 - N", transaction2.Reference1);
			AssertEquals("[T1] TransactionReference03", "WD01", transaction2.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction2.AdditionalRefs);

			var transaction3 = FindRowByRef2(transactions, "#8.1");
			AssertEquals("[T3] CompanyCode", "DEM", transaction3.GetCompanyCode());
			AssertEquals("[T3] BranchCode", "DEM", transaction3.GetBranchCode());
			AssertEquals("[T3] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 23, 0), transaction3.ServiceOccuredUTC);
			AssertEquals("[T3] UserCode", "U09", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);
			AssertEquals("[T1] TransactionReference01", "WH1 - N", transaction3.Reference1);
			AssertEquals("[T1] TransactionReference03", "WD08", transaction3.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction3.AdditionalRefs);

			var transaction4 = FindRowByRef2(transactions, "#3.1");
			AssertEquals("[T4] CompanyCode", "DEM", transaction4.GetCompanyCode());
			AssertEquals("[T4] BranchCode", "DEM", transaction4.GetBranchCode());
			AssertEquals("[T4] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 25, 0), transaction4.ServiceOccuredUTC);
			AssertEquals("[T4] UserCode", "U04", transaction4.ClientStaffCode);
			AssertEquals("[T4] ItemCount", 1, transaction4.BillableCount);
			AssertEquals("[T1] TransactionReference01", "WH1 - N", transaction4.Reference1);
			AssertEquals("[T1] TransactionReference03", "WD03", transaction4.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction4.AdditionalRefs);

			var transaction5 = FindRowByRef2(transactions, "#13.1");
			AssertEquals("[T5] CompanyCode", "DEM", transaction5.GetCompanyCode());
			AssertEquals("[T5] BranchCode", "DEM", transaction5.GetBranchCode());
			AssertEquals("[T5] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 23, 0), transaction5.ServiceOccuredUTC);
			AssertEquals("[T5] UserCode", "U14", transaction5.ClientStaffCode);
			AssertEquals("[T5] ItemCount", 1, transaction5.BillableCount);
			AssertEquals("[T5] TransactionReference01", "WH2 - Y", transaction5.Reference1);
			AssertEquals("[T5] TransactionReference03", "WD13", transaction5.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 2\"}"
				, transaction5.AdditionalRefs);

			var transaction6 = FindRowByRef2(transactions, "#14.1");
			AssertEquals("[T6] CompanyCode", "DEM", transaction6.GetCompanyCode());
			AssertEquals("[T6] BranchCode", "DEM", transaction6.GetBranchCode());
			AssertEquals("[T6] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 23, 0), transaction6.ServiceOccuredUTC);
			AssertEquals("[T6] UserCode", "U15", transaction6.ClientStaffCode);
			AssertEquals("[T6] ItemCount", 1, transaction6.BillableCount);
			AssertEquals("[T6] TransactionReference01", "WH1 - N", transaction6.Reference1);
			AssertEquals("[T6] TransactionReference03", "WD14", transaction6.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction6.AdditionalRefs);

			var transaction7 = FindRowByRef2(transactions, "#14.2");
			AssertEquals("[T6] CompanyCode", "DEM", transaction7.GetCompanyCode());
			AssertEquals("[T6] BranchCode", "DEM", transaction7.GetBranchCode());
			AssertEquals("[T6] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 23, 0), transaction7.ServiceOccuredUTC);
			AssertEquals("[T6] UserCode", "U15", transaction7.ClientStaffCode);
			AssertEquals("[T6] ItemCount", 1, transaction7.BillableCount);
			AssertEquals("[T6] TransactionReference01", "WH1 - N", transaction7.Reference1);
			AssertEquals("[T6] TransactionReference03", "WD14", transaction7.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction7.AdditionalRefs);

			var transaction8 = FindRowByRef2(transactions, "#15.1");
			AssertEquals("[T8] CompanyCode", "DEM", transaction8.GetCompanyCode());
			AssertEquals("[T8] BranchCode", "DEM", transaction8.GetBranchCode());
			AssertEquals("[T8] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 25, 0), transaction8.ServiceOccuredUTC);
			AssertEquals("[T8] UserCode", "U16", transaction8.ClientStaffCode);
			AssertEquals("[T8] ItemCount", 1, transaction8.BillableCount);
			AssertEquals("[T8] TransactionReference01", "WH1 - N", transaction8.Reference1);
			AssertEquals("[T8] TransactionReference03", "WD15", transaction8.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction8.AdditionalRefs);

			var transaction9 = FindRowByRef2(transactions, "#15.2");
			AssertEquals("[T9] CompanyCode", "DEM", transaction9.GetCompanyCode());
			AssertEquals("[T9] BranchCode", "DEM", transaction9.GetBranchCode());
			AssertEquals("[T9] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 25, 0), transaction9.ServiceOccuredUTC);
			AssertEquals("[T9] UserCode", "U16", transaction9.ClientStaffCode);
			AssertEquals("[T9] ItemCount", 1, transaction9.BillableCount);
			AssertEquals("[T9] TransactionReference01", "WH1 - N", transaction9.Reference1);
			AssertEquals("[T9] TransactionReference03", "WD15", transaction9.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction9.AdditionalRefs);
		}

		public void TestVersions()
		{
			AssertEquals("22.8.23.90", ScriptToTest.MinCW1Version);
			AssertEquals("24.2.10.69", ScriptToTest.MaxCW1Version);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(Now.Year, Now.Month);
		ZDateTime Now => (now ?? (now = DateTime.Now)).Value;
		ZDateTime? now;
	}
}
