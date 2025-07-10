using System;
using System.Linq.Expressions;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsDocket_PreventClientChangeWhenHasLines))]
	class TG_WhsDocket_PreventClientChangeWhenHasLinesTest : DBCreateTriggerScriptTest
	{
	}

	[UseSnapshotProtection]
	class Trigger_TG_WhsDocket_PreventClientChangeWhenHasLinesTest : TestCase
	{
		#region TestTrigger_Client

		public void TestTrigger_Client()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(mainConnection);
				var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
				var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

				var client1 = new OrgHeader("CLIENT1").AppendInsertAndReturnObject(sql);
				var client2 = new OrgHeader("CLIENT2").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				var receive1 = new WhsDocket(client1.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
				var receive2 = new WhsDocket(client1.PK, whs.PK, "INW", "REC", "ENT", "R2").AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive2, product.PK, 10m, locationA1.PK) { WE_StockOnHand = 10m, WE_OriginalInventoryStatus = "PUT", WE_CurrentInventoryStatus = "PUT" }.AppendInsertAndReturnObject(sql);

				ExecuteSQLInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());

				AssertTrigger_Update("Update must be allowed when the docket has no lines.", mainConnection, receive1.PK, (d) => d.WD_OH_Client, client2.PK, TestTriggerResult.Success);
				AssertTrigger_Update("Update must be prevented when the docket has captured lines.", mainConnection, receive2.PK, (d) => d.WD_OH_Client, client2.PK, TestTriggerResult.Fail);
				AssertTrigger_Update("Update must be allowed when the docket has captured lines but the value has not changed.", mainConnection, receive2.PK, (d) => d.WD_OH_Client, client1.PK, TestTriggerResult.Success);
			}
		}

		#endregion

		#region TestAssertions

		void AssertTrigger_Update<T>(string errorMessage, DbConnection connection, Guid docketPK, Expression<Func<WhsDocket, T>> property, T value, TestTriggerResult expectedResult)
		{
			var sql = WhsDocket
				.UpdateWhere(docketPK)
				.Set(property, value).AsSQL();

			if (expectedResult == TestTriggerResult.Success)
			{
				AssertNoExceptionThrown(errorMessage, () => ExecuteSQLInTransaction(connection, sql));
			}
			else
			{
				AssertExceptionThrown(typeof(SqlException), "Attempt to change client for a job with captured lines.", () => ExecuteSQLInTransaction(connection, sql), true);
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

