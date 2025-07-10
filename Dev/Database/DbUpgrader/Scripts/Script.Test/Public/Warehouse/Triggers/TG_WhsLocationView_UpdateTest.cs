using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers
{
	using CargoWise.Database.TestFramework.ObjectModel;
	using WhsLocationView = CargoWise.Database.TestFramework.ObjectModel.WhsLocationView;

	[TestedType(typeof(TG_WhsLocationView_Update))]
	class TG_WhsLocationView_UpdateTest : DBCreateTriggerScriptTest
	{
		#region TestView_WhsLocationView_UpdatingPKColumn

		public void TestView_WhsLocationView_UpdatingPKColumn()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(sql);
			var pickingAreaPK = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var putawayAreaPK = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var location = new WhsLocation(rowPK, pickingAreaPK, putawayAreaPK).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			AssertEquals("Pre-Condition: WhsLocation does exist", true, WhsLocation.ExistsInDB(TestConnection, location.PK));

			var newLocationPK = Guid.NewGuid();
			var newLocation = WhsLocation.UpdateWhere(location.PK).Set(l => l.PK, newLocationPK).Post(TestConnection);
			AssertEquals("PK Value should have been updated.", newLocationPK, newLocation.PK);
			AssertEquals("WhsLocation exists in DB", true, WhsLocation.ExistsInDB(TestConnection, newLocationPK));

			// Attempt to Update PK via WhsLocationView - Should not work because the Instead Of Trigger specifically does not handle PK column
			AssertExceptionThrown(typeof(InvalidOperationException), () => WhsLocationView.UpdateWhere(newLocationPK).Set(l => l.PK, location.PK).Post(TestConnection));
		}

		#endregion

		#region TestView_WhsLocationView_UpdateSkipSameColumns

		public void TestView_WhsLocationView_UpdateSkipUnchangedColumns()
		{
			var now = DateTime.Now;
			var utcToday = DateTime.UtcNow.Date;
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var pickingAreaPK = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var putawayAreaPK = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var docType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "DOC").Single().PK;
			var locationPK = new WhsLocation(rowPK, pickingAreaPK, putawayAreaPK, docType)
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
				WL_PalletStackHeight = 2,
				WL_ApprovedKnownLocation = "NO",
				WL_MaximumPickCountBeforeAutomatedStocktake = 1,
				WL_FinalisedPickCount = 1,
				WL_LastInventoryChangeDate = null,
				WL_LastAllocatedOrChangedDateUtc = null,
				WL_PickPathSequence = 1,
				WL_PutawayPathSequence = 2,
				WL_CycleCountPathSequence = 0,
				WL_CycleCountLastPerformed = null,
				WL_SystemCreateTimeUtc = utcToday,
				WL_SystemLastEditTimeUtc = utcToday,
				WL_SystemCreateUser = "~BP",
				WL_SystemLastEditUser = "~BP"
			}.AppendInsertAndReturnObject(sql).PK;

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertEquals("Pre-Condition: WhsLocation does exist", true, WhsLocation.ExistsInDB(TestConnection, locationPK));

			WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_PickMethod, "XXX").Post(TestConnection);

			WhsLocation.AssertFromDB(TestConnection, locationPK)
				.ExpectEquals("WL_WA_PickingArea", l => l.WL_WA_PickingArea, pickingAreaPK)
				.ExpectEquals("WL_WA_PutawayArea", l => l.WL_WA_PutawayArea, putawayAreaPK)
				.ExpectEquals("WL_WR", l => l.WL_WR, rowPK)
				.ExpectEquals("WL_Column", l => l.WL_Column, 1)
				.ExpectEquals("WL_Level", l => l.WL_Level, 1)
				.ExpectEquals("WL_Tray", l => l.WL_Tray, 1)
				.ExpectEquals("WL_WLT_LocationType", l => l.WL_WLT_LocationType, docType)
				.ExpectEquals("WL_LocationStatus", l => l.WL_LocationStatus, "NOR")
				.ExpectEquals("WL_PickMethod", l => l.WL_PickMethod, "XXX")
				.ExpectEquals("WL_MaxWeight", l => l.WL_MaxWeight, 1)
				.ExpectEquals("WL_MaxWeightUnit", l => l.WL_MaxWeightUnit, "KG")
				.ExpectEquals("WL_MaxCubic", l => l.WL_MaxCubic, 1)
				.ExpectEquals("WL_MaxCubicUnit", l => l.WL_MaxCubicUnit, "M3")
				.ExpectEquals("WL_MaxQuantity", l => l.WL_MaxQuantity, 10)
				.ExpectEquals("WL_MaxQuantityUnit", l => l.WL_MaxQuantityUnit, "UNT")
				.ExpectEquals("WL_MaxWidth", l => l.WL_MaxWidth, 1)
				.ExpectEquals("WL_MaxHeight", l => l.WL_MaxHeight, 1)
				.ExpectEquals("WL_MaxDepth", l => l.WL_MaxDepth, 1)
				.ExpectEquals("WL_MaxDimensionUnit", l => l.WL_MaxDimensionUnit, "M")
				.ExpectEquals("WL_PalletFloorSpaces", l => l.WL_PalletFloorSpaces, 1)
				.ExpectEquals("WL_PalletStackHeight", l => l.WL_PalletStackHeight, 2)
				.ExpectEquals("WL_ApprovedKnownLocation", l => l.WL_ApprovedKnownLocation, "NO")
				.ExpectEquals("WL_MaximumPickCountBeforeAutomatedStocktake", l => l.WL_MaximumPickCountBeforeAutomatedStocktake, 1)
				.ExpectEquals("WL_FinalisedPickCount", l => l.WL_FinalisedPickCount, 1)
				.ExpectEquals("WL_LastInventoryChangeDate", l => l.WL_LastInventoryChangeDate, null)
				.ExpectEquals("WL_LastAllocatedOrChangedDateUtc", l => l.WL_LastAllocatedOrChangedDateUtc, null)
				.ExpectEquals("WL_PickPathSequence", l => l.WL_PickPathSequence, 1)
				.ExpectEquals("WL_PutawayPathSequence", l => l.WL_PutawayPathSequence, 2)
				.ExpectEquals("WL_TransitDischargeLRC", l => l.WL_TransitDischargeLRC, "")
				.ExpectEquals("WL_RS_NKTransitServiceLevel", l => l.WL_RS_NKTransitServiceLevel, "")
				.ExpectEquals("WL_CycleCountPathSequence", l => l.WL_CycleCountPathSequence, 0)
				.ExpectEquals("WL_CycleCountLastPerformed", l => l.WL_CycleCountLastPerformed, null)
				.VerifyAll("Only PickMethod was changed");
		}

		#endregion

		#region TestView_WhsLocationView_Update_LocationUsedCanBeUpdated

		public void TestView_WhsLocationView_Update_LocationUsedCanBeUpdated()
		{
			var now = DateTime.Now;
			var utcToday = DateTime.UtcNow.Date;
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var pickingAreaPK = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var putawayAreaPK = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var docType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "DOC").Single().PK;
			var locationPK = new WhsLocation(rowPK, pickingAreaPK, putawayAreaPK, docType)
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
				WL_ApprovedKnownLocation = "NO",
				WL_MaximumPickCountBeforeAutomatedStocktake = 1,
				WL_FinalisedPickCount = 1,
				WL_LastInventoryChangeDate = null,
				WL_LastAllocatedOrChangedDateUtc = null,
				WL_PickPathSequence = 1,
				WL_PutawayPathSequence = 2,
				WL_CycleCountPathSequence = 0,
				WL_CycleCountLastPerformed = null,
				WL_SystemCreateTimeUtc = utcToday,
				WL_SystemLastEditTimeUtc = utcToday,
				WL_SystemCreateUser = "~BP",
				WL_SystemLastEditUser = "~BP"
			}.AppendInsertAndReturnObject(sql).PK;

			var productPK = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql).PK;
			var clientPK = new OrgHeader("ABC").AppendInsertAndReturnObject(sql).PK;
			var receive = new WhsDocket(clientPK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = now, WD_ArrivalDate = now }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, productPK, 5m) { WE_CurrentInventoryStatus = "AVL", WE_OriginalInventoryStatus = "AVL", WE_WL = locationPK, WE_StockOnHand = 5m }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.DisableConstraint(WhsDocketLineSchema.Constants.TableName, "Constraint_WE_AdjustmentArrivalDate"))
			using (TestWhsDataSetupHelper.DisableConstraint(WhsDocketLineSchema.Constants.TableName, "Constraint_StockOnHandMustHaveArrivalDate"))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals("Pre-Condition: WhsLocation does exist", true, WhsLocation.ExistsInDB(TestConnection, locationPK));

			AssertNoExceptionThrown("Should not thrown exception when Location has been used", () => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_PickMethod, "XXX").Post(TestConnection));
		}

		#endregion

		#region TestView_WhsLocationView_Update

		public void TestView_WhsLocationView_Update_PickingArea()
		{
			var newPickingArea = Guid.Empty;
			TestView_WhsLocationView_UpdateCore(
				locationPK =>
				{
					var whs = WhsWarehouse.ShallowLoadFromDB(TestConnection).Single();
					newPickingArea = new WhsArea(whs.PK, "P3").InsertAndReturnObject(TestConnection).PK;
					WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_WA_PickingArea, newPickingArea).Post(TestConnection);
				},
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_WA_PickingArea", l => l.WL_WA_PickingArea, newPickingArea)
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_PutawayArea()
		{
			var newPutawayArea = Guid.Empty;
			TestView_WhsLocationView_UpdateCore(
				locationPK =>
				{
					var whs = WhsWarehouse.ShallowLoadFromDB(TestConnection).Single();
					newPutawayArea = new WhsArea(whs.PK, "P3").InsertAndReturnObject(TestConnection).PK;
					WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_WA_PutawayArea, newPutawayArea).Post(TestConnection);
				},
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_WA_PutawayArea", l => l.WL_WA_PutawayArea, newPutawayArea)
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_LocationType()
		{
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, locationType => locationType.WLT_Code == "RNO").Single().PK;
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_WLT_LocationType, rnoType).Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_WLT_LocationType", l => l.WL_WLT_LocationType, rnoType)
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_Column()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_Column, (short)99).Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_Column", l => l.WL_Column, 99)
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_Level()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_Level, (short)99).Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_Level", l => l.WL_Level, 99)
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_Tray()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_Tray, (short)99).Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_Tray", l => l.WL_Tray, 99)
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_CheckDigit()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_CheckDigit, (byte)23).Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_CheckDigit", l => l.WL_CheckDigit, 23)
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_LocationStatus()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_LocationStatus, "DMG").Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_LocationStatus", l => l.WL_LocationStatus, "DMG")
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_PickMethod()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_PickMethod, "XXX").Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_PickMethod", l => l.WL_PickMethod, "XXX")
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_MaxWeight()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_MaxWeight, 99).Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_MaxWeight", l => l.WL_MaxWeight, 99)
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_MaxWeightUnit()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_MaxWeightUnit, "G").Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_MaxWeightUnit", l => l.WL_MaxWeightUnit, "G")
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_MaxCubic()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_MaxCubic, 99).Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_MaxCubic", l => l.WL_MaxCubic, 99)
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_MaxCubicUnit()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_MaxCubicUnit, "CF").Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_MaxCubicUnit", l => l.WL_MaxCubicUnit, "CF")
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_MaxQuantity()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_MaxQuantity, 5).Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_MaxQuantity", l => l.WL_MaxQuantity, 5)
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_MaxQuantityUnit()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_MaxQuantityUnit, "BOX").Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_MaxQuantityUnit", l => l.WL_MaxQuantityUnit, "BOX")
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_MaxWidth()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_MaxWidth, 5).Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_MaxWidth", l => l.WL_MaxWidth, 5)
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_MaxHeight()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_MaxHeight, 5).Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_MaxHeight", l => l.WL_MaxHeight, 5)
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_MaxDepth()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_MaxDepth, 5).Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_MaxDepth", l => l.WL_MaxDepth, 5)
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_MaxDimensionUnit()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_MaxDimensionUnit, "FT").Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_MaxDimensionUnit", l => l.WL_MaxDimensionUnit, "FT")
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_PalletFloorSpaces()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_PalletFloorSpaces, (byte)2).Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_PalletFloorSpaces", l => l.WL_PalletFloorSpaces, (byte)2)
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_PalletStackHeight()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_PalletStackHeight, (byte)2).Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_PalletStackHeight", l => l.WL_PalletStackHeight, (byte)2)
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_ApprovedKnownLocation()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_ApprovedKnownLocation, "YES").Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_ApprovedKnownLocation", l => l.WL_ApprovedKnownLocation, "YES")
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_MaximumPickCountBeforeAutomatedStocktake()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_MaximumPickCountBeforeAutomatedStocktake, 5).Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_MaximumPickCountBeforeAutomatedStocktake", l => l.WL_MaximumPickCountBeforeAutomatedStocktake, 5)
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_FinalisedPickCount()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_FinalisedPickCount, 5).Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_FinalisedPickCount", l => l.WL_FinalisedPickCount, 5)
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_LastInventoryChangeDate()
		{
			var today = DateTimeOffset.Now.Date;
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_LastInventoryChangeDate, today).Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_LastInventoryChangeDate", l => l.WL_LastInventoryChangeDate, today)
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_LastAllocatedOrChangedDate()
		{
			var today = DateTime.Today;
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_LastAllocatedOrChangedDateUtc, today).Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_LastAllocatedOrChangedDateUtc", l => l.WL_LastAllocatedOrChangedDateUtc, today)
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_PickPathSequence()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_PickPathSequence, 5).Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_PickPathSequence", l => l.WL_PickPathSequence, 5)
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_PutawayPathSequence()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_PutawayPathSequence, 6).Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_PutawayPathSequence", l => l.WL_PutawayPathSequence, 6)
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_TransitDischargeLRC()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_TransitDischargeLRC, "CN").Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_TransitDischargeLRC", l => l.WL_TransitDischargeLRC, "CN")
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_TransitServiceLevel()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_RS_NKTransitServiceLevel, "D2D").Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_RS_NKTransitServiceLevel", l => l.WL_RS_NKTransitServiceLevel, "D2D")
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_SystemCreateTimeUtc()
		{
			var utcToday = DateTime.UtcNow.Date;
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_SystemCreateTimeUtc, utcToday).Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_SystemCreateTimeUtc", l => l.WL_SystemCreateTimeUtc, utcToday)
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_SystemLastEditTimeUtc()
		{
			var utcToday = DateTime.UtcNow.Date;
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_SystemLastEditTimeUtc, utcToday).Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_SystemLastEditTimeUtc", l => l.WL_SystemLastEditTimeUtc, utcToday)
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_SystemCreateUser()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_SystemCreateUser, "~BP").Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_SystemCreateUser", l => l.WL_SystemCreateUser, "~BP")
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_SystemLastEditUser()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_SystemLastEditUser, "~BP").Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_SystemLastEditUser", l => l.WL_SystemLastEditUser, "~BP")
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_CycleCountPathSequence()
		{
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_CycleCountPathSequence, 5).Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_CycleCountPathSequence", l => l.WL_CycleCountPathSequence, 5)
						.VerifyAll()
			);
		}

		public void TestView_WhsLocationView_Update_CycleCountLastPerformed()
		{
			var today = DateTimeOffset.Now.Date;
			TestView_WhsLocationView_UpdateCore(
				locationPK => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_CycleCountLastPerformed, today).Post(TestConnection),
				locationPK => WhsLocation.AssertFromDB(TestConnection, locationPK)
						.ExpectEquals("WL_CycleCountLastPerformed", l => l.WL_CycleCountLastPerformed, today)
						.VerifyAll()
			);
		}

		void TestView_WhsLocationView_UpdateCore(Action<Guid> updateAction, Action<Guid> assertAction)
		{
			var utcToday = DateTime.UtcNow.Date;
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var pickingAreaPK = new WhsArea(whs.PK, "P1").AppendInsertAndReturnObject(sql).PK;
			var putawayAreaPK = new WhsArea(whs.PK, "P2").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var docType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "DOC").Single().PK;
			var locationPK = new WhsLocation(rowPK, pickingAreaPK, putawayAreaPK, docType)
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
				WL_PutawayPathSequence = 2,
				WL_TransitDischargeLRC = "AU",
				WL_RS_NKTransitServiceLevel = "STD",
				WL_CycleCountPathSequence = 1,
				WL_CycleCountLastPerformed = null,
				WL_SystemCreateTimeUtc = utcToday,
				WL_SystemLastEditTimeUtc = utcToday,
				WL_SystemCreateUser = "~BP",
				WL_SystemLastEditUser = "~BP"
			}.AppendInsertAndReturnObject(sql).PK;

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertEquals("Pre-Condition: WhsLocation does exist", true, WhsLocation.ExistsInDB(TestConnection, locationPK));

			updateAction(locationPK);

			assertAction(locationPK);
		}

		public void TestView_WhsLocationView_Update_TriggerPreventsRowUpdate()
		{
			var today = DateTime.Today;
			var utcToday = DateTime.UtcNow.Date;
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var pickingAreaPK = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var putawayAreaPK = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql).PK;
			var rowPK1 = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rowPK2 = new WhsRow(whs, "R2").AppendInsertAndReturnObject(sql).PK;
			var docType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "DOC").Single().PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single().PK;
			var locationPK = new WhsLocation(rowPK1, pickingAreaPK, putawayAreaPK, docType)
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
				WL_PutawayPathSequence = 2,
				WL_TransitDischargeLRC = "AU",
				WL_RS_NKTransitServiceLevel = "STD",
				WL_CycleCountPathSequence = 0,
				WL_CycleCountLastPerformed = null,
				WL_SystemCreateTimeUtc = utcToday,
				WL_SystemLastEditTimeUtc = utcToday,
				WL_SystemCreateUser = "~BP",
				WL_SystemLastEditUser = "~BP"
			}.AppendInsertAndReturnObject(sql).PK;

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertEquals("Pre-Condition: WhsLocation does exist", true, WhsLocation.ExistsInDB(TestConnection, locationPK));

			void codeToRun() => WhsLocationView.UpdateWhere(locationPK).Set(l => l.WLV_WR, rowPK2).Post(TestConnection);
			NUnit.Framework.Assert.That(codeToRun, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to change the Row for a Location that is already saved.", true), "Row change must not be allowed by the trigger.");
		}

		#endregion

		public void TestView_WhsLocationView_Update_ColumnsTrackedByTrigger()
		{
			var printerPk = Guid.NewGuid();
			var newPrintQueueSql = @"DECLARE @ServerPK uniqueidentifier = NEWID();
				INSERT dbo.StmPrintServer(SPS_PK, SPS_ServerName) VALUES(@ServerPK, 'SERVER');
				INSERT dbo.StmPrintQueue(SQ_PK, SQ_SPS_Server, SQ_QueueName) VALUES('" + printerPk + "', @ServerPK, 'PRINTER')";
			TestConnection.ExecuteNonQuery(newPrintQueueSql);
			var today = DateTime.Today;
			var utcToday = DateTime.UtcNow.Date;
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var pickingAreaPK = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var secondPickingAreaPk = new WhsArea(whs.PK, "A-2").AppendInsertAndReturnObject(sql).PK;
			var putawayAreaPK = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql).PK;
			var secondPutawayAreaPK = new WhsArea(whs.PK, "P-2").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var docType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "DOC").Single().PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single().PK;
			var changeID = Guid.NewGuid();
			var locationPK = new WhsLocation(rowPK, pickingAreaPK, putawayAreaPK, docType)
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
				WL_LastAllocatedOrChangedID = Guid.Empty,
				WL_PickPathSequence = 1,
				WL_PutawayPathSequence = 2,
				WL_TransitDischargeLRC = "AU",
				WL_RS_NKTransitServiceLevel = "STD",
				WL_CycleCountPathSequence = 0,
				WL_CycleCountLastPerformed = null,
				WL_SystemCreateTimeUtc = utcToday,
				WL_SystemLastEditTimeUtc = utcToday,
				WL_SystemCreateUser = "~BP",
				WL_SystemLastEditUser = "~BP"
			}.AppendInsertAndReturnObject(sql).PK;

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertEquals("Pre-Condition: WhsLocation does exist", true, WhsLocation.ExistsInDB(TestConnection, locationPK));

			var whitelist = new HashSet<string>
			{
				"WLV_PK",
				"WLV_WW_Whs",
				"WLV_WR",
				"WLV_RowName",
				"WLV_IsValidLocationForProductWarehousePutaway",
				"WLV_IsVirtualWarehouse",
				"WLV_WarehouseType",
				"WLV_LastConfigChangedUtc",
				"WLV_LocationClass",
				"WLV_LocationTypeCode",
				"WLV_FormattedColumn",
				"WLV_FormattedLevel",
				"WLV_FormattedTray",
				"WLV_LocationString",
				"WLV_LocationString_UserFriendly",
				"WLV_TransitDischargeAndServiceLevelFakeColumnForUX",
				"WLV_PickingAreaType",
				"WLV_PutawayAreaType",
			};

			var editable = new (string column, string sqlValue, object assertValue)[]
			{
				("WLV_IsValid", "1" , true),
				("WLV_WA_PickingArea", secondPickingAreaPk.ToString() , secondPickingAreaPk),
				("WLV_WA_PutawayArea", secondPutawayAreaPK.ToString() , secondPutawayAreaPK),
				("WLV_Column", "2" , (short)2),
				("WLV_Level", "2" , (short)2),
				("WLV_Tray", "2" , (short)2),
				("WLV_CheckDigit", "2" , (byte)2),
				("WLV_WLT_LocationType", rnoType.ToString() , rnoType),
				("WLV_LocationStatus", string.Empty , string.Empty),
				("WLV_PickMethod", string.Empty , string.Empty),
				("WLV_MaxWeight", "2" , (decimal)2),
				("WLV_MaxWeightUnit", "LB" , "LB"),
				("WLV_MaxCubic", "2" , (decimal)2),
				("WLV_MaxCubicUnit", "L" , "L"),
				("WLV_MaxWidth", "2" , (decimal)2),
				("WLV_MaxHeight", "2" , (decimal)2),
				("WLV_MaxDepth", "2" , (decimal)2),
				("WLV_MaxDimensionUnit", "KM" , "KM"),
				("WLV_PalletFloorSpaces", "2" , (byte)2),
				("WLV_PalletStackHeight", "2" , (byte)2),
				("WLV_ApprovedKnownLocation", "YE" , "YE"),
				("WLV_MaximumPickCountBeforeAutomatedStocktake", "0" , 0),
				("WLV_PickPathSequence", "0" , 0),
				("WLV_MaxQuantity", "100" , (decimal)100),
				("WLV_MaxQuantityUnit", "BOX" , "BOX"),
				("WLV_SQ_DefaultPrintQueue", printerPk.ToString() , printerPk),
				("WLV_TransitDischargeLRC", "NZ", "NZ"),
				("WLV_RS_NKTransitServiceLevel", "STI", "STI"),
				("WLV_CycleCountPathSequence", "1" , 1),
				("WLV_PutawayPathSequence", "10" , 10),
				("WLV_FinalisedPickCount", "5", 5),
				("WLV_LastInventoryChangeDate", "2018-01-01 00:00", new DateTimeOffset(new DateTime(2018, 01, 01), new TimeSpan())),
				("WLV_CycleCountLastPerformed", "2018-01-02 00:00", new DateTimeOffset(new DateTime(2018, 01, 02), new TimeSpan())),
				("WLV_LastAllocatedOrChangedDateUtc", "2019-01-01 00:00", new DateTime(2019, 01, 01)),
				("WLV_LastAllocatedOrChangedID", changeID.ToString(), changeID),
				("WLV_SystemCreateTimeUtc", "2020-01-01 00:00", new DateTime(2020, 01, 01)),
				("WLV_SystemCreateUser", "JOE", "JOE"),
				("WLV_SystemLastEditTimeUtc", "2020-01-02 00:00", new DateTime(2020, 01, 02)),
				("WLV_SystemLastEditUser", "JOE", "JOE"),
			};

			AssertContainsExactElementsInAnyOrder("All fields in the view must be accounted for. It is the purpose of this test to ensure no columns are forgotten.", ColumnNames("WhsLocationView").Except(whitelist), editable.Select(e => e.column).Distinct());

			CombineAssertions("The update trigger is failing for the following:", () =>
			{
				foreach (var (column, sqlValue, assertValue) in editable)
				{
					var originalValue = TestConnection.ExecuteScalar($"SELECT {column} FROM dbo.WhsLocationView WHERE WLV_PK='{locationPK}'");

					TestConnection.ExecuteNonQuery($"UPDATE dbo.WhsLocationView SET {column}='{sqlValue}' WHERE WLV_PK='{locationPK}'");

					var value = TestConnection.ExecuteScalar($"SELECT {column} FROM dbo.WhsLocationView WHERE WLV_PK='{locationPK}'");
					AssertNotEquals(column, originalValue, value);
					AssertEquals(column, assertValue, value);
				}
			});
		}

		ICollection<string> ColumnNames(string tablename)
		{
			var result = new HashSet<string>();
			TestConnection.ExecuteReader(
				"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @table",
				p => p.AddParameter("@table", System.Data.SqlDbType.NVarChar, tablename),
				r => result.Add(r.GetString(0))
			);

			return result;
		}
	}
}

