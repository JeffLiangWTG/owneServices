using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsPick_HasCorrectWarehouse))]
	class TG_WhsPick_HasCorrectWarehouseTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_WhsPick_HasCorrectWarehouseTest : TestCase
	{
		const string ErrorMessage = "Order / Work Order warehouse does not match Pick warehouse.";

		#region TestTrigger_Update_StandalonePick

		[UseSnapshotProtection]
		public void TestTrigger_Update_StandalonePick()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch1 = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(mainConnection);
				var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(mainConnection);
				var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(mainConnection);

				var pick = new WhsPick(whs1, "P1", "ENT").AppendInsertAndReturnObject(sql);
				SaveToDB(mainConnection, sql);

				AssertNoExceptionThrown(
					"Standalone picks can have their warehouse changed to any value.",
					() =>
					{
						WhsPick
						.UpdateWhere(pick.PK)
						.Set(p => p.WP_WW_Whs, null)
						.Set(p => p.WP_WL_DockDoor, null).Post(mainConnection);

						WhsPick
						.UpdateWhere(pick.PK)
						.Set(p => p.WP_WW_Whs, whs1).Post(mainConnection);

						WhsPick
						.UpdateWhere(pick.PK)
						.Set(p => p.WP_WW_Whs, whs2).Post(mainConnection);
					});
			}
		}

		#endregion

		#region TestTrigger_Update_WithOrder

		[UseSnapshotProtection]
		public void TestTrigger_Update_WithOrder()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch1 = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(mainConnection);
				var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(mainConnection);
				var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(mainConnection);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				var pick = new WhsPick(whs1, "P1", "ENT").AppendInsertAndReturnObject(sql);
				var order = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, product.PK, 10m).AppendInsertAndReturnObject(sql);
				SaveToDB(mainConnection, sql);

				AssertExceptionThrown(
					"Trigger should not allow to change warehouse of pick with order.",
					typeof(SqlException),
					ErrorMessage,
					() =>
					{
						WhsPick
						.UpdateWhere(pick.PK)
						.Set(p => p.WP_WW_Whs, whs2)
						.Set(p => p.WP_WL_DockDoor, whs2.WW_DefaultOutboundDockDoor).Post(mainConnection);
					},
					assertStartsWith: true);
			}
		}

		#endregion

		#region TestTrigger_Update_WithOrder_Locks

		[UseSnapshotProtection]
		public void TestTrigger_Update_WithOrder_Locks()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch1 = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(mainConnection);
				var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(mainConnection);
				var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(mainConnection);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				var pick = new WhsPick(whs1, "P1", "ENT").AppendInsertAndReturnObject(sql);
				var order = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
				new WhsDocketLine(order, product.PK, 10m).AppendInsertAndReturnObject(sql);
				SaveToDB(mainConnection, sql);

				using (var connection1 = Db.NewExtraConnectionToMainDb())
				using (connection1.BeginTransactionWithManager())
				{
					WhsPick.UpdateWhere(pick.PK).Set(p => p.WP_WW_Whs.FK, whs2.PK).Set(p => p.WP_WL_DockDoor, whs2.WW_DefaultOutboundDockDoor).Post(connection1);

					var task = Task.Run(() =>
					{
						using (var connection2 = Db.NewExtraConnectionToMainDb())
						using (connection2.BeginTransactionWithManager())
						{
							AssertExceptionThrown("Trigger should not allow to change warehouse of pick with order.", typeof(SqlException), ErrorMessage,
							() => connection2.ExecuteNonQuery(WhsDocket.UpdateWhere(order.PK).Set(d => d.WD_WP, pick.PK).Set(d => d.WD_DocketStatus, "PIC").AsSQL(), 3), assertStartsWith: true);
						}
					});

					Thread.Sleep(1000);

					connection1.CommitTransaction();
					task.Wait(2000);
				}
			}
		}

		#endregion

		#region TestTrigger_Update_WithWorkOrder

		[UseSnapshotProtection]
		public void TestTrigger_Update_WithWorkOrder()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch1 = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(mainConnection);
				var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(mainConnection);
				var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(mainConnection);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var mainProduct = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
				var componentProduct = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
				new OrgPartBOM(mainProduct, componentProduct).AppendInsertAndReturnObject(sql);

				var pick = new WhsPick(whs1, "P1", "ENT") { WP_WL_DockDoor = null }.AppendInsertAndReturnObject(sql);
				var workOrder = new WhsDocket(client.PK, whs1.PK, "WOR", "ASS", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
				var workOrderLine = new WhsDocketLine(workOrder, mainProduct.PK, 10m).AppendInsertAndReturnObject(sql);
				SaveToDB(mainConnection, sql);

				AssertExceptionThrown(
					"Trigger should not allow to change warehouse of pick with order.",
					typeof(SqlException),
					ErrorMessage,
					() =>
					{
						WhsPick
						.UpdateWhere(pick.PK)
						.Set(p => p.WP_WW_Whs, whs2).Post(mainConnection);
					},
					assertStartsWith: true);
			}
		}

		#endregion

		static void SaveToDB(DbConnection connection, SqlQueryBuilder sql) => SaveToDB(connection, sql.ToStringWithNewLineBetweenAppends());

		static void SaveToDB(DbConnection connection, string sql)
		{
			using (connection.BeginTransactionWithManager())
			{
				connection.ExecuteNonQuery(sql);
				connection.CommitTransaction();
			}
		}
	}
}

