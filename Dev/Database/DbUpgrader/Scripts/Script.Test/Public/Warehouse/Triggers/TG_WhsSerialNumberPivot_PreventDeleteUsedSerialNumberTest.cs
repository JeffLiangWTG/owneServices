using System;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsSerialNumberPivot_PreventDeleteUsedSerialNumber))]
	class TG_WhsSerialNumberPivot_PreventDeleteUsedSerialNumberTest : DBCreateTriggerScriptTest
	{
		#region Test_TG_WhsSerialNumberPivot_PreventDeleteUsedSerialNumber

		public void Test_TG_WhsSerialNumberPivot_PreventDeleteUsedSerialNumber_DeletetUsed()
		{
			Test_TG_WhsSerialNumberPivot_PreventDeleteUsedSerialNumber(deletetUsed: true);
		}

		public void Test_TG_WhsSerialNumberPivot_PreventDeleteUsedSerialNumber_DeletetNotUsed()
		{
			Test_TG_WhsSerialNumberPivot_PreventDeleteUsedSerialNumber(deletetUsed: false);
		}

		void Test_TG_WhsSerialNumberPivot_PreventDeleteUsedSerialNumber(bool deletetUsed)
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var today = DateTime.Today;
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 10m, location.PK) { WE_StockOnHand = 10m, WE_CurrentInventoryStatus = "PUT", WE_OriginalInventoryStatus = "PUT" }.AppendInsertAndReturnObject(sql);

			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 1m).AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine, orderLine, 1m) { WZ_OriginalReservedQty = 1m }.AppendInsertAndReturnObject(sql);

			var whsSerialNumber1 = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var whsSerialNumber2 = new WhsSerialNumber(client, product, "SN2").AppendInsertAndReturnObject(sql);
			var whsSerialNumberPivotRL1 = new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber1.PK).AppendInsertAndReturnObject(sql);
			var whsSerialNumberPivotRL2 = new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber2.PK).AppendInsertAndReturnObject(sql);
			var whsSerialNumberPivotOL1 = new WhsSerialNumberPivot(orderLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber1.PK).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => Save(sql));

			AssertEquals("Precondition", 3, WhsSerialNumberPivot.CountInDB(TestConnection));

			AssertNoExceptionThrown("Should be able delete not used serial number.", () => DeletePivot(whsSerialNumberPivotRL2));
			if (deletetUsed)
			{
				AssertEquals(2, WhsSerialNumberPivot.CountInDB(TestConnection));
				NUnit.Framework.Assert.That(() => DeletePivot(whsSerialNumberPivotRL1), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), TriggerErrorMessage, true), "Should not be able delete used serial number.");
			}
			else
			{
				AssertNoExceptionThrown("Should be able delete not used serial number. (Delete Order First)", () => DeletePivot(whsSerialNumberPivotOL1));
				AssertNoExceptionThrown("Should be able delete not used serial number.", () => DeletePivot(whsSerialNumberPivotRL1));

				AssertEquals(0, WhsSerialNumberPivot.CountInDB(TestConnection));
			}

			void DeletePivot(WhsSerialNumberPivot pivot) => WhsSerialNumberPivot.DeleteInDB(TestConnection, pivot.PK);
		}

		#endregion

		#region Helper

		void Save(SqlQueryBuilder sql)
		{
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}

		string TriggerErrorMessage => "Attempt to delete a serial number used in transaction jobs.";

		#endregion
	}
}
