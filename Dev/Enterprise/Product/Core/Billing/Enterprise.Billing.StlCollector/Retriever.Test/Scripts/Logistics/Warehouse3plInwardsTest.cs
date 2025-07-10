using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(Warehouse3plInwards))]
	sealed class Warehouse3plInwardsTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			var company = GlbCompany.ShallowLoadFromDB(TestConnection, c => c.GC_Code == "DEM").First();
			var branch = GlbBranch.ShallowLoadFromDB(TestConnection, b => b.GB_GC == company.PK).First();
			var orgAddress = OrgAddress.ShallowLoadFromDB(TestConnection).First();
			var orgHeader = OrgHeader.ShallowLoadFromDB(TestConnection).First();

			var whs1 = new WhsWarehouse("WH1", branch.PK, orgAddress.PK) { WW_WarehouseName = "Warehouse 1", WW_IsVirtualWarehouse = false }.WithDockDoor(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch.PK, orgAddress.PK) { WW_WarehouseName = "Warehouse 2", WW_IsVirtualWarehouse = true }.WithDockDoor(TestConnection);

			var workOrder1 = new WhsDocket(orgHeader.PK, whs1.PK, "WOR", "ASS", "", "WDP01")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 09, 01),
				WD_SystemCreateUser = "US1",
			}.InsertAndReturnObject(TestConnection);

			var receive1 = new WhsDocket(orgHeader.PK, whs1.PK, "INW", "REC", "", "WDP02")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 08, 01), // Out of Range for collector
				WD_SystemCreateUser = "US2",
			}.InsertAndReturnObject(TestConnection);

			var dynamicWorkOrder1 = new WhsDocket(orgHeader.PK, whs1.PK, "WOR", "ASS", "", "WDP03")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 09, 01),
				WD_SystemCreateUser = "US3",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs1.PK, "INW", "CUS", "", "WDP04")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 09, 01),
				WD_SystemCreateUser = "US4",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs1.PK, "INW", "REC", "", "WDP05")
			{
				WD_WD_ParentDocket = workOrder1,
				WD_SystemCreateTimeUtc = new DateTime(2012, 09, 01),
				WD_SystemCreateUser = "US5",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs1.PK, "INW", "REC", "", "WDP06")
			{
				WD_WD_ParentDocket = receive1,
				WD_SystemCreateTimeUtc = new DateTime(2012, 09, 01),
				WD_SystemCreateUser = "US6",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs1.PK, "INW", "REC", "", "WDP07")
			{
				WD_WD_ParentDocket = dynamicWorkOrder1,
				WD_SystemCreateTimeUtc = new DateTime(2012, 09, 01),
				WD_SystemCreateUser = "US7",
			}.InsertAndReturnObject(TestConnection);

			CreateReceiveFromPickByBOM(orgHeader.PK, whs1, "WDP08");

			var workOrder2 = new WhsDocket(orgHeader.PK, whs2.PK, "WOR", "ASS", "", "WDV01")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 09, 01),
				WD_SystemCreateUser = "US1",
			}.InsertAndReturnObject(TestConnection);

			var receive2 = new WhsDocket(orgHeader.PK, whs2.PK, "INW", "REC", "", "WDV02")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 08, 01), // Out of Range for collector
				WD_SystemCreateUser = "US2",
			}.InsertAndReturnObject(TestConnection);

			var dynamicWorkOrder2 = new WhsDocket(orgHeader.PK, whs2.PK, "WOR", "ASS", "", "WDV03")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 09, 01),
				WD_SystemCreateUser = "US3",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs2.PK, "INW", "CUS", "", "WDV04")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 09, 01),
				WD_SystemCreateUser = "US4",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs2.PK, "INW", "REC", "", "WDV05")
			{
				WD_WD_ParentDocket = workOrder2,
				WD_SystemCreateTimeUtc = new DateTime(2012, 09, 01),
				WD_SystemCreateUser = "US5",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs2.PK, "INW", "REC", "", "WDV06")
			{
				WD_WD_ParentDocket = receive2,
				WD_SystemCreateTimeUtc = new DateTime(2012, 09, 01),
				WD_SystemCreateUser = "US6",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs2.PK, "INW", "REC", "", "WDV07")
			{
				WD_WD_ParentDocket = dynamicWorkOrder2,
				WD_SystemCreateTimeUtc = new DateTime(2012, 09, 01),
				WD_SystemCreateUser = "US7",
			}.InsertAndReturnObject(TestConnection);

			var order = new WhsDocket(orgHeader.PK, whs1.PK, "ORD", "ORD", "", "ORD1")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 09, 01),
				WD_SystemCreateUser = "US1",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs1.PK, "INW", "RET", "", "RETURN1")
			{
				WD_WD_ParentDocket = order,
				WD_SystemCreateTimeUtc = new DateTime(2012, 09, 01),
				WD_SystemCreateUser = "US2",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs1.PK, "INW", "RET", "", "RETURN2")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 09, 01),
				WD_SystemCreateUser = "US3",
			}.InsertAndReturnObject(TestConnection);
		}

		void CreateReceiveFromPickByBOM(Guid clientPK, WhsWarehouse whs, string reference)
		{
			var pick = new WhsPick(whs, "P01", "NEW").InsertAndReturnObject(TestConnection);
			new WhsDocket(clientPK, whs.PK, "INW", "REC", "", reference)
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 09, 01),
				WD_SystemCreateUser = "US8",
				WD_WP_ParentPickForReceive = pick
			}.InsertAndReturnObject(TestConnection);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());

			var transaction1 = FindRowByRef2(transactions, "WDP06");
			AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 9, 1), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "US6", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "WH1 - N", transaction1.Reference1);
			AssertEquals("TransactionReference02", "WDP06", transaction1.Reference2);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction1.AdditionalRefs);

			var transaction2 = FindRowByRef2(transactions, "WDV06");
			AssertEquals("CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 9, 1), transaction2.ServiceOccuredUTC);
			AssertEquals("UserCode", "US6", transaction2.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction2.BillableCount);
			AssertEquals("TransactionReference01", "WH2 - Y", transaction2.Reference1);
			AssertEquals("TransactionReference02", "WDV06", transaction2.Reference2);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 2\"}"
				, transaction2.AdditionalRefs);

			var transaction3 = FindRowByRef2(transactions, "RETURN2");
			AssertEquals("CompanyCode", "DEM", transaction3.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction3.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 9, 1), transaction3.ServiceOccuredUTC);
			AssertEquals("UserCode", "US3", transaction3.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction3.BillableCount);
			AssertEquals("TransactionReference01", "WH1 - N", transaction3.Reference1);
			AssertEquals("TransactionReference02", "RETURN2", transaction3.Reference2);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction3.AdditionalRefs);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2012, 9);
			}
		}
	}
}
