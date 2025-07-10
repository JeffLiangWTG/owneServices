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
	[TestedType(typeof(TG_PreventAddingIncorrectOrdersToPicks))]
	class TG_PreventAddingIncorrectOrdersToPicksTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_PreventAddingIncorrectOrdersToPicksTest : TestCase
	{
		const string ErrorMessage = "Attempt to attach order to cancelled pick.";

		#region TestTrigger_Update_Order_Locks

		[UseSnapshotProtection]
		public void TestTrigger_Update_Order_Locks()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(mainConnection);
				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				var pick1 = new WhsPick(whs, "P1", "CAN").AppendInsertAndReturnObject(sql);
				var pick2 = new WhsPick(whs, "P2", "CAN").AppendInsertAndReturnObject(sql);
				var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, product.PK, 10m).AppendInsertAndReturnObject(sql);

				var unrelatedPick = new WhsPick(whs, "P3", "CAN").AppendInsertAndReturnObject(sql);

				SaveAndDisableTrigger(mainConnection, sql);

				using (var connection = Db.NewExtraConnectionToMainDb())
				using (connection.BeginTransactionWithManager())
				{
					AssertExceptionThrown("Should not allow to attach to cancel pick!", typeof(SqlException), ErrorMessage,
						() => connection.ExecuteNonQuery(WhsDocket.UpdateWhere(order.PK).Set(o => o.WD_WP, pick2.PK).AsSQL(), 2), assertStartsWith: true);
				}

				using (var connection = Db.NewExtraConnectionToMainDb())
				using (connection.BeginTransactionWithManager())
				{
					AssertNoExceptionThrown("Should allow to detach from cancel pick!",
						() => connection.ExecuteNonQuery(WhsDocket.UpdateWhere(order.PK).Set(o => o.WD_DocketStatus, "ENT").Set(o => o.WD_WP, null).AsSQL(), 2));
				}
			}
		}

		#endregion

		#region TestTrigger_AttemptToAttachNotFinalisedOrder_ToFinalisedPick

		[UseSnapshotProtection]
		public void TestTrigger_AttemptToAttachNotFinalisedOrder_ToFinalisedPick_Insert()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(mainConnection);
				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				var pick = new WhsPick(whs, "P1", "FIN") { WP_FinalizedDateUtc = DateTime.UtcNow, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
				new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);

				using (mainConnection.BeginTransactionWithManager())
				{
					AssertExceptionThrown(
							typeof(SqlException),
							"Attempt to attach not finalised order to finalised pick.",
							() => mainConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends()),
							assertStartsWith: true);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_AttemptToAttachNotFinalisedOrder_ToFinalisedPick_Update()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(mainConnection);
				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				var pick1 = new WhsPick(whs, "P1", "FIN") { WP_FinalizedDateUtc = DateTime.UtcNow, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
				var pick2 = new WhsPick(whs, "P2", "FIN") { WP_FinalizedDateUtc = DateTime.UtcNow, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
				var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);

				SaveAndDisableTrigger(mainConnection, sql);

				using (var connection = Db.NewExtraConnectionToMainDb())
				using (connection.BeginTransactionWithManager())
				{
					AssertExceptionThrown(
							typeof(SqlException),
							"Attempt to attach not finalised order to finalised pick.",
							() => connection.ExecuteNonQuery(WhsDocket.UpdateWhere(order.PK).Set(o => o.WD_WP, pick2.PK).AsSQL(), 2),
							assertStartsWith: true);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_AttemptToAttachNonDepartedOrder_ToFinalisedPick_Update()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(mainConnection);
				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				var pick1 = new WhsPick(whs, "P1", "FIN") { WP_FinalizedDateUtc = DateTime.UtcNow, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
				var pick2 = new WhsPick(whs, "P2", "FIN") { WP_FinalizedDateUtc = DateTime.UtcNow, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
				var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick1.PK, WD_FinalisedDate = DateTime.UtcNow, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);

				SaveAndDisableTrigger(mainConnection, sql);

				using (var connection = Db.NewExtraConnectionToMainDb())
				using (connection.BeginTransactionWithManager())
				{
					AssertExceptionThrown(
							typeof(SqlException),
							"Attempt to attach not finalised order to finalised pick.",
							() => connection.ExecuteNonQuery(WhsDocket.UpdateWhere(order.PK).Set(o => o.WD_WP, pick2.PK).AsSQL(), 2),
							assertStartsWith: true);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_AttachDepartedOrder_ToFinalisedPick_Update()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(mainConnection);
				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				var pick1 = new WhsPick(whs, "P1", "FIN") { WP_FinalizedDateUtc = DateTime.UtcNow, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
				var pick2 = new WhsPick(whs, "P2", "FIN") { WP_FinalizedDateUtc = DateTime.UtcNow, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
				var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O1") { WD_WP = pick1.PK, WD_FinalisedDate = DateTime.UtcNow, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);

				SaveAndDisableTrigger(mainConnection, sql);

				using (var connection = Db.NewExtraConnectionToMainDb())
				using (connection.BeginTransactionWithManager())
				{
					AssertNoExceptionThrown(
							"Attach departed order to finalised pick.",
							() => connection.ExecuteNonQuery(WhsDocket.UpdateWhere(order.PK).Set(o => o.WD_WP, pick2.PK).AsSQL(), 2));
				}
			}
		}

		#endregion

		static void SaveAndDisableTrigger(DbConnection connection, SqlQueryBuilder sqlQueryBuilder)
		{
			using (connection.BeginTransactionWithManager())
			{
				var sql = $@"
DISABLE TRIGGER [dbo].[TG_PreventAddingIncorrectOrdersToPicks] ON [dbo].[WhsDocket];
{sqlQueryBuilder.ToStringWithNewLineBetweenAppends()};
ENABLE TRIGGER [dbo].[TG_PreventAddingIncorrectOrdersToPicks] ON [dbo].[WhsDocket];";
				connection.ExecuteNonQuery(sql);
				connection.CommitTransaction();
			}
		}
	}
}
