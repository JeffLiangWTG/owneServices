using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.Integration.Billing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(Warehouse3plCycleCountLocation))]
	sealed class Warehouse3plCycleCountLocationTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			var today = DateTimeOffset.Now;

			var company = GlbCompany.ShallowLoadFromDB(TestConnection, c => c.GC_Code == "DEM").First();
			var branch = GlbBranch.ShallowLoadFromDB(TestConnection, b => b.GB_GC == company.PK).First();
			var orgAddress = OrgAddress.ShallowLoadFromDB(TestConnection).First();
			var orgHeader = OrgHeader.ShallowLoadFromDB(TestConnection).First();
			var ddlLocationType = WhsLocationType.ShallowLoadFromDB(TestConnection, lt => lt.WLT_LocationClass == "DDL")[0];

			var whs1 = new WhsWarehouse("WH1", branch.PK, orgAddress.PK) { WW_WarehouseName = "Warehouse 1", WW_IsVirtualWarehouse = false }.WithDockDoor(TestConnection);
			var area1 = new WhsArea(whs1.PK, "SV1").InsertAndReturnObject(TestConnection);
			var row1 = new WhsRow(whs1, "DDL").InsertAndReturnObject(TestConnection);
			var location1 = new WhsLocation(row1.PK, area1.PK, area1.PK, ddlLocationType.PK).InsertAndReturnObject(TestConnection);

			var whs2 = new WhsWarehouse("WH2", branch.PK, orgAddress.PK) { WW_WarehouseName = "Warehouse 2", WW_IsVirtualWarehouse = true }.WithDockDoor(TestConnection);
			var area2 = new WhsArea(whs2.PK, "SV2").InsertAndReturnObject(TestConnection);
			var row2 = new WhsRow(whs2, "DDL").InsertAndReturnObject(TestConnection);
			var location2 = new WhsLocation(row2.PK, area2.PK, area2.PK, ddlLocationType.PK).InsertAndReturnObject(TestConnection);

			new WhsCycleCountLocation(location1.PK, "PID")
			{
				WCL_JobID = "WC01",
				WCL_StartTime = today,
				WCL_EndTime = today.AddHours(2),
				WCL_SystemCreateTimeUtc = new DateTime(2012, 7, 3),
				WCL_SystemCreateUser = "US1",
				WCL_GS_NKAssignedTo = "AAA"
			}.InsertAndReturnObject(TestConnection);

			new WhsCycleCountLocation(location2.PK, "PID")
			{
				WCL_JobID = "WC02",
				WCL_StartTime = today,
				WCL_EndTime = today.AddHours(2),
				WCL_SystemCreateTimeUtc = new DateTime(2012, 6, 1),  // Out of Range for collector
				WCL_GS_NKAssignedTo = "AAA"
			}.InsertAndReturnObject(TestConnection);

			new WhsCycleCountLocation(location2.PK, "PID")  // WCL_EndTime is Null
			{
				WCL_JobID = "WC03",
				WCL_StartTime = today,
				WCL_SystemCreateTimeUtc = new DateTime(2012, 7, 8),
				WCL_GS_NKAssignedTo = "AAA"
			}.InsertAndReturnObject(TestConnection);

			new WhsCycleCountLocation(location2.PK, "PID")
			{
				WCL_JobID = "WC04",
				WCL_StartTime = today,
				WCL_EndTime = today.AddHours(2),
				WCL_SystemCreateTimeUtc = new DateTime(2012, 7, 8),
				WCL_GS_NKAssignedTo = "AAA",
				WCL_SystemCreateUser = "US2",
			}.InsertAndReturnObject(TestConnection);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "WH1 - N");
			AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 7, 3), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "US1", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "WH1 - N", transaction1.Reference1);
			AssertEquals("TransactionReference02", "WC01", transaction1.Reference2);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction1.AdditionalRefs);

			var transaction2 = FindRowByRef1(transactions, "WH2 - Y");
			AssertEquals("CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 7, 8), transaction2.ServiceOccuredUTC);
			AssertEquals("UserCode", "US2", transaction2.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction2.BillableCount);
			AssertEquals("TransactionReference01", "WH2 - Y", transaction2.Reference1);
			AssertEquals("TransactionReference02", "WC04", transaction2.Reference2);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 2\"}"
				, transaction2.AdditionalRefs);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2012, 7);
			}
		}

		public void TestMinimumVersionRequired()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("24.9.14.136", DateTime.Now, "ALP")))
			{
				Assert("Should not be active in versions below 24.9.14.137", !ScriptToTest.IsActive);
			}

			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("24.9.14.137", DateTime.Now, "ALP")))
			{
				Assert("Should be active in versions equals or above 24.9.14.137", ScriptToTest.IsActive);
			}
		}
	}
}
