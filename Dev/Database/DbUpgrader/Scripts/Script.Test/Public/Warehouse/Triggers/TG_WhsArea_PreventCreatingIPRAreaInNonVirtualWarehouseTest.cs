using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsArea_PreventCreatingIPRAreaInNonVirtualWarehouse))]
	class TG_WhsArea_PreventCreatingIPRAreaInNonVirtualWarehouseTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_TG_WhsArea_PreventCreatingIPRAreaInNonVirtualWarehouseTest : TransactionedTestCase
	{
		public void TestTrigger_VirtualWhs_IPRArea()
		{
			var sql = new SqlQueryBuilder();
			var warehouse = new WhsWarehouse("W") { WW_IsVirtualWarehouse = true }.WithDockDoor(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			Assert(
				"Warehouse must exist in DB",
				WhsWarehouse.ExistsInDB(TestConnection, warehouse.PK));

			AssertTrigger_Insert_Success(
				"Should be able to create IPR area in virtual warehouse.",
				warehouse.PK,
				"IPR");
		}

		public void TestTrigger_VirtualWhs_FREArea()
		{
			var sql = new SqlQueryBuilder();
			var warehouse = new WhsWarehouse("W") { WW_IsVirtualWarehouse = true }.WithDockDoor(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			Assert(
				"Warehouse must exist in DB",
				WhsWarehouse.ExistsInDB(TestConnection, warehouse.PK));

			AssertTrigger_Insert_Success(
				"Should be able to create FRE area in virtual warehouse.",
				warehouse.PK,
				"FRE");
		}

		public void TestTrigger_NonVirtualWhs_IPRArea()
		{
			var sql = new SqlQueryBuilder();
			var warehouse = new WhsWarehouse("W") { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			Assert(
				"Warehouse must exist in DB",
				WhsWarehouse.ExistsInDB(TestConnection, warehouse.PK));

			var sql2 = new SqlQueryBuilder();
			new WhsArea(warehouse.PK, "AreaN") { WA_AreaType = "IPR" }.AppendInsertAndReturnObject(sql2);

			AssertExceptionThrown(
				"Should NOT be able to create IPR area in non-virtual warehouse.",
				typeof(SqlException),
				"Non-virtual Warehouses cannot have Areas with type IPR.",
				() => TestConnection.ExecuteNonQuery(sql2.ToStringWithNewLineBetweenAppends()),
				assertStartsWith: true);
		}

		public void TestTrigger_NonVirtualWhs_FREArea()
		{
			var sql = new SqlQueryBuilder();
			var warehouse = new WhsWarehouse("W") { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			Assert(
				"Warehouse must exist in DB",
				WhsWarehouse.ExistsInDB(TestConnection, warehouse.PK));

			AssertTrigger_Insert_Success(
				"Should be able to create FRE area in non-virtual warehouse.",
				warehouse.PK,
				"FRE");
		}

		#region Implementation

		void AssertTrigger_Insert_Success(string errorDescription, Guid whsPK, string areaType)
		{
			var sql = new SqlQueryBuilder();
			new WhsArea(whsPK, "AreaT") { WA_AreaType = areaType }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown(
				errorDescription,
				() => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends()));
		}

		#endregion
	}
}

