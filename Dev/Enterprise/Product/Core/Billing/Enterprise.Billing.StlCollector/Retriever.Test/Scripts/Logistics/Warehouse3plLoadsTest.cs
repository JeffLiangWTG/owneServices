using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(Warehouse3plLoads))]
	sealed class Warehouse3plLoadsTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
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

			var equipment1 = new RefEquipment("ABC", "ABC")
			{
				RQ_CubicCapacity = 1m,
				RQ_CubicUnit = "M3",
				RQ_WeightCapacity = 1m,
				RQ_WeightUnit = "KG"
			}.InsertAndReturnObject(TestConnection);

			var load1 = new WhsLoad("WL01", "STD", orgHeader.PK, location1.PK)
			{
				WLO_StartTime = DateTimeOffset.Now,
				WLO_CompleteTime = DateTimeOffset.Now.AddHours(2),
				WLO_SystemCreateTimeUtc = new DateTime(2012, 7, 3),
				WLO_TransportationUnitNumber = "ABC",
				WLO_RQ_TransportationUnit = equipment1.PK
			}.InsertAndReturnObject(TestConnection);

			var load2 = new WhsLoad("WL02", "STD", orgHeader.PK, location2.PK)
			{
				WLO_StartTime = DateTimeOffset.Now,
				WLO_CompleteTime = DateTimeOffset.Now.AddHours(2),
				WLO_SystemCreateTimeUtc = new DateTime(2012, 6, 1),
				WLO_TransportationUnitNumber = "ABC",
				WLO_RQ_TransportationUnit = equipment1.PK
			}.InsertAndReturnObject(TestConnection);

			var load3 = new WhsLoad("WL03", "STD", orgHeader.PK, location2.PK)
			{
				WLO_StartTime = DateTimeOffset.Now,
				WLO_SystemCreateTimeUtc = new DateTime(2012, 7, 8),
				WLO_TransportationUnitNumber = "ABC",
				WLO_RQ_TransportationUnit = equipment1.PK
			}.InsertAndReturnObject(TestConnection);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "WH1 - N");
			AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 7, 3), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "A", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "WH1 - N", transaction1.Reference1);
			AssertEquals("TransactionReference02", "WL01", transaction1.Reference2);
			AssertEquals("AdditionalRefs should be as expected",
				"{\"Warehouse Name\":\"Warehouse 1\"}"
				, transaction1.AdditionalRefs);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2012, 7);
			}
		}
	}
}
