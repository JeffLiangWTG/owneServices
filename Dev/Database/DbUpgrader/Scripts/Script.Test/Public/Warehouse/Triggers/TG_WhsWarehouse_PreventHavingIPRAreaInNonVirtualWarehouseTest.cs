using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsWarehouse_PreventHavingIPRAreaInNonVirtualWarehouse))]
	class TG_WhsWarehouse_PreventHavingIPRAreaInNonVirtualWarehouseTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_TG_WhsWarehouse_PreventHavingIPRAreaInNonVirtualWarehouseTest : TransactionedTestCase
	{
		public void TestTrigger_VirtualWhs_ToNonVirtualWhs_HasIPR()
		{
			var sql = new SqlQueryBuilder();
			var warehouse = new WhsWarehouse("W") { WW_IsVirtualWarehouse = true }.WithDockDoor(sql);
			var iPR_areaNoSOHPK = new WhsArea(warehouse.PK, "IPR_NON")
			{
				WA_IsPutawayArea = true,
				WA_IsPickingArea = true,
				WA_AreaType = "IPR"
			}.AppendInsertAndReturnObject(sql).PK;

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			Assert(
				"IPR Area must exist in DB",
				WhsArea.ExistsInDB(TestConnection, iPR_areaNoSOHPK));
			Assert(
				"Warehouse must exist in DB",
				WhsWarehouse.ExistsInDB(TestConnection, warehouse.PK));

			AssertExceptionThrown(
					"Should NOT be able to change warehouse to virtual, since warehouse has IPR area.",
					typeof(SqlException),
					"Non-virtual Warehouses cannot have Areas with type IPR.",
					() => TestConnection.ExecuteNonQuery(
						WhsWarehouse
							.UpdateWhere(warehouse.PK)
							.Set(w => w.WW_IsVirtualWarehouse, false)
							.AsSQL()),
					assertStartsWith: true);
		}

		public void TestTrigger_VirtualWhs_ToNonVirtualWhs()
		{
			var sql = new SqlQueryBuilder();
			var warehouse = new WhsWarehouse("W") { WW_IsVirtualWarehouse = true }.WithDockDoor(sql);
			var fRE_areaNoSOHPK = new WhsArea(warehouse.PK, "FRE_NON")
			{
				WA_IsPutawayArea = true,
				WA_IsPickingArea = true,
				WA_AreaType = "FRE"
			}.AppendInsertAndReturnObject(sql).PK;

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			Assert(
				"Warehouse must exist in DB",
				WhsWarehouse.ExistsInDB(TestConnection, warehouse.PK));
			Assert(
				"FRE Area must exist in DB",
				WhsArea.ExistsInDB(TestConnection, fRE_areaNoSOHPK));

			AssertTrigger_Success(
				"Should be able to change warehouse to non-virtual, since warehouse has no IPR area.",
				WhsWarehouse
					.UpdateWhere(warehouse.PK)
					.Set(w => w.WW_IsVirtualWarehouse, false)
					.AsSQL());
		}

		public void TestTrigger_NonVirtualWhs_VirtualWhs()
		{
			var sql = new SqlQueryBuilder();
			var warehouse = new WhsWarehouse("W") { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);
			var fRE_areaNoSOHPK = new WhsArea(warehouse.PK, "FRE_NON")
			{
				WA_IsPutawayArea = true,
				WA_IsPickingArea = true,
				WA_AreaType = "FRE"
			}.AppendInsertAndReturnObject(sql).PK;

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			Assert(
				"Warehouse must exist in DB",
				WhsWarehouse.ExistsInDB(TestConnection, warehouse.PK));
			Assert(
				"FRE Area must exist in DB",
				WhsArea.ExistsInDB(TestConnection, fRE_areaNoSOHPK));

			AssertTrigger_Success(
				"Should be able to change warehouse to virtual.",
				WhsWarehouse
					.UpdateWhere(warehouse.PK)
					.Set(w => w.WW_IsVirtualWarehouse, true)
					.AsSQL());
		}

		#region Implementation

		void AssertTrigger_Success(
			string message,
			string sql)
		{
			AssertNoExceptionThrown(
				message,
				() => TestConnection.ExecuteNonQuery(sql));
		}

		#endregion
	}
}
