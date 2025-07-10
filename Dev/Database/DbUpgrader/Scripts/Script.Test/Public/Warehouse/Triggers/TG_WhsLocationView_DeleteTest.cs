using System;
using System.Linq;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsLocationView_Delete))]
	class TG_WhsLocationView_DeleteTest : DBCreateTriggerScriptTest
	{
		#region TestView_WhsLocationView_Delete

		public void TestView_WhsLocationView_Delete()
		{
			var sql = new SqlQueryBuilder();
			var today = DateTime.Today;
			var utcToday = DateTime.UtcNow.Date;
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var pickingAreaPK1 = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var pickingAreaPK2 = new WhsArea(whs.PK, "B").AppendInsertAndReturnObject(sql).PK;
			var putawayAreaPK1 = new WhsArea(whs.PK, "P1").AppendInsertAndReturnObject(sql).PK;
			var putawayAreaPK2 = new WhsArea(whs.PK, "P2").AppendInsertAndReturnObject(sql).PK;
			var rowPK1 = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rowPK2 = new WhsRow(whs, "R2").AppendInsertAndReturnObject(sql).PK;
			var docType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "DOC").Single().PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single().PK;
			var loc1 = new WhsLocation(rowPK1, pickingAreaPK1, putawayAreaPK1, docType)
			{
				WL_Column = 1,
				WL_Level = 1,
				WL_Tray = 1,
				WL_LocationStatus = "NOR",
				WL_PickMethod = "ANY",
				WL_MaxWeight = 1,
				WL_MaxWeightUnit = "KG",
				WL_MaxCubic = 1,
				WL_MaxCubicUnit = "M3",
				WL_MaxQuantity = 10,
				WL_MaxQuantityUnit = "UNT",
				WL_MaxWidth = 1,
				WL_MaxHeight = 1,
				WL_MaxDepth = 1,
				WL_MaxDimensionUnit = "M",
				WL_PalletFloorSpaces = 1,
				WL_PalletStackHeight = 1,
				WL_ApprovedKnownLocation = "NO",
				WL_MaximumPickCountBeforeAutomatedStocktake = 1,
				WL_FinalisedPickCount = 1,
				WL_LastInventoryChangeDate = null,
				WL_LastAllocatedOrChangedDateUtc = null,
				WL_PickPathSequence = 1,
				WL_CycleCountPathSequence = 1,
				WL_SystemCreateTimeUtc = utcToday,
				WL_SystemLastEditTimeUtc = utcToday,
				WL_SystemCreateUser = "~BP",
				WL_SystemLastEditUser = "~BP"
			}.AppendInsertAndReturnObject(sql).PK;
			var loc2 = new WhsLocation(rowPK2, pickingAreaPK2, putawayAreaPK2, rnoType)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "DMG",
				WL_PickMethod = "ANY",
				WL_MaxWeight = 4,
				WL_MaxWeightUnit = "DT",
				WL_MaxCubic = 5,
				WL_MaxCubicUnit = "CF",
				WL_MaxQuantity = 6,
				WL_MaxQuantityUnit = "CNT",
				WL_MaxWidth = 7,
				WL_MaxHeight = 8,
				WL_MaxDepth = 9,
				WL_MaxDimensionUnit = "M",
				WL_PalletFloorSpaces = 1,
				WL_PalletStackHeight = 1,
				WL_ApprovedKnownLocation = "NO",
				WL_MaximumPickCountBeforeAutomatedStocktake = 10,
				WL_FinalisedPickCount = 11,
				WL_LastInventoryChangeDate = DateTimeOffset.Now,
				WL_LastAllocatedOrChangedDateUtc = today,
				WL_PickPathSequence = 12,
				WL_CycleCountPathSequence = 2,
				WL_SystemCreateTimeUtc = utcToday,
				WL_SystemLastEditTimeUtc = utcToday,
				WL_SystemCreateUser = "~BP",
				WL_SystemLastEditUser = "~BP"
			}.AppendInsertAndReturnObject(sql).PK;

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertEquals("Pre-Condition: WhsLocation does exist", true, WhsLocation.ExistsInDB(TestConnection, loc1));
			AssertEquals("Pre-Condition: WhsLocation does exist", true, WhsLocation.ExistsInDB(TestConnection, loc2));

			var deleteSQL1 = $@"
Delete From dbo.WhsLocationView
Where
	WLV_PK = '{loc1}'";

			TestConnection.ExecuteNonQuery(deleteSQL1);

			AssertEquals("Matched WhsLocation should be removed", false, WhsLocation.ExistsInDB(TestConnection, loc1));
			AssertEquals("Unmatched Whslocation should still exists ", true, WhsLocation.ExistsInDB(TestConnection, loc2));
		}
		#endregion
	}
}

