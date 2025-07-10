using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Warehouse.Triggers
{
	[TestedType(typeof(TG_WhsPick_PackingStationIsValid))]
	class TG_WhsPick_PackingStationIsValidTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_TG_WhsPick_PackingStationIsValid : TransactionedTestCase
	{
		#region TestTrigger_Insert

		public void TestTrigger_Insert()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			// setup data (non-virtual warehouse, packing station of type PST and status NOR, and an associated pick
			var whs1 = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A").AppendInsertAndReturnObject(sql);

			var packingStationType = new WhsLocationType("PST") { WLT_LocationClass = "PST", WLT_DefaultCycleCountGranularity = string.Empty }.AppendInsertAndReturnObject(sql);
			var packingStation = new WhsLocation(row1.PK, area1.PK, area1.PK, packingStationType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs1, "PICK1", "NEW") { WP_WL_PackingStation = packingStation }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "ATP", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown(
				"WhsPick with a valid packing station and associated 'ORD' docket should be inserted without errors",
				() => SaveToDB(sql));
		}

		#endregion

		#region TestTrigger_Insert_PackingStationNotSameWhsAsPick

		public void TestTrigger_Insert_PackingStationNotSameWhsAsPick()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			// create non-virtual warehouse
			var whs1 = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A").AppendInsertAndReturnObject(sql);

			// create packing station in first whs
			var packingStationType = new WhsLocationType("PST") { WLT_LocationClass = "PST", WLT_DefaultCycleCountGranularity = string.Empty }.AppendInsertAndReturnObject(sql);
			var packingStation = new WhsLocation(row1.PK, area1.PK, area1.PK, packingStationType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);

			// create another non-virtual warehouse
			var branch2 = new GlbBranch("BR2").AppendInsertAndReturnObject(sql);
			var whs2 = new WhsWarehouse("WH2", branch2.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);

			var pick = new WhsPick(whs2, "PICK1", "NEW") { WP_WL_PackingStation = packingStation }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs2.PK, "ORD", "ORD", "ATP", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);

			AssertExceptionThrown(
					"Trigger should not allow to a Pick and the associated Packing Station to be located at different warehouses.",
					typeof(SqlException),
					InvalidPickAndPackingStationError,
					() => SaveToDB(sql),
					assertStartsWith: true);
		}

		#endregion

		#region TestTrigger_Insert_PackingStationInVirtualWhs

		public void TestTrigger_Insert_PackingStationInVirtualWhs()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			var whs1 = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = true }.AppendInsertAndReturnObject(sql);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A").AppendInsertAndReturnObject(sql);

			var packingStationType = new WhsLocationType("PST") { WLT_LocationClass = "PST", WLT_DefaultCycleCountGranularity = string.Empty }.AppendInsertAndReturnObject(sql);
			var packingStation1 = new WhsLocation(row1.PK, area1.PK, area1.PK, packingStationType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);
			var pick1 = new WhsPick(whs1, "PICK1", "NEW") { WP_WL_PackingStation = packingStation1 }.AppendInsertAndReturnObject(sql);
			var order1 = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "ATP", "O1") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);

			AssertExceptionThrown(
					"Trigger should not allow to a Packing Station to be located at a Virtual Warehouse.",
					typeof(SqlException),
					InvalidPickAndPackingStationError,
					() => SaveToDB(sql),
					assertStartsWith: true
				);
		}

		#endregion

		#region TestTrigger_Insert_PackingStationIncorrectLocationClass

		public void TestTrigger_Insert_PackingStationIncorrectLocationClass()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			var whs1 = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A").AppendInsertAndReturnObject(sql);

			var packingStationType = new WhsLocationType("FIX")
			{
				WLT_LocationClass = "FIX",
				WLT_DefaultCycleCountGranularity = "PWA",
				WLT_MaximumNumberOfProducts = 1
			}.AppendInsertAndReturnObject(sql);
			var packingStation1 = new WhsLocation(row1.PK, area1.PK, area1.PK, packingStationType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);

			var pick1 = new WhsPick(whs1, "PICK1", "NEW") { WP_WL_PackingStation = packingStation1 }.AppendInsertAndReturnObject(sql);
			var order1 = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "ATP", "O1") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);

			AssertExceptionThrown(
					"Trigger should not allow a Packing Station to have a Location Type other than PST (Packing Station).",
					typeof(SqlException),
					InvalidPickAndPackingStationError,
					() => SaveToDB(sql),
					assertStartsWith: true);
		}

		#endregion

		#region TestTrigger_Insert_PackingStationIncorrectStatus

		public void TestTrigger_Insert_PackingStationIncorrectStatus()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			var whs1 = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A").AppendInsertAndReturnObject(sql);

			var packingStationType = new WhsLocationType("PST") { WLT_LocationClass = "PST", WLT_DefaultCycleCountGranularity = string.Empty }.AppendInsertAndReturnObject(sql);
			var packingStation1 = new WhsLocation(row1.PK, area1.PK, area1.PK, packingStationType.PK) { WL_LocationStatus = "VOI" }.AppendInsertAndReturnObject(sql);

			var pick1 = new WhsPick(whs1, "PICK1", "NEW") { WP_WL_PackingStation = packingStation1 }.AppendInsertAndReturnObject(sql);
			var order1 = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "ATP", "O1") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);

			AssertExceptionThrown(
					"Trigger should not allow a Packing Station to have a Location Status other than NOR (Normal).",
					typeof(SqlException),
					InvalidPickAndPackingStationError,
					() => SaveToDB(sql),
					assertStartsWith: true);
		}

		#endregion

		#region TestTrigger_Insert_PickWithNoAssociatedOrder

		public void TestTrigger_Insert_PickWithNoAssociatedOrder()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			var whs1 = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A").AppendInsertAndReturnObject(sql);
			var packingStationType = new WhsLocationType("PST") { WLT_LocationClass = "PST", WLT_DefaultCycleCountGranularity = string.Empty }.AppendInsertAndReturnObject(sql);
			var packingStation1 = new WhsLocation(row1.PK, area1.PK, area1.PK, packingStationType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);

			var pick1 = new WhsPick(whs1, "PICK1", "NEW") { WP_WL_PackingStation = packingStation1 }.AppendInsertAndReturnObject(sql);

			AssertExceptionThrown(
					"Trigger should not allow a Pick to exist with no associated Order.",
					typeof(SqlException),
					"Picks with Packing Stations must have at least one Whs Order.",
					() => SaveToDB(sql),
					assertStartsWith: true);
		}

		#endregion

		#region TestTrigger_Insert_PickWithAssociatedWorkOrder

		public void TestTrigger_Insert_PickWithAssociatedWorkOrder()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			var whs1 = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A").AppendInsertAndReturnObject(sql);
			var packingStationType = new WhsLocationType("PST") { WLT_LocationClass = "PST", WLT_DefaultCycleCountGranularity = string.Empty }.AppendInsertAndReturnObject(sql);
			var packingStation1 = new WhsLocation(row1.PK, area1.PK, area1.PK, packingStationType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);

			var pick1 = new WhsPick(whs1, "PICK1", "NEW") { WP_WL_PackingStation = packingStation1 }.AppendInsertAndReturnObject(sql);
			var workorder1 = new WhsDocket(client.PK, whs1.PK, "WOR", "ASS", "ATP", "O1") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);

			AssertExceptionThrown(
					"Trigger should not allow a Pick to exist with no associated Order.",
					typeof(SqlException),
					"Picks with Packing Stations must have at least one Whs Order.",
					() => SaveToDB(sql),
					assertStartsWith: true);
		}

		#endregion

		#region TestTrigger_Update

		public void TestTrigger_Update()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			// setup data (non-virtual warehouse, packing station of type PST and status NOR, and an associated pick
			var whs1 = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A").AppendInsertAndReturnObject(sql);
			var packingStationType = new WhsLocationType("PST") { WLT_LocationClass = "PST", WLT_DefaultCycleCountGranularity = string.Empty }.AppendInsertAndReturnObject(sql);
			var packingStation = new WhsLocation(row1.PK, area1.PK, area1.PK, packingStationType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs1, "PICK1", "NEW").AppendInsertAndReturnObject(sql);
			new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "ATP", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			SaveToDB(sql);

			AssertNoExceptionThrown(
				"WhsPick with a valid packing station and associated 'ORD' docket should be inserted without errors",
				() => UpdatePackingStation(packingStation, pick));
		}

		#endregion

		#region TestTrigger_Update_PackingStationNotSameWhsAsPick

		public void TestTrigger_Update_PackingStationNotSameWhsAsPick()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			// create non-virtual warehouse
			var whs1 = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A").AppendInsertAndReturnObject(sql);

			// create packing station in first whs
			var packingStationType = new WhsLocationType("PST") { WLT_LocationClass = "PST", WLT_DefaultCycleCountGranularity = string.Empty }.AppendInsertAndReturnObject(sql);
			var packingStation = new WhsLocation(row1.PK, area1.PK, area1.PK, packingStationType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);

			// create another non-virtual warehouse
			var branch2 = new GlbBranch("BR2").AppendInsertAndReturnObject(sql);
			var whs2 = new WhsWarehouse("WH2", branch2.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);

			var pick = new WhsPick(whs2, "PICK1", "NEW").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs2.PK, "ORD", "ORD", "ATP", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			SaveToDB(sql);

			AssertExceptionThrown(
					"Trigger should not allow to a Pick and the associated Packing Station to be located at different warehouses.",
					typeof(SqlException),
					InvalidPickAndPackingStationError,
					() => UpdatePackingStation(packingStation, pick),
					assertStartsWith: true);
		}

		#endregion

		#region TestTrigger_Update_PackingStationInVirtualWhs

		public void TestTrigger_Update_PackingStationInVirtualWhs()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			var whs1 = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = true }.WithDockDoor(sql);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A").AppendInsertAndReturnObject(sql);

			var packingStationType = new WhsLocationType("PST") { WLT_LocationClass = "PST", WLT_DefaultCycleCountGranularity = string.Empty }.AppendInsertAndReturnObject(sql);
			var packingStation = new WhsLocation(row1.PK, area1.PK, area1.PK, packingStationType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs1, "PICK1", "NEW").AppendInsertAndReturnObject(sql);
			new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "ATP", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			SaveToDB(sql);

			AssertExceptionThrown(
					"Trigger should not allow to a Packing Station to be located at a Virtual Warehouse.",
					typeof(SqlException),
					InvalidPickAndPackingStationError,
					() => UpdatePackingStation(packingStation, pick),
					assertStartsWith: true
				);
		}

		#endregion

		#region TestTrigger_Update_PackingStationIncorrectLocationClass

		public void TestTrigger_Update_PackingStationIncorrectLocationClass()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			var whs1 = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A").AppendInsertAndReturnObject(sql);

			var packingStationType = new WhsLocationType("FIX")
			{
				WLT_LocationClass = "FIX",
				WLT_DefaultCycleCountGranularity = "PWA",
				WLT_MaximumNumberOfProducts = 1
			}.AppendInsertAndReturnObject(sql);
			var packingStation = new WhsLocation(row1.PK, area1.PK, area1.PK, packingStationType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs1, "PICK1", "NEW").AppendInsertAndReturnObject(sql);
			new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "ATP", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			SaveToDB(sql);

			AssertExceptionThrown(
					"Trigger should not allow a Packing Station to have a Location Type other than PST (Packing Station).",
					typeof(SqlException),
					InvalidPickAndPackingStationError,
					() => UpdatePackingStation(packingStation, pick),
					assertStartsWith: true);
		}

		#endregion

		#region TestTrigger_Update_PackingStationIncorrectStatus

		public void TestTrigger_Update_PackingStationIncorrectStatus()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			var whs1 = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A").AppendInsertAndReturnObject(sql);

			var packingStationType = new WhsLocationType("PST") { WLT_LocationClass = "PST", WLT_DefaultCycleCountGranularity = string.Empty }.AppendInsertAndReturnObject(sql);
			var packingStation = new WhsLocation(row1.PK, area1.PK, area1.PK, packingStationType.PK) { WL_LocationStatus = "VOI" }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs1, "PICK1", "NEW").AppendInsertAndReturnObject(sql);
			new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "ATP", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			SaveToDB(sql);

			AssertExceptionThrown(
					"Trigger should not allow a Packing Station to have a Location Status other than NOR (Normal).",
					typeof(SqlException),
					InvalidPickAndPackingStationError,
					() => UpdatePackingStation(packingStation, pick),
					assertStartsWith: true);
		}

		#endregion

		#region TestTrigger_Update_PickWithNoAssociatedOrder

		public void TestTrigger_Update_PickWithNoAssociatedOrder()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			var whs1 = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A").AppendInsertAndReturnObject(sql);

			var packingStationType = new WhsLocationType("PST") { WLT_LocationClass = "PST", WLT_DefaultCycleCountGranularity = string.Empty }.AppendInsertAndReturnObject(sql);
			var packingStation = new WhsLocation(row1.PK, area1.PK, area1.PK, packingStationType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs1, "PICK1", "NEW").AppendInsertAndReturnObject(sql);
			SaveToDB(sql);

			AssertExceptionThrown(
					"Trigger should not allow a Packing Station to be changed once set and valid.",
					typeof(SqlException),
					"Picks with Packing Stations must have at least one Whs Order.",
					() => UpdatePackingStation(packingStation, pick),
					assertStartsWith: true);
		}

		#endregion

		#region TestTrigger_Update_PickWithAssociatedWorkOrder

		public void TestTrigger_Update_PickWithAssociatedWorkOrder()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			var whs1 = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A").AppendInsertAndReturnObject(sql);

			var packingStationType = new WhsLocationType("PST") { WLT_LocationClass = "PST", WLT_DefaultCycleCountGranularity = string.Empty }.AppendInsertAndReturnObject(sql);
			var packingStation = new WhsLocation(row1.PK, area1.PK, area1.PK, packingStationType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs1, "PICK1", "NEW").AppendInsertAndReturnObject(sql);
			var workorder = new WhsDocket(client.PK, whs1.PK, "WOR", "ASS", "ATP", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			SaveToDB(sql);

			AssertExceptionThrown(
					"Trigger should not allow a Pick to exist with no associated Order.",
					typeof(SqlException),
					"Picks with Packing Stations must have at least one Whs Order.",
					() => UpdatePackingStation(packingStation, pick),
					assertStartsWith: true);
		}

		#endregion

		#region Implementations

		void SaveToDB(SqlQueryBuilder sql)
		{
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsPick_PackingStationIsValid, WhsPickSchema.Constants.TableName, WhsPickSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckPickPackingStationCorrect))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}

		void UpdatePackingStation(WhsLocation packingStation, WhsPick pick)
			=> TestConnection.ExecuteNonQuery(WhsPick.UpdateWhere(pick.PK).Set(pl => pl.WP_WL_PackingStation, packingStation).AsSQL());

		string InvalidPickAndPackingStationError => "Attempted to enter a invalid Packing Station and Pick combination.";

		#endregion

	}
}
