using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(Warehouse3plInwardReturns))]
	sealed class Warehouse3plInwardReturnsTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			var company = GlbCompany.ShallowLoadFromDB(TestConnection, c => c.GC_Code == "DEM").First();
			var branch = GlbBranch.ShallowLoadFromDB(TestConnection, b => b.GB_GC == company.PK).First();
			var orgAddress = OrgAddress.ShallowLoadFromDB(TestConnection).First();
			var orgHeader = OrgHeader.ShallowLoadFromDB(TestConnection).First();

			var whs1 = new WhsWarehouse("WH1", branch.PK, orgAddress.PK) { WW_WarehouseName = "Warehouse 1", WW_IsVirtualWarehouse = false }.WithDockDoor(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch.PK, orgAddress.PK) { WW_WarehouseName = "Warehouse 2", WW_IsVirtualWarehouse = true }.WithDockDoor(TestConnection);

			var order1 = new WhsDocket(orgHeader.PK, whs1.PK, "ORD", "ORD", "", "ORD1")
			{
				WD_SystemCreateTimeUtc = new DateTime(2025, 03, 07),
				WD_SystemCreateUser = "US1",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs1.PK, "INW", "RET", "", "RETWHS1")
			{
				WD_WD_ParentDocket = order1,
				WD_SystemCreateTimeUtc = new DateTime(2025, 03, 07),
				WD_SystemCreateUser = "US2",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs1.PK, "INW", "REC", "", "RECEIVE")
			{
				WD_SystemCreateTimeUtc = new DateTime(2025, 03, 07),
				WD_SystemCreateUser = "US3",
			}.InsertAndReturnObject(TestConnection);

			var order2 = new WhsDocket(orgHeader.PK, whs2.PK, "ORD", "ORD", "", "ORD2")
			{
				WD_SystemCreateTimeUtc = new DateTime(2025, 03, 07),
				WD_SystemCreateUser = "US4",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs2.PK, "INW", "RET", "", "RETWHS2")
			{
				WD_WD_ParentDocket = order2,
				WD_SystemCreateTimeUtc = new DateTime(2025, 03, 07),
				WD_SystemCreateUser = "US5",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs2.PK, "INW", "RET", "", "RETWHS3")
			{
				WD_WD_ParentDocket = order2,
				WD_SystemCreateTimeUtc = new DateTime(2025, 02, 07),
				WD_SystemCreateUser = "US6",
			}.InsertAndReturnObject(TestConnection);

			new WhsDocket(orgHeader.PK, whs1.PK, "INW", "RET", "", "RETWHS4")
			{
				WD_SystemCreateTimeUtc = new DateTime(2025, 03, 07),
				WD_SystemCreateUser = "US7",
			}.InsertAndReturnObject(TestConnection);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByRef2(transactions, "RETWHS1");
			AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2025, 3, 7), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "US2", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "WH1 - N", transaction1.Reference1);
			AssertEquals("TransactionReference02", "RETWHS1", transaction1.Reference2);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction1.AdditionalRefs);

			var transaction2 = FindRowByRef2(transactions, "RETWHS2");
			AssertEquals("CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2025, 3, 7), transaction2.ServiceOccuredUTC);
			AssertEquals("UserCode", "US5", transaction2.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction2.BillableCount);
			AssertEquals("TransactionReference01", "WH2 - Y", transaction2.Reference1);
			AssertEquals("TransactionReference02", "RETWHS2", transaction2.Reference2);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 2\"}"
				, transaction2.AdditionalRefs);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2025, 3);
			}
		}
	}
}
