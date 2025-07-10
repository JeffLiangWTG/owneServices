using System;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsDocket_PreventDetachedOrderPickLines))]
	class TG_WhsDocket_PreventDetachedOrderPickLinesTest : DBCreateTriggerScriptTest
	{
		#region Trigger Runs Tests

		public void TestTrigger_DetachedOrders_WithPickLines()
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
			var receiveLine = new WhsDocketLine(receive, product.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "Pick1", "PIC").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 10m).AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine, orderLine, 10m).AppendInsertAndReturnObject(sql);

			RunSetupScript(sql);

			AssertTriggerFailed("Detached orders should not have unreserved pick lines", GetDetachPickSql(order.PK));
		}

		public void TestTrigger_DetachedWorkOrders_WithPickLines()
		{
			TestTrigger_DetachedComponentOrders_WithPickLines("WOR");
		}

		public void TestTrigger_DetachedDynamicWorkOrders_WithPickLines()
		{
			TestTrigger_DetachedComponentOrders_WithPickLines("DWO");
		}

		void TestTrigger_DetachedComponentOrders_WithPickLines(string docketType)
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product1 = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var product2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			new OrgPartBOM(product1, product2) { OE_ComponentQty = 1m }.AppendInsertAndReturnObject(sql);

			var today = DateTime.Today;
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product2.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "Pick1", "PIC").AppendInsertAndReturnObject(sql);
			var workOrder = new WhsDocket(client.PK, whs.PK, docketType, "ASS", "PIC", "WO1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var workOrderMainLine = new WhsDocketLine(workOrder, product1.PK, 10m).AppendInsertAndReturnObject(sql);
			var workOrderComponentLine = new WhsDocketLine(workOrder, product2.PK, 10m) { WE_WE_ParentDocketLine = workOrderMainLine }.AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine, workOrderComponentLine, 10m).AppendInsertAndReturnObject(sql);

			RunSetupScript(sql);

			AssertTriggerFailed("Detached work orders should not have unreserved pick lines", GetDetachPickSql(workOrder.PK));
		}

		#endregion

		#region Trigger Ignored Tests

		public void TestTrigger_WrongDocketTypes()
		{
			var sql = new SqlQueryBuilder();
			// Should never happen usually, please don't copy around
			TestWhsDataSetupHelper.DropConstraint(sql, "WhsDocket", "Constraint_WD_WP_IsNullForNonOrderAndWorkOrder");

			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var locationA2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var today = DateTime.Today;
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 50m, locationA1.PK) { WE_StockOnHand = 40m }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "Pick1", "PIC").AppendInsertAndReturnObject(sql);
			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "TFR1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var transferLine = new WhsDocketLine(transfer, product.PK, 10m, locationA2.PK) { WE_WL_TransferFrom = locationA1.PK, WE_DocketLineStatus = "HFT", WE_CurrentInventoryStatus = "INT", WE_OriginalInventoryStatus = "INT", WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine, transferLine, 10m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = today }.AppendInsertAndReturnObject(sql);

			RunSetupScript(sql);

			// Should never happen usually, please don't copy around
			AssertTriggerIgnored("Non orders/work orders should not have the check triggered", GetDetachPickSql(transfer.PK));
			AssertPickNull(transfer.PK);
		}

		public void TestTrigger_AttachedOrders()
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
			var receiveLine = new WhsDocketLine(receive, product.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

			var pick1 = new WhsPick(whs, "Pick1", "PIC").AppendInsertAndReturnObject(sql);
			var pick2 = new WhsPick(whs, "Pick2", "PIC").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 10m).AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine, orderLine, 10m).AppendInsertAndReturnObject(sql);

			RunSetupScript(sql);

			AssertTriggerIgnored("Attached orders should not have the check triggered", GetAttachPickSql(order.PK, pick2.PK));
			AssertPick(order.PK, pick2.PK);
		}

		public void TestTrigger_AttachedWorkOrders()
		{
			AssertTrigger_AttachedWorkOrders("WOR");
		}

		public void TestTrigger_AttachedDynamicWorkOrders()
		{
			AssertTrigger_AttachedWorkOrders("DWO");
		}

		void AssertTrigger_AttachedWorkOrders(string docketType)
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product1 = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var product2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			new OrgPartBOM(product1, product2) { OE_ComponentQty = 1m }.AppendInsertAndReturnObject(sql);

			var today = DateTime.Today;
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product2.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

			var pick1 = new WhsPick(whs, "Pick1", "PIC").AppendInsertAndReturnObject(sql);
			var pick2 = new WhsPick(whs, "Pick2", "PIC").AppendInsertAndReturnObject(sql);
			var workOrder = new WhsDocket(client.PK, whs.PK, docketType, "ASS", "PIC", "WO1") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);
			var workOrderMainLine = new WhsDocketLine(workOrder, product1.PK, 10m).AppendInsertAndReturnObject(sql);
			var workOrderComponentLine = new WhsDocketLine(workOrder, product2.PK, 10m) { WE_WE_ParentDocketLine = workOrderMainLine }.AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine, workOrderComponentLine, 10m).AppendInsertAndReturnObject(sql);

			RunSetupScript(sql);

			AssertTriggerIgnored("Attached work orders should not have the check triggered", GetAttachPickSql(workOrder.PK, pick2.PK));
			AssertPick(workOrder.PK, pick2.PK);
		}

		public void TestTrigger_DetachedOrders_WithReservedPickLines()
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
			var receiveLine = new WhsDocketLine(receive, product.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "Pick1", "PIC").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 10m).AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine, orderLine, 10m) { WZ_OriginalReservedQty = 10m }.AppendInsertAndReturnObject(sql);

			RunSetupScript(sql);

			AssertTriggerIgnored("Detached orders with reserved pick lines should be allowed", GetDetachPickSql(order.PK));
			AssertPickNull(order.PK);
		}

		public void TestTrigger_DetachedWorkOrders_WithReservedPickLines()
		{
			AssertTrigger_DetachedWorkOrders_WithReservedPickLines("WOR");
		}

		public void TestTrigger_DetachedDynamicWorkOrders_WithReservedPickLines()
		{
			AssertTrigger_DetachedWorkOrders_WithReservedPickLines("DWO");
		}

		void AssertTrigger_DetachedWorkOrders_WithReservedPickLines(string docketType)
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product1 = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var product2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			new OrgPartBOM(product1, product2) { OE_ComponentQty = 1m }.AppendInsertAndReturnObject(sql);

			var today = DateTime.Today;
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product2.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "Pick1", "PIC").AppendInsertAndReturnObject(sql);
			var workOrder = new WhsDocket(client.PK, whs.PK, docketType, "ASS", "PIC", "WO1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var workOrderMainLine = new WhsDocketLine(workOrder, product1.PK, 10m).AppendInsertAndReturnObject(sql);
			var workOrderComponentLine = new WhsDocketLine(workOrder, product2.PK, 10m) { WE_WE_ParentDocketLine = workOrderMainLine }.AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine, workOrderComponentLine, 10m) { WZ_OriginalReservedQty = 10m }.AppendInsertAndReturnObject(sql);

			RunSetupScript(sql);

			AssertTriggerIgnored("Detached work orders with reserved pick lines should be allowed", GetDetachPickSql(workOrder.PK));
			AssertPickNull(workOrder.PK);
		}

		#endregion

		#region Implementation

		void RunSetupScript(SqlQueryBuilder sql)
		{
			// defer triggers to run in the end
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}

		void AssertTriggerFailed(string errorMessage, string triggerSql)
		{
			try
			{
				var command = Db.Connection.Command(triggerSql);
				command.CommandTimeout = 5; // seconds
				command.ExecuteNonQuery();
				Assert(errorMessage, false);
			}
			catch (SqlException ex)
			{
				Assert(errorMessage, ex.Message.Contains("Cannot detach order/work order with unreserved pick lines."));
			}
		}

		void AssertTriggerIgnored(string errorMessage, string triggerSql)
		{
			AssertNoExceptionThrown(errorMessage, () =>
			{
				var command = Db.Connection.Command(triggerSql);
				command.CommandTimeout = 5; // seconds
				command.ExecuteNonQuery();
			});
		}

		string GetAttachPickSql(Guid docketPK, Guid pickPK)
		{
			return WhsDocket
				.UpdateWhere(docketPK)
				.Set(d => d.WD_WP, pickPK)
				.Set(d => d.WD_DocketStatus, "PIC").AsSQL();
		}

		string GetDetachPickSql(Guid docketPK)
		{
			return WhsDocket
				.UpdateWhere(docketPK)
				.Set(d => d.WD_WP, null)
				.Set(d => d.WD_DocketStatus, "ENT").AsSQL();
		}

		void AssertPickNull(Guid docketPK)
		{
			Assert(Db.Connection.ExecuteScalar($"select WD_WP from dbo.WhsDocket where WD_PK = '{docketPK}'") == DBNull.Value);
		}

		void AssertPick(Guid docketPK, Guid pickPk)
		{
			AssertEquals(pickPk, Db.Connection.ExecuteScalar($"select WD_WP from dbo.WhsDocket where WD_PK = '{docketPK}'"));
		}

		#endregion
	}
}
