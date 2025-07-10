using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsArea_PreventChangeToFromIPR))]
	class TG_WhsArea_PreventChangeToFromIPRTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_TG_WhsArea_PreventChangeToFromIPRTest : TransactionedTestCase
	{
		public void TestTrigger_IPRToFRE()
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

			AssertTrigger_Update_Fails(
				"Should Not be able to change area type from IPR to FRE.",
				iPR_areaNoSOHPK,
				"FRE");
		}

		public void TestTrigger_FREToIPR()
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
				"FRE Area must exist in DB",
				WhsArea.ExistsInDB(TestConnection, fRE_areaNoSOHPK));

			AssertTrigger_Update_Fails(
				"Should Not be able to change area type from FRE to IPR.",
				fRE_areaNoSOHPK,
				"IPR");
		}

		public void TestTrigger_FREToEXE()
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
				"FRE Area must exist in DB",
				WhsArea.ExistsInDB(TestConnection, fRE_areaNoSOHPK));

			AssertTrigger_Update_Success(
				"Should be able to change area type from FRE to EXE.",
				fRE_areaNoSOHPK,
				"EXE");
		}

		#region Implementation

		void AssertTrigger_Update_Success(
			string message,
			Guid areaPK,
			string newAreaType)
		{
			AssertNoExceptionThrown(
				message,
				() => TestConnection.ExecuteNonQuery(
					WhsArea.UpdateWhere(areaPK)
					.Set(a => a.WA_AreaType, newAreaType)
					.AsSQL()));
		}

		void AssertTrigger_Update_Fails(
			string errorDescription,
			Guid areaPK,
			string newAreaType)
		{
			AssertExceptionThrown(
				errorDescription,
				typeof(SqlException),
				"Attempt to change the Area Type to/from IPR type.",
				() => TestConnection.ExecuteNonQuery(
					WhsArea.UpdateWhere(areaPK)
					.Set(a => a.WA_AreaType, newAreaType)
					.AsSQL()),
				assertStartsWith: true);
		}

		#endregion
	}
}

