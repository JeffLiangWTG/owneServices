using System;
using System.Threading;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.ProductWarehouse
{
	[TestedType(typeof(ClearIncorrectlySetWZ_WE_OriginalPickedInventoryLine))]
	class ClearIncorrectlySetWZ_WE_OriginalPickedInventoryLineTest : DataTransformationTestCase
	{
		public void TestOriginalPickedInventoryLineClearedCorrectly()
		{
			TestConnection.ExecuteNonQuery("ALTER TABLE WhsPickLine DROP CONSTRAINT Constraint_WZ_WE_OriginalOrderLine");

			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var area = new WhsArea(warehouse.PK, "TESt").InsertAndReturnObject(TestConnection);
			var row = new WhsRow(warehouse, "A").InsertAndReturnObject(TestConnection);
			var location = new WhsLocation(row.PK, area.PK, area.PK).InsertAndReturnObject(TestConnection);
			var client = TestWhsDataSetupHelper.GetOrgHeader();
			var product = new OrgSupplierPart("P1").InsertAndReturnObject(TestConnection);

			var sql = new SqlQueryBuilder();
			var receive = new WhsDocket(client, warehouse.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = DateTimeOffset.Now }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 100m, location.PK) { WE_StockOnHand = 90m }.AppendInsertAndReturnObject(sql);
			var otherReceiveLine = new WhsDocketLine(receive, product.PK, 100m, location.PK) { WE_StockOnHand = 100m }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(warehouse, "P1", "PIS").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client, warehouse.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 20m).AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client, warehouse.PK, "TFR", "TFR", "ENT", "T1") { WD_WP_ParentPickForTransfer = pick }.AppendInsertAndReturnObject(sql);
			var transferLine = new WhsDocketLine(transfer, product.PK, 10m, warehouse.WW_DefaultOutboundDockDoor.FK)
			{
				WE_DocketLineStatus = "HFT",
				WE_WL_TransferFrom = location.PK,
				WE_OriginalInventoryStatus = "INT",
				WE_CurrentInventoryStatus = "INT",
				WE_StockOnHand = 10m
			}.AppendInsertAndReturnObject(sql);

			// setup bad outbound transfer line where it has originalPickedInventoryLine set.
			var transferPickLine = new WhsPickLine(receiveLine, transferLine, 10m)
			{
				WZ_GS_NKAssignedTo = "~BP",
				WZ_PickedDateTime = DateTimeOffset.Now,
				WZ_WE_OriginalPickedInventoryLine = otherReceiveLine,
				WZ_SystemLastEditTimeUtc = DateTime.UtcNow.AddDays(-1),
				WZ_SystemLastEditUser = "AAA"
			}.AppendInsertAndReturnObject(sql);
			var orderPickLine = new WhsPickLine(transferLine, orderLine, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_WE_OriginalPickedInventoryLine = receiveLine }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			// property on SQLDO is read only, don't want to have a multi repo patch back for one test.
			WhsPickLine.UpdateWhere(transferPickLine.PK).Set(pl => pl.WZ_WE_OriginalOrderLine, orderLine).Post(TestConnection);

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			WhsPickLine.AssertFromDB(TestConnection, transferPickLine.PK)
				.ExpectNull("WZ_WE_OriginalPickedInventoryLine should be cleared from Transfer Line PickLine.", pl => pl.WZ_WE_OriginalPickedInventoryLine)
				.ExpectEquals("WZ_WE_OriginalOrderLine should be untouched.", pl => pl.WZ_WE_OriginalOrderLine, orderLine)
				.ExpectEquals(nameof(WhsPickLine.WZ_SystemLastEditUser), pl => pl.WZ_SystemLastEditUser, "~BP")
				.ExpectEquals(nameof(WhsPickLine.WZ_SystemLastEditTimeUtc), pl => pl.WZ_SystemLastEditTimeUtc > DateTime.UtcNow.AddHours(-1), true)
				.VerifyAll();
			WhsPickLine.AssertFromDB(TestConnection, orderPickLine.PK)
				.ExpectEquals("WZ_WE_OriginalPickedInventoryLine should be untouched for Order Line.", pl => pl.WZ_WE_OriginalPickedInventoryLine, receiveLine)
				.ExpectNull("WZ_WE_OriginalOrderLine should be untouched.", pl => pl.WZ_WE_OriginalOrderLine)
				.VerifyAll();

			var addConstraintSql = @"
ALTER TABLE WhsPickLine
ADD CONSTRAINT Constraint_WZ_WE_OriginalOrderLine CHECK
(WZ_WE_OriginalOrderLine IS NULL OR (WZ_PickedDateTime IS NOT NULL AND WZ_WE_OriginalPickedInventoryLine IS NULL))";
			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(addConstraintSql));
		}

		public override string[] expectedIndex => new[]
		{
			"NONCLUSTERED INDEX [_WTG__Clear Incorrectly Set WZ_WE_OriginalPickedInventoryLine_1] ON [dbo].[WhsPickLine] ([WZ_WE_OriginalOrderLine], [WZ_WE_OriginalPickedInventoryLine]) WHERE ([WZ_WE_OriginalOrderLine] IS NOT NULL AND [WZ_WE_OriginalPickedInventoryLine] IS NOT NULL) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		#region Implementation

		protected override void PrepareTestData()
		{
		}

		protected override void AssertTransformationResults()
		{
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new ClearIncorrectlySetWZ_WE_OriginalPickedInventoryLine();

		#endregion
	}
}
