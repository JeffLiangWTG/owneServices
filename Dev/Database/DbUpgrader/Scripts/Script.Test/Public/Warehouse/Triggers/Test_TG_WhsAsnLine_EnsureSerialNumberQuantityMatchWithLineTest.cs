using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsAsnLine_EnsureSerialNumberQuantityMatchWithLine))]
	class Test_TG_WhsAsnLine_EnsureSerialNumberQuantityMatchWithLineTest : DBCreateTriggerScriptTest
	{
		#region Insert

		public void Test_TG_WhsAsnLine_EnsureSerialNumberQuantityMatchWithLine_Insert_NonSerialNumber()
		{
			Test_TG_WhsAsnLine_EnsureSerialNumberQuantityMatchWithLine_Insert_Core(numberOfSN: 0);
		}

		[ExpectNoExceptions]
		public void Test_TG_WhsAsnLine_EnsureSerialNumberQuantityMatchWithLine_Insert_Less()
		{
			Test_TG_WhsAsnLine_EnsureSerialNumberQuantityMatchWithLine_Insert_Core(numberOfSN: 1);
		}

		public void Test_TG_WhsAsnLine_EnsureSerialNumberQuantityMatchWithLine_Insert_Exact()
		{
			Test_TG_WhsAsnLine_EnsureSerialNumberQuantityMatchWithLine_Insert_Core(numberOfSN: 2);
		}

		[ExpectNoExceptions]
		public void Test_TG_WhsAsnLine_EnsureSerialNumberQuantityMatchWithLine_Insert_More()
		{
			Test_TG_WhsAsnLine_EnsureSerialNumberQuantityMatchWithLine_Insert_Core(numberOfSN: 3);
		}

		void Test_TG_WhsAsnLine_EnsureSerialNumberQuantityMatchWithLine_Insert_Core(int numberOfSN)
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
			var receiveLine = new WhsDocketLine(receive, product.PK, 2m, location.PK) { WE_StockOnHand = 2m }.AppendInsertAndReturnObject(sql);
			var asnLine = new WhsAsnLine(receive, product.PK, 2m);
			for (var i = 0; i < numberOfSN; i++)
			{
				var whsSerialNumber = new WhsSerialNumber(client, product, $"SN{i}").AppendInsertAndReturnObject(sql);
				new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sql);
				new WhsSerialNumberPivot(asnLine.PK, WhsAsnLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sql);
			}
			asnLine.AppendInsertAndReturnObject(sql);

			var isValid = numberOfSN == asnLine.WN_Quantity || numberOfSN == 0;
			if (isValid)
			{
				AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sql.ToStringWithNewLineBetweenAppends()));
				AssertEquals(numberOfSN, WhsSerialNumberPivot.CountInDB(TestConnection, p => p.WSV_ParentTableCode == WhsDocketLineSchema.Constants.Prefix));
			}
			else
			{
				NUnit.Framework.Assert.That(() => SaveToDB(sql.ToStringWithNewLineBetweenAppends()), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), TriggerErrorMessage, true), "trigger should prevent invalid update.");
			}
		}

		#endregion

		#region Update

		public void Test_TG_WhsAsnLine_EnsureSerialNumberQuantityMatchWithLine_Update_Less()
		{
			Test_TG_WhsAsnLine_EnsureSerialNumberQuantityMatchWithLine_Update_Core(wnQuantity: 1);
		}

		public void Test_TG_WhsAsnLine_EnsureSerialNumberQuantityMatchWithLine_Update_Exact()
		{
			Test_TG_WhsAsnLine_EnsureSerialNumberQuantityMatchWithLine_Update_Core(wnQuantity: 2);
		}

		public void Test_TG_WhsAsnLine_EnsureSerialNumberQuantityMatchWithLine_Update_More()
		{
			Test_TG_WhsAsnLine_EnsureSerialNumberQuantityMatchWithLine_Update_Core(wnQuantity: 3);
		}

		void Test_TG_WhsAsnLine_EnsureSerialNumberQuantityMatchWithLine_Update_Core(int wnQuantity)
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
			var receiveLine = new WhsDocketLine(receive, product.PK, 2m, location.PK) { WE_StockOnHand = 2m }.AppendInsertAndReturnObject(sql);
			var asnLine = new WhsAsnLine(receive, product.PK, 0m).AppendInsertAndReturnObject(sql);
			var whsSerialNumber1 = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber1.PK).AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(asnLine.PK, WhsAsnLineSchema.Constants.Prefix, whsSerialNumber1.PK).AppendInsertAndReturnObject(sql);
			var whsSerialNumber2 = new WhsSerialNumber(client, product, "SN2").AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber2.PK).AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(asnLine.PK, WhsAsnLineSchema.Constants.Prefix, whsSerialNumber2.PK).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sql.ToStringWithNewLineBetweenAppends()));

			var sqlUpdate = new SqlQueryBuilder();
			sqlUpdate.AppendLine(WhsAsnLine.UpdateWhere(asnLine.PK).Set(a => a.WN_Quantity, wnQuantity).AsSQL());

			var isValid = wnQuantity == 2 || wnQuantity == 0;
			if (isValid)
			{
				AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sqlUpdate.ToStringWithNewLineBetweenAppends()));
				AssertEquals(wnQuantity, WhsSerialNumberPivot.CountInDB(TestConnection, p => p.WSV_ParentTableCode == WhsDocketLineSchema.Constants.Prefix));
			}
			else
			{
				NUnit.Framework.Assert.That(() => SaveToDB(sqlUpdate.ToStringWithNewLineBetweenAppends()), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), TriggerErrorMessage, true), "trigger should prevent invalid update.");
			}
		}

		#endregion

		public void Test_TG_WhsAsnLine_EnsureSerialNumberQuantityMatchWithLine_CanChangeUpRelatedColumn()
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
			var receiveLine = new WhsDocketLine(receive, product.PK, 2m, location.PK) { WE_StockOnHand = 2m }.AppendInsertAndReturnObject(sql);
			var asnLine = new WhsAsnLine(receive, product.PK, 1m).AppendInsertAndReturnObject(sql);
			var whsSerialNumber1 = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var whsSerialNumber2 = new WhsSerialNumber(client, product, "SN2").AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber1.PK).AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber2.PK).AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(asnLine.PK, WhsAsnLineSchema.Constants.Prefix, whsSerialNumber1.PK).AppendInsertAndReturnObject(sql);

			AssertEquals("Precondition", 0, WhsAsnLine.CountInDB(TestConnection));

			using (TestWhsDataSetupHelper.SuspendTrigger(nameof(TG_WhsAsnLine_EnsureSerialNumberQuantityMatchWithLine), WhsAsnLineSchema.Constants.TableName, TestConnection))
			{
				AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sql));
			}

			AssertEquals(2, WhsSerialNumberPivot.CountInDB(TestConnection, p => p.WSV_ParentTableCode == WhsDocketLineSchema.Constants.Prefix));
			AssertEquals(1, WhsSerialNumberPivot.CountInDB(TestConnection, p => p.WSV_ParentTableCode == WhsAsnLineSchema.Constants.Prefix));

			WhsDocketLine.AssertFromDB(TestConnection, receiveLine.PK)
				.ExpectEquals("WE_DocketLineStatus: ", i => i.WE_DocketLineStatus, "FIN")
				.VerifyAll();

			WhsAsnLine.AssertFromDB(TestConnection, asnLine.PK)
				.ExpectNotEquals("WN_SystemLastEditUser", l => l.WN_SystemLastEditUser, "Me")
				.VerifyAll();

			var sqlUpdateValid = WhsAsnLine.UpdateWhere(asnLine.PK).Set(l => l.WN_SystemLastEditUser, "Me").AsSQL();
			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sqlUpdateValid));

			WhsAsnLine.AssertFromDB(TestConnection, asnLine.PK)
				.ExpectEquals("WN_SystemLastEditUser", l => l.WN_SystemLastEditUser, "Me")
				.VerifyAll();

			var invalidUpdate = WhsAsnLine.UpdateWhere(asnLine.PK).Set(l => l.WN_Quantity, 10m).AsSQL();
			NUnit.Framework.Assert.That(() => SaveToDB(invalidUpdate), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), TriggerErrorMessage, true), "trigger should prevent invalid update.");
		}

		#region Helper

		void SaveToDB(SqlQueryBuilder sql) => SaveToDB(sql.ToStringWithNewLineBetweenAppends());

		void SaveToDB(string sql)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(nameof(TG_WhsSerialNumberPivot_PreventDeleteWhenJobFinalised), WhsSerialNumberPivotSchema.Constants.TableName, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql);
			}
		}

		string TriggerErrorMessage => "Attempt to modify AsnLine where the quantity does not match the number of entered serial numbers.";

		#endregion
	}
}
