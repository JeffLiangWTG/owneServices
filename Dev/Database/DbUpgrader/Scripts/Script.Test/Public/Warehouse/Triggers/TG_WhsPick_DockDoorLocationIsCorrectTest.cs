using System.Linq;
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
	[TestedType(typeof(TG_WhsPick_DockDoorLocationIsCorrect))]
	class TG_WhsPick_DockDoorLocationIsCorrectTest : DBCreateTriggerScriptTest
	{
		#region TestTriggerWithWhsPickWithDDLAnalysisDroppedAndNoRowsAreUpdated

		public void TestTriggerWithWhsPickWithDDLAnalysisDroppedAndNoRowsAreUpdated()
		{
			var sql = @"
DROP VIEW WhsPickWithDDLAnalysis
UPDATE dbo.WhsPick
SET WP_IsCartonised = 1,
	WP_SystemLastEditTimeUtc = SYSUTCDATETIME(),
	WP_SystemLastEditUser = '~BP'
WHERE
	WP_PK = '00000000-0000-0000-0000-000000000000'
";
			AssertNoExceptionThrown("The trigger should not hit the dropped view as there are no updated rows.", () => Db.Connection.ExecuteNonQuery(sql));
		}
		#endregion
	}

	class Trigger_TG_WhsPick_DockDoorLocationIsCorrectTest : TestCase
	{
		#region TestTrigger_Update_StandalonePick

		[UseSnapshotProtection]
		public void TestTrigger_Update_StandalonePick()
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

				// create second warehouse
				var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(mainConnection);

				// create Pick to update
				var pick = new WhsPick(null, "P1", "NEW").AppendInsertAndReturnObject(sql);

				// insert data into DB
				using (mainConnection.BeginTransactionWithManager())
				{
					mainConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
					mainConnection.CommitTransaction();
				}

				// Standalone Pick
				AssertTrigger_Update_Success("Both warehouse and DDL can be empty", mainConnection, pick, null, null);
				AssertTrigger_Update_Success("Pick's warehouse can be entered without DDL if no orders attached.", mainConnection, pick, whs1, null);
				AssertTrigger_Update_Success("Any DDL can go with correct warehouse", mainConnection, pick, whs1, locationA1);

				// check update of WP_WL_DockDoor will activate trigger
				AssertTrigger_Update_Fails("Updating to not DDL location should fail.", pick, whs1, locationA2);
				AssertTrigger_Update_Success("Any DDL can go with correct warehouse", mainConnection, pick, whs1, locationA1); // clean up

				// check update of WP_WW_Whs will activate trigger
				AssertTrigger_Update_Fails("Updating Warehouse to different from DDL warehouse should fail.", pick, whs2, locationA1);
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
				var branch3 = new GlbBranch("BR3").InsertAndReturnObject(mainConnection);
				var branch4 = new GlbBranch("BR4").InsertAndReturnObject(mainConnection);
				var ddlLocationType = WhsLocationType.ShallowLoadFromDB(mainConnection, lt => lt.WLT_LocationClass == "DDL").First();
				var otherLocationType = WhsLocationType.ShallowLoadFromDB(mainConnection, lt => lt.WLT_LocationClass != "DDL").First();

				// create warehouse with dock door locations
				var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(mainConnection);
				var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row1 = new WhsRow(whs1, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
				var locationA1 = new WhsLocation(row1.PK, area1.PK, area1.PK, ddlLocationType.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
				var locationA2 = new WhsLocation(row1.PK, area1.PK, area1.PK, otherLocationType.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

				// create second warehouse
				var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(mainConnection);
				var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
				var row2 = new WhsRow(whs2, "B").AppendInsertAndReturnObject(sql);
				var locationInAnotherWhs = new WhsLocation(row2.PK, area2.PK, area2.PK, ddlLocationType.PK).AppendInsertAndReturnObject(sql);

				// create inactive warehouse
				var whs3 = new WhsWarehouse("WH3", branch3.PK) { WW_IsActive = false }.WithDockDoor(mainConnection);
				var area3 = new WhsArea(whs3.PK, "AREA3").AppendInsertAndReturnObject(sql);
				var row3 = new WhsRow(whs3, "C").AppendInsertAndReturnObject(sql);
				var locationC1 = new WhsLocation(row3.PK, area3.PK, area3.PK, ddlLocationType.PK).AppendInsertAndReturnObject(sql);

				// create virtual warehouse
				var whs4 = new WhsWarehouse("WH4", branch4.PK) { WW_IsVirtualWarehouse = true }.WithDockDoor(mainConnection);
				var area4 = new WhsArea(whs4.PK, "AREA4").AppendInsertAndReturnObject(sql);
				var row4 = new WhsRow(whs4, "D").AppendInsertAndReturnObject(sql);
				var locationD1 = new WhsLocation(row4.PK, area4.PK, area4.PK, ddlLocationType.PK).AppendInsertAndReturnObject(sql);

				// create client and product for orders
				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				// create Picks to update
				var pick_ForWhs1 = CreatePickWithOrder(sql, whs1, locationA1, "P00000001", client, part, "W00000001");
				var pick_ForInactiveWhs = CreatePickWithOrder(sql, whs3, locationC1, "P00000003", client, part, "W00000003");
				var pick_ForVirtualWhs = CreatePickWithOrder(sql, whs4, locationD1, "P00000004", client, part, "W00000004");

				// insert data into DB
				using (mainConnection.BeginTransactionWithManager())
				{
					mainConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
					mainConnection.CommitTransaction();
				}

				AssertTrigger_Update_Fails("Updating to not DDL location should fail.", pick_ForWhs1, whs1, locationA2);
				AssertTrigger_Update_Fails("DDL is mandatory for picks with attached orders.", pick_ForWhs1, whs1, null);
				AssertTrigger_Update_Fails("DDL cannot be entered without warehouse.", pick_ForWhs1, null, locationA1);
				AssertTrigger_Update_Fails("DDL should be from same warehouse as pick.", pick_ForWhs1, whs1, locationInAnotherWhs);
				AssertTrigger_Update_Success("Any DDL can go with correct warehouse.", mainConnection, pick_ForWhs1, whs1, locationA1);

				AssertTrigger_Update_Fails("DDL cannot be entered without warehouse even for inactive warehouses.", pick_ForInactiveWhs, null, locationC1);
				AssertTrigger_Update_Fails("DDL should be from same warehouse as pick even for inactive warehouses.", pick_ForInactiveWhs, whs3, locationA1);
				AssertTrigger_Update_Success("Picks for inactive warehouses don't need DDL.", mainConnection, pick_ForInactiveWhs, whs3, null);
				AssertTrigger_Update_Success("Any DDL can go with correct warehouse.", mainConnection, pick_ForInactiveWhs, whs3, locationC1);

				AssertTrigger_Update_Fails("DDL cannot be entered without warehouse even for virtual warehouses.", pick_ForVirtualWhs, null, locationD1);
				AssertTrigger_Update_Fails("DDL should be from same warehouse as pick even for virtual warehouses.", pick_ForVirtualWhs, whs4, locationA1);
				AssertTrigger_Update_Success("Picks for virtual warehouses don't need DDL.", mainConnection, pick_ForVirtualWhs, whs4, null);
				AssertTrigger_Update_Success("Any DDL can go with correct warehouse.", mainConnection, pick_ForVirtualWhs, whs4, locationD1);
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
				var ddlLocationType = WhsLocationType.ShallowLoadFromDB(mainConnection, lt => lt.WLT_LocationClass == "DDL").First();
				var otherLocationType = WhsLocationType.ShallowLoadFromDB(mainConnection, lt => lt.WLT_LocationClass != "DDL").First();

				// create warehouse with dock door locations
				var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(mainConnection);
				var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row1 = new WhsRow(whs1, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
				var locationA1 = new WhsLocation(row1.PK, area1.PK, area1.PK, ddlLocationType.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
				var locationA2 = new WhsLocation(row1.PK, area1.PK, area1.PK, otherLocationType.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

				// create second warehouse
				var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(mainConnection);
				var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
				var row2 = new WhsRow(whs2, "B").AppendInsertAndReturnObject(sql);
				var locationInAnotherWhs = new WhsLocation(row2.PK, area2.PK, area2.PK, ddlLocationType.PK).AppendInsertAndReturnObject(sql);

				// create client and product for orders
				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				// create Picks to update
				var pick = CreatePickWithOrder(sql, whs1, locationA1, "P00000001", client, part, "W00000001", "WOR", "DIS");

				// insert data into DB
				using (TestWhsDataSetupHelper.DisableConstraint(WhsLocationSchema.Constants.TableName, "Constraint_WL_PutawayPathSequence"))
				using (mainConnection.BeginTransactionWithManager())
				{
					mainConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
					mainConnection.CommitTransaction();
				}

				AssertTrigger_Update_Fails("Updating to not DDL location should fail.", pick, whs1, locationA2);
				AssertTrigger_Update_Fails("DDL cannot be entered without warehouse.", pick, null, locationA1);
				AssertTrigger_Update_Fails("DDL should be from same warehouse as pick.", pick, whs1, locationInAnotherWhs);
				AssertTrigger_Update_Success("DDL is not mandatory for picks with attached work orders.", mainConnection, pick, whs1, null);
				AssertTrigger_Update_Success("Any DDL can go with correct warehouse.", mainConnection, pick, whs1, locationA1);
			}
		}

		#endregion

		#region CreatePickWithOrder

		WhsPick CreatePickWithOrder(SqlQueryBuilder sql, WhsWarehouse whs, WhsLocation ddl, string pickNo, OrgHeader client, OrgSupplierPart part, string orderNo, string orderType = "ORD", string orderSubType = "ORD")
		{
			var pick = new WhsPick(whs, pickNo, "NEW") { WP_WL_DockDoor = ddl }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, orderType, orderSubType, "PIC", orderNo) { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			new WhsDocketLine(order, part.PK, 10m).AppendInsertAndReturnObject(sql);

			return pick;
		}

		#endregion

		#region Asserts

		void AssertTrigger_Update_Success(string errorDescription, DbConnection connection, WhsPick pick, WhsWarehouse whs, WhsLocation ddl)
		{
			AssertNoExceptionThrown(errorDescription,
				() => WhsPick
				.UpdateWhere(pick.PK)
				.Set(p => p.WP_WW_Whs, whs)
				.Set(p => p.WP_WL_DockDoor, ddl).Post(connection));
		}

		void AssertTrigger_Update_Fails(string errorDescription, WhsPick pick, WhsWarehouse whs, WhsLocation ddl)
		{
			using (Db.DisposableActionForDbConnection())
			using (var newConnection = Db.NewExtraConnectionToMainDb())
			using (newConnection.BeginTransactionWithManager())
			{
				var sql = WhsPick
				.UpdateWhere(pick.PK)
				.Set(p => p.WP_WW_Whs, whs)
				.Set(p => p.WP_WL_DockDoor, ddl).AsSQL();

				try
				{
					using (var command = newConnection.Command(sql))
					{
						command.CommandTimeout = 5; // seconds
						command.ExecuteNonQuery();
					}
					newConnection.CommitTransaction();
					Assert(errorDescription, false);
				}
				catch (SqlException ex)
				{
					Assert(errorDescription, ex.Message.StartsWith("Entered Dock Door Location is not correct."));
				}
			}
		}
		#endregion
	}
}

