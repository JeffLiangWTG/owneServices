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
	[TestedType(typeof(TG_WhsLoad_CannotChangePlannedDockDoorIfWarehouseNotTheSameToAnyOrders))]
	class TG_WhsLoad_CannotChangePlannedDockDoorIfWarehouseNotTheSameToAnyOrdersTest : DBCreateTriggerScriptTest
	{
	}

	[UseSnapshotProtection]
	class Trigger_TG_WhsLoad_CannotChangePlannedDockDoorIfWarehouseNotTheSameToAnyOrdersTest : TestCase
	{
		#region TestTrigger_PlannedDockDoor

		public void TestTrigger_PlannedDockDoor()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch1 = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(mainConnection);
				var branch3 = new GlbBranch("BR3").InsertAndReturnObject(mainConnection);
				var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(mainConnection);
				var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(mainConnection);
				var whs3 = new WhsWarehouse("WH3", branch3.PK).WithDockDoor(mainConnection);
				var client = new OrgHeader("C1").InsertAndReturnObject(mainConnection);

				var areaDDL1 = new WhsArea(whs1.PK, "DDLArea").AppendInsertAndReturnObject(sql);
				var rowDDL1 = new WhsRow(whs1, "DDL").AppendInsertAndReturnObject(sql);
				var ddlLocationType1 = WhsLocationType.ShallowLoadFromDB(mainConnection, lt => lt.WLT_LocationClass == "DDL").First();
				var dockDoorLocation1 = new WhsLocation(rowDDL1.PK, areaDDL1.PK, areaDDL1.PK, ddlLocationType1.PK).AppendInsertAndReturnObject(sql);

				var areaDDL2 = new WhsArea(whs2.PK, "DDLArea").AppendInsertAndReturnObject(sql);
				var rowDDL2 = new WhsRow(whs2, "DDL").AppendInsertAndReturnObject(sql);
				var ddlLocationType2 = WhsLocationType.ShallowLoadFromDB(mainConnection, lt => lt.WLT_LocationClass == "DDL").First();
				var dockDoorLocation2 = new WhsLocation(rowDDL2.PK, areaDDL2.PK, areaDDL2.PK, ddlLocationType2.PK).AppendInsertAndReturnObject(sql);

				var areaDDL3 = new WhsArea(whs3.PK, "DDLArea").AppendInsertAndReturnObject(sql);
				var rowDDL3 = new WhsRow(whs3, "DDL").AppendInsertAndReturnObject(sql);
				var ddlLocationType3 = WhsLocationType.ShallowLoadFromDB(mainConnection, lt => lt.WLT_LocationClass == "DDL").First();
				var dockDoorLocation3 = new WhsLocation(rowDDL3.PK, areaDDL3.PK, areaDDL3.PK, ddlLocationType3.PK).AppendInsertAndReturnObject(sql);

				var transportCompany = new OrgHeader("TRANSPORT").AppendInsertAndReturnObject(sql);
				var load1 = new WhsLoad("WL01", "STD", transportCompany.PK, dockDoorLocation1.PK)
				{
					WLO_StartTime = DateTimeOffset.Now,
					WLO_TransportationUnitNumber = "ABC"
				}.AppendInsertAndReturnObject(sql);

				var load2 = new WhsLoad("WL02", "STD", transportCompany.PK, dockDoorLocation2.PK)
				{
					WLO_StartTime = DateTimeOffset.Now,
					WLO_TransportationUnitNumber = "EFG"
				}.AppendInsertAndReturnObject(sql);

				var order1 = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "ENT", "O1");
				order1.WD_WLO_PlannedLoad = load1.PK;
				order1.AppendInsertAndReturnObject(sql);

				var order2 = new WhsDocket(client.PK, whs2.PK, "ORD", "ORD", "ENT", "O2");
				order2.WD_WLO_PlannedLoad = load2.PK;
				order2.AppendInsertAndReturnObject(sql);

				ExecuteSQLInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());
				AssertTrigger_Update("Update must be allowed when the planned DockDoor has no same warehouse as any orders but the value has not changed.", mainConnection, load1.PK, dockDoorLocation1.PK, TestTriggerResult.Success);
				AssertTrigger_Update("Update must be prevented when the planned DockDoor has no same warehouse as any orders.", mainConnection, load1.PK, dockDoorLocation3.PK, TestTriggerResult.Fail);
				AssertTrigger_Update("Update must be prevented when the planned DockDoor has no same warehouse as any orders.", mainConnection, load1.PK, dockDoorLocation2.PK, TestTriggerResult.Fail);
			}
		}

		#endregion

		#region TestAssertions

		void AssertTrigger_Update(string errorMessage, DbConnection connection, Guid loadPK, Guid newPlannedDockDoorGuid, TestTriggerResult expectedResult)
		{
			var sql = WhsLoad.UpdateWhere(loadPK).Set(o => o.WLO_WL_PlannedDockDoor, newPlannedDockDoorGuid).AsSQL();

			if (expectedResult == TestTriggerResult.Success)
			{
				AssertNoExceptionThrown(errorMessage, () => ExecuteSQLInTransaction(connection, sql));
			}
			else
			{
				AssertExceptionThrown(typeof(SqlException), "Attempt to change Warehouse on Load when Orders are attached.", () => ExecuteSQLInTransaction(connection, sql), true);
			}
		}

		void ExecuteSQLInTransaction(DbConnection connection, string sql)
		{
			using (connection.BeginTransactionWithManager())
			{
				connection.ExecuteNonQuery(sql);
				connection.CommitTransaction();
			}
		}

		#endregion

		#region Implementation

		enum TestTriggerResult
		{
			Success,
			Fail
		}
		#endregion
	}
}
