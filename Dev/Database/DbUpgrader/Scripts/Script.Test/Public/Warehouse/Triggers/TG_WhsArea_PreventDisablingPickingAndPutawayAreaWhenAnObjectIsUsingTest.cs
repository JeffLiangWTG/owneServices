using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsArea_PreventDisablingPickingAndPutawayAreaWhenAnObjectIsUsing))]
	class TG_WhsArea_PreventDisablingPickingAndPutawayAreaWhenAnObjectIsUsingTest : DBCreateTriggerScriptTest
	{
	}

	class TG_WhsArea_PreventDisablingPickingAndPutawayAreaWhenAnObjectIsUsingTestNonTransactionedTest : TestCase
	{
		#region TestWhsArea_Update_WA_IsPutawayAreaAndWA_IsPickingArea_LocationIsLinked

		[UseSnapshotProtection]
		public void TestWhsArea_Update_WA_IsPutawayAreaAndWA_IsPickingArea_LocationIsLinked()
		{
			var sql = new SqlQueryBuilder();
			var warehouse = new WhsWarehouse("W").WithDockDoor(Db.Connection);
			var pickingAreaPK = new WhsArea(warehouse.PK, "PIC") { WA_IsPutawayArea = false, WA_IsPickingArea = true }.AppendInsertAndReturnObject(sql).PK;
			var putawayAreaPK = new WhsArea(warehouse.PK, "PUT") { WA_IsPutawayArea = true, WA_IsPickingArea = false }.AppendInsertAndReturnObject(sql).PK;
			var areaWithNoLocationsPK = new WhsArea(warehouse.PK, "NON") { WA_IsPutawayArea = true, WA_IsPickingArea = true }.AppendInsertAndReturnObject(sql).PK;
			var row = new WhsRow(warehouse, "B").AppendInsertAndReturnObject(sql).PK;
			var location = new WhsLocation(row, pickingAreaPK, putawayAreaPK).AppendInsertAndReturnObject(sql).PK;
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertTrigger_Update_Fails("Changing putaway area to picking area must fail since there is a location referencing it as putaway area.", "Attempt to update area as picking only although there are locations referencing to it as putaway area.\r\nThe transaction ended in the trigger. The batch has been aborted.", putawayAreaPK, isPicking: true, isPutaway: false);
			AssertTrigger_Update_Fails("Changing picking area to putaway area must fail since there is a location referencing it as picking area.", "Attempt to update area as putaway only although there are locations referencing to it as picking area.\r\nThe transaction ended in the trigger. The batch has been aborted.", pickingAreaPK, isPicking: false, isPutaway: true);

			AssertTrigger_Update_Success("Since no locations are referencing areaWithNoLocations, user can mark it as putaway only.", areaWithNoLocationsPK, isPicking: false, isPutaway: true);
			AssertTrigger_Update_Success("Since no locations are referencing areaWithNoLocations, user can mark it as picking only.", areaWithNoLocationsPK, isPicking: true, isPutaway: false);

			AssertTrigger_Update_Success("Should be able to change the picking only area to both.", pickingAreaPK, isPicking: true, isPutaway: true);
			AssertTrigger_Update_Success("Should be able to change the putaway only area to both.", putawayAreaPK, isPicking: true, isPutaway: true);
		}

		#endregion

		#region Implementation

		void AssertTrigger_Update_Success(string message, Guid areaPK, bool isPicking, bool isPutaway)
		{
			var isPickingString = isPicking.AsSQL();
			var isPutawayString = isPutaway.AsSQL();

			AssertNoExceptionThrown(message,
				() => WhsArea
				.UpdateWhere(areaPK)
				.Set(a => a.WA_IsPickingArea, isPicking)
				.Set(a => a.WA_IsPutawayArea, isPutaway).Post(Db.Connection));
		}

		void AssertTrigger_Update_Fails(string message, string expectedErrorMessage, Guid areaPK, bool isPicking, bool isPutaway)
		{
			var isPickingString = isPicking.AsSQL();
			var isPutawayString = isPutaway.AsSQL();

			var sql = WhsArea
				.UpdateWhere(areaPK)
				.Set(a => a.WA_IsPickingArea, isPicking)
				.Set(a => a.WA_IsPutawayArea, isPutaway).AsSQL();

			AssertTriggerFails(message, expectedErrorMessage, sql);
		}

		static void AssertTriggerFails(string errorDescription, string expectedErrorMessage, string sql)
		{
			using (Db.DisposableActionForDbConnection())
			using (var newConnection = Db.NewExtraConnectionToMainDb())
			{
				AssertExceptionThrown<SqlException>(errorDescription, expectedErrorMessage, () => newConnection.ExecuteNonQuery(sql));
			}
		}
		#endregion
	}
}

