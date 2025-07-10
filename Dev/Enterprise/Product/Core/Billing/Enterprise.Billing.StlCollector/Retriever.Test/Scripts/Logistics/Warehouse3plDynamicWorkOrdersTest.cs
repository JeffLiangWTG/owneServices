using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(Warehouse3plDynamicWorkOrders))]
	class Warehouse3plDynamicWorkOrdersTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			var company = GlbCompany.ShallowLoadFromDB(TestConnection, c => c.GC_Code == "DEM").First();
			var branch = GlbBranch.ShallowLoadFromDB(TestConnection, b => b.GB_GC == company.PK).First();
			var orgAddress = OrgAddress.ShallowLoadFromDB(TestConnection).First();
			var orgHeader = OrgHeader.ShallowLoadFromDB(TestConnection).First();

			var whs1 = new WhsWarehouse("WH1", branch.PK, orgAddress.PK) { WW_WarehouseName = "Warehouse 1", WW_IsVirtualWarehouse = false }.WithDockDoor(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch.PK, orgAddress.PK) { WW_WarehouseName = "Warehouse 2", WW_IsVirtualWarehouse = true }.WithDockDoor(TestConnection);

			new WhsDocket(orgHeader.PK, whs1.PK, "DWO", "ASS", "", "WD00")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 07, 01), // Out of Range for collector
				WD_SystemCreateUser = "US1",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs1.PK, "DWO", "ASS", "", "WD01")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 08, 01),
				WD_SystemCreateUser = "US1",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs1.PK, "DWO", "ASS", "", "WD02")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 09, 01), // Out of Range for collector
				WD_SystemCreateUser = "US1",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs1.PK, "WOR", "ASS", "", "WD03")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 08, 02),
				WD_SystemCreateUser = "US2",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs1.PK, "WOR", "DIS", "", "WD04")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 08, 03),
				WD_SystemCreateUser = "US3",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs1.PK, "INW", "CUS", "", "WD05")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 08, 04),
				WD_SystemCreateUser = "US4",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs2.PK, "DWO", "ASS", "", "WD06")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 07, 01), // Out of Range for collector
				WD_SystemCreateUser = "US1",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs2.PK, "DWO", "ASS", "", "WD07")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 08, 01),
				WD_SystemCreateUser = "US1",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs2.PK, "DWO", "ASS", "", "WD08")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 09, 01), // Out of Range for collector
				WD_SystemCreateUser = "US1",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs2.PK, "WOR", "ASS", "", "WD09")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 08, 02),
				WD_SystemCreateUser = "US2",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs2.PK, "WOR", "DIS", "", "WD10")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 08, 03),
				WD_SystemCreateUser = "US3",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs2.PK, "INW", "CUS", "", "WD11")
			{
				WD_SystemCreateTimeUtc = new DateTime(2012, 08, 04),
				WD_SystemCreateUser = "US4",
			}.InsertAndReturnObject(TestConnection);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByRef2(transactions, "WD01");
			AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 8, 1), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "US1", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "WH1 - N", transaction1.Reference1);
			AssertEquals("TransactionReference03", "ASS", transaction1.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction1.AdditionalRefs);

			var transaction2 = FindRowByRef2(transactions, "WD07");
			AssertEquals("CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 8, 1), transaction2.ServiceOccuredUTC);
			AssertEquals("UserCode", "US1", transaction2.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction2.BillableCount);
			AssertEquals("TransactionReference01", "WH2 - Y", transaction2.Reference1);
			AssertEquals("TransactionReference03", "ASS", transaction2.Reference3);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 2\"}"
				, transaction2.AdditionalRefs);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2012, 8);
			}
		}
	}
}
