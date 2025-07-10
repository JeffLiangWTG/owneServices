using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsDocketLine_EnsureSerialNumberQuantityMatchWithLine))]
	class TG_WhsDocketLine_EnsureSerialNumberQuantityMatchWithLineTest : DBCreateTriggerScriptTest
	{
		public void Test_TG_WhsDocketLine_EnsureSerialNumberQuantityMatchWithLine_Update_NoSerilNumber()
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
			var receiveLine = new WhsDocketLine(receive, product.PK, 2m, location.PK) { WE_ClientOrderedUnits = 2m, WE_StockOnHand = 2m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);

			AssertNotEquals("Precondition", "FIN", receiveLine.WE_DocketLineStatus);
			AssertNoExceptionThrown(() => Save(sql));

			AssertNoExceptionThrown(() => Save(FinaliseDocketSQL(receive, receiveLine)));
		}

		public void Test_TG_WhsDocketLine_EnsureSerialNumberQuantityMatchWithLine_Update_WithValidSerilNumber()
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
			var receiveLine = new WhsDocketLine(receive, product.PK, 2m, location.PK) { WE_ClientOrderedUnits = 2m, WE_StockOnHand = 2m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
			var whsSerialNumber1 = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber1.PK).AppendInsertAndReturnObject(sql);
			var whsSerialNumber2 = new WhsSerialNumber(client, product, "SN2").AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber2.PK).AppendInsertAndReturnObject(sql);

			AssertNotEquals("Precondition", "FIN", receiveLine.WE_DocketLineStatus);
			AssertNoExceptionThrown(() => Save(sql));

			AssertNoExceptionThrown(() => Save(FinaliseDocketSQL(receive, receiveLine)));
		}

		public void Test_TG_WhsDocketLine_EnsureSerialNumberQuantityMatchWithLine_Update_WithValidSerialNumberNotMatch()
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
			var receiveLine = new WhsDocketLine(receive, product.PK, 1m, location.PK) { WE_ClientOrderedUnits = 1m, WE_StockOnHand = 1m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
			var whsSerialNumber1 = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber1.PK).AppendInsertAndReturnObject(sql);
			var whsSerialNumber2 = new WhsSerialNumber(client, product, "SN2").AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber2.PK).AppendInsertAndReturnObject(sql);

			AssertNotEquals("Precondition", "FIN", receiveLine.WE_DocketLineStatus);
			AssertNoExceptionThrown(() => Save(sql));

			AssertTriggerExceptionThrown(() => Save(FinaliseDocketSQL(receive, receiveLine)));
		}

		public void Test_TG_WhsDocketLine_EnsureSerialNumberQuantityMatchWithLine_Insert_NoSerilNumber()
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

			AssertNoExceptionThrown(() => Save(sql));
			AssertEquals("Precondition", 0, WhsDocketLine.CountInDB(TestConnection));

			var sqlInsert = new SqlQueryBuilder();
			new WhsDocketLine(receive, product.PK, 1m, location.PK) { WE_StockOnHand = 1m, WE_DocketLineStatus = "FIN", WE_FinalisedDate = today }.AppendInsertAndReturnObject(sqlInsert);
			AssertNoExceptionThrown(() => Save(sqlInsert));

			AssertEquals("Should save in DB", 1, WhsDocketLine.CountInDB(TestConnection));
		}

		public void Test_TG_WhsDocketLine_EnsureSerialNumberQuantityMatchWithLine_Insert_WithValidSerilNumber()
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
			var receiveLine = new WhsDocketLine(receive, product.PK, 2m, location.PK) { WE_StockOnHand = 2m, WE_DocketLineStatus = "FIN", WE_FinalisedDate = today };
			var whsSerialNumber1 = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber1.PK).AppendInsertAndReturnObject(sql);
			var whsSerialNumber2 = new WhsSerialNumber(client, product, "SN2").AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber2.PK).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown(() => Save(sql));
			AssertEquals("Precondition", 0, WhsDocketLine.CountInDB(TestConnection));

			var sqlInsert = new SqlQueryBuilder();
			receiveLine.AppendInsertAndReturnObject(sqlInsert);
			AssertNoExceptionThrown("Should insert when serial number not match.", () => Save(sqlInsert));
			AssertEquals("Should save in DB", 1, WhsDocketLine.CountInDB(TestConnection));
		}

		public void Test_TG_WhsDocketLine_EnsureSerialNumberQuantityMatchWithLine_Insert_WithSerialNumberNotMatch()
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
			var receiveLine = new WhsDocketLine(receive, product.PK, 1m, location.PK) { WE_StockOnHand = 1m, WE_DocketLineStatus = "FIN", WE_FinalisedDate = today };
			var whsSerialNumber1 = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber1.PK).AppendInsertAndReturnObject(sql);
			var whsSerialNumber2 = new WhsSerialNumber(client, product, "SN2").AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber2.PK).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown(() => Save(sql));
			AssertEquals("Precondition", 0, WhsDocketLine.CountInDB(TestConnection));

			var sqlInsert = new SqlQueryBuilder();
			receiveLine.AppendInsertAndReturnObject(sqlInsert);
			AssertTriggerExceptionThrown(() => Save(sqlInsert));
		}

		#region Helper

		void AssertTriggerExceptionThrown(AnonymousMethod codeToRun)
		{
			AssertExceptionThrown(
					"Expected to Trigger prevent apply changes.",
					typeof(SqlException),
					TriggerErrorMessage,
					codeToRun,
					assertStartsWith: true);
		}

		SqlQueryBuilder FinaliseDocketSQL(WhsDocket docket, WhsDocketLine docketLine)
		{
			var finaliseQuery = new SqlQueryBuilder();
			//Suspend Trigger to allow finalise to succeed when updating docket + docketline
			finaliseQuery.AppendLine($"IF OBJECT_ID('{"TG_CheckDocketLineStatusAndDateForDocketLine"}', 'TR') IS NOT NULL DISABLE TRIGGER {"TG_CheckDocketLineStatusAndDateForDocketLine"} ON {WhsDocketLineSchema.Constants.TableName}");

			finaliseQuery.AppendFormat(@"
UPDATE dbo.WhsDocketLine SET WE_FinalisedDate = '{0}', WE_DocketLineStatus = '{1}', WE_SystemLastEditTimeUtc = SYSUTCDATETIME(), WE_SystemLastEditUser = '~BP' WHERE WE_PK = '{2}'
UPDATE dbo.WhsDocket SET WD_DocketStatus = '{1}', WD_FinalisedDate = '{0}', WD_GS_NKFinalizedBy = 'A', WD_UnloadCompletedTime = '{0}', WD_SystemLastEditTimeUtc = SYSUTCDATETIME(), WD_SystemLastEditUser = '~BP' WHERE WD_PK = '{3}'",
DateTime.Today.ToSqlFormat(), "FIN", docketLine.PK.ToString(), docket.PK.ToString())
				.AppendLine();

			finaliseQuery.Append($"IF OBJECT_ID('{"TG_CheckDocketLineStatusAndDateForDocketLine"}', 'TR') IS NOT NULL ENABLE TRIGGER {"TG_CheckDocketLineStatusAndDateForDocketLine"}  ON {WhsDocketLineSchema.Constants.TableName}");
			return finaliseQuery;
		}

		void Save(SqlQueryBuilder sql) => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

		string TriggerErrorMessage => "TriggerLikelyConcurrencyError: Attempt to change finalise docket line where the quantity does not match the number of serial numbers.";

		#endregion
	}
}
