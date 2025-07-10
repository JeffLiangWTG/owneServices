using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsDocketLine_PreventDeleteOfHoldCodeLines))]
	class TG_WhsDocketLine_PreventDeleteOfHoldCodeLinesTest : DBCreateTriggerScriptTest
	{
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestTrigger_Delete()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(connection);
				var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
				var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				var today = DateTime.Today;
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 30m }.AppendInsertAndReturnObject(sql);
				var receiveLine_HCC = new WhsDocketLine(receive, part.PK, 20m, locationA1.PK)
				{
					WE_StockOnHand = 20m,
					WE_WE_ParentDocketLine = receiveLine,
					WE_IsOriginalInventory = false,
					WE_WE_OriginalDocketLineForRating = receiveLine
				}.AppendInsertAndReturnObject(sql);

				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
				{
					ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());
				}

				AssertExceptionThrown("Should not be allowed to Delete Hold Change Lines.",
					typeof(SqlException),
					"Attempt to delete Hold Code Change Line.",
					() => WhsDocketLine.DeleteInDB(connection, receiveLine_HCC.PK),
					assertStartsWith: true);
			}
		}

		public void TestTrigger_DeleteWorksForComponentLine()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var today = DateTime.Today;
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, part.PK, 10m).AppendInsertAndReturnObject(sql);
			var componentLine = new WhsDocketLine(order, part.PK, 20m) { WE_WE_ParentDocketLine = orderLine }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			AssertNoExceptionThrown("Should be allowed to delete component line.", () => WhsDocketLine.DeleteInDB(TestConnection, componentLine.PK));
		}

		void ExecuteSqlInTransaction(DbConnection connection, string sql)
		{
			using (connection.BeginTransactionWithManager())
			{
				connection.ExecuteNonQuery(sql);
				connection.CommitTransaction();
			}
		}
	}
}
