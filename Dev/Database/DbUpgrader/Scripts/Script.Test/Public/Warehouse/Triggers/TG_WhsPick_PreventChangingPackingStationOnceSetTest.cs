using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Warehouse.Triggers
{
	[TestedType(typeof(TG_WhsPick_PreventChangingPackingStationOnceSet))]
	class TG_WhsPick_PreventChangingPackingStationOnceSetTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_TG_WhsPick_PreventChangingPackingStationOnceSet : TransactionedTestCase
	{
		#region TestTrigger_Update_UpdateValidPackingStationFromNull

		public void TestTrigger_Update_UpdateValidPackingStationFromNull()
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
			new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "ATP", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			SaveToDB(sql);

			AssertNoExceptionThrown(
					"Trigger should allow a Packing Station to be changed from null to valid value.",
					() => TestConnection.ExecuteNonQuery(WhsPick.UpdateWhere(pick.PK).Set(pl => pl.WP_WL_PackingStation, packingStation).AsSQL()));
		}

		#endregion

		#region TestTrigger_Update_UpdateValidPackingStationToAnotherValue

		public void TestTrigger_Update_UpdateValidPackingStationToAnotherValue()
		{
			TestTrigger_Update_UpdateValidPackingStationToAnotherValueCore(replaceWithNullLoc: false);
		}

		public void TestTrigger_Update_UpdateValidPackingStationToAnotherValue_Null()
		{
			TestTrigger_Update_UpdateValidPackingStationToAnotherValueCore(replaceWithNullLoc: true);
		}

		void TestTrigger_Update_UpdateValidPackingStationToAnotherValueCore(bool replaceWithNullLoc)
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			var whs1 = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A").AppendInsertAndReturnObject(sql);
			var packingStationType = new WhsLocationType("PST") { WLT_LocationClass = "PST", WLT_DefaultCycleCountGranularity = string.Empty }.AppendInsertAndReturnObject(sql);
			var packingStation1 = new WhsLocation(row1.PK, area1.PK, area1.PK, packingStationType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);

			var area2 = new WhsArea(whs1.PK, "AREA2").AppendInsertAndReturnObject(sql);
			var row2 = new WhsRow(whs1, "B").AppendInsertAndReturnObject(sql);
			var packingStation2 = new WhsLocation(row2.PK, area2.PK, area2.PK, packingStationType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs1, "PICK1", "NEW") { WP_WL_PackingStation = packingStation1 }.AppendInsertAndReturnObject(sql);
			new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "ATP", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			SaveToDB(sql);

			AssertExceptionThrown(
					"Trigger should not allow a Packing Station to be changed once set and valid.",
					typeof(SqlException),
					"Attempt to change Packing Station value once set is invalid.",
					() =>
					{
						var newPackingStation = replaceWithNullLoc ? null : packingStation2;
						TestConnection.ExecuteNonQuery(WhsPick.UpdateWhere(pick.PK).Set(pl => pl.WP_WL_PackingStation, newPackingStation).AsSQL());
					},
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

		#endregion
	}
}
