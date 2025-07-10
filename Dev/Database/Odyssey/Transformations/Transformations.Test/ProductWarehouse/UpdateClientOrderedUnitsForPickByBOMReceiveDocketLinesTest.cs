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
	[TestedType(typeof(UpdateClientOrderedUnitsForPickByBOMReceiveDocketLines))]
	public class UpdateClientOrderedUnitsForPickByBOMReceiveDocketLinesTest : DataTransformationTestCase
	{
		#region Basic Test

		protected override void PrepareTestData()
		{
			PrepareTestDataForFinalisedAndUnFinalisedDocketLines(false, 10m);
		}

		protected override void AssertPreConditions()
		{
			AssertEquals("There is 1 finalised INW line with WE_ClientOrderedUnits = 0 before transformation.", 1, WhsDocketLine.CountInDB(TestConnection, d => d.WE_ClientOrderedUnits == 0 && d.WE_DocketLineType == "INW" && d.WE_DocketLineStatus == "FIN" && d.WE_TransactionQuantity > 0));
			AssertEquals("There is 1 unfinalised INW line with WE_ClientOrderedUnits = 0 before transformation.", 1, WhsDocketLine.CountInDB(TestConnection, d => d.WE_ClientOrderedUnits == 0 && d.WE_DocketLineType == "INW" && d.WE_DocketLineStatus == "" && d.WE_TransactionQuantity > 0));
			AssertEquals("There is 1 ORD line with WE_ClientOrderedUnits = 0 before transformation.", 1, WhsDocketLine.CountInDB(TestConnection, d => d.WE_ClientOrderedUnits == 0 && d.WE_DocketLineType == "ORD" && d.WE_TransactionQuantity > 0));
			AssertEquals("There is 1 TFR line with WE_ClientOrderedUnits = 0 before transformation.", 1, WhsDocketLine.CountInDB(TestConnection, d => d.WE_ClientOrderedUnits == 0 && d.WE_DocketLineType == "TFR" && d.WE_TransactionQuantity > 0));
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals("WE_ClientOrderedUnits can be updated to WE_TransactionQuantity.", 0, WhsDocketLine.CountInDB(TestConnection, d => d.WE_ClientOrderedUnits == 0 && d.WE_DocketLineType == "INW" && d.WE_DocketLineStatus == "FIN" && d.WE_TransactionQuantity > 0));
			AssertEquals("WE_ClientOrderedUnits can be updated to WE_TransactionQuantity.", 0, WhsDocketLine.CountInDB(TestConnection, d => d.WE_ClientOrderedUnits == 0 && d.WE_DocketLineType == "INW" && d.WE_DocketLineStatus == "" && d.WE_TransactionQuantity > 0));

			WhsDocketLine.AssertFromDB(TestConnection, receiveLine1.PK)
				.ExpectEquals("WE_ClientOrderedUnits can be updated to WE_TransactionQuantity.", l => l.WE_ClientOrderedUnits, receiveLine1.WE_TransactionQuantity)
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, receiveLine2.PK)
				.ExpectEquals("WE_ClientOrderedUnits can be updated to WE_TransactionQuantity.", l => l.WE_ClientOrderedUnits, receiveLine2.WE_TransactionQuantity)
				.VerifyAll();

			AssertEquals("Not updated when WE_DocketLineType != 'INW'.", 1, WhsDocketLine.CountInDB(TestConnection, d => d.WE_ClientOrderedUnits == 0 && d.WE_DocketLineType == "ORD" && d.WE_TransactionQuantity > 0));
			AssertEquals("Not updated when WE_DocketLineType != 'INW'.", 1, WhsDocketLine.CountInDB(TestConnection, d => d.WE_ClientOrderedUnits == 0 && d.WE_DocketLineType == "TFR" && d.WE_TransactionQuantity > 0));
		}

		#endregion

		#region TestNotUpdateDocketLinesWithNoParentPickForReceive

		public void TestNotUpdateDocketLinesWithNoParentPickForReceive()
		{
			PrepareTestDataForFinalisedAndUnFinalisedDocketLines(true, 10m);

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			WhsDocketLine.AssertFromDB(TestConnection, receiveLine1.PK)
				.ExpectEquals("WE_ClientOrderedUnits can't be updated when ParentPickForReceive is null.", l => l.WE_ClientOrderedUnits, 0m)
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, receiveLine2.PK)
				.ExpectEquals("WE_ClientOrderedUnits can't be updated when ParentPickForReceive is null.", l => l.WE_ClientOrderedUnits, 0m)
				.VerifyAll();
		}

		#endregion

		#region TestNotUpdateDocketLinesWhenTransactionQuantityLessOrEqualToZero

		public void TestNotUpdateDocketLinesWhenTransactionQuantityNotBiggerThanZero()
		{
			PrepareTestDataForFinalisedAndUnFinalisedDocketLines(false, 0m);

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			WhsDocketLine.AssertFromDB(TestConnection, receiveLine1.PK)
				.ExpectEquals("WE_ClientOrderedUnits can't be updated when TransactionQuantity is 0.", l => l.WE_ClientOrderedUnits, 0m)
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, receiveLine2.PK)
				.ExpectEquals("WE_ClientOrderedUnits can't be updated when TransactionQuantity is 0.", l => l.WE_ClientOrderedUnits, 0m)
				.VerifyAll();
		}

		#endregion

		#region TestOnlinePhase_RespondToCancellationToken

		public void TestOnlinePhase_RespondToCancellationToken()
		{
			var numberOfRecords = 1234;
			var date = new DateTime(2018, 1, 1);
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var picks = new WhsPick[numberOfRecords];
			var receives = new WhsDocket[numberOfRecords];
			var receiveLines = new WhsDocketLine[numberOfRecords];

			for (var i = 0; i < numberOfRecords; i++)
			{
				picks[i] = new WhsPick(whs, $"PICK{i}", "FIN")
				{
					WP_FinalizedDateUtc = date.AddSeconds(-i),
					WP_SystemCreateTimeUtc = date,
					WP_GS_NKFinalizedBy = "A",
					WP_SystemLastEditUser = "XYZ",
				};
				receives[i] = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", $"R{i}") { WD_FinalisedDate = date.AddSeconds(-i), WD_WP_ParentPickForReceive = picks[i] };
				receiveLines[i] = new WhsDocketLine(receives[i], component1.PK, 2m, location1.PK)
				{
					WE_OriginalInventoryStatus = "AVL",
					WE_CurrentInventoryStatus = "AVL",
					WE_StockOnHand = 0m,
					WE_FinalisedDate = date.AddSeconds(-i),
					WE_ClientOrderedUnits = 0
				};
			}

			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect))
			{
				TestConnection.ExecuteNonQuery(WhsPick.GetBulkInsertStatement(picks));
				TestConnection.ExecuteNonQuery(WhsDocket.GetBulkInsertStatement(receives));
				TestConnection.ExecuteNonQuery(WhsDocketLine.GetBulkInsertStatement(receiveLines));
			}

			AssertExceptionThrown<OperationCanceledException>(() => GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, new CancellationToken(true)));

			AssertEquals("First batch completed.", 1000, WhsDocketLine.CountInDB(TestConnection, d => d.WE_DocketLineType == "INW" && d.WE_DocketLineStatus == "FIN" && d.WE_TransactionQuantity == d.WE_ClientOrderedUnits));

			AssertNoExceptionThrown(() => GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, new CancellationToken(false)));

			AssertEquals("Second batch completed.", 1234, WhsDocketLine.CountInDB(TestConnection, d => d.WE_DocketLineType == "INW" && d.WE_DocketLineStatus == "FIN" && d.WE_TransactionQuantity == d.WE_ClientOrderedUnits));
		}

		#endregion

		#region TestNoValidDataToProcess

		public void TestNoValidDataToProcess()
		{
			AssertEquals("Precondition", 0, WhsDocketLine.CountInDB(TestConnection));

			AssertNoExceptionThrown(() => GetNewTestTransformationInstance().Run());
			AssertNoExceptionThrown(() => GetNewTestTransformationInstance().Run());

			AssertEquals(0, WhsDocketLine.CountInDB(TestConnection));
		}

		#endregion

		void PrepareTestDataForFinalisedAndUnFinalisedDocketLines(bool parentPickForReceiveIsNull, decimal transactionQuantity)
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			var component2 = new OrgSupplierPart("P3").AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 1, OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var bom2 = new OrgPartBOM(kit, component2) { OE_ComponentQty = 2, OE_IsValid = true }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "P1", "NEW") { WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0) }.AppendInsertAndReturnObject(sql);

			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today, WD_WP_ParentPickForReceive = parentPickForReceiveIsNull ? null : pick }.AppendInsertAndReturnObject(sql);
			receiveLine1 = new WhsDocketLine(receive1, kit.PK, transactionQuantity, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today,
				WE_ClientOrderedUnits = 0,
			}.AppendInsertAndReturnObject(sql);

			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O1")
			{
				WD_WP = pick.PK,
				WD_FinalisedDate = today,
				WD_GS_NKFinalizedBy = "E",
			}.AppendInsertAndReturnObject(sql);
			var orderLine1 = new WhsDocketLine(order, kit.PK, 10m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
			}.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "T1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsDocketLine(transfer, kit.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 10m,
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = location1.PK,
			}.AppendInsertAndReturnObject(sql);

			var pickLine1 = new WhsPickLine(receiveLine1, transferLine1, 10m) { WZ_PickedDateTime = today.AddHours(1), WZ_GS_NKAssignedTo = "E" }.AppendInsertAndReturnObject(sql);
			var pickLine3 = new WhsPickLine(transferLine1, orderLine1, 10m).AppendInsertAndReturnObject(sql);

			var pick2 = new WhsPick(whs, "P2", "NEW") { WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0) }.AppendInsertAndReturnObject(sql);
			var receive2 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R2") { WD_SystemCreateTimeUtc = new DateTime(2014, 1, 1), WD_WP_ParentPickForReceive = parentPickForReceiveIsNull ? null : pick2 }.AppendInsertAndReturnObject(sql);
			receiveLine2 = new WhsDocketLine(receive2, kit.PK, transactionQuantity, location1.PK)
			{
				WE_OriginalInventoryStatus = "REC",
				WE_CurrentInventoryStatus = "REC",
				WE_StockOnHand = transactionQuantity,
				WE_UnloadedTime = DateTime.Now,
				WE_GS_NKUnloadedBy = "AAA",
			}.AppendInsertAndReturnObject(sql);

			SaveToDB(sql);
		}

		#region SaveToDB

		void SaveToDB(SqlQueryBuilder sql)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}

		#endregion

		#region Implementation

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateClientOrderedUnitsForPickByBOMReceiveDocketLines();
		}

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Update ClientOrderedUnits To WE_TransactionQuantity on Pick by BOM Receive Lines_1] ON [dbo].[WhsDocketLine] ([WE_DocketLineStatus], [WE_ClientOrderedUnits], [WE_TransactionQuantity], [WE_DocketLineType]) WHERE ([WE_DocketLineType]='INW') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		#endregion

		WhsDocketLine receiveLine1;
		WhsDocketLine receiveLine2;
	}
}
