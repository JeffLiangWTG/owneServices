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
	[TestedType(typeof(Warehouse3plBookedOrderLine_Pre_24_2_10_70))]
	sealed class Warehouse3plBookedOrderLine_Pre_24_2_10_70Test : RefStlScriptWithDefaultsTest
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
			var physicalPick4 = new WhsPick(whs1, "PP4", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 02, 26, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);

			var physicalPick5 = new WhsPick(whs1, "PP5", "FIN") { WP_FinalizedDateUtc = new DateTime(beforeQueryPeriod.Year, beforeQueryPeriod.Month, 01), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var physicalPick6 = new WhsPick(whs1, "PP6", "FIN") { WP_FinalizedDateUtc = new DateTime(afterQueryPeriod.Year, afterQueryPeriod.Month, 01), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var physicalPick7 = new WhsPick(whs1, "PP7", "ENT") { WP_FinalizedDateUtc = null }.InsertAndReturnObject(TestConnection);

			var virtualPick1 = new WhsPick(whs2, "VP1", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 01, 23, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var virtualPick2 = new WhsPick(whs2, "VP2", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 02, 24, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var virtualPick3 = new WhsPick(whs2, "VP3", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 01, 25, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var virtualPick4 = new WhsPick(whs2, "VP4", "FIN") { WP_FinalizedDateUtc = new DateTime(year, month, day, 02, 26, 00), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);

			var virtualPick5 = new WhsPick(whs2, "VP5", "FIN") { WP_FinalizedDateUtc = new DateTime(beforeQueryPeriod.Year, beforeQueryPeriod.Month, 01), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var virtualPick6 = new WhsPick(whs2, "VP6", "FIN") { WP_FinalizedDateUtc = new DateTime(afterQueryPeriod.Year, afterQueryPeriod.Month, 01), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
			var virtualPick7 = new WhsPick(whs2, "VP7", "ENT") { WP_FinalizedDateUtc = null }.InsertAndReturnObject(TestConnection);

			var physicalOrder1 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick1.PK, nowUTC, "ORD", "ORD", "ECO", "DEP", "WDP01"); // pick finalised booked eCommerce order
			var physicalOrder2 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick2.PK, nowUTC, "ORD", "CUS", "ECO", "DEP", "WDP02"); // pick finalised booked eCommerce customs order
			var physicalOrder3 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick3.PK, nowUTC, "ORD", "ORD", "ECO", "DEP", "WDP03"); // pick finalised booked eCommerce order
			var physicalOrder4 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick5.PK, nowUTC, "ORD", "ORD", "ECO", "DEP", "WDP04"); // pick finalised booked eCommerce order, pick finalisation date out of bounds - before query
			var physicalOrder5 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick4.PK, null, "WOR", "ASS", "", "FIN", "WDP05"); // not order
			var physicalOrder6 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick6.PK, nowUTC, "ORD", "ORD", "ECO", "DEP", "WDP06"); // pick finalised booked eCommerce order, pick finalisation date out of bounds - after query
			var physicalOrder7 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick1.PK, nowUTC, "ORD", "ORD", "BLK", "DEP", "WDP07"); // pick finalised booked bulk order
			var physicalOrder8 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick1.PK, nowUTC, "ORD", "ORD", "UNK", "DEP", "WDP08"); // pick finalised booked unknown order
			var physicalOrder9 = CreateDocketForPick(orgHeader.PK, whs1.PK, null, nowUTC, "ORD", "ORD", "", "ENT", "WDP09", isFinalised: false); // unpicked order
			var physicalOrder10 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick2.PK, null, "ORD", "ORD", "ECO", "DEP", "WDP10"); // pick finalised unbooked ecommerce order
			var physicalOrder11 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick2.PK, null, "ORD", "ORD", "UNK", "DEP", "WDP11"); // pick finalised unbooked unknown order
			var physicalOrder12 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick7.PK, null, "ORD", "ORD", "ECO", "ATP", "WDP12"); // finalised booked eCommerce order, pick not finalised
			var pickedOnSaleOrder1 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick1.PK, nowUTC, "ORD", "ORD", "ECO", "DEP", "WDP13"); // pick finalised booked eCommerce, picked on sale order
			var pickedOnSaleOrder2 = CreateDocketForPick(orgHeader.PK, whs1.PK, physicalPick3.PK, nowUTC, "ORD", "ORD", "ECO", "DEP", "WDP14"); // pick finalised bulk picked on sale order, without receive on pick

			CreateDocketLineForDocket(physicalOrder1, "U01", "FIN", 11, 1);
			CreateDocketLineForDocket(physicalOrder1, "U02", "FIN", 11, 2);
			CreateDocketLineForDocket(physicalOrder2, "U03", "FIN", 12, 1);
			CreateDocketLineForDocket(physicalOrder3, "U04", "FIN", 13, 1);
			CreateDocketLineForDocket(physicalOrder4, "U05", "FIN", 14, 1);
			CreateDocketLineForDocket(physicalOrder5, "U06", "FIN", 15, 1);
			CreateDocketLineForDocket(physicalOrder6, "U07", "FIN", 16, 1);
			CreateDocketLineForDocket(physicalOrder7, "U08", "FIN", 17, 1);
			CreateDocketLineForDocket(physicalOrder8, "U09", "FIN", 18, 1);
			CreateDocketLineForDocket(physicalOrder9, "U010", "", 19, 1, isFinalised: false);
			CreateDocketLineForDocket(physicalOrder10, "U11", "FIN", 110, 1);
			CreateDocketLineForDocket(physicalOrder11, "U12", "FIN", 111, 1);
			CreateDocketLineForDocket(physicalOrder12, "U13", "FIN", 112, 1);

			var parentLine = CreateDocketLineForDocket(pickedOnSaleOrder1, "U14", "FIN", 30, 1);
			var componentLine = CreateDocketLineForDocket(pickedOnSaleOrder1, "U14", "FIN", 30, 2, parentLine: parentLine);
			CreateReceiveFromPickByBOM(orgHeader.PK, whs1, "REC01", physicalPick1);

			var parentLine2 = CreateDocketLineForDocket(pickedOnSaleOrder2, "U15", "FIN", 31, 1);
			var componentLine2 = CreateDocketLineForDocket(pickedOnSaleOrder2, "U15", "FIN", 31, 2, parentLine: parentLine2);

			var virtualOrder1 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick1.PK, nowUTC, "ORD", "ORD", "ECO", "DEP", "WDV01"); // pick finalised booked eCommerce order
			var virtualOrder2 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick2.PK, nowUTC, "ORD", "CUS", "ECO", "DEP", "WDV02"); // pick finalised booked eCommerce customs order
			var virtualOrder3 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick3.PK, nowUTC, "ORD", "ORD", "ECO", "DEP", "WDV03"); // pick finalised booked eCommerce order
			var virtualOrder4 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick5.PK, nowUTC, "ORD", "ORD", "ECO", "DEP", "WDV04"); // pick finalised booked eCommerce order, pick finalisation date out of bounds - before query
			var virtualOrder5 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick4.PK, null, "WOR", "ASS", "", "FIN", "WDV05"); // not order
			var virtualOrder6 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick6.PK, nowUTC, "ORD", "ORD", "ECO", "DEP", "WDV06"); // pick finalised booked eCommerce order, pick finalisation date out of bounds - after query
			var virtualOrder7 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick1.PK, nowUTC, "ORD", "ORD", "BLK", "DEP", "WDV07"); // pick finalised booked bulk order
			var virtualOrder8 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick1.PK, nowUTC, "ORD", "ORD", "UNK", "DEP", "WDV08"); // pick finalised booked unknown order
			var virtualOrder9 = CreateDocketForPick(orgHeader.PK, whs2.PK, null, nowUTC, "ORD", "ORD", "", "ENT", "WDV09", isFinalised: false); // unpicked order
			var virtualOrder10 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick2.PK, null, "ORD", "ORD", "ECO", "DEP", "WDV10"); // pick finalised unbooked ecommerce order
			var virtualOrder11 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick2.PK, null, "ORD", "ORD", "UNK", "DEP", "WDV11"); // pick finalised unbooked unknown order
			var virtualOrder12 = CreateDocketForPick(orgHeader.PK, whs2.PK, virtualPick7.PK, null, "ORD", "ORD", "ECO", "ATP", "WDV12"); // finalised booked eCommerce order, pick not finalised

			CreateDocketLineForDocket(virtualOrder1, "U01", "FIN", 21, 1);
			CreateDocketLineForDocket(virtualOrder1, "U02", "FIN", 21, 2);
			CreateDocketLineForDocket(virtualOrder2, "U03", "FIN", 22, 1);
			CreateDocketLineForDocket(virtualOrder3, "U04", "FIN", 23, 1);
			CreateDocketLineForDocket(virtualOrder4, "U05", "FIN", 24, 1);
			CreateDocketLineForDocket(virtualOrder5, "U06", "FIN", 25, 1);
			CreateDocketLineForDocket(virtualOrder6, "U07", "FIN", 26, 1);
			CreateDocketLineForDocket(virtualOrder7, "U08", "FIN", 27, 1);
			CreateDocketLineForDocket(virtualOrder8, "U09", "FIN", 28, 1);
			CreateDocketLineForDocket(virtualOrder9, "U10", "", 29, 1, isFinalised: false);
			CreateDocketLineForDocket(virtualOrder10, "U11", "FIN", 210, 1);
			CreateDocketLineForDocket(virtualOrder11, "U12", "FIN", 211, 1);
			CreateDocketLineForDocket(virtualOrder12, "U13", "FIN", 212, 1);

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
			AssertEquals("Number of Transactions", 12, transactions.Count());

			var transaction1 = FindRowByRef2(transactions, "#11.1");
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 23, 0), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "U01", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference01", "WH1 - N", transaction1.Reference1);
			AssertEquals("[T1] TransactionReference03", "WDP01", transaction1.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction1.AdditionalRefs);

			var transaction2 = FindRowByRef2(transactions, "#11.2");
			AssertEquals("[T2] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 23, 0), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "U02", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference01", "WH1 - N", transaction1.Reference1);
			AssertEquals("[T2] TransactionReference03", "WDP01", transaction2.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction2.AdditionalRefs);

			var transaction3 = FindRowByRef2(transactions, "#18.1");
			AssertEquals("[T3] CompanyCode", "DEM", transaction3.GetCompanyCode());
			AssertEquals("[T3] BranchCode", "DEM", transaction3.GetBranchCode());
			AssertEquals("[T3] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 23, 0), transaction3.ServiceOccuredUTC);
			AssertEquals("[T3] UserCode", "U09", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);
			AssertEquals("[T3] TransactionReference01", "WH1 - N", transaction3.Reference1);
			AssertEquals("[T3] TransactionReference03", "WDP08", transaction3.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction3.AdditionalRefs);

			var transaction4 = FindRowByRef2(transactions, "#13.1");
			AssertEquals("[T4] CompanyCode", "DEM", transaction4.GetCompanyCode());
			AssertEquals("[T4] BranchCode", "DEM", transaction4.GetBranchCode());
			AssertEquals("[T4] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 25, 0), transaction4.ServiceOccuredUTC);
			AssertEquals("[T4] UserCode", "U04", transaction4.ClientStaffCode);
			AssertEquals("[T4] ItemCount", 1, transaction4.BillableCount);
			AssertEquals("[T4] TransactionReference01", "WH1 - N", transaction4.Reference1);
			AssertEquals("[T4] TransactionReference03", "WDP03", transaction4.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction4.AdditionalRefs);

			var transaction5 = FindRowByRef2(transactions, "#21.1");
			AssertEquals("[T5] CompanyCode", "DEM", transaction5.GetCompanyCode());
			AssertEquals("[T5] BranchCode", "DEM", transaction5.GetBranchCode());
			AssertEquals("[T5] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 23, 0), transaction5.ServiceOccuredUTC);
			AssertEquals("[T5] UserCode", "U01", transaction5.ClientStaffCode);
			AssertEquals("[T5] ItemCount", 1, transaction5.BillableCount);
			AssertEquals("[T5] TransactionReference01", "WH2 - Y", transaction5.Reference1);
			AssertEquals("[T5] TransactionReference03", "WDV01", transaction5.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 2\"}"
				, transaction5.AdditionalRefs);

			var transaction6 = FindRowByRef2(transactions, "#21.2");
			AssertEquals("[T6] CompanyCode", "DEM", transaction6.GetCompanyCode());
			AssertEquals("[T6] BranchCode", "DEM", transaction6.GetBranchCode());
			AssertEquals("[T6] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 23, 0), transaction6.ServiceOccuredUTC);
			AssertEquals("[T6] UserCode", "U02", transaction6.ClientStaffCode);
			AssertEquals("[T6] ItemCount", 1, transaction6.BillableCount);
			AssertEquals("[T6] TransactionReference01", "WH2 - Y", transaction6.Reference1);
			AssertEquals("[T6] TransactionReference03", "WDV01", transaction6.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 2\"}"
				, transaction6.AdditionalRefs);

			var transaction7 = FindRowByRef2(transactions, "#28.1");
			AssertEquals("[T7] CompanyCode", "DEM", transaction7.GetCompanyCode());
			AssertEquals("[T7] BranchCode", "DEM", transaction7.GetBranchCode());
			AssertEquals("[T7] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 23, 0), transaction7.ServiceOccuredUTC);
			AssertEquals("[T7] UserCode", "U09", transaction7.ClientStaffCode);
			AssertEquals("[T7] ItemCount", 1, transaction7.BillableCount);
			AssertEquals("[T7] TransactionReference01", "WH2 - Y", transaction7.Reference1);
			AssertEquals("[T7] TransactionReference03", "WDV08", transaction7.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 2\"}"
				, transaction7.AdditionalRefs);

			var transaction8 = FindRowByRef2(transactions, "#23.1");
			AssertEquals("[T8] CompanyCode", "DEM", transaction8.GetCompanyCode());
			AssertEquals("[T8] BranchCode", "DEM", transaction8.GetBranchCode());
			AssertEquals("[T8] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 25, 0), transaction8.ServiceOccuredUTC);
			AssertEquals("[T8] UserCode", "U04", transaction8.ClientStaffCode);
			AssertEquals("[T8] ItemCount", 1, transaction8.BillableCount);
			AssertEquals("[T8] TransactionReference01", "WH2 - Y", transaction8.Reference1);
			AssertEquals("[T8] TransactionReference03", "WDV03", transaction8.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 2\"}"
				, transaction8.AdditionalRefs);

			var transaction9 = FindRowByRef2(transactions, "#30.1");
			AssertEquals("[T9] CompanyCode", "DEM", transaction9.GetCompanyCode());
			AssertEquals("[T9] BranchCode", "DEM", transaction9.GetBranchCode());
			AssertEquals("[T9] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 23, 0), transaction9.ServiceOccuredUTC);
			AssertEquals("[T9] UserCode", "U14", transaction9.ClientStaffCode);
			AssertEquals("[T9] ItemCount", 1, transaction9.BillableCount);
			AssertEquals("[T9] TransactionReference01", "WH1 - N", transaction9.Reference1);
			AssertEquals("[T9] TransactionReference03", "WDP13", transaction9.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction9.AdditionalRefs);

			var transaction10 = FindRowByRef2(transactions, "#30.2");
			AssertEquals("[T10] CompanyCode", "DEM", transaction10.GetCompanyCode());
			AssertEquals("[T10] BranchCode", "DEM", transaction10.GetBranchCode());
			AssertEquals("[T10] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 23, 0), transaction10.ServiceOccuredUTC);
			AssertEquals("[T10] UserCode", "U14", transaction10.ClientStaffCode);
			AssertEquals("[T10] ItemCount", 1, transaction10.BillableCount);
			AssertEquals("[T10] TransactionReference01", "WH1 - N", transaction10.Reference1);
			AssertEquals("[T10] TransactionReference03", "WDP13", transaction10.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction10.AdditionalRefs);

			var transaction11 = FindRowByRef2(transactions, "#31.1");
			AssertEquals("[T11] CompanyCode", "DEM", transaction11.GetCompanyCode());
			AssertEquals("[T11] BranchCode", "DEM", transaction11.GetBranchCode());
			AssertEquals("[T11] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 25, 0), transaction11.ServiceOccuredUTC);
			AssertEquals("[T11] UserCode", "U15", transaction11.ClientStaffCode);
			AssertEquals("[T11] ItemCount", 1, transaction11.BillableCount);
			AssertEquals("[T11] TransactionReference01", "WH1 - N", transaction11.Reference1);
			AssertEquals("[T11] TransactionReference03", "WDP14", transaction11.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction11.AdditionalRefs);

			var transaction12 = FindRowByRef2(transactions, "#31.2");
			AssertEquals("[T12] CompanyCode", "DEM", transaction12.GetCompanyCode());
			AssertEquals("[T12] BranchCode", "DEM", transaction12.GetBranchCode());
			AssertEquals("[T12] TransactionDateUtc", new DateTime(Now.Year, Now.Month, Now.Day, 1, 25, 0), transaction12.ServiceOccuredUTC);
			AssertEquals("[T12] UserCode", "U15", transaction12.ClientStaffCode);
			AssertEquals("[T12] ItemCount", 1, transaction12.BillableCount);
			AssertEquals("[T12] TransactionReference01", "WH1 - N", transaction12.Reference1);
			AssertEquals("[T12] TransactionReference03", "WDP14", transaction12.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction12.AdditionalRefs);
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
