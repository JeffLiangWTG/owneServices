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
	[TestedType(typeof(TG_WhsDocketLine_AdjustmentInHaveNoPickLines))]
	class TG_WhsDocketLine_AdjustmentInHaveNoPickLinesTest : DBCreateTriggerScriptTest
	{
		#region TestTrigger_Update_WE_TransactionQuantity

		[UseSnapshotProtection]
		public void TestTrigger_Update_WE_TransactionQuantity()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var today = DateTime.Today;
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 50m, locationA1.PK) { WE_StockOnHand = 0m }.AppendInsertAndReturnObject(sql);

			// adjust out stock
			var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "FIN", "AD1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var adjustmentLine = new WhsDocketLine(adjustment, product.PK, -50m, locationA1.PK).AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine, adjustmentLine, 50m) { WZ_GS_NKAssignedTo = "AA", WZ_PickedDateTime = today }.AppendInsertAndReturnObject(sql);

			// defer triggers to run in the end
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
			AssertTriggerFails("Changing adjustment line from Out to In without deleting PickLines is incorrect",
				WhsDocketLine
				.UpdateWhere(adjustmentLine.PK)
				.Set(l => l.WE_TransactionQuantity, 50m)
				.Set(l => l.WE_StockOnHand, 50m)
				.Set(l => l.WE_AdjustmentArrivalDate, DateTime.UtcNow)
				.Set(l => l.WE_WE_OriginalDocketLineForRating, adjustmentLine).AsSQL());
		}

		static void AssertTriggerFails(string errorDescription, string sql)
		{
			try
			{
				var command = Db.Connection.Command(sql);
				command.CommandTimeout = 5; // seconds
				command.ExecuteNonQuery();
				Fail(errorDescription);
			}
			catch (SqlException ex)
			{
				Assert(errorDescription, ex.Message.StartsWith("Adjustment In lines should not pick any stock."));
			}
		}
		#endregion
	}
}

