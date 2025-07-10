using System;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	using CargoWise.Database.TestFramework.ObjectModel;
	using WhsLocationView = CargoWise.Database.TestFramework.ObjectModel.WhsLocationView;

	[TestedType(typeof(TG_WhsLocationView_Insert))]
	class TG_WhsLocationView_InsertTest : DBCreateTriggerScriptTest
	{
		#region TestView_WhsLocationView_Insert

		public void TestView_WhsLocationView_Insert()
		{
			var today = DateTime.Today;
			var todayWithTimeZone = DateTimeOffset.Now.Date;
			var utcNowToday = DateTime.UtcNow.Date;
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var pickingAreaPK = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var putawayAreaPK = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single().PK;
			var changeID = Guid.NewGuid();
			var locationPK = new WhsLocationView(rowPK, pickingAreaPK, putawayAreaPK, rnoType)
			{
				WLV_IsValid = true,
				WLV_Column = 1,
				WLV_Level = 2,
				WLV_Tray = 3,
				WLV_CheckDigit = 14,
				WLV_LocationStatus = "DAM",
				WLV_PickMethod = "ANY",
				WLV_MaxWeight = 4,
				WLV_MaxWeightUnit = "DT",
				WLV_MaxCubic = 5,
				WLV_MaxCubicUnit = "CF",
				WLV_MaxQuantity = 6,
				WLV_MaxQuantityUnit = "CNT",
				WLV_MaxWidth = 7,
				WLV_MaxHeight = 8,
				WLV_MaxDepth = 9,
				WLV_MaxDimensionUnit = "M",
				WLV_PalletFloorSpaces = 1,
				WLV_PalletStackHeight = 2,
				WLV_ApprovedKnownLocation = "NO",
				WLV_MaximumPickCountBeforeAutomatedStocktake = 10,
				WLV_FinalisedPickCount = 11,
				WLV_LastInventoryChangeDate = todayWithTimeZone,
				WLV_LastAllocatedOrChangedDateUtc = today,
				WLV_PickPathSequence = 12,
				WLV_PutawayPathSequence = 13,
				WLV_TransitDischargeLRC = "CN",
				WLV_RS_NKTransitServiceLevel = "D2D",
				WLV_CycleCountPathSequence = 13,
				WLV_CycleCountLastPerformed = todayWithTimeZone.AddDays(1),
				WLV_LastAllocatedOrChangedID = changeID,
				WLV_SystemCreateTimeUtc = utcNowToday,
				WLV_SystemLastEditTimeUtc = utcNowToday,
				WLV_SystemCreateUser = "~BP",
				WLV_SystemLastEditUser = "~BP"
			}.AppendInsertAndReturnObject(sql).PK;

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			WhsLocation.AssertFromDB(TestConnection, locationPK)
				.ExpectEquals("WL_WA_PickingArea", l => l.WL_WA_PickingArea, pickingAreaPK)
				.ExpectEquals("WL_WA_PutawayArea", l => l.WL_WA_PutawayArea, putawayAreaPK)
				.ExpectEquals("WL_WR", l => l.WL_WR, rowPK)
				.ExpectEquals("WL_Column", l => l.WL_Column, 1)
				.ExpectEquals("WL_Level", l => l.WL_Level, 2)
				.ExpectEquals("WL_Tray", l => l.WL_Tray, 3)
				.ExpectEquals("WL_CheckDigit", l => l.WL_CheckDigit, 14)
				.ExpectEquals("WL_WLT_LocationType", l => l.WL_WLT_LocationType, rnoType)
				.ExpectEquals("WL_LocationStatus", l => l.WL_LocationStatus, "DAM")
				.ExpectEquals("WL_PickMethod", l => l.WL_PickMethod, "ANY")
				.ExpectEquals("WL_MaxWeight", l => l.WL_MaxWeight, 4)
				.ExpectEquals("WL_MaxWeightUnit", l => l.WL_MaxWeightUnit, "DT")
				.ExpectEquals("WL_MaxCubic", l => l.WL_MaxCubic, 5)
				.ExpectEquals("WL_MaxCubicUnit", l => l.WL_MaxCubicUnit, "CF")
				.ExpectEquals("WL_MaxQuantity", l => l.WL_MaxQuantity, 6)
				.ExpectEquals("WL_MaxQuantityUnit", l => l.WL_MaxQuantityUnit, "CNT")
				.ExpectEquals("WL_MaxWidth", l => l.WL_MaxWidth, 7)
				.ExpectEquals("WL_MaxHeight", l => l.WL_MaxHeight, 8)
				.ExpectEquals("WL_MaxDepth", l => l.WL_MaxDepth, 9)
				.ExpectEquals("WL_MaxDimensionUnit", l => l.WL_MaxDimensionUnit, "M")
				.ExpectEquals("WL_PalletFloorSpaces", l => l.WL_PalletFloorSpaces, 1)
				.ExpectEquals("WL_PalletStackHeight", l => l.WL_PalletStackHeight, 2)
				.ExpectEquals("WL_ApprovedKnownLocation", l => l.WL_ApprovedKnownLocation, "NO")
				.ExpectEquals("WL_MaximumPickCountBeforeAutomatedStocktake", l => l.WL_MaximumPickCountBeforeAutomatedStocktake, 10)
				.ExpectEquals("WL_FinalisedPickCount", l => l.WL_FinalisedPickCount, 11)
				.ExpectEquals("WL_LastInventoryChangeDate", l => l.WL_LastInventoryChangeDate, todayWithTimeZone)
				.ExpectEquals("WL_LastAllocatedOrChangedDateUtc", l => l.WL_LastAllocatedOrChangedDateUtc, today)
				.ExpectEquals("WL_PickPathSequence", l => l.WL_PickPathSequence, 12)
				.ExpectEquals("WL_PutawayPathSequence", l => l.WL_PutawayPathSequence, 13)
				.ExpectEquals("WL_TransitDischargeLRC", l => l.WL_TransitDischargeLRC, "CN")
				.ExpectEquals("WL_RS_NKTransitServiceLevel", l => l.WL_RS_NKTransitServiceLevel, "D2D")
				.ExpectEquals("WL_CycleCountPathSequence", l => l.WL_CycleCountPathSequence, 13)
				.ExpectEquals("WL_CycleCountLastPerformed", l => l.WL_CycleCountLastPerformed, todayWithTimeZone.AddDays(1))
				.ExpectEquals("WL_LastAllocatedOrChangedID", l => l.WL_LastAllocatedOrChangedID, changeID)
				.ExpectEquals("WL_SystemCreateTimeUtc", l => l.WL_SystemCreateTimeUtc, utcNowToday)
				.ExpectEquals("WL_SystemLastEditTimeUtc", l => l.WL_SystemLastEditTimeUtc, utcNowToday)
				.ExpectEquals("WL_SystemLastEditUser", l => l.WL_SystemLastEditUser, "~BP")
				.ExpectEquals("WL_SystemCreateUser", l => l.WL_SystemCreateUser, "~BP")
				.VerifyAll();
		}
		#endregion
	}
}

