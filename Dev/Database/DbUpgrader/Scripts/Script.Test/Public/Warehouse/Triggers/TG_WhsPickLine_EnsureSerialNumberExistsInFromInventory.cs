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
	[TestedType(typeof(TG_WhsPickLine_EnsureSerialNumberExistsInFromInventory))]
	class TG_WhsPickLine_EnsureSerialNumberExistsInFromInventoryTest : DBCreateTriggerScriptTest
	{
		#region Test_TG_WhsPickLine_EnsureSerialNumberExistsInFromInventory

		public void Test_TG_WhsPickLine_EnsureSerialNumberExistsInFromInventory_SerialNumberExistsInInventory()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var today = DateTime.Today;
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 10m, location.PK) { WE_DocketLineStatus = "FIN", WE_FinalisedDate = today, WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 2m).AppendInsertAndReturnObject(sql);

			var whsSerialNumber = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var whsSerialNumberPivotRL = new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sql);
			var whsSerialNumberPivotOL = new WhsSerialNumberPivot(orderLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => Save(sql));

			var updateBuilder = new SqlQueryBuilder();
			var pickLine = new WhsPickLine(receiveLine, orderLine, 1m) { WZ_OriginalReservedQty = 1m }.AppendInsertAndReturnObject(updateBuilder);
			updateBuilder.AppendLine(WhsSerialNumberPivot.UpdateWhere(whsSerialNumberPivotRL.PK).Set(s => s.WSV_WZ_PickingLine, pickLine.PK).AsSQL());

			AssertNoExceptionThrown("Should save without issue.", () => Save(updateBuilder));

			WhsSerialNumberPivot.AssertFromDB(TestConnection, whsSerialNumberPivotRL.PK)
				.ExpectEquals("WSV_ParentID: ", i => i.WSV_ParentID, receiveLine.PK)
				.ExpectEquals("WSV_ParentTableCode: ", i => i.WSV_ParentTableCode, WhsDocketLineSchema.Constants.Prefix)
				.ExpectEquals("WSV_IsReleaseCaptured: ", i => i.WSV_IsReleaseCaptured, false)
				.ExpectEquals("WSV_WZ_PickingLine: ", i => i.WSV_WZ_PickingLine, pickLine.PK)
				.VerifyAll();
		}

		public void Test_TG_WhsPickLine_EnsureSerialNumberExistsInFromInventory_HasInventorySerialNumber()
		{
			Test_TG_WhsPickLine_EnsureSerialNumberExistsInFromInventory_Core(hasInventorySNPivoit: true);
		}

		public void Test_TG_WhsPickLine_EnsureSerialNumberExistsInFromInventory_NotHaveInventorySerialNumber()
		{
			Test_TG_WhsPickLine_EnsureSerialNumberExistsInFromInventory_Core(hasInventorySNPivoit: false);
		}

		void Test_TG_WhsPickLine_EnsureSerialNumberExistsInFromInventory_Core(bool hasInventorySNPivoit)
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var locationA2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var today = DateTime.Today;
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 10m, locationA1.PK) { WE_DocketLineStatus = "FIN", WE_FinalisedDate = today, WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 2m).AppendInsertAndReturnObject(sql);

			var whsSerialNumber = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var whsSerialNumberPivotOL = new WhsSerialNumberPivot(orderLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sql);

			var whsSerialNumberPivotRL =
				hasInventorySNPivoit ?
				new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sql) :
				null;

			AssertNoExceptionThrown("Should save without issue.", () => Save(sql));

			var updateBuilder = new SqlQueryBuilder();
			var pickLine = new WhsPickLine(receiveLine, orderLine, 1m) { WZ_OriginalReservedQty = 1m }.AppendInsertAndReturnObject(updateBuilder);

			if (hasInventorySNPivoit)
			{
				updateBuilder.AppendLine(WhsSerialNumberPivot.UpdateWhere(whsSerialNumberPivotRL.PK).Set(s => s.WSV_WZ_PickingLine, pickLine.PK).AsSQL());
				AssertNoExceptionThrown("When a Serial Number is in source location, no exception should be thrown.", () => Save(updateBuilder));

				WhsSerialNumberPivot.AssertFromDB(TestConnection, whsSerialNumberPivotRL.PK)
					.ExpectEquals("WSV_ParentID: ", i => i.WSV_ParentID, receiveLine.PK)
					.ExpectEquals("WSV_ParentTableCode: ", i => i.WSV_ParentTableCode, WhsDocketLineSchema.Constants.Prefix)
					.ExpectEquals("WSV_IsReleaseCaptured: ", i => i.WSV_IsReleaseCaptured, false)
					.ExpectEquals("WSV_WZ_PickingLine: ", i => i.WSV_WZ_PickingLine, pickLine.PK)
					.VerifyAll();

				WhsSerialNumberPivot.AssertFromDB(TestConnection, whsSerialNumberPivotOL.PK)
					.ExpectEquals("WSV_ParentID: ", i => i.WSV_ParentID, orderLine.PK)
					.ExpectEquals("WSV_ParentTableCode: ", i => i.WSV_ParentTableCode, WhsDocketLineSchema.Constants.Prefix)
					.ExpectEquals("WSV_IsReleaseCaptured: ", i => i.WSV_IsReleaseCaptured, false)
					.ExpectEquals("WSV_WZ_PickingLine: ", i => i.WSV_WZ_PickingLine, null)
					.VerifyAll();
			}
			else
			{
				NUnit.Framework.Assert.That(() => Save(updateBuilder), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), TriggerErrorMessage, true), "When a Serial Number is not in source location, an exception should be thrown.");
				AssertEquals("Should not save in DB.", 0, WhsSerialNumberPivot.CountInDB(TestConnection));
			}
		}

		#endregion

		#region Helper

		void Save(SqlQueryBuilder sql)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}

		string TriggerErrorMessage => "Attempt to assign a serial number which does not exists in from inventory location.";

		#endregion
	}
}
