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
	[TestedType(typeof(Warehouse3plPickByBOMOrderLine))]
	sealed class Warehouse3plPickByBOMOrderLineTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			var nowUTC = DateTime.UtcNow;
			var year = Now.Year;
			var month = Now.Month;
			var day = Now.Day;
			var beforeQueryPeriod = Now.AddMonths(-2);
			var afterQueryPeriod = Now.AddMonths(1);

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
			var physicalPick8 = new WhsPick(whs1, "P8", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 01, 24, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var virtualPick1 = new WhsPick(whs2, "P9", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 01, 25, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);

			var physicalOrder1 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick1.PK, null, "ORD", "ORD", "BLK", "DEP", "WD01"); // pick finalised bulk order
			var physicalOrder2 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick2.PK, null, "ORD", "CUS", "BLK", "DEP", "WD02"); // pick finalised bulk customs order
			var physicalOrder3 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick3.PK, null, "ORD", "ORD", "BLK", "DEP", "WD03"); // pick finalised bulk order
			var physicalOrder4 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick5.PK, null, "ORD", "ORD", "BLK", "DEP", "WD04"); // pick finalised bulk order, pick finalisation date out of bounds - before query
			var physicalOrder5 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick4.PK, null, "WOR", "ASS", "", "FIN", "WD05"); // not order
			var physicalOrder6 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick6.PK, null, "ORD", "ORD", "BLK", "DEP", "WD06"); // pick finalised bulk order, pick finalisation date out of bounds - after query
			var physicalOrder7 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick1.PK, null, "ORD", "ORD", "ECO", "DEP", "WD07"); // pick finalised eCommerce order
			var physicalOrder8 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick1.PK, null, "ORD", "ORD", "UNK", "DEP", "WD08"); // pick finalised unknown order
			var physicalOrder9 = CreateDocketForPick(orgHeader.PK, whs1.PK, null, nowUTC, "ORD", "ORD", "", "ENT", "WD09", isFinalised: false); // unpicked order
			var physicalOrder10 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick8.PK, nowUTC, "ORD", "ORD", "BLK", "DEP", "WD10"); // pick finalised booked ecommerce order
			var virtualOrder1 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick1.PK, null, "ORD", "ORD", "BLK", "DEP", "WD11"); // virtual pick finalised bulk order
			var pickedOnSaleOrder1 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick1.PK, null, "ORD", "ORD", "BLK", "DEP", "WD12"); // pick finalised bulk picked on sale order
			var pickedOnSaleOrder2 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick1.PK, null, "ORD", "ORD", "ECO", "DEP", "WD13"); // pick finalised economic picked on sale order
			var pickedOnSaleOrder3 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick1.PK, null, "ORD", "ORD", "UNK", "DEP", "WD14"); // pick finalised bulk picked on sale order
			var pickedOnSaleOrder99 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick5.PK, null, "ORD", "ORD", "BLK", "DEP", "WD99"); // pick finalised bulk picked on sale order, pick finalisation date out of bounds - before query
			var pickedOnSaleOrder98 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick6.PK, null, "ORD", "ORD", "BLK", "DEP", "WD98"); // pick finalised bulk picked on sale order, pick finalisation date out of bounds - after query
			var pickedOnSaleOrder97 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick3.PK, null, "ORD", "ORD", "BLK", "DEP", "WD97"); // pick finalised bulk picked on sale order, without receive on pick
			var virtualOrder2 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick1.PK, null, "ORD", "ORD", "BLK", "DEP", "WD15"); // pick finalised bulk picked on sale order
			var virtualOrder3 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick1.PK, null, "ORD", "ORD", "ECO", "DEP", "WD16"); // pick finalised economic picked on sale order
			var virtualOrder4 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick1.PK, null, "ORD", "ORD", "UNK", "DEP", "WD17"); // pick finalised bulk picked on sale order

			CreateDocketLineForDocket(physicalOrder1, "U01", "DEP", 1, 1);
			CreateDocketLineForDocket(physicalOrder1, "U02", "DEP", 1, 2);
			CreateDocketLineForDocket(physicalOrder2, "U03", "DEP", 2, 1);
			CreateDocketLineForDocket(physicalOrder3, "U04", "DEP", 3, 1);
			CreateDocketLineForDocket(physicalOrder4, "U05", "DEP", 4, 1);
			CreateDocketLineForDocket(physicalOrder5, "U06", "FIN", 5, 1);
			CreateDocketLineForDocket(physicalOrder6, "U07", "DEP", 6, 1);
			CreateDocketLineForDocket(physicalOrder7, "U08", "DEP", 7, 1);
			CreateDocketLineForDocket(physicalOrder8, "U09", "DEP", 8, 1);
			CreateDocketLineForDocket(physicalOrder9, "U10", "", 9, 1, isFinalised: false);
			CreateDocketLineForDocket(physicalOrder10, "U11", "DEP", 10, 1);
			CreateDocketLineForDocket(virtualOrder1, "U12", "DEP", 11, 1);

			var parentLine1 = CreateDocketLineForDocket(pickedOnSaleOrder1, "U13", "DEP", 12, 1);
			var componentLine1 = CreateDocketLineForDocket(pickedOnSaleOrder1, "U13", "DEP", 12, 2, parentLine: parentLine1);
			CreateReceiveFromPickByBOM(orgHeader.PK, whs1, "REC01", physicalPick1);

			var parentLine2 = CreateDocketLineForDocket(pickedOnSaleOrder2, "U14", "DEP", 13, 1);
			var componentLine2 = CreateDocketLineForDocket(pickedOnSaleOrder2, "U14", "DEP", 13, 2, parentLine: parentLine2);

			var parentLine3 = CreateDocketLineForDocket(pickedOnSaleOrder3, "U15", "DEP", 14, 1);
			var componentLine3 = CreateDocketLineForDocket(pickedOnSaleOrder3, "U15", "DEP", 14, 2, parentLine: parentLine3);
			var componentLine4 = CreateDocketLineForDocket(pickedOnSaleOrder3, "U15", "DEP", 14, 3, parentLine: parentLine3);

			var parentLine99 = CreateDocketLineForDocket(pickedOnSaleOrder99, "U99", "DEP", 99, 1);
			var componentLine99 = CreateDocketLineForDocket(pickedOnSaleOrder99, "U99", "DEP", 99, 2, parentLine: parentLine99);
			CreateReceiveFromPickByBOM(orgHeader.PK, whs1, "REC99", physicalPick5);

			var parentLine98 = CreateDocketLineForDocket(pickedOnSaleOrder98, "U98", "DEP", 98, 1);
			var componentLine98 = CreateDocketLineForDocket(pickedOnSaleOrder98, "U98", "DEP", 98, 2, parentLine: parentLine98);
			CreateReceiveFromPickByBOM(orgHeader.PK, whs1, "REC98", physicalPick6);

			var parentLine97 = CreateDocketLineForDocket(pickedOnSaleOrder97, "U97", "DEP", 97, 1);
			var componentLine97 = CreateDocketLineForDocket(pickedOnSaleOrder97, "U97", "DEP", 97, 2, parentLine: parentLine97);

			var parentLine4 = CreateDocketLineForDocket(virtualOrder2, "U16", "DEP", 15, 1);
			var componentLine5 = CreateDocketLineForDocket(virtualOrder2, "U16", "DEP", 15, 2, parentLine: parentLine4);
			CreateReceiveFromPickByBOM(orgHeader.PK, whs2, "REC02", virtualPick1);

			var parentLine5 = CreateDocketLineForDocket(virtualOrder3, "U17", "DEP", 16, 1);
			var componentLine6 = CreateDocketLineForDocket(virtualOrder3, "U17", "DEP", 16, 2, parentLine: parentLine5);

			var parentLine6 = CreateDocketLineForDocket(virtualOrder4, "U18", "DEP", 17, 1);
			var componentLine7 = CreateDocketLineForDocket(virtualOrder4, "U18", "DEP", 17, 2, parentLine: parentLine6);
			var componentLine8 = CreateDocketLineForDocket(virtualOrder4, "U18", "DEP", 17, 3, parentLine: parentLine6);

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

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 8, transactions.Count());

			var transaction1 = FindRowByRef2(transactions, "#12.2");
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 23, 0), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "U13", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference01", "WH1 - N", transaction1.Reference1);
			AssertEquals("[T1] TransactionReference03", "WD12", transaction1.Reference3);

			var transaction2 = FindRowByRef2(transactions, "#13.2");
			AssertEquals("[T2] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 23, 0), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "U14", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference01", "WH1 - N", transaction2.Reference1);
			AssertEquals("[T2] TransactionReference03", "WD13", transaction2.Reference3);

			var transaction3 = FindRowByRef2(transactions, "#14.2");
			AssertEquals("[T3] CompanyCode", "DEM", transaction3.GetCompanyCode());
			AssertEquals("[T3] BranchCode", "DEM", transaction3.GetBranchCode());
			AssertEquals("[T3] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 23, 0), transaction3.ServiceOccuredUTC);
			AssertEquals("[T3] UserCode", "U15", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);
			AssertEquals("[T3] TransactionReference01", "WH1 - N", transaction3.Reference1);
			AssertEquals("[T3] TransactionReference03", "WD14", transaction3.Reference3);

			var transaction4 = FindRowByRef2(transactions, "#14.3");
			AssertEquals("[T4] CompanyCode", "DEM", transaction4.GetCompanyCode());
			AssertEquals("[T4] BranchCode", "DEM", transaction4.GetBranchCode());
			AssertEquals("[T4] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 23, 0), transaction4.ServiceOccuredUTC);
			AssertEquals("[T4] UserCode", "U15", transaction4.ClientStaffCode);
			AssertEquals("[T4] ItemCount", 1, transaction4.BillableCount);
			AssertEquals("[T4] TransactionReference01", "WH1 - N", transaction4.Reference1);
			AssertEquals("[T4] TransactionReference03", "WD14", transaction4.Reference3);

			var transaction5 = FindRowByRef2(transactions, "#15.2");
			AssertEquals("[T5] CompanyCode", "DEM", transaction5.GetCompanyCode());
			AssertEquals("[T5] BranchCode", "DEM", transaction5.GetBranchCode());
			AssertEquals("[T5] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 25, 0), transaction5.ServiceOccuredUTC);
			AssertEquals("[T5] UserCode", "U16", transaction5.ClientStaffCode);
			AssertEquals("[T5] ItemCount", 1, transaction5.BillableCount);
			AssertEquals("[T5] TransactionReference01", "WH2 - Y", transaction5.Reference1);
			AssertEquals("[T5] TransactionReference03", "WD15", transaction5.Reference3);

			var transaction6 = FindRowByRef2(transactions, "#16.2");
			AssertEquals("[T6] CompanyCode", "DEM", transaction6.GetCompanyCode());
			AssertEquals("[T6] BranchCode", "DEM", transaction6.GetBranchCode());
			AssertEquals("[T6] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 25, 0), transaction6.ServiceOccuredUTC);
			AssertEquals("[T6] UserCode", "U17", transaction6.ClientStaffCode);
			AssertEquals("[T6] ItemCount", 1, transaction6.BillableCount);
			AssertEquals("[T6] TransactionReference01", "WH2 - Y", transaction6.Reference1);
			AssertEquals("[T6] TransactionReference03", "WD16", transaction6.Reference3);

			var transaction7 = FindRowByRef2(transactions, "#17.2");
			AssertEquals("[T7] CompanyCode", "DEM", transaction7.GetCompanyCode());
			AssertEquals("[T7] BranchCode", "DEM", transaction7.GetBranchCode());
			AssertEquals("[T7] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 25, 0), transaction7.ServiceOccuredUTC);
			AssertEquals("[T7] UserCode", "U18", transaction7.ClientStaffCode);
			AssertEquals("[T7] ItemCount", 1, transaction7.BillableCount);
			AssertEquals("[T7] TransactionReference01", "WH2 - Y", transaction7.Reference1);
			AssertEquals("[T7] TransactionReference03", "WD17", transaction7.Reference3);

			var transaction8 = FindRowByRef2(transactions, "#17.3");
			AssertEquals("[T8] CompanyCode", "DEM", transaction8.GetCompanyCode());
			AssertEquals("[T8] BranchCode", "DEM", transaction8.GetBranchCode());
			AssertEquals("[T8] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 25, 0), transaction8.ServiceOccuredUTC);
			AssertEquals("[T8] UserCode", "U18", transaction8.ClientStaffCode);
			AssertEquals("[T8] ItemCount", 1, transaction8.BillableCount);
			AssertEquals("[T8] TransactionReference01", "WH2 - Y", transaction8.Reference1);
			AssertEquals("[T8] TransactionReference03", "WD17", transaction8.Reference3);
		}

		public void TestVersions()
		{
			AssertEquals("24.2.10.70", ScriptToTest.MinCW1Version);
			AssertEquals("", ScriptToTest.MaxCW1Version);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(Now.Year, Now.Month);
		ZDateTime Now => (now ?? (now = DateTime.Now)).Value;
		ZDateTime? now;
	}
}
