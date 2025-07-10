using System;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsLocation_PreventChangingLocationType))]
	class TG_WhsLocation_PreventChangingLocationTypeTest : DBCreateTriggerScriptTest
	{
		// Also Tested in Enterprise.Warehouse.Environment.Business.WhsLocation

		public void TestTrigger_LocationType_WithStock_Updated_NOR_HPL_ShouldAllow()
		{
			TestTrigger_LocationType_WithStock_Updated_Core("NOR", "HPL", ExpectedTriggerBehavior.AllowChange);
		}

		public void TestTrigger_LocationType_WithStock_Updated_NOR_FIX_ShouldPrevent()
		{
			TestTrigger_LocationType_WithStock_Updated_Core("NOR", "FIX", ExpectedTriggerBehavior.PreventChange);
		}

		public void TestTrigger_LocationType_WithStock_Updated_NOR_DDL_ShouldPrevent()
		{
			TestTrigger_LocationType_WithStock_Updated_Core("NOR", "DDL", ExpectedTriggerBehavior.PreventChange);
		}

		public void TestTrigger_LocationType_WithStock_Updated_HPL_NOR_ShouldAllow()
		{
			TestTrigger_LocationType_WithStock_Updated_Core("HPL", "NOR", ExpectedTriggerBehavior.AllowChange);
		}

		public void TestTrigger_LocationType_WithStock_Updated_FIX_NOR_NoPickface_ShouldAllow()
		{
			TestTrigger_LocationType_WithStock_Updated_Core("FIX", "NOR", ExpectedTriggerBehavior.AllowChange);
		}

		public void TestTrigger_LocationType_WithStock_Updated_FIX_HPL_NoPickface_ShouldAllow()
		{
			TestTrigger_LocationType_WithStock_Updated_Core("FIX", "HPL", ExpectedTriggerBehavior.AllowChange);
		}

		public void TestTrigger_LocationType_WithStock_Updated_FIX_DDL_NoPickface_ShouldPrevent()
		{
			TestTrigger_LocationType_WithStock_Updated_Core("FIX", "DDL", ExpectedTriggerBehavior.PreventChange);
		}

		public void TestTrigger_LocationType_WithStock_Updated_FIX_FIX_ShouldPrevent()
		{
			TestTrigger_LocationType_WithStock_Updated_Core("FIX", "FIX", ExpectedTriggerBehavior.PreventChange);
		}

		public void TestTrigger_LocationType_WithStock_Updated_DDL_NOR_ShouldPrevent()
		{
			TestTrigger_LocationType_WithStock_Updated_Core("DDL", "NOR", ExpectedTriggerBehavior.PreventChange);
		}

		public void TestTrigger_LocationType_WithStock_Updated_DDL_DDL_ShouldAllow()
		{
			TestTrigger_LocationType_WithStock_Updated_Core("DDL", "DDL", ExpectedTriggerBehavior.AllowChange);
		}

		void TestTrigger_LocationType_WithStock_Updated_Core(string changedFromClass, string changedToClass, ExpectedTriggerBehavior expected)
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A") { WR_Columns = 2, WR_Levels = 2 }.AppendInsertAndReturnObject(sql);

			var locationTypeFrom = new WhsLocationType("LTF", changedFromClass) { WLT_MaximumNumberOfProducts = changedFromClass == "FIX" ? 1 : 0 }.AppendInsertAndReturnObject(sql);
			var locationTypeTo = new WhsLocationType("LTT", changedToClass) { WLT_MaximumNumberOfProducts = changedToClass == "FIX" ? 1 : 0 }.AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK, locationTypeFrom.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);
			var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_ArrivalDate = DateTime.Today, WD_FinalisedDate = DateTime.Today }.AppendInsertAndReturnObject(sql);
			new WhsDocketLine(receive, part.PK, 10m)
			{
				WE_StockOnHand = 10m,
				WE_WL = location.PK,
				WE_AdjustmentArrivalDate = DateTime.Today,
				WE_CurrentInventoryStatus = "AVL",
				WE_OriginalInventoryStatus = "AVL"
			}.AppendInsertAndReturnObject(sql);
			SaveToDB(sql);

			var updBuilder = WhsLocation.UpdateWhere(location.PK).Set(l => l.WL_WLT_LocationType, locationTypeTo.PK);
			if (expected == ExpectedTriggerBehavior.AllowChange)
			{
				AssertNoExceptionThrown($"Trigger should not prevent changing location type from {changedFromClass} to {changedToClass}.", () => updBuilder.Post(TestConnection));
			}
			else
			{
				AssertTriggerPreventsLocationTypeChange(updBuilder, "Attempt to change Location Type for a Location with Existing or Pending Inventory added by a user in another instance.");
			}
		}

		public void TestTrigger_LocationType_NoStock_Updated_FIX_DDL_ShouldAllow()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A") { WR_Columns = 2, WR_Levels = 2 }.AppendInsertAndReturnObject(sql);
			var locationTypeDDL = new WhsLocationType("DDL", "DDL").AppendInsertAndReturnObject(sql);
			var locationTypeFIX = new WhsLocationType("FIX", "FIX") { WLT_MaximumNumberOfProducts = 1 }.AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK, locationTypeFIX.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);
			var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			// Receive with SOH = 0 to simulate order out.
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_ArrivalDate = DateTime.Today, WD_FinalisedDate = DateTime.Today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, part.PK, 10m) { WE_StockOnHand = 0m, WE_WL = location.PK, WE_AdjustmentArrivalDate = DateTime.Today, WE_CurrentInventoryStatus = "AVL", WE_OriginalInventoryStatus = "AVL" }.AppendInsertAndReturnObject(sql);

			// Order out stock
			var pick = new WhsPick(whs, "Pick1", "FIN") { WP_FinalizedDateUtc = DateTime.UtcNow, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O1") { WD_WP = pick.PK, WD_FinalisedDate = DateTime.Today, WD_GS_NKFinalizedBy = "E" }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, part.PK, 10m) { WE_DocketLineStatus = "DEP", WE_FinalisedDate = DateTime.Today, WE_AdjustmentArrivalDate = null }.AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine, orderLine, 10m) { WZ_PickedDateTime = DateTime.Today, WZ_GS_NKAssignedTo = "E" }.AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			var updBuilder = WhsLocation.UpdateWhere(location.PK).Set(l => l.WL_WLT_LocationType, locationTypeDDL.PK);
			AssertNoExceptionThrown("Trigger should not prevent changing location type from FIX to DDL when no stock on hand", () => updBuilder.Post(TestConnection));
		}

		#region TestTrigger_LocationType_AttachedToPick_DDL

		public void TestTrigger_LocationType_AttachedToPick_DDL_DifferentDDL_ShouldPrevent()
		{
			TestTrigger_LocationType_AttachedToPick_DDLCore("DDL", false);
		}

		public void TestTrigger_LocationType_AttachedToPick_DDL_NOR_ShouldPrevent()
		{
			TestTrigger_LocationType_AttachedToPick_DDLCore("NOR", false);
		}

		public void TestTrigger_LocationType_AttachedToPick_DDL_FIX_ShouldPrevent()
		{
			TestTrigger_LocationType_AttachedToPick_DDLCore("FIX", false);
		}

		public void TestTrigger_LocationType_AttachedToPick_DDL_HPL_ShouldPrevent()
		{
			TestTrigger_LocationType_AttachedToPick_DDLCore("HPL", false);
		}

		public void TestTrigger_LocationType_AttachedToPick_DDL_PST_ShouldPrevent()
		{
			TestTrigger_LocationType_AttachedToPick_DDLCore("PST", false);
		}

		public void TestTrigger_LocationType_AttachedToPick_DDL_DockDoorAssignment_DifferentDDL_ShouldPrevent()
		{
			TestTrigger_LocationType_AttachedToPick_DDLCore("DDL", true);
		}

		public void TestTrigger_LocationType_AttachedToPick_DDL_DockDoorAssignment_NOR_ShouldPrevent()
		{
			TestTrigger_LocationType_AttachedToPick_DDLCore("NOR", true);
		}

		public void TestTrigger_LocationType_AttachedToPick_DDL_DockDoorAssignment_FIX_ShouldPrevent()
		{
			TestTrigger_LocationType_AttachedToPick_DDLCore("FIX", true);
		}

		public void TestTrigger_LocationType_AttachedToPick_DDL_DockDoorAssignment_HPL_ShouldPrevent()
		{
			TestTrigger_LocationType_AttachedToPick_DDLCore("HPL", true);
		}

		public void TestTrigger_LocationType_AttachedToPick_DDL_DockDoorAssignment_PST_ShouldPrevent()
		{
			TestTrigger_LocationType_AttachedToPick_DDLCore("PST", true);
		}

		void TestTrigger_LocationType_AttachedToPick_DDLCore(string changedToClass, bool hasDockDoorAssignment)
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A") { WR_Columns = 2, WR_Levels = 2 }.AppendInsertAndReturnObject(sql);

			var locationTypeFrom = new WhsLocationType("LTF", "DDL") { WLT_MaximumNumberOfProducts = 0 }.AppendInsertAndReturnObject(sql);
			var locationTypeTo = new WhsLocationType("LTT", changedToClass) { WLT_MaximumNumberOfProducts = changedToClass == "FIX" ? 1 : 0 }.AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK, locationTypeFrom.PK).AppendInsertAndReturnObject(sql);

			if (hasDockDoorAssignment)
			{
				var dockDoorAssignment = new WhsDockDoorAssignment(location).AppendInsertAndReturnObject(sql);
				new WhsPick(whs, "P1", "NEW") { WP_WL_DockDoor = null, WP_WDA_DockDoorAssignment = dockDoorAssignment }.AppendInsertAndReturnObject(sql);
			}
			else
			{
				new WhsPick(whs, "P1", "NEW") { WP_WL_DockDoor = location }.AppendInsertAndReturnObject(sql);
			}

			SaveToDB(sql);

			var updBuilder = WhsLocation.UpdateWhere(location.PK).Set(l => l.WL_WLT_LocationType, locationTypeTo.PK);
			AssertTriggerPreventsLocationTypeChange(updBuilder, "Attempt to change Location Type for a Dock Door Location that is used by Existing Picks.");
		}

		#endregion

		#region TestTrigger_LocationType_AttachedToPick_PST
		public void TestTrigger_LocationType_AttachedToPick_PST_DifferentPST_ShouldPrevent()
		{
			TestTrigger_LocationType_AttachedToPick_PSTCore("PST");
		}

		public void TestTrigger_LocationType_AttachedToPick_PST_NOR_ShouldPrevent()
		{
			TestTrigger_LocationType_AttachedToPick_PSTCore("NOR");
		}

		public void TestTrigger_LocationType_AttachedToPick_PST_FIX_ShouldPrevent()
		{
			TestTrigger_LocationType_AttachedToPick_PSTCore("FIX");
		}

		public void TestTrigger_LocationType_AttachedToPick_PST_HPL_ShouldPrevent()
		{
			TestTrigger_LocationType_AttachedToPick_PSTCore("HPL");
		}

		public void TestTrigger_LocationType_AttachedToPick_PST_DDL_ShouldPrevent()
		{
			TestTrigger_LocationType_AttachedToPick_PSTCore("DDL");
		}

		void TestTrigger_LocationType_AttachedToPick_PSTCore(string changedToClass)
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW", branch.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A") { WR_Columns = 2, WR_Levels = 2 }.AppendInsertAndReturnObject(sql);

			var locationTypeTo = new WhsLocationType("LTT", changedToClass) { WLT_MaximumNumberOfProducts = changedToClass == "FIX" ? 1 : 0 }.AppendInsertAndReturnObject(sql);
			var packingStationType = new WhsLocationType("PST") { WLT_LocationClass = "PST", WLT_DefaultCycleCountGranularity = string.Empty }.AppendInsertAndReturnObject(sql);
			var packingStation = new WhsLocation(row.PK, area.PK, area.PK, packingStationType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "PICK1", "NEW") { WP_WL_PackingStation = packingStation }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ATP", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			var updBuilder = WhsLocation.UpdateWhere(packingStation.PK).Set(l => l.WL_WLT_LocationType, locationTypeTo.PK);
			AssertTriggerPreventsLocationTypeChange(updBuilder, "Attempt to change Location Type for a Packing Station Location that is used by Existing Picks.");
		}

		#endregion

		#region Implementation

		enum ExpectedTriggerBehavior
		{
			AllowChange,
			PreventChange
		}

		void AssertTriggerPreventsLocationTypeChange(IUpdateBuilder<WhsLocation, WhsLocation> updateBuilder, string message)
		{
			AssertExceptionThrown("Expected trigger to prevent change of LocationType for Location",
				typeof(SqlException),
				message,
				() => updateBuilder.Post(TestConnection),
				assertStartsWith: true);
		}

		void SaveToDB(SqlQueryBuilder sql)
		{
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsPick_PackingStationIsValid, WhsPickSchema.Constants.TableName, WhsPickSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckPickPackingStationCorrect))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsDockDoorAssignment_EnsureDockDoorAssignmentsAreReferenced_ByAPick, WhsDockDoorAssignmentSchema.Constants.TableName, TestConnection))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}
		#endregion
	}
}

