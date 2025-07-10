using System;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.ExtendedProperties;
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
	[TestedType(typeof(PopulateWZ_WE_OriginalOrderLine))]
	class PopulateWZ_WE_OriginalOrderLineTest : DataTransformationTestCase
	{
		const string UpdateTriggerName = "TG_WhsPickLine_KeepWZ_WE_OriginalOrderLineInSync_Update";
		const string InsertTriggerName = "TG_WhsPickLine_KeepWZ_WE_OriginalOrderLineInSync_Insert";
		const string LastProcessedChunkPKName = "PopulateWZ_WE_OriginalOrderLine.LastProcessedChunkPK";

		public void TestTriggers()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var area = new WhsArea(warehouse.PK, "TESt").InsertAndReturnObject(TestConnection);
			var row = new WhsRow(warehouse, "A").InsertAndReturnObject(TestConnection);
			var location = new WhsLocation(row.PK, area.PK, area.PK).InsertAndReturnObject(TestConnection);
			var client = TestWhsDataSetupHelper.GetOrgHeader();
			var product = new OrgSupplierPart("P1").InsertAndReturnObject(TestConnection);

			var sql = new SqlQueryBuilder();
			var receive = new WhsDocket(client, warehouse.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = DateTimeOffset.Now }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 100m, location.PK) { WE_StockOnHand = 90m }.AppendInsertAndReturnObject(sql);

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

			var transferPickLine = new WhsPickLine(receiveLine, transferLine, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_PickedDateTime = DateTimeOffset.Now }.AppendInsertAndReturnObject(sql);
			var orderPickLine = new WhsPickLine(receiveLine, orderLine, 10m) { WZ_GS_NKAssignedTo = "~BP" }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals("Update Sync Trigger should not exist", false, DbObjectCreator.TriggerExists(TestConnection, WhsPickLineSchema.Constants.TableName, UpdateTriggerName));
			AssertEquals("Insert Sync Trigger should not exist", false, DbObjectCreator.TriggerExists(TestConnection, WhsPickLineSchema.Constants.TableName, InsertTriggerName));

			var transformation = GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("Update Sync Trigger Definition should be correct.", @"
CREATE TRIGGER dbo.TG_WhsPickLine_KeepWZ_WE_OriginalOrderLineInSync_Update
	ON dbo.WhsPickLine
	FOR UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	IF UPDATE (WZ_WE_InventoryLine)
	BEGIN
		EXEC dbo.SuspendTrigger 'TG_WhsPickLine_TransactionAndPickedQtyIsCorrect'
		EXEC dbo.SuspendTrigger 'TG_WhsPickLine_StockOnHandIsBalanced'

		UPDATE wp
		SET
			wp.WZ_WE_OriginalOrderLine = i.WZ_WE_TransactionLine,
			wp.WZ_SystemLastEditUser = CASE WHEN wp.WZ_SystemLastEditUser <> '' THEN wp.WZ_SystemLastEditUser ELSE '~BP' END,
			wp.WZ_SystemLastEditTimeUtc = ISNULL(wp.WZ_SystemLastEditTimeUtc, GetUtcDate())
		FROM
			dbo.WhsPickLine wp
			JOIN inserted i ON i.WZ_WE_InventoryLine = wp.WZ_WE_TransactionLine
		WHERE
			i.WZ_WE_OriginalPickedInventoryLine IS NOT NULL

		EXEC dbo.ResumeTrigger 'TG_WhsPickLine_TransactionAndPickedQtyIsCorrect'
		EXEC dbo.ResumeTrigger 'TG_WhsPickLine_StockOnHandIsBalanced'
	END
END", DbObjectCreator.GetTriggerDefinition(TestConnection, UpdateTriggerName));

			AssertEquals("Insert Sync Trigger Definition should be correct.", @"
CREATE TRIGGER dbo.TG_WhsPickLine_KeepWZ_WE_OriginalOrderLineInSync_Insert
	ON dbo.WhsPickLine
	FOR INSERT
AS
BEGIN
	SET NOCOUNT ON;

	EXEC dbo.SuspendTrigger 'TG_WhsPickLine_TransactionAndPickedQtyIsCorrect'
	EXEC dbo.SuspendTrigger 'TG_WhsPickLine_StockOnHandIsBalanced'

	UPDATE wp
	SET
		wp.WZ_WE_OriginalOrderLine = op.WZ_WE_TransactionLine,
		wp.WZ_SystemLastEditUser = wp.WZ_SystemLastEditUser,
		wp.WZ_SystemLastEditTimeUtc = wp.WZ_SystemLastEditTimeUtc
	FROM
		dbo.WhsPickLine wp
		JOIN inserted i ON wp.WZ_PK = i.WZ_PK
		JOIN dbo.WhsDocketLine ON wp.WZ_WE_TransactionLine = WE_PK
		JOIN dbo.WhsDocket ON WE_WD = WD_PK
		JOIN dbo.WhsPickLine op ON op.WZ_WE_InventoryLine = WE_PK
	WHERE
		WE_DocketLineType = 'TFR'
		AND WD_WP_ParentPickForTransfer IS NOT NULL

	EXEC dbo.ResumeTrigger 'TG_WhsPickLine_TransactionAndPickedQtyIsCorrect'
	EXEC dbo.ResumeTrigger 'TG_WhsPickLine_StockOnHandIsBalanced'
END", DbObjectCreator.GetTriggerDefinition(TestConnection, InsertTriggerName));

			// simulate the WZ_WE_InventoryLine being set when outbound transfer creator kicks in
			WhsPickLine.UpdateWhere(orderPickLine.PK).Set(pl => pl.WZ_WE_InventoryLine, transferLine).Set(pl => pl.WZ_WE_OriginalPickedInventoryLine, receiveLine).Post(TestConnection);

			WhsPickLine.AssertFromDB(TestConnection, transferPickLine.PK)
				.ExpectEquals("Original Order Line should be updated from Trigger.", pl => pl.WZ_WE_OriginalOrderLine, orderLine)
				.ExpectNull("Original Picked Inventory Line should not be touched.", pl => pl.WZ_WE_OriginalPickedInventoryLine)
				.VerifyAll();

			sql.Clear();
			var clonedTransferLine = new WhsDocketLine(transfer, product.PK, 10m, warehouse.WW_DefaultOutboundDockDoor.FK)
			{
				WE_DocketLineStatus = "HFT",
				WE_WL_TransferFrom = location.PK,
				WE_OriginalInventoryStatus = "INT",
				WE_CurrentInventoryStatus = "INT",
				WE_StockOnHand = 10m
			}.AppendInsertAndReturnObject(sql);

			new WhsPickLine(clonedTransferLine, orderLine, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_WE_OriginalPickedInventoryLine = receiveLine }.AppendInsertAndReturnObject(sql);

			sql.Append(WhsDocketLine.UpdateWhere(receiveLine.PK).Set(dl => dl.WE_StockOnHand, 80m).AsSQL());

			// simulate Transfer line being split by creating a new PickLine
			var newPickLine = new WhsPickLine(receiveLine, clonedTransferLine, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_PickedDateTime = DateTimeOffset.Now }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsPickLine_TransactionAndPickedQtyIsCorrect, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.WZ_WE_TransactionLine, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			WhsPickLine.AssertFromDB(TestConnection, newPickLine.PK)
				.ExpectEquals("Original Order Line should be updated from Trigger.", pl => pl.WZ_WE_OriginalOrderLine, orderLine)
				.ExpectNull("Original Picked Inventory Line should not be touched.", pl => pl.WZ_WE_OriginalPickedInventoryLine)
				.VerifyAll();

			transformation.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			AssertEquals("Update Sync Trigger should be dropped", false, DbObjectCreator.TriggerExists(TestConnection, WhsPickLineSchema.Constants.TableName, UpdateTriggerName));
			AssertEquals("Insert Sync Trigger should be dropped", false, DbObjectCreator.TriggerExists(TestConnection, WhsPickLineSchema.Constants.TableName, InsertTriggerName));
		}

		public void TestUpdateTriggerWorks_WhenLastEditUserAndTimeAreNotSet()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var area = new WhsArea(warehouse.PK, "TESt").InsertAndReturnObject(TestConnection);
			var row = new WhsRow(warehouse, "A").InsertAndReturnObject(TestConnection);
			var location = new WhsLocation(row.PK, area.PK, area.PK).InsertAndReturnObject(TestConnection);
			var client = TestWhsDataSetupHelper.GetOrgHeader();
			var product = new OrgSupplierPart("P1").InsertAndReturnObject(TestConnection);

			var sql = new SqlQueryBuilder();
			var receive = new WhsDocket(client, warehouse.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = DateTimeOffset.Now }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 100m, location.PK) { WE_StockOnHand = 90m }.AppendInsertAndReturnObject(sql);

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

			var transferPickLine = new WhsPickLine(receiveLine, transferLine, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_PickedDateTime = DateTimeOffset.Now }.AppendInsertAndReturnObject(sql);
			var orderPickLine = new WhsPickLine(receiveLine, orderLine, 10m)
			{
				WZ_GS_NKAssignedTo = "~BP",
				WZ_SystemLastEditTimeUtc = null,
				WZ_SystemLastEditUser = string.Empty
			}.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTrigger("TG_WhsPickLine_AuditDetailsAreNotMissing_Insert", WhsPickLineSchema.Constants.TableName))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			var transformation = GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			// simulate the WZ_WE_InventoryLine being set when outbound transfer creator kicks in
			WhsPickLine.UpdateWhere(orderPickLine.PK).Set(pl => pl.WZ_WE_InventoryLine, transferLine).Set(pl => pl.WZ_WE_OriginalPickedInventoryLine, receiveLine).Post(TestConnection);

			WhsPickLine.AssertFromDB(TestConnection, transferPickLine.PK)
				.ExpectEquals("Original Order Line should be updated from Trigger.", pl => pl.WZ_WE_OriginalOrderLine, orderLine)
				.ExpectNull("Original Picked Inventory Line should not be touched.", pl => pl.WZ_WE_OriginalPickedInventoryLine)
				.ExpectNotEquals("Last Edit Time should have been set.", pl => pl.WZ_SystemLastEditTimeUtc, null)
				.ExpectEquals("Last Edit User should have been set.", pl => pl.WZ_SystemLastEditUser, "~BP")
				.VerifyAll();
		}

		public void TestWZ_WE_OriginalOrderLineColumnIsCreatedIfNotExist()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var area = new WhsArea(warehouse.PK, "TESt").InsertAndReturnObject(TestConnection);
			var row = new WhsRow(warehouse, "A").InsertAndReturnObject(TestConnection);
			var location = new WhsLocation(row.PK, area.PK, area.PK).InsertAndReturnObject(TestConnection);
			var client = TestWhsDataSetupHelper.GetOrgHeader();
			var product = new OrgSupplierPart("P1").InsertAndReturnObject(TestConnection);

			var sql = new SqlQueryBuilder();
			var receive = new WhsDocket(client, warehouse.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = DateTimeOffset.Now }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 100m, location.PK) { WE_StockOnHand = 90m }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(warehouse, "P1", "PIS").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client, warehouse.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 10m).AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client, warehouse.PK, "TFR", "TFR", "ENT", "T1") { WD_WP_ParentPickForTransfer = pick }.AppendInsertAndReturnObject(sql);
			var transferLine = new WhsDocketLine(transfer, product.PK, 10m, warehouse.WW_DefaultOutboundDockDoor.FK)
			{
				WE_DocketLineStatus = "HFT",
				WE_WL_TransferFrom = location.PK,
				WE_OriginalInventoryStatus = "INT",
				WE_CurrentInventoryStatus = "INT",
				WE_StockOnHand = 10m
			}.AppendInsertAndReturnObject(sql);

			var transferPickLine = new WhsPickLine(receiveLine, transferLine, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_PickedDateTime = DateTimeOffset.Now }.AppendInsertAndReturnObject(sql);
			var orderLinePickLine = new WhsPickLine(transferLine, orderLine, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_WE_OriginalPickedInventoryLine = receiveLine }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			new DbColumnDependencyRemover(WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.WZ_WE_OriginalOrderLine).DropRelateObjects(TestConnection);
			TestConnection.ExecuteNonQuery("ALTER TABLE WhsPickLine DROP COLUMN WZ_WE_OriginalOrderLine");

			var transformation = GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("OriginalOrderLine Column should be added.", true,
				DbObjectCreator.ColumnExists(TestConnection, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.WZ_WE_OriginalOrderLine));

			WhsPickLine.AssertFromDB(TestConnection, transferPickLine.PK)
				.ExpectEquals("Original Order Line should be updated from Transform.", pl => pl.WZ_WE_OriginalOrderLine, orderLine)
				.ExpectNull("Original Picked Inventory Line should not be touched.", pl => pl.WZ_WE_OriginalPickedInventoryLine)
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, orderLinePickLine.PK)
				.ExpectNull("Order Line Pick Line should be untouched.", pl => pl.WZ_WE_OriginalOrderLine)
				.VerifyAll();
		}

		public void TestOnePickLineGetsTransformed()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var area = new WhsArea(warehouse.PK, "TESt").InsertAndReturnObject(TestConnection);
			var row = new WhsRow(warehouse, "A").InsertAndReturnObject(TestConnection);
			var location = new WhsLocation(row.PK, area.PK, area.PK).InsertAndReturnObject(TestConnection);
			var client = TestWhsDataSetupHelper.GetOrgHeader();
			var product = new OrgSupplierPart("P1").InsertAndReturnObject(TestConnection);

			var sql = new SqlQueryBuilder();
			var receive = new WhsDocket(client, warehouse.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = DateTimeOffset.Now }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 100m, location.PK) { WE_StockOnHand = 90m }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(warehouse, "P1", "PIS").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client, warehouse.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 10m).AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client, warehouse.PK, "TFR", "TFR", "ENT", "T1") { WD_WP_ParentPickForTransfer = pick }.AppendInsertAndReturnObject(sql);
			var transferLine = new WhsDocketLine(transfer, product.PK, 10m, warehouse.WW_DefaultOutboundDockDoor.FK)
			{
				WE_DocketLineStatus = "HFT",
				WE_WL_TransferFrom = location.PK,
				WE_OriginalInventoryStatus = "INT",
				WE_CurrentInventoryStatus = "INT",
				WE_StockOnHand = 10m
			}.AppendInsertAndReturnObject(sql);

			var transferPickLine = new WhsPickLine(receiveLine, transferLine, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_PickedDateTime = DateTimeOffset.Now }.AppendInsertAndReturnObject(sql);
			var orderLinePickLine = new WhsPickLine(transferLine, orderLine, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_WE_OriginalPickedInventoryLine = receiveLine }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertNull("Nothing has run yet.", ExtProperty.Database.Select(TestConnection, LastProcessedChunkPKName));

			var transformation = GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			WhsPickLine.AssertFromDB(TestConnection, transferPickLine.PK)
				.ExpectEquals("Original Order Line should be updated from Transform.", pl => pl.WZ_WE_OriginalOrderLine, orderLine)
				.ExpectNull("Original Picked Inventory Line should not be touched.", pl => pl.WZ_WE_OriginalPickedInventoryLine)
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, orderLinePickLine.PK)
				.ExpectNull("Order Line Pick Line should be untouched.", pl => pl.WZ_WE_OriginalOrderLine)
				.VerifyAll();
		}

		public void TestTwoPickLinesGetTransformed()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var area = new WhsArea(warehouse.PK, "TESt").InsertAndReturnObject(TestConnection);
			var row = new WhsRow(warehouse, "A").InsertAndReturnObject(TestConnection);
			var location = new WhsLocation(row.PK, area.PK, area.PK).InsertAndReturnObject(TestConnection);
			var consolidationRow = new WhsRow(warehouse, "CON").InsertAndReturnObject(TestConnection);
			var consolidationLocation = new WhsLocation(consolidationRow.PK, area.PK, area.PK).InsertAndReturnObject(TestConnection);
			var client = TestWhsDataSetupHelper.GetOrgHeader();
			var product = new OrgSupplierPart("P1").InsertAndReturnObject(TestConnection);

			var sql = new SqlQueryBuilder();
			var receive = new WhsDocket(client, warehouse.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = DateTimeOffset.Now }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 100m, location.PK) { WE_StockOnHand = 90m }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(warehouse, "P1", "PIS").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client, warehouse.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 10m).AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client, warehouse.PK, "TFR", "TFR", "ENT", "T1") { WD_WP_ParentPickForTransfer = pick }.AppendInsertAndReturnObject(sql);
			var transferLineToConsolidation = new WhsDocketLine(transfer, product.PK, 10m, consolidationLocation.PK)
			{
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = location.PK,
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "RTP",
				WE_StockOnHand = 0m,
				WE_FinalisedDate = DateTimeOffset.Now,
				WE_PutawayTime = DateTimeOffset.Now,
				WE_GS_NKPutawayBy = "~BP"
			}.AppendInsertAndReturnObject(sql);
			var transferLine = new WhsDocketLine(transfer, product.PK, 10m, warehouse.WW_DefaultOutboundDockDoor.FK)
			{
				WE_DocketLineStatus = "HFT",
				WE_WL_TransferFrom = consolidationLocation.PK,
				WE_OriginalInventoryStatus = "INT",
				WE_CurrentInventoryStatus = "INT",
				WE_StockOnHand = 10m
			}.AppendInsertAndReturnObject(sql);

			var consolidationPickLine = new WhsPickLine(receiveLine, transferLineToConsolidation, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_PickedDateTime = DateTimeOffset.Now }.AppendInsertAndReturnObject(sql);
			var transferPickLine = new WhsPickLine(transferLineToConsolidation, transferLine, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_PickedDateTime = DateTimeOffset.Now }.AppendInsertAndReturnObject(sql);
			var orderLinePickLine = new WhsPickLine(transferLine, orderLine, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_WE_OriginalPickedInventoryLine = receiveLine }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			var transformation = GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			WhsPickLine.AssertFromDB(TestConnection, consolidationPickLine.PK)
				.ExpectEquals("Original Order Line should be updated from Transform.", pl => pl.WZ_WE_OriginalOrderLine, orderLine)
				.ExpectNull("Original Picked Inventory Line should not be touched.", pl => pl.WZ_WE_OriginalPickedInventoryLine)
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, transferPickLine.PK)
				.ExpectEquals("Original Order Line should be updated from Transform.", pl => pl.WZ_WE_OriginalOrderLine, orderLine)
				.ExpectNull("Original Picked Inventory Line should not be touched.", pl => pl.WZ_WE_OriginalPickedInventoryLine)
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, orderLinePickLine.PK)
				.ExpectNull("Order Line Pick Line should be untouched.", pl => pl.WZ_WE_OriginalOrderLine)
				.VerifyAll();
		}

		public void TestThreePickLinesGetTransformed()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var area = new WhsArea(warehouse.PK, "TESt").InsertAndReturnObject(TestConnection);
			var row = new WhsRow(warehouse, "A").InsertAndReturnObject(TestConnection);
			var location = new WhsLocation(row.PK, area.PK, area.PK).InsertAndReturnObject(TestConnection);
			var packingStationRow = new WhsRow(warehouse, "PST").InsertAndReturnObject(TestConnection);
			var consolidationRow = new WhsRow(warehouse, "CON").InsertAndReturnObject(TestConnection);
			var packingStationLocation = new WhsLocation(packingStationRow.PK, area.PK, area.PK).InsertAndReturnObject(TestConnection);
			var consolidationLocation = new WhsLocation(consolidationRow.PK, area.PK, area.PK).InsertAndReturnObject(TestConnection);
			var client = TestWhsDataSetupHelper.GetOrgHeader();
			var product = new OrgSupplierPart("P1").InsertAndReturnObject(TestConnection);

			var sql = new SqlQueryBuilder();
			var receive = new WhsDocket(client, warehouse.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = DateTimeOffset.Now }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 100m, location.PK) { WE_StockOnHand = 90m }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(warehouse, "P1", "PIS").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client, warehouse.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 10m).AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client, warehouse.PK, "TFR", "TFR", "ENT", "T1") { WD_WP_ParentPickForTransfer = pick }.AppendInsertAndReturnObject(sql);
			var transferLineToPackingStation = new WhsDocketLine(transfer, product.PK, 10m, packingStationLocation.PK)
			{
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = location.PK,
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "RTP",
				WE_StockOnHand = 0m,
				WE_FinalisedDate = DateTimeOffset.Now,
				WE_PutawayTime = DateTimeOffset.Now,
				WE_GS_NKPutawayBy = "~BP"
			}.AppendInsertAndReturnObject(sql);
			var transferLineToConsolidation = new WhsDocketLine(transfer, product.PK, 10m, consolidationLocation.PK)
			{
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = packingStationLocation.PK,
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "RTP",
				WE_StockOnHand = 0m,
				WE_FinalisedDate = DateTimeOffset.Now,
				WE_PutawayTime = DateTimeOffset.Now,
				WE_GS_NKPutawayBy = "~BP"
			}.AppendInsertAndReturnObject(sql);
			var transferLine = new WhsDocketLine(transfer, product.PK, 10m, warehouse.WW_DefaultOutboundDockDoor.FK)
			{
				WE_DocketLineStatus = "HFT",
				WE_WL_TransferFrom = consolidationLocation.PK,
				WE_OriginalInventoryStatus = "INT",
				WE_CurrentInventoryStatus = "INT",
				WE_StockOnHand = 10m
			}.AppendInsertAndReturnObject(sql);

			var packingPickLine = new WhsPickLine(receiveLine, transferLineToPackingStation, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_PickedDateTime = DateTimeOffset.Now }.AppendInsertAndReturnObject(sql);
			var consolidationPickLine = new WhsPickLine(transferLineToPackingStation, transferLineToConsolidation, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_PickedDateTime = DateTimeOffset.Now }.AppendInsertAndReturnObject(sql);
			var transferPickLine = new WhsPickLine(transferLineToConsolidation, transferLine, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_PickedDateTime = DateTimeOffset.Now }.AppendInsertAndReturnObject(sql);
			var orderLinePickLine = new WhsPickLine(transferLine, orderLine, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_WE_OriginalPickedInventoryLine = receiveLine }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			var transformation = GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			WhsPickLine.AssertFromDB(TestConnection, packingPickLine.PK)
				.ExpectEquals("Original Order Line should be updated from Transform.", pl => pl.WZ_WE_OriginalOrderLine, orderLine)
				.ExpectNull("Original Picked Inventory Line should not be touched.", pl => pl.WZ_WE_OriginalPickedInventoryLine)
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, consolidationPickLine.PK)
				.ExpectEquals("Original Order Line should be updated from Transform.", pl => pl.WZ_WE_OriginalOrderLine, orderLine)
				.ExpectNull("Original Picked Inventory Line should not be touched.", pl => pl.WZ_WE_OriginalPickedInventoryLine)
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, transferPickLine.PK)
				.ExpectEquals("Original Order Line should be updated from Transform.", pl => pl.WZ_WE_OriginalOrderLine, orderLine)
				.ExpectNull("Original Picked Inventory Line should not be touched.", pl => pl.WZ_WE_OriginalPickedInventoryLine)
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, orderLinePickLine.PK)
				.ExpectNull("Order Line Pick Line should be untouched.", pl => pl.WZ_WE_OriginalOrderLine)
				.VerifyAll();
		}

		public void TestTransformResumesFromLastPK()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var area = new WhsArea(warehouse.PK, "TESt").InsertAndReturnObject(TestConnection);
			var row = new WhsRow(warehouse, "A").InsertAndReturnObject(TestConnection);
			var location = new WhsLocation(row.PK, area.PK, area.PK).InsertAndReturnObject(TestConnection);
			var client = TestWhsDataSetupHelper.GetOrgHeader();
			var product = new OrgSupplierPart("P1").InsertAndReturnObject(TestConnection);

			var sql = new SqlQueryBuilder();
			var receive = new WhsDocket(client, warehouse.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = DateTimeOffset.Now }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 100m, location.PK) { WE_StockOnHand = 80m }.AppendInsertAndReturnObject(sql);

			var pick1 = new WhsPick(warehouse, "P1", "PIS").AppendInsertAndReturnObject(sql);
			var pick2 = new WhsPick(warehouse, "P2", "PIS").AppendInsertAndReturnObject(sql);
			var order1 = new WhsDocket(client, warehouse.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);
			var order2 = new WhsDocket(client, warehouse.PK, "ORD", "ORD", "PIC", "O2") { WD_WP = pick2.PK }.AppendInsertAndReturnObject(sql);
			var orderLine1 = new WhsDocketLine(order1, product.PK, 10m).AppendInsertAndReturnObject(sql);
			var orderLine2 = new WhsDocketLine(order2, product.PK, 10m).AppendInsertAndReturnObject(sql);

			var transfer1 = new WhsDocket(client, warehouse.PK, "TFR", "TFR", "ENT", "T1") { WD_WP_ParentPickForTransfer = pick1 }.AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsDocketLine(transfer1, product.PK, 10m, warehouse.WW_DefaultOutboundDockDoor.FK)
			{
				WE_DocketLineStatus = "HFT",
				WE_WL_TransferFrom = location.PK,
				WE_OriginalInventoryStatus = "INT",
				WE_CurrentInventoryStatus = "INT",
				WE_StockOnHand = 10m
			}.AppendInsertAndReturnObject(sql);
			var transfer2 = new WhsDocket(client, warehouse.PK, "TFR", "TFR", "ENT", "T2") { WD_WP_ParentPickForTransfer = pick2 }.AppendInsertAndReturnObject(sql);
			var transferLine2 = new WhsDocketLine(transfer2, product.PK, 10m, warehouse.WW_DefaultOutboundDockDoor.FK)
			{
				WE_DocketLineStatus = "HFT",
				WE_WL_TransferFrom = location.PK,
				WE_OriginalInventoryStatus = "INT",
				WE_CurrentInventoryStatus = "INT",
				WE_StockOnHand = 10m
			}.AppendInsertAndReturnObject(sql);

			var transferPickLine1 = new WhsPickLine(receiveLine, transferLine1, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_PickedDateTime = DateTimeOffset.Now }.AppendInsertAndReturnObject(sql);
			var transferPickLine2 = new WhsPickLine(receiveLine, transferLine2, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_PickedDateTime = DateTimeOffset.Now }.AppendInsertAndReturnObject(sql);
			new WhsPickLine(transferLine1, orderLine1, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_WE_OriginalPickedInventoryLine = receiveLine }.AppendInsertAndReturnObject(sql);
			new WhsPickLine(transferLine2, orderLine2, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_WE_OriginalPickedInventoryLine = receiveLine }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			var minGuid = TestConnection.ExecuteScalar<Guid>($@"
SELECT MIN(Value)
FROM
(
	SELECT CAST('{transfer1.PK}' as uniqueidentifier) as Value
	UNION
	SELECT CAST('{transfer2.PK}' as uniqueidentifier)
) as Guids");

			ExtProperty.Database.Update(TestConnection, LastProcessedChunkPKName, minGuid.ToString());

			var transformation = GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var pickLineUpdated = transfer2.PK == minGuid ? transferPickLine1 : transferPickLine2;
			var pickLineNotUpdated = transfer2.PK == minGuid ? transferPickLine2 : transferPickLine1;
			var orderLineUsed = transfer2.PK == minGuid ? orderLine1 : orderLine2;

			WhsPickLine.AssertFromDB(TestConnection, pickLineUpdated.PK)
				.ExpectEquals("Original Order Line should be updated by Transform.", pl => pl.WZ_WE_OriginalOrderLine, orderLineUsed)
				.ExpectNull("Original Picked Inventory Line should not be touched.", pl => pl.WZ_WE_OriginalPickedInventoryLine)
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, pickLineNotUpdated.PK)
				.ExpectNull("Original Order Line should not be updated by Transform as it was before last processed PK.", pl => pl.WZ_WE_OriginalOrderLine)
				.VerifyAll();

			AssertNotNull("Last Processed PK is not cleared yet.", ExtProperty.Database.Select(TestConnection, LastProcessedChunkPKName));

			transformation.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			AssertNull("Last Processed PK is cleared offline.", ExtProperty.Database.Select(TestConnection, LastProcessedChunkPKName));
		}

		public void TestTransformBatching()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var area = new WhsArea(warehouse.PK, "TESt").InsertAndReturnObject(TestConnection);
			var row = new WhsRow(warehouse, "A").InsertAndReturnObject(TestConnection);
			var location = new WhsLocation(row.PK, area.PK, area.PK).InsertAndReturnObject(TestConnection);
			var client = TestWhsDataSetupHelper.GetOrgHeader();
			var product = new OrgSupplierPart("P1").InsertAndReturnObject(TestConnection);

			var sql = new SqlQueryBuilder();
			var receive = new WhsDocket(client, warehouse.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = DateTimeOffset.Now }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 2100m, location.PK) { WE_StockOnHand = 0m }.AppendInsertAndReturnObject(sql);

			const int OutboundTransfersToCreate = 2100;

			var picks = new WhsPick[OutboundTransfersToCreate];
			var orders = new WhsDocket[OutboundTransfersToCreate];
			var transfers = new WhsDocket[OutboundTransfersToCreate];
			var orderLines = new WhsDocketLine[OutboundTransfersToCreate];
			var transferLines = new WhsDocketLine[OutboundTransfersToCreate];
			var transferPickLines = new WhsPickLine[OutboundTransfersToCreate];
			var orderLinePickLines = new WhsPickLine[OutboundTransfersToCreate];

			for (var i = 0; i < OutboundTransfersToCreate; i++)
			{
				var pick = new WhsPick(warehouse, $"P{i + 1}", "PIS");
				picks[i] = pick;

				var order = new WhsDocket(client, warehouse.PK, "ORD", "ORD", "PIC", $"O{i + 1}") { WD_WP = pick.PK };
				orders[i] = order;

				var orderLine = new WhsDocketLine(order, product.PK, 1m);
				orderLines[i] = orderLine;

				var transfer = new WhsDocket(client, warehouse.PK, "TFR", "TFR", "ENT", $"T{i + 1}") { WD_WP_ParentPickForTransfer = pick };
				transfers[i] = transfer;

				var transferLine = new WhsDocketLine(transfer, product.PK, 1m, warehouse.WW_DefaultOutboundDockDoor.FK)
				{
					WE_DocketLineStatus = "HFT",
					WE_WL_TransferFrom = location.PK,
					WE_OriginalInventoryStatus = "INT",
					WE_CurrentInventoryStatus = "INT",
					WE_StockOnHand = 1m
				};
				transferLines[i] = transferLine;

				transferPickLines[i] = new WhsPickLine(receiveLine, transferLine, 1m) { WZ_GS_NKAssignedTo = "~BP", WZ_PickedDateTime = DateTimeOffset.Now };
				orderLinePickLines[i] = new WhsPickLine(transferLine, orderLine, 1m) { WZ_GS_NKAssignedTo = "~BP", WZ_WE_OriginalPickedInventoryLine = receiveLine };
			}

			sql.Append(picks.GetBulkInsertStatement());
			sql.Append(orders.GetBulkInsertStatement());
			sql.Append(transfers.GetBulkInsertStatement());
			sql.Append(orderLines.GetBulkInsertStatement());
			sql.Append(transferLines.GetBulkInsertStatement());
			sql.Append(transferPickLines.GetBulkInsertStatement());
			sql.Append(orderLinePickLines.GetBulkInsertStatement());

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			var transformation = GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			for (var i = 0; i < OutboundTransfersToCreate; i++)
			{
				var transferPickLine = transferPickLines[i];
				var orderLine = orderLines[i];

				WhsPickLine.AssertFromDB(TestConnection, transferPickLine.PK)
					.ExpectEquals("Original Order Line should be updated from Transform.", pl => pl.WZ_WE_OriginalOrderLine, orderLine)
					.ExpectNull("Original Picked Inventory Line should not be touched.", pl => pl.WZ_WE_OriginalPickedInventoryLine)
					.VerifyAll();
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestTransformWorksWithoutTransaction()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var warehouse = new WhsWarehouse("WHS").WithDockDoor(connection);
				var area = new WhsArea(warehouse.PK, "TESt").InsertAndReturnObject(connection);
				var row = new WhsRow(warehouse, "A").InsertAndReturnObject(connection);
				var location = new WhsLocation(row.PK, area.PK, area.PK).InsertAndReturnObject(connection);
				var client = TestWhsDataSetupHelper.GetOrgHeader();
				var product = new OrgSupplierPart("P1").InsertAndReturnObject(connection);

				var sql = new SqlQueryBuilder();
				var receive = new WhsDocket(client, warehouse.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = DateTimeOffset.Now }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, product.PK, 100m, location.PK) { WE_StockOnHand = 90m }.AppendInsertAndReturnObject(sql);

				var pick = new WhsPick(warehouse, "P1", "PIS").AppendInsertAndReturnObject(sql);
				var order = new WhsDocket(client, warehouse.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, product.PK, 10m).AppendInsertAndReturnObject(sql);

				var transfer = new WhsDocket(client, warehouse.PK, "TFR", "TFR", "ENT", "T1") { WD_WP_ParentPickForTransfer = pick }.AppendInsertAndReturnObject(sql);
				var transferLine = new WhsDocketLine(transfer, product.PK, 10m, warehouse.WW_DefaultOutboundDockDoor.FK)
				{
					WE_DocketLineStatus = "HFT",
					WE_WL_TransferFrom = location.PK,
					WE_OriginalInventoryStatus = "INT",
					WE_CurrentInventoryStatus = "INT",
					WE_StockOnHand = 10m
				}.AppendInsertAndReturnObject(sql);

				var transferPickLine = new WhsPickLine(receiveLine, transferLine, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_PickedDateTime = DateTimeOffset.Now }.AppendInsertAndReturnObject(sql);
				var orderLinePickLine = new WhsPickLine(transferLine, orderLine, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_WE_OriginalPickedInventoryLine = receiveLine }.AppendInsertAndReturnObject(sql);

				using (connection.BeginTransactionWithManager())
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
				{
					connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
					connection.CommitTransaction();
				}

				var transformation = GetNewTestTransformationInstance();
				transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

				WhsPickLine.AssertFromDB(connection, transferPickLine.PK)
					.ExpectEquals("Original Order Line should be updated from Transform.", pl => pl.WZ_WE_OriginalOrderLine, orderLine)
					.ExpectNull("Original Picked Inventory Line should not be touched.", pl => pl.WZ_WE_OriginalPickedInventoryLine)
					.VerifyAll();

				WhsPickLine.AssertFromDB(TestConnection, orderLinePickLine.PK)
					.ExpectNull("Order Line Pick Line should be untouched.", pl => pl.WZ_WE_OriginalOrderLine)
					.VerifyAll();
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateWZ_WE_OriginalOrderLine();
	}
}
