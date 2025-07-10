using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsDocketLine_PreventDeleteFinalizedJobUsingSerialNumber))]
	class TG_WhsDocketLine_PreventDeleteFinalizedJobUsingSerialNumberTest : DBCreateTriggerScriptTest
	{
		public void Test_TG_WhsDocketLine_PreventDeleteFinaliseLineWithSerialNumber_NonFinalised()
		{
			var today = DateTime.UtcNow.ToSmallDateTimeFloor();
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 10m, location.PK) { WE_StockOnHand = 10m, WE_CurrentInventoryStatus = "PUT", WE_OriginalInventoryStatus = "PUT" }.AppendInsertAndReturnObject(sql);
			var whsSerialNumber = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown(() => Save(sql));

			AssertEquals("Precondition", 1, WhsDocketLine.CountInDB(TestConnection));
			AssertNoExceptionThrown(() => WhsDocketLine.DeleteInDB(TestConnection, receiveLine.PK));
			AssertEquals(0, WhsDocketLine.CountInDB(TestConnection));
		}

		public void Test_TG_WhsDocketLine_PreventDeleteFinalizedJobUsingSerialNumber_Finalise()
		{
			var today = DateTime.UtcNow.ToSmallDateTimeFloor();
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 1m, location.PK) { WE_StockOnHand = 1m, WE_DocketLineStatus = "FIN", WE_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var whsSerialNumber = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown(() => Save(sql));

			AssertTriggerExceptionThrown(() => WhsDocketLine.DeleteInDB(TestConnection, receiveLine.PK));
		}

		public void Test_TG_WhsDocketLine_PreventDeleteFinalizedJobUsingSerialNumber_WhenDoNotHaveSerialNumberPivot()
		{
			var today = DateTime.UtcNow.ToSmallDateTimeFloor();
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 1m, location.PK) { WE_StockOnHand = 1m, WE_DocketLineStatus = "FIN", WE_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var whsSerialNumber = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown(() => Save(sql));

			AssertEquals("Precondition", 1, WhsDocketLine.CountInDB(TestConnection));
			// It does not need to be deletable; this can be changed if necessary.
			AssertNoExceptionThrown(() => WhsDocketLine.DeleteInDB(TestConnection, receiveLine.PK));
			AssertEquals(0, WhsDocketLine.CountInDB(TestConnection));
		}

		#region Helper

		void Save(SqlQueryBuilder sql) => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

		void AssertTriggerExceptionThrown(AnonymousMethod codeToRun)
		{
			AssertExceptionThrown(
					"Expected to Trigger prevent apply changes.",
					typeof(SqlException),
					TriggerErrorMessage,
					codeToRun,
					assertStartsWith: true);
		}

		string TriggerErrorMessage => "Attempt to delete finalized job using Serial Number.";

		#endregion
	}
}
