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
	[TestedType(typeof(TG_WhsDocket_PreventWarehouseChangeWhenAttachedToLoad))]
	class TG_WhsDocket_PreventWarehouseChangeWhenAttachedToLoadTest : DBCreateTriggerScriptTest
	{
	}

	[UseSnapshotProtection]
	class Trigger_TG_WhsDocket_PreventWarehouseChangeWhenAttachedToLoadTest : TestCase
	{
		#region TestTrigger_Warehouse

		public void TestTrigger_Warehouse()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch1 = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(mainConnection);
				var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(mainConnection);
				var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(mainConnection);
				var client = new OrgHeader("C1").InsertAndReturnObject(mainConnection);
				var areaDDL = new WhsArea(whs1.PK, "DDLArea").AppendInsertAndReturnObject(sql);
				var rowDDL = new WhsRow(whs1, "DDL").AppendInsertAndReturnObject(sql);
				var ddlLocationType = WhsLocationType.ShallowLoadFromDB(mainConnection, lt => lt.WLT_LocationClass == "DDL").First();
				var dockDoorLocation = new WhsLocation(rowDDL.PK, areaDDL.PK, areaDDL.PK, ddlLocationType.PK).AppendInsertAndReturnObject(sql);

				var transportCompany = new OrgHeader("TRANSPORT").AppendInsertAndReturnObject(sql);
				var load = new WhsLoad("WL01", "STD", transportCompany.PK, dockDoorLocation.PK)
				{
					WLO_StartTime = DateTimeOffset.Now,
					WLO_TransportationUnitNumber = "ABC"
				}.AppendInsertAndReturnObject(sql);

				var order1 = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
				var order2 = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "ENT", "O2");
				order2.WD_WLO_PlannedLoad = load.PK;
				order2.AppendInsertAndReturnObject(sql);
				ExecuteSQLInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());
				AssertTrigger_Update("Update must be allowed when the order is not attached to a Load.", mainConnection, order1.PK, whs2.PK, TestTriggerResult.Success);
				AssertTrigger_Update("Update must be prevented when the order is attached to a Load.", mainConnection, order2.PK, whs2.PK, TestTriggerResult.Fail);
				AssertTrigger_Update("Update must be allowed when the order is attached to a Load but the value has not changed.", mainConnection, order1.PK, whs1.PK, TestTriggerResult.Success);
			}
		}

		#endregion

		#region TestAssertions

		void AssertTrigger_Update(string errorMessage, DbConnection connection, Guid docketPK, Guid newWarehouseGuid, TestTriggerResult expectedResult)
		{
			var sql = WhsDocket.UpdateWhere(docketPK).Set(o => o.WD_WW_Whs, newWarehouseGuid).AsSQL();

			if (expectedResult == TestTriggerResult.Success)
			{
				AssertNoExceptionThrown(errorMessage, () => ExecuteSQLInTransaction(connection, sql));
			}
			else
			{
				AssertExceptionThrown(typeof(SqlException), "Attempt to change warehouse when the order is attached to a Load.", () => ExecuteSQLInTransaction(connection, sql), true);
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
