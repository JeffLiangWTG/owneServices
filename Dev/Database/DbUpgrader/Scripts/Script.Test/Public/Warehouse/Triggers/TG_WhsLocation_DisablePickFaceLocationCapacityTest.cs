using System;
using System.Linq.Expressions;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsLocation_DisablePickFaceLocationCapacity))]
	class TG_WhsLocation_DisablePickFaceLocationCapacityTest : DBCreateTriggerScriptTest
	{
		const string ErrorMessage = "Max capacity of Pick Face Location must be 0.";

		#region Insert

		#region TestTrigger_Insert_WL_MaxWeightIsZero

		public void TestTrigger_Insert_FIX_WL_MaxWeightIsZero()
		{
			TestTrigger_Insert_PickFaceCore("FIX", l => l.WL_MaxWeight = 0m, false);
		}

		public void TestTrigger_Insert_FIX_WL_MaxCubicIsZero()
		{
			TestTrigger_Insert_PickFaceCore("FIX", l => l.WL_MaxCubic = 0m, false);
		}

		public void TestTrigger_Insert_FIX_WL_MaxQuantityIsZero()
		{
			TestTrigger_Insert_PickFaceCore("FIX", l => l.WL_MaxQuantity = 0m, false);
		}

		public void TestTrigger_Insert_DPF_WL_MaxWeightIsZero()
		{
			TestTrigger_Insert_PickFaceCore("DPF", l => l.WL_MaxWeight = 0m, false);
		}

		public void TestTrigger_Insert_DPF_WL_MaxCubicIsZero()
		{
			TestTrigger_Insert_PickFaceCore("DPF", l => l.WL_MaxCubic = 0m, false);
		}

		public void TestTrigger_Insert_DPF_WL_MaxQuantityIsZero()
		{
			TestTrigger_Insert_PickFaceCore("DPF", l => l.WL_MaxQuantity = 0m, false);
		}

		#endregion

		#region TestTrigger_Insert_WL_MaxWeightNotZero

		public void TestTrigger_Insert_FIX_WL_MaxWeightNotZero()
		{
			TestTrigger_Insert_PickFaceCore("FIX", l => l.WL_MaxWeight = 5m, true);
		}

		public void TestTrigger_Insert_FIX_WL_MaxCubicNotZero()
		{
			TestTrigger_Insert_PickFaceCore("FIX", l => l.WL_MaxCubic = 5m, true);
		}

		public void TestTrigger_Insert_FIX_WL_MaxQuantityNotZero()
		{
			TestTrigger_Insert_PickFaceCore("FIX", l => l.WL_MaxQuantity = 5m, true);
		}

		public void TestTrigger_Insert_DPF_WL_MaxWeightNotZero()
		{
			TestTrigger_Insert_PickFaceCore("DPF", l => l.WL_MaxWeight = 5m, true);
		}

		public void TestTrigger_Insert_DPF_WL_MaxCubicNotZero()
		{
			TestTrigger_Insert_PickFaceCore("DPF", l => l.WL_MaxCubic = 5m, true);
		}

		public void TestTrigger_Insert_DPF_WL_MaxQuantityNotZero()
		{
			TestTrigger_Insert_PickFaceCore("DPF", l => l.WL_MaxQuantity = 5m, true);
		}

		#endregion

		#region TestTrigger_Insert_NonPickFace_ValueNotZero

		public void TestTrigger_Insert_NonPickFace_WL_MaxWeightNotZero()
		{
			TestTrigger_Insert_PickFaceCore("NOR", l => l.WL_MaxWeight = 5m, false);
		}

		public void TestTrigger_Insert_NonPickFace_WL_MaxCubicNotZero()
		{
			TestTrigger_Insert_PickFaceCore("NOR", l => l.WL_MaxCubic = 5m, false);
		}

		public void TestTrigger_Insert_NonPickFace_WL_MaxQuantityNotZero()
		{
			TestTrigger_Insert_PickFaceCore("NOR", l => l.WL_MaxQuantity = 5m, false);
		}

		#endregion

		void TestTrigger_Insert_PickFaceCore(string locationClass, Action<WhsLocation> setValue, bool expectedHasException)
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var rowA = new WhsRow(whs, "roa").AppendInsertAndReturnObject(sql).PK;
			var area = new WhsArea(whs.PK, "hello").AppendInsertAndReturnObject(sql).PK;
			var locationType = new WhsLocationType("AAA") { WLT_LocationClass = locationClass, WLT_MaximumNumberOfProducts = locationClass == "FIX" ? 1 : 0 }.AppendInsertAndReturnObject(sql).PK;
			var location = new WhsLocation(rowA, area, area, locationType) { WL_MaxQuantityUnit = "UNT", WL_MaxWeightUnit = "KG", WL_MaxCubicUnit = "M3" };

			setValue(location);

			location.AppendInsertAndReturnObject(sql);

			if (expectedHasException)
			{
				AssertExceptionThrown(typeof(SqlException), ErrorMessage, () => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends()), true);
			}
			else
			{
				AssertNoExceptionThrown("Should not have exception", () => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends()));
			}
		}

		#endregion

		#region Update

		public void TestTrigger_Update_FIX_WL_MaxWeightNotZero()
		{
			TestTrigger_Update_PickFaceCore("FIX", (l) => l.WL_MaxWeight, 5m, true);
		}

		public void TestTrigger_Update_FIX_WL_MaxCubicNotZero()
		{
			TestTrigger_Update_PickFaceCore("FIX",  (l) => l.WL_MaxCubic, 5m, true);
		}

		public void TestTrigger_Update_FIX_WL_MaxQuantityNotZero()
		{
			TestTrigger_Update_PickFaceCore("FIX",  (l) => l.WL_MaxQuantity, 5m, true);
		}

		public void TestTrigger_Update_DPF_WL_MaxWeightNotZero()
		{
			TestTrigger_Update_PickFaceCore("DPF",  (l) => l.WL_MaxWeight, 5m, true);
		}

		public void TestTrigger_Update_DPF_WL_MaxCubicNotZero()
		{
			TestTrigger_Update_PickFaceCore("DPF",  (l) => l.WL_MaxCubic, 5m, true);
		}

		public void TestTrigger_Update_DPF_WL_MaxQuantityNotZero()
		{
			TestTrigger_Update_PickFaceCore("DPF",  (l) => l.WL_MaxQuantity, 5m, true);
		}

		public void TestTrigger_Update_ChangeWL_WLT_LocationTypeToFIX()
		{
			var locationType = new WhsLocationType("FFF") { WLT_LocationClass = "FIX", WLT_MaximumNumberOfProducts = 1 };
			locationType.Insert(TestConnection);
			TestTrigger_Update_PickFaceCore("NOR",  (l) => l.WL_WLT_LocationType, locationType.PK, true, l => l.WL_MaxCubic = 5m);
		}

		public void TestTrigger_Update_ChangeWL_WLT_LocationTypeToDPF()
		{
			var locationType = new WhsLocationType("DDD") { WLT_LocationClass = "DPF" };
			locationType.Insert(TestConnection);
			TestTrigger_Update_PickFaceCore("NOR",  (l) => l.WL_WLT_LocationType, locationType.PK, true, l => l.WL_MaxCubic = 5m);
		}

		public void TestTrigger_Update_NonPickFace_WL_MaxWeightNotZero()
		{
			TestTrigger_Update_PickFaceCore("NOR",  (l) => l.WL_MaxWeight, 5m, false);
		}

		public void TestTrigger_Update_NonPickFace_WL_MaxCubicNotZero()
		{
			TestTrigger_Update_PickFaceCore("NOR",  (l) => l.WL_MaxCubic, 5m, false);
		}

		public void TestTrigger_Update_NonPickFace_WL_MaxQuantityNotZero()
		{
			TestTrigger_Update_PickFaceCore("NOR",  (l) => l.WL_MaxQuantity, 5m, false);
		}

		void TestTrigger_Update_PickFaceCore<T>(string locationClass, Expression<Func<WhsLocation, T>> property, T value, bool expectedHasException, Action<WhsLocation> setValueAction = null)
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var rowA = new WhsRow(whs, "roa").AppendInsertAndReturnObject(sql).PK;
			var area = new WhsArea(whs.PK, "hello").AppendInsertAndReturnObject(sql).PK;

			var locationType = new WhsLocationType("AAA") { WLT_LocationClass = locationClass, WLT_MaximumNumberOfProducts = locationClass == "FIX" ? 1 : 0 }.AppendInsertAndReturnObject(sql).PK;
			var location = new WhsLocation(rowA, area, area, locationType) { WL_MaxQuantityUnit = "UNT", WL_MaxWeightUnit = "KG", WL_MaxCubicUnit = "M3" };

			if (setValueAction != null)
			{
				setValueAction(location);
			}

			location.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var updateSql = WhsLocation
				.UpdateWhere(location.PK)
				.Set(property, value).AsSQL();
			if (expectedHasException)
			{
				AssertExceptionThrown(typeof(SqlException), ErrorMessage, () => TestConnection.ExecuteNonQuery(updateSql), true);
			}
			else
			{
				AssertNoExceptionThrown("Should not have exception", () => TestConnection.ExecuteNonQuery(updateSql));
			}
		}
		#endregion
	}
}

