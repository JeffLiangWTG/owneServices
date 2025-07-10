using System;
using System.Linq;
using System.Text;
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
	[TestedType(typeof(TG_WhsDocketLine_StockOnHandIsBalanced_InsertForParent))]
	class TG_WhsDocketLine_StockOnHandIsBalanced_InsertForParentTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_WhsDocketLine_StockOnHandIsBalanced_InsertForParentTest : TestCase
	{
		#region TestTrigger_IsDeferred

		[UseSnapshotProtection]
		public void TestTrigger_IsDeferred()
		{
			TestTrigger_DeferralCore(isDeferred: true);
		}

		[UseSnapshotProtection]
		public void TestTrigger_IsNotDeferred()
		{
			TestTrigger_DeferralCore(isDeferred: false);
		}

		void TestTrigger_DeferralCore(bool isDeferred)
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
				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_ClientOrderedUnits = 10m, WE_StockOnHand = 50m };
				var childReceiveLine = new WhsDocketLine(receive, part.PK, 10m, locationA1.PK) { WE_ClientOrderedUnits = 10m, WE_StockOnHand = 10m, WE_WE_ParentDocketLine = receiveLine, WE_IsOriginalInventory = false, WE_WE_OriginalDocketLineForRating = receiveLine };
				if (isDeferred)
				{
					var insertReceiveLineSql = new StringBuilder();
					insertReceiveLineSql.AppendLine($"SuspendTrigger '{TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_InsertForParent}'");
					insertReceiveLineSql.Append(receiveLine.GetInsertStatement());
					insertReceiveLineSql.Append(childReceiveLine.GetInsertStatement());

					AssertNoExceptionThrown("If trigger is suspended then check procedure should not be run.",
						() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(connection, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced, insertReceiveLineSql.ToString()));
				}
				else
				{
					var insertReceiveLineSql = new StringBuilder();
					insertReceiveLineSql.Append(receiveLine.GetInsertStatement());
					insertReceiveLineSql.Append(childReceiveLine.GetInsertStatement());
					AssertCheckProcedureRanForTheInventoryLines(connection, "If trigger is not suspended then check procedure should be run.", insertReceiveLineSql.ToString(), receiveLine);
				}
			}
		}

		#endregion

		#region TestTrigger_Insert

		[UseSnapshotProtection]
		public void TestTrigger_Insert()
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
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_ClientOrderedUnits = 10m, WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);
				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				var childReceiveLine = new WhsDocketLine(receive, part.PK, 10m, locationA1.PK) { WE_ClientOrderedUnits = 10m, WE_StockOnHand = 10m, WE_WE_ParentDocketLine = receiveLine, WE_IsOriginalInventory = false, WE_WE_OriginalDocketLineForRating = receiveLine };
				AssertCheckProcedureRanForTheInventoryLines(connection, "Should fire trigger when inserting docket lines.", childReceiveLine.GetInsertStatement(), receiveLine);
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Insert_NotRunForOrdersAndWorkOrders()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(connection);
				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				// order
				var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, part.PK, 10m).AppendInsertAndReturnObject(sql);

				// work order
				var workOrder = new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "ENT", "WO1").AppendInsertAndReturnObject(sql);
				var workOrderLine = new WhsDocketLine(workOrder, part.PK, 10m).AppendInsertAndReturnObject(sql);
				var workOrderComponentLine = new WhsDocketLine(workOrder, part.PK, 20m) { WE_WE_ParentDocketLine = workOrderLine }.AppendInsertAndReturnObject(sql);

				// dynamic work order
				var dynamicWorkOrder = new WhsDocket(client.PK, whs.PK, "DWO", "ASS", "ENT", "DO1").AppendInsertAndReturnObject(sql);
				var dynamicWorkOrderLine = new WhsDocketLine(dynamicWorkOrder, part.PK, 10m).AppendInsertAndReturnObject(sql);
				var dynamicWorkOrderComponentLine = new WhsDocketLine(dynamicWorkOrder, part.PK, 20m) { WE_WE_ParentDocketLine = dynamicWorkOrderLine }.AppendInsertAndReturnObject(sql);

				AssertNoExceptionThrown("The check procedure should not be run when inserting / updating Order or Work Order.",
					() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(connection, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced, sql.ToStringWithNewLineBetweenAppends()));
			}
		}

		#endregion

		#region TestTrigger_InterWhsTransfer_MasterSource_Entered

		[UseSnapshotProtection]
		public void TestTrigger_InterWhsTransfer_MasterSource_Entered()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var data = new SimpleDataEnvironment(numberOfLocations: 1);
				var sql = data.Sql;
				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(connection);
				var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(connection);
				var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
				var row2 = new WhsRow(whs2, "B").AppendInsertAndReturnObject(sql);
				var location2 = new WhsLocation(row2.PK, area2.PK, area2.PK).AppendInsertAndReturnObject(sql);

				// receive stock
				var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

				// transfer stock
				var transfer_Master = new WhsDocket(data.Client.PK, data.Whs.PK, "TFR", "IWS", "ENT", "TR1").AppendInsertAndReturnObject(sql);
				var transferLine_Master = new WhsDocketLine(transfer_Master, data.Product.PK, 10m, location2.PK)
				{
					WE_StockOnHand = 0m,
					WE_WL_TransferFrom = data.Locations[0].PK,
					WE_DocketLineStatus = "ENT"
				}.AppendInsertAndReturnObject(sql);
				new WhsPickLine(receiveLine, transferLine_Master, 10m).AppendInsertAndReturnObject(sql);

				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
				{
					AssertNoExceptionThrown("Inter-Whs Source Transfer should have 0 SOH.", () => ExecuteSqlInTransaction(connection, data.Sql.ToStringWithNewLineBetweenAppends()));
				}
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_InterWhsTransfer_MasterSource_Entered_FailsIfNotZero()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var data = new SimpleDataEnvironment(numberOfLocations: 1);
				var sql = data.Sql;
				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(connection);
				var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(connection);
				var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
				var row2 = new WhsRow(whs2, "B").AppendInsertAndReturnObject(sql);
				var location2 = new WhsLocation(row2.PK, area2.PK, area2.PK).AppendInsertAndReturnObject(sql);

				// receive stock
				var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

				data.Sql.Append($"EXEC dbo.SuspendTrigger '{TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert}'");

				// transfer stock
				var transfer_Master = new WhsDocket(data.Client.PK, data.Whs.PK, "TFR", "IWS", "ENT", "TR1").AppendInsertAndReturnObject(sql);
				var transferLine_Master = new WhsDocketLine(transfer_Master, data.Product.PK, 10m, location2.PK)
				{
					WE_StockOnHand = 10m,
					WE_WL_TransferFrom = data.Locations[0].PK,
					WE_DocketLineStatus = "HFT",
					WE_CurrentInventoryStatus = "INT",
					WE_OriginalInventoryStatus = "INT",
				}.AppendInsertAndReturnObject(sql);
				new WhsPickLine(receiveLine, transferLine_Master, 10m).AppendInsertAndReturnObject(sql);

				AssertExceptionThrown("IWS Transfer Lines should not have Stock on Hand.",
					typeof(SqlException),
					"Attempt to set Stock on IWS Transfer.",
					() => ExecuteSqlInTransaction(connection, data.Sql.ToStringWithNewLineBetweenAppends()),
					assertStartsWith: true);
			}
		}

		#endregion

		#region Implementations

		void ExecuteSqlInTransaction(DbConnection connection, string sql)
		{
			using (connection.BeginTransactionWithManager())
			{
				connection.ExecuteNonQuery(sql);
				connection.CommitTransaction();
			}
		}

		void AssertCheckProcedureRanForTheInventoryLines(DbConnection connection, string errorMessage, string sqlToRun, params WhsDocketLine[] expectedDocketLinesInTheProcedure)
		{
			var actualPKsInTheProcedure = TestWhsDataSetupHelper.AssertCheckProcedureRanAndReturnGuidsPassedIntoProcedure(connection, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced, sqlToRun, expectedDocketLinesInTheProcedure);
			if (actualPKsInTheProcedure == null)
			{
				Fail(errorMessage);
			}
			else
			{
				AssertContainsExactElementsInAnyOrder(errorMessage, expectedDocketLinesInTheProcedure.Select(l => l.PK), actualPKsInTheProcedure);
			}
		}

		class SimpleDataEnvironment
		{
			public SimpleDataEnvironment(int numberOfLocations)
			{
				Sql = new SqlQueryBuilder();
				Whs = new WhsWarehouse("WH1").WithDockDoor(Db.Connection);
				Locations = new WhsLocation[numberOfLocations];

				var area = new WhsArea(Whs.PK, "AREA1").AppendInsertAndReturnObject(Sql);
				var row = new WhsRow(Whs, "A") { WR_Columns = (short)numberOfLocations }.AppendInsertAndReturnObject(Sql);
				for (short i = 1; i <= numberOfLocations; i++)
				{
					Locations[i - 1] = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = i }.AppendInsertAndReturnObject(Sql);
				}

				Client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(Sql);
				Product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(Sql);
				Today = DateTime.Today;
			}

			public readonly SqlQueryBuilder Sql;
			public readonly WhsWarehouse Whs;
			public readonly WhsLocation[] Locations;
			public readonly OrgHeader Client;
			public readonly OrgSupplierPart Product;
			public readonly DateTime Today;
		}

		#endregion
	}
}
