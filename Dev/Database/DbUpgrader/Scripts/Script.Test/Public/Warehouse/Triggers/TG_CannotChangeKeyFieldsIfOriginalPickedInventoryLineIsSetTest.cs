using System;
using System.Linq.Expressions;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_CannotChangeKeyFieldsIfOriginalPickedInventoryLineIsSet))]
	[UseSnapshotProtection]
	class TG_CannotChangeKeyFieldsIfOriginalPickedInventoryLineIsSetTest : DBCreateTriggerScriptTest
	{
		#region TestTrigger_WZ_VerifiedEmpty

		public void TestTrigger_WZ_VerifiedEmpty_OriginalPickedInventoryLineNotSet()
		{
			TestTrigger_UpdateKeyFields((l) => l.WZ_VerifiedEmpty, "Y", "", setOriginalPickedInventoryLine: false, expectException: false);
		}

		public void TestTrigger_WZ_VerifiedEmpty_OriginalPickedInventoryLineSet()
		{
			TestTrigger_UpdateKeyFields((l) => l.WZ_VerifiedEmpty, "Y", "", setOriginalPickedInventoryLine: true, expectException: true);
		}

		#endregion

		#region TestTrigger_WZ_OriginalReservedQty

		public void TestTrigger_WZ_OriginalReservedQty_OriginalPickedInventoryLineNotSet()
		{
			TestTrigger_UpdateKeyFields((l) => l.WZ_OriginalReservedQty, 10m, 0m, setOriginalPickedInventoryLine: false, expectException: false);
		}

		public void TestTrigger_WZ_OriginalReservedQty_OriginalPickedInventoryLineSet()
		{
			TestTrigger_UpdateKeyFields((l) => l.WZ_OriginalReservedQty, 10m, 0m, setOriginalPickedInventoryLine: true, expectException: true);
		}

		#endregion

		#region TestTrigger_WZ_F3_NKAllocatedPackType

		public void TestTrigger_WZ_F3_NKAllocatedPackType_OriginalPickedInventoryLineNotSet()
		{
			TestTrigger_UpdateKeyFields((l) => l.WZ_F3_NKAllocatedPackType, "ABC", "", setOriginalPickedInventoryLine: false, expectException: false);
		}

		public void TestTrigger_WZ_F3_NKAllocatedPackType_OriginalPickedInventoryLineSet()
		{
			TestTrigger_UpdateKeyFields((l) => l.WZ_F3_NKAllocatedPackType, "ABC", "", setOriginalPickedInventoryLine: true, expectException: true);
		}

		#endregion

		#region TestTrigger_UpdateKeyFields

		void TestTrigger_UpdateKeyFields<T>(Expression<Func<WhsPickLine, T>> property, T valueChange, T noValueChange, bool setOriginalPickedInventoryLine, bool expectException)
		{
			var sql = new SqlQueryBuilder();
			var now = DateTime.Now.ToSmallDateTimeFloor();
			var data = SetupTestData(sql);
			var receive = new WhsDocket(data.Client.PK, data.Warehouse.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = now, WD_ArrivalDate = now }.AppendInsertAndReturnObject(sql);
			var receiveLine = CreateReceiveLine(sql, data, receive, now);
			var originalPickedInventoryLine = CreateReceiveLine(sql, data, receive, now);

			var pick = new WhsPick(data.Warehouse, "P1", "PIC").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(data.Client.PK, data.Warehouse.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, data.Product.PK, 10m).AppendInsertAndReturnObject(sql);
			var pickline = new WhsPickLine(receiveLine, orderLine, 5m)
			{
				WZ_WE_OriginalPickedInventoryLine = setOriginalPickedInventoryLine ? originalPickedInventoryLine : null
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown("Should not blowup as there was no change to the value.",
				() => WhsPickLine
				.UpdateWhere(pickline.PK)
				.Set(property, noValueChange).Post(TestConnection));

			var changeValueUpdateSQL = new SqlQueryBuilder(
				WhsPickLine
				.UpdateWhere(pickline.PK)
				.Set(property, valueChange).AsSQL());

			if (expectException)
			{
				AssertExceptionThrown(typeof(SqlException), "Attempted to change key fields when original pick inventoryLine is set.", () => TestConnection.ExecuteNonQuery(changeValueUpdateSQL.ToStringWithNewLineBetweenAppends()), true);
			}
			else
			{
				AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(changeValueUpdateSQL.ToStringWithNewLineBetweenAppends()));
			}
		}

		#endregion

		#region Implementation

		WhsDocketLine CreateReceiveLine(SqlQueryBuilder sql, TestData data, WhsDocket receive, DateTime arrivalDate)
		{
			return new WhsDocketLine(receive, data.Product.PK, 10m)
			{
				WE_AdjustmentArrivalDate = arrivalDate,
				WE_CurrentInventoryStatus = "AVL",
				WE_OriginalInventoryStatus = "AVL",
				WE_WL = data.Location.PK,
				WE_StockOnHand = 10m
			}.AppendInsertAndReturnObject(sql);
		}
		class TestData
		{
			public TestData(WhsWarehouse warehouse, OrgHeader client, OrgSupplierPart product, WhsLocation location)
			{
				Warehouse = warehouse;
				Client = client;
				Location = location;
				Product = product;
			}

			public readonly WhsWarehouse Warehouse;
			public readonly OrgHeader Client;
			public readonly WhsLocation Location;
			public readonly OrgSupplierPart Product;
		}

		TestData SetupTestData(SqlQueryBuilder sql)
		{
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var warehouse = new WhsWarehouse("W1", branch.PK).WithDockDoor(TestConnection);
			var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(warehouse, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(warehouse.PK, "A1").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			return new TestData(warehouse, client, product, location);
		}
		#endregion
	}
}

