using System;
using System.Linq.Expressions;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder))]
	class TG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrderTest : DBCreateTriggerScriptTest
	{
	}

	[UseSnapshotProtection]
	class Trigger_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrderTest : TestCase
	{
		const string PreventChangeCriticalFieldsOnPickedOrder = "TG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder";

		#region TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_AttachToPick

		public void TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_AttachToPick()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("B01").InsertAndReturnObject(mainConnection);
				var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(mainConnection);
				var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
				var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
				var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
				var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, product.PK, 15m, location.PK) { WE_ClientOrderedUnits = 15m, WE_StockOnHand = 15m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
				var pick = new WhsPick(whs, "Pick", "NEW").AppendInsertAndReturnObject(sql);
				var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, product.PK, 15m).AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, orderLine, 15m) { WZ_OriginalReservedQty = 15m }.AppendInsertAndReturnObject(sql);

				AssertNoExceptionThrown("Save in transaction.", () => ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends()));

				AssertNoExceptionThrown("Should be able to attach to pick.", () => AttachToPickAndSave(mainConnection, order.PK, pick.PK));
			}
		}

		#endregion

		#region TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_Update

		public void TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_Update_WE_OP()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var productNew = new OrgSupplierPart("ProductNew").AppendInsertAndReturnObject(sql);
				ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());
				TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_Update("ORD", "ORD", (ol) => ol.WE_OP, productNew.PK, expectError: true);
			}
		}

		public void TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_Update_WE_ExpiryDate()
		{
			TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_Update("ORD", "ORD", (ol) => ol.WE_ExpiryDate, DateTime.Now, expectError: true);
		}

		public void TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_Update_WE_PackingDate()
		{
			TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_Update("ORD", "ORD", (ol) => ol.WE_PackingDate, DateTime.Now, expectError: true);
		}

		public void TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_Update_WE_PartAttrib1()
		{
			TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_Update("ORD", "ORD", (ol) => ol.WE_PartAttrib1, "test", expectError: true);
		}

		public void TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_Update_WE_PartAttrib2()
		{
			TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_Update("ORD", "ORD", (ol) => ol.WE_PartAttrib2, "test", expectError: true);
		}

		public void TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_Update_WE_PartAttrib3()
		{
			TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_Update("ORD", "ORD", (ol) => ol.WE_PartAttrib3, "test", expectError: true);
		}

		public void TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_Update_WE_SerialNumber()
		{
			TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_Update("ORD", "ORD", (ol) => ol.WE_SerialNumber, "test", expectError: true);
		}

		public void TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_Update_WE_PalletID()
		{
			TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_Update("ORD", "ORD", (ol) => ol.WE_PalletID, "test", expectError: true);
		}

		public void TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_Update_NotCriticalFields()
		{
			TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_Update("ORD", "ORD", (ol) => ol.WE_BondedEntryKey, "test", expectError: false);
		}

		void TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_Update<T>(string docketType, string docketSubType, Expression<Func<WhsDocketLine, T>> property, T value, bool expectError)
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("B01").InsertAndReturnObject(mainConnection);
				var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(mainConnection);
				var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
				var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
				var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
				var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, product.PK, 15m, location.PK) { WE_ClientOrderedUnits = 15m, WE_StockOnHand = 15m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
				var pick = new WhsPick(whs, "Pick", "NEW").AppendInsertAndReturnObject(sql);
				var order = new WhsDocket(client.PK, whs.PK, docketType, docketSubType, "ENT", "O1").AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, product.PK, 15m).AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, orderLine, 15m) { WZ_OriginalReservedQty = 15m }.AppendInsertAndReturnObject(sql);

				AssertNoExceptionThrown("Save in transaction.", () => ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends()));

				AssertNoExceptionThrown("Should be able to attach to pick.", () => AttachToPickAndSave(mainConnection, order.PK, pick.PK));

				var updateSQL = WhsDocketLine.UpdateWhere(orderLine.PK).Set(property, value).AsSQL();
				if (expectError)
				{
					NUnit.Framework.Assert.That(() => ExecuteSqlInTransaction(mainConnection, updateSQL), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to change critical fields on picked order.", true), "Should not be able to update line if order is picked.");
					using (TestWhsDataSetupHelper.SuspendTrigger(PreventChangeCriticalFieldsOnPickedOrder,
						WhsDocketLineSchema.Constants.TableName, mainConnection))
					{
						AssertNoExceptionThrown("Should not have exception when triger is Suspended.", () => ExecuteSqlInTransaction(mainConnection, updateSQL));
					}
				}
				else
				{
					AssertNoExceptionThrown("Should not have exception when update non critical fields.", () => ExecuteSqlInTransaction(mainConnection, updateSQL));
				}
			}
		}

		#endregion

		#region TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_CheckOnlyForOrder

		public void TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_CheckOnlyForOrder()
		{
			TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_Update("WOR", "DIS", (ol) => ol.WE_PartAttrib1, "test", expectError: false);
		}

		#endregion

		// Concurrency tested in TG_PreventInvalidConcurrencyChangeTest (WhsDocketLine)

		#region Implementation

		void ExecuteSqlInTransaction(DbConnection connection, string sql)
		{
			using (connection.BeginTransactionWithManager())
			{
				connection.ExecuteNonQuery(sql);
				connection.CommitTransaction();
			}
		}

		void AttachToPickAndSave(DbConnection connection, Guid orderPK, Guid pickPK) => ExecuteSqlInTransaction(connection, AttachToPickSql(orderPK, pickPK));

		static string AttachToPickSql(Guid orderPK, Guid pickPK) =>
			WhsDocket
			.UpdateWhere(orderPK)
			.Set(d => d.WD_WP, pickPK)
			.Set(d => d.WD_DocketStatus, "PIC").AsSQL();

		#endregion
	}
}

