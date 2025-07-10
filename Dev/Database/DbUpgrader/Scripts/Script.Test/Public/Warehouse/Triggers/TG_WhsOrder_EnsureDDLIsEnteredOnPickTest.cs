using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsOrder_EnsureDDLIsEnteredOnPick))]
	class TG_WhsOrder_EnsureDDLIsEnteredOnPickTest : DBCreateTriggerScriptTest
	{
		#region TestTriggerWithWhsPickWithDDLAnalysisDroppedAndNoRowsAreUpdated

		public void TestTriggerWithWhsPickWithDDLAnalysisDroppedAndNoRowsAreUpdated()
		{
			var sql = @"
DROP VIEW WhsPickWithDDLAnalysis
UPDATE dbo.WhsDocket
SET WD_IsAuthorisedToLeave = 1
WHERE
	WD_PK = '00000000-0000-0000-0000-000000000000'
";
			AssertNoExceptionThrown("The trigger should not hit the dropped view as there are no updated rows.", () => Db.Connection.ExecuteNonQuery(sql));
		}
		#endregion
	}

	class Trigger_TG_WhsOrder_EnsureDDLIsEnteredOnPickTest : TestCase
	{
		#region TestTrigger_Insert

		[UseSnapshotProtection]
		public void TestTrigger_Insert()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch1 = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(mainConnection);
				var ddlLocationType = WhsLocationType.ShallowLoadFromDB(mainConnection, lt => lt.WLT_LocationClass == "DDL").First();
				var otherLocationType = WhsLocationType.ShallowLoadFromDB(mainConnection, lt => lt.WLT_LocationClass != "DDL").First();

				// create warehouse with dock door locations
				var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(mainConnection);
				var area = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row = new WhsRow(whs1, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
				var locationA1 = new WhsLocation(row.PK, area.PK, area.PK, ddlLocationType.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
				var locationA2 = new WhsLocation(row.PK, area.PK, area.PK, otherLocationType.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

				var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(mainConnection);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);

				// create Pick to update
				var pick_WithNoDDL = new WhsPick(whs1, "P1", "NEW") { WP_WL_DockDoor = null }.AppendInsertAndReturnObject(sql);
				var pick_WithNoDDL_ForWorkOrders = new WhsPick(whs1, "P2", "NEW") { WP_WL_DockDoor = null }.AppendInsertAndReturnObject(sql);
				var pick_WithDDL = new WhsPick(whs1, "P3", "NEW") { WP_WL_DockDoor = locationA1 }.AppendInsertAndReturnObject(sql);
				var pick_WithIncorrectDDL = new WhsPick(whs1, "P4", "NEW") { WP_WL_DockDoor = locationA2 }.AppendInsertAndReturnObject(sql);
				var pick_WithPicksWhsDifferentFromWhsOfDDL = new WhsPick(whs2, "P5", "NEW") { WP_WL_DockDoor = locationA1 }.AppendInsertAndReturnObject(sql);

				// insert data into DB
				ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());

				// correct inserts
				AssertTrigger_Insert_Success("Inserting orders that are not picked should be fine.", mainConnection, null, client.PK, whs1.PK, "ORD", "ORD", "W00000001", "ENT");
				AssertTrigger_Insert_Success("Inserting work orders linked to a pick without DDL should be fine.", mainConnection, pick_WithNoDDL_ForWorkOrders.PK, client.PK, whs1.PK, "WOR", "ASS", "W00000002", "PIC");
				AssertTrigger_Insert_Success("Inserting orders to picks with correct DDL should be fine.", mainConnection, pick_WithDDL.PK, client.PK, whs1.PK, "ORD", "ORD", "W00000003", "PIC");

				// incorrect inserts
				AssertTrigger_Insert_Fails("Cannot insert order for Pick with no DDL.", pick_WithNoDDL.PK, client.PK, whs1.PK, "ORD", "ORD", "W00000004");
				AssertTrigger_Insert_Fails("Cannot insert order for Pick with incorrect DDL.", pick_WithIncorrectDDL.PK, client.PK, whs1.PK, "ORD", "ORD", "W00000005");
				AssertTrigger_Insert_Fails("Cannot insert order for Pick with picks Whs different from Whs of DDL.", pick_WithPicksWhsDifferentFromWhsOfDDL.PK, client.PK, whs2.PK, "ORD", "ORD", "W00000006");
			}
		}

		void AssertTrigger_Insert_Success(string errorDescription, DbConnection connection, Guid? pickPK, Guid clientPK, Guid whs1PK, string orderType, string docketSubType, string docketID, string docketStatus)
		{
			var sql = new SqlQueryBuilder();
			new WhsDocket(clientPK, whs1PK, orderType, docketSubType, docketStatus, docketID) { WD_FinalisedDate = null, WD_WP = pickPK }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown(errorDescription, () => ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends()));
		}

		void AssertTrigger_Insert_Fails(string errorDescription, Guid pickPK, Guid clientPK, Guid whs1PK, string orderType, string docketSubType, string docketID)
		{
			var sql = new SqlQueryBuilder();
			new WhsDocket(clientPK, whs1PK, orderType, docketSubType, "PIC", docketID) { WD_FinalisedDate = null, WD_WP = pickPK }.AppendInsertAndReturnObject(sql);

			AssertTriggerFails(errorDescription, sql.ToStringWithNewLineBetweenAppends());
		}

		static void AssertTriggerFails(string errorDescription, string sql, bool useTransaction = false)
		{
			using (Db.DisposableActionForDbConnection())
			using (var newConnection = Db.NewExtraConnectionToMainDb())
			using (newConnection.BeginTransactionWithManager())
			{
				try
				{
					using (var command = newConnection.Command(sql))
					{
						command.CommandTimeout = 5; // seconds
						command.ExecuteNonQuery();
					}
					Assert(errorDescription, false);
				}
				catch (SqlException ex)
				{
					Assert(errorDescription, ex.Message.StartsWith("Order(s) are on Pick(s) with either empty or invalid Dock Door Locations."));
				}
			}
		}

		#endregion

		#region TestTrigger_Update

		[UseSnapshotProtection]
		public void TestTrigger_Update()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch1 = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(mainConnection);
				var ddlLocationType = WhsLocationType.ShallowLoadFromDB(mainConnection, lt => lt.WLT_LocationClass == "DDL").First();
				var otherLocationType = WhsLocationType.ShallowLoadFromDB(mainConnection, lt => lt.WLT_LocationClass != "DDL").First();

				// create warehouse with dock door locations
				var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(mainConnection);
				var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row1 = new WhsRow(whs1, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
				var locationA1 = new WhsLocation(row1.PK, area1.PK, area1.PK, ddlLocationType.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
				var locationA2 = new WhsLocation(row1.PK, area1.PK, area1.PK, otherLocationType.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

				var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(mainConnection);
				var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
				var row2 = new WhsRow(whs2, "B").AppendInsertAndReturnObject(sql);
				var locationInAnotherWhs = new WhsLocation(row2.PK, area2.PK, area2.PK, ddlLocationType.PK).AppendInsertAndReturnObject(sql);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);

				// create Pick to update
				var pick_WithNoDDL = new WhsPick(whs1, "P1", "NEW") { WP_WL_DockDoor = null }.AppendInsertAndReturnObject(sql);
				var pick_WithNoDDL_ForWorkOrders = new WhsPick(whs1, "P2", "NEW") { WP_WL_DockDoor = null }.AppendInsertAndReturnObject(sql);
				var pick_WithDDL = new WhsPick(whs1, "P3", "NEW") { WP_WL_DockDoor = locationA1 }.AppendInsertAndReturnObject(sql);
				var pick_WithIncorrectDDL = new WhsPick(whs1, "P4", "NEW") { WP_WL_DockDoor = locationA2 }.AppendInsertAndReturnObject(sql);
				var pick_WithPicksWhsDifferentFromWhsOfDDL = new WhsPick(whs1, "P5", "NEW") { WP_WL_DockDoor = locationInAnotherWhs }.AppendInsertAndReturnObject(sql);

				var order = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
				var workOrder = new WhsDocket(client.PK, whs1.PK, "WOR", "DIS", "ENT", "WO1").AppendInsertAndReturnObject(sql);

				// insert data into DB
				ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());

				// correct updates
				using (mainConnection.BeginTransactionWithManager())
				{
					AssertTrigger_Update_Success("Linking orders to pick with DDL should be fine.", mainConnection, order.PK, pick_WithDDL.PK, "PIC");
					AssertTrigger_Update_Success("Un-Linking orders from pick should be fine.", mainConnection, order.PK, null, "ENT");
					AssertTrigger_Update_Success("Linking work orders to pick with no DDL should be fine.", mainConnection, workOrder.PK, pick_WithNoDDL_ForWorkOrders.PK, "PIC");
					mainConnection.CommitTransaction();
				}

				// incorrect updates
				AssertTrigger_Update_Fails("Linking work orders to pick with no DDL should be fine.", order.PK, pick_WithNoDDL.PK, "PIC");
				AssertTrigger_Update_Fails("Cannot insert order for Pick with incorrect DDL.", order.PK, pick_WithIncorrectDDL.PK, "PIC");
				AssertTrigger_Update_Fails("Cannot insert order for Pick with picks Whs different from Whs of DDL.", order.PK, pick_WithPicksWhsDifferentFromWhsOfDDL.PK, "PIC");
			}
		}

		void AssertTrigger_Update_Success(string errorDescription, DbConnection connection, Guid orderPK, Guid? pickPK, string docketStatus)
		{
			AssertNoExceptionThrown(errorDescription,
				() => WhsDocket
				.UpdateWhere(orderPK)
				.Set(d => d.WD_WP, pickPK)
				.Set(d => d.WD_DocketStatus, docketStatus).Post(connection));
		}

		void AssertTrigger_Update_Fails(string errorDescription, Guid orderPK, Guid pickPK, string docketStatus)
		{
			var sql = WhsDocket
				.UpdateWhere(orderPK)
				.Set(d => d.WD_WP, pickPK)
				.Set(d => d.WD_DocketStatus, docketStatus).AsSQL();

			AssertTriggerFails(errorDescription, sql, true);
		}

		#endregion

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

