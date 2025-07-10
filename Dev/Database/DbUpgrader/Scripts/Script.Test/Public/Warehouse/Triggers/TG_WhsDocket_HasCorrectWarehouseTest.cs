using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsDocket_HasCorrectWarehouse))]
	class TG_WhsDocket_HasCorrectWarehouseTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_WhsDocket_HasCorrectWarehouseTest : TestCase
	{
		const string ErrorMessage = "Order / Work Order warehouse does not match Pick warehouse.";

		#region TestTrigger_Insert_Order

		[UseSnapshotProtection]
		public void TestTrigger_Insert_Order()
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
				var order = new WhsDocket(client.PK, whs2.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, product.PK, 10m).AppendInsertAndReturnObject(sql);

				AssertExceptionThrown(
					"Should not allow to insert order that points to warehouse different from Pick.",
					typeof(SqlException),
					ErrorMessage,
					() => SaveToDB(mainConnection, sql),
					assertStartsWith: true);
			}
		}

		#endregion

		#region TestTrigger_Insert_WorkOrder

		[UseSnapshotProtection]
		public void TestTrigger_Insert_WorkOrder()
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
				var workOrder = new WhsDocket(client.PK, whs2.PK, "WOR", "ASS", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
				var workOrderLine = new WhsDocketLine(workOrder, mainProduct.PK, 10m).AppendInsertAndReturnObject(sql);

				AssertExceptionThrown(
					"Should not allow to insert work order that points to warehouse different from Pick.",
					typeof(SqlException),
					ErrorMessage,
					() => SaveToDB(mainConnection, sql),
					assertStartsWith: true);
			}
		}

		#endregion

		#region TestTrigger_Update_Order

		[UseSnapshotProtection]
		public void TestTrigger_Update_Order()
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

				var pick1 = new WhsPick(whs1, "P1", "ENT").AppendInsertAndReturnObject(sql);
				var pick2 = new WhsPick(whs2, "P2", "ENT").AppendInsertAndReturnObject(sql);
				var order = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, product.PK, 10m).AppendInsertAndReturnObject(sql);

				SaveToDB(mainConnection, sql.ToStringWithNewLineBetweenAppends());

				var changePickSQL = WhsDocket
					.UpdateWhere(order.PK)
					.Set(d => d.WD_WP, pick2.PK).AsSQL();
				AssertExceptionThrown(
					"Should not allow to re-link order to Pick from different warehouse.",
					typeof(SqlException),
					ErrorMessage,
					() => SaveToDB(mainConnection, changePickSQL),
					assertStartsWith: true);
			}
		}

		#endregion

		#region TestTrigger_Update_Order_Locks

		[UseSnapshotProtection]
		public void TestTrigger_Update_Order_Locks()
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

				var pick1 = new WhsPick(whs1, "P1", "ENT").AppendInsertAndReturnObject(sql);
				var pick2 = new WhsPick(whs2, "P2", "ENT").AppendInsertAndReturnObject(sql);
				var order = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, product.PK, 10m).AppendInsertAndReturnObject(sql);

				var unrelatedPick = new WhsPick(whs1, "P3", "ENT").AppendInsertAndReturnObject(sql);

				SaveToDB(mainConnection, sql);

				using (var connection1 = Db.NewExtraConnectionToMainDb())
				using (connection1.BeginTransactionWithManager())
				{
					WhsPick.UpdateWhere(unrelatedPick.PK).Set(p => p.WP_PercentageComplete, (byte)1).Post(connection1);

					using (var connection2 = Db.NewExtraConnectionToMainDb())
					using (connection2.BeginTransactionWithManager())
					{
						AssertExceptionThrown("Should fail immediately", typeof(SqlException), ErrorMessage,
							() => connection2.ExecuteNonQuery(WhsDocket.UpdateWhere(order.PK).Set(o => o.WD_WP, pick2.PK).AsSQL(), 2), assertStartsWith: true); // this should not fail with timeout
					}

					connection1.RollbackTransaction();
				}
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Update_Order_LocksSamePick()
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

				var pick1 = new WhsPick(whs1, "P1", "ENT").AppendInsertAndReturnObject(sql);
				var pick2 = new WhsPick(whs2, "P2", "ENT").AppendInsertAndReturnObject(sql);
				var order = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, product.PK, 10m).AppendInsertAndReturnObject(sql);

				SaveToDB(mainConnection, sql);

				using (var connection1 = Db.NewExtraConnectionToMainDb())
				using (connection1.BeginTransactionWithManager())
				{
					WhsPick.UpdateWhere(pick2.PK).Set(p => p.WP_PercentageComplete, (byte)1).Post(connection1);

					using (var connection2 = Db.NewExtraConnectionToMainDb())
					using (connection2.BeginTransactionWithManager())
					{
						AssertExceptionThrown("Should fail with timeout", typeof(SqlException), "Execution Timeout Expired.  The timeout period elapsed prior to completion of the operation or the server is not responding.",
							() => connection2.ExecuteNonQuery(WhsDocket.UpdateWhere(order.PK).Set(o => o.WD_WP, pick2.PK).AsSQL(), 2), assertStartsWith: true);
					}

					connection1.RollbackTransaction();
				}
			}
		}

		#endregion

		#region TestTrigger_Update_WorkOrder

		[UseSnapshotProtection]
		public void TestTrigger_Update_WorkOrder()
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

				var pick1 = new WhsPick(whs1, "P1", "ENT") { WP_WL_DockDoor = null }.AppendInsertAndReturnObject(sql);
				var pick2 = new WhsPick(whs2, "P2", "ENT") { WP_WL_DockDoor = null }.AppendInsertAndReturnObject(sql);
				var workOrder = new WhsDocket(client.PK, whs1.PK, "WOR", "ASS", "PIC", "O1") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);
				var workOrderLine = new WhsDocketLine(workOrder, mainProduct.PK, 10m).AppendInsertAndReturnObject(sql);

				SaveToDB(mainConnection, sql.ToStringWithNewLineBetweenAppends());

				var changePickSQL = WhsDocket
					.UpdateWhere(workOrder.PK)
					.Set(d => d.WD_WP, pick2.PK).AsSQL();
				AssertExceptionThrown(
					"Should not allow to re-link work order to Pick from different warehouse.",
					typeof(SqlException),
					ErrorMessage,
					() => SaveToDB(mainConnection, changePickSQL),
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

