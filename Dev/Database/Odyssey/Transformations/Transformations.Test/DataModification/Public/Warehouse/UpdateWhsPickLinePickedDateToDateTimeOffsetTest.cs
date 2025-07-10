using System;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Warehouse.Testing
{
	[TestedType(typeof(UpdateWhsPickLinePickedDateToDateTimeOffset))]
	class UpdateWhsPickLinePickedDateToDateTimeOffsetTest : ConvertDateTimeToDateTimeOffsetTransformTestCase
	{
		#region AssertTransformationResults

		protected override void PrepareTestData()
		{
			var sql = new SqlQueryBuilder();

			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "AUSYD" }.InsertAndReturnObject(TestConnection);

			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("PRD").AppendInsertAndReturnObject(sql);

			var now = new DateTime(2022, 06, 08, 0, 0, 0);

			var receive = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "FIN", "R1") { WD_FinalisedDate = new DateTimeOffset(now) }.AppendInsertAndReturnObject(sql);

			var receiveLine = new WhsDocketLine(receive, product.PK, 2m, location.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_PalletID = "ABC",
				WE_StockOnHand = 0m,
				WE_UnloadedTime = new DateTimeOffset(now),
				WE_GS_NKUnloadedBy = "A",
				WE_AdjustmentArrivalDate = now,
				WE_SystemCreateTimeUtc = DateTime.UtcNow,
				WE_SystemLastEditTimeUtc = DateTime.UtcNow,
				WE_SystemCreateUser = "A",
				WE_SystemLastEditUser = "A"
			}.AppendInsertAndReturnObject(sql);

			var pickWithOrder = new WhsPick(whs, "P1", "ENT").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pickWithOrder.PK }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 20m).AppendInsertAndReturnObject(sql);

			var pickline = new WhsPickLine(receiveLine, orderLine, 2m)
			{
				WZ_PickedDateTime = new DateTimeOffset(now, TimeSpan.FromHours(0)),
				WZ_GS_NKAssignedTo = "US1",
				WZ_SystemCreateTimeUtc = now,
				WZ_SystemLastEditTimeUtc = now,
				WZ_SystemCreateUser = "A",
				WZ_SystemLastEditUser = "A",
				WZ_OriginalReservedQty = 2m,
				WZ_F3_NKAllocatedPackType = "BAG"
			}
			.InsertAsDateTimeForColumn(pl => pl.WZ_PickedDateTime)
			.AppendToInsert(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals("Precondition", new DateTimeOffset(2022, 06, 08, 00, 00, 00, TimeSpan.FromHours(0)), pickline.WZ_PickedDateTime);
		}

		protected override void AssertTransformationResults()
		{
			SetupTemplateDBAndRunColumnSync();

			var pickLine = WhsPickLine.ShallowLoadFromDB(TestConnection).Single();
			AssertEquals("WZ_PickedDateTime should be correct.", new DateTimeOffset(2022, 06, 08, 00, 00, 00, TimeSpan.FromHours(10)), pickLine.WZ_PickedDateTime);
		}

		protected override void InsertDataWithThisTimeZone(string timeZoneUnloco)
		{
			var sql = new SqlQueryBuilder();

			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = timeZoneUnloco }.InsertAndReturnObject(TestConnection);

			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("PRD").AppendInsertAndReturnObject(sql);

			var now = new DateTime(2022, 06, 08, 0, 0, 0);

			var receive = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "FIN", "R1") { WD_FinalisedDate = new DateTimeOffset(now) }.AppendInsertAndReturnObject(sql);

			var receiveLine = new WhsDocketLine(receive, product.PK, 2m, location.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_PalletID = "ABC",
				WE_StockOnHand = 0m,
				WE_UnloadedTime = new DateTimeOffset(now),
				WE_GS_NKUnloadedBy = "A",
				WE_AdjustmentArrivalDate = now,
				WE_SystemCreateTimeUtc = DateTime.UtcNow,
				WE_SystemLastEditTimeUtc = DateTime.UtcNow,
				WE_SystemCreateUser = "A",
				WE_SystemLastEditUser = "A"
			}.AppendInsertAndReturnObject(sql);

			var pickWithOrder = new WhsPick(whs, "P1", "ENT").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pickWithOrder.PK }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 20m).AppendInsertAndReturnObject(sql);

			var pickline = new WhsPickLine(receiveLine, orderLine, 2m)
			{
				WZ_PickedDateTime = new DateTimeOffset(now, TimeSpan.FromHours(0)),
				WZ_GS_NKAssignedTo = "US1",
				WZ_SystemCreateTimeUtc = now,
				WZ_SystemLastEditTimeUtc = now,
				WZ_SystemCreateUser = "A",
				WZ_SystemLastEditUser = "A",
				WZ_OriginalReservedQty = 2m,
				WZ_F3_NKAllocatedPackType = "BAG"
			}
			.InsertAsDateTimeForColumn(pl => pl.WZ_PickedDateTime)
			.AppendToInsert(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}

		#endregion

		#region TestTwoBranchs

		public void TestTwoBranchs()
		{
			var now = new DateTime(2022, 06, 08, 00, 00, 00);

			var branch1 = new GlbBranch("BR1") { GB_RL_NKHomePort = "AUSYD" }.InsertAndReturnObject(TestConnection);
			var branch2 = new GlbBranch("BR2") { GB_RL_NKHomePort = "MAGIC" }.InsertAndReturnObject(TestConnection);

			var sql = new SqlQueryBuilder();

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var whs1 = new WhsWarehouse("WHS", "PRW", branch1.PK).WithDockDoor(sql);
			var whs2 = new WhsWarehouse("WH2", "PRW", branch2.PK).WithDockDoor(sql);

			var row1 = new WhsRow(whs1, "Row1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area1 = new WhsArea(whs1.PK, "Area1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row1.PK, area1.PK, area1.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var row2 = new WhsRow(whs2, "Row2") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area2 = new WhsArea(whs2.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row2.PK, area2.PK, area2.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var receive1 = new WhsDocket(client.PK, whs1.PK, "ADJ", "NEA", "FIN", "R1") { WD_FinalisedDate = new DateTimeOffset(now) }.AppendInsertAndReturnObject(sql);
			var receive2 = new WhsDocket(client.PK, whs2.PK, "ADJ", "NEA", "FIN", "R2") { WD_FinalisedDate = new DateTimeOffset(now) }.AppendInsertAndReturnObject(sql);

			var receiveLine1 = new WhsDocketLine(receive1, product.PK, 2m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_PalletID = "ABC",
				WE_StockOnHand = 0m,
				WE_UnloadedTime = new DateTimeOffset(now),
				WE_GS_NKUnloadedBy = "A",
				WE_AdjustmentArrivalDate = now,
				WE_SystemCreateTimeUtc = DateTime.UtcNow,
				WE_SystemLastEditTimeUtc = DateTime.UtcNow,
				WE_SystemCreateUser = "A",
				WE_SystemLastEditUser = "A"
			}.AppendInsertAndReturnObject(sql);

			var receiveLine2 = new WhsDocketLine(receive2, product.PK, 2m, location2.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_PalletID = "ABC",
				WE_StockOnHand = 0m,
				WE_UnloadedTime = new DateTimeOffset(now),
				WE_GS_NKUnloadedBy = "A",
				WE_AdjustmentArrivalDate = now,
				WE_SystemCreateTimeUtc = DateTime.UtcNow,
				WE_SystemLastEditTimeUtc = DateTime.UtcNow,
				WE_SystemCreateUser = "A",
				WE_SystemLastEditUser = "A"
			}.AppendInsertAndReturnObject(sql);

			var pickWithOrder1 = new WhsPick(whs1, "P1", "ENT").AppendInsertAndReturnObject(sql);
			var pickWithOrder2 = new WhsPick(whs2, "P2", "ENT").AppendInsertAndReturnObject(sql);
			var order1 = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pickWithOrder1.PK }.AppendInsertAndReturnObject(sql);
			var order2 = new WhsDocket(client.PK, whs2.PK, "ORD", "ORD", "PIC", "O2") { WD_WP = pickWithOrder2.PK }.AppendInsertAndReturnObject(sql);
			var orderLine1 = new WhsDocketLine(order1, product.PK, 20m).AppendInsertAndReturnObject(sql);
			var orderLine2 = new WhsDocketLine(order2, product.PK, 20m).AppendInsertAndReturnObject(sql);

			var pickline1 = new WhsPickLine(receiveLine1, orderLine1, 2m)
			{
				WZ_PickedDateTime = new DateTimeOffset(now, TimeSpan.FromHours(0)),
				WZ_GS_NKAssignedTo = "US1",
				WZ_SystemCreateTimeUtc = now,
				WZ_SystemLastEditTimeUtc = now,
				WZ_SystemCreateUser = "A",
				WZ_SystemLastEditUser = "A",
				WZ_OriginalReservedQty = 2m,
				WZ_F3_NKAllocatedPackType = "BAG"
			}
			.InsertAsDateTimeForColumn(pl => pl.WZ_PickedDateTime)
			.AppendToInsert(sql);

			var pickline2 = new WhsPickLine(receiveLine2, orderLine2, 2m)
			{
				WZ_PickedDateTime = new DateTimeOffset(now, TimeSpan.FromHours(0)),
				WZ_GS_NKAssignedTo = "US1",
				WZ_SystemCreateTimeUtc = now,
				WZ_SystemLastEditTimeUtc = now,
				WZ_SystemCreateUser = "A",
				WZ_SystemLastEditUser = "A",
				WZ_OriginalReservedQty = 2m,
				WZ_F3_NKAllocatedPackType = "BAG"
			}
			.InsertAsDateTimeForColumn(pl => pl.WZ_PickedDateTime)
			.AppendToInsert(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			instance.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			SetupTemplateDBAndRunColumnSync();

			WhsPickLine.AssertFromDB(TestConnection, pickline1.PK)
				.ExpectEquals("WZ_PickedDateTime not changed.", d => d.WZ_PickedDateTime, new DateTimeOffset(2022, 06, 08, 0, 0, 0, TimeSpan.FromHours(10)))
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, pickline2.PK)
				.ExpectEquals("WZ_PickedDateTime not changed.", d => d.WZ_PickedDateTime, new DateTimeOffset(2022, 06, 08, 0, 0, 0, TimeSpan.FromHours(2)))
				.VerifyAll();
		}

		#endregion

		#region TestNoNeedToUpdate

		public void TestNoNeedToUpdate()
		{
			var today = new DateTime(2022, 6, 6, 0, 0, 0);
			var sql = new SqlQueryBuilder();

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var branch1 = new GlbBranch("BR1") { GB_RL_NKHomePort = "AAAAA" }.InsertAndReturnObject(TestConnection);
			var whs1 = new WhsWarehouse("WHS", "PRW", branch1.PK).WithDockDoor(TestConnection);

			var order1 = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "ENT", "O001")
			{
				WD_BookingDate = new DateTimeOffset(2022, 6, 8, 0, 0, 0, TimeSpan.FromHours(0)),
				WD_SystemLastEditTimeUtc = DateTime.UtcNow.ToSmallDateTimeFloor(),
			}.InsertAsDateTimeForColumn(d => d.WD_BookingDate).AppendToInsert(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var instance = GetNewTestTransformationInstance();
			instance.Run();

			WhsDocket.AssertFromDB(TestConnection, order1.PK)
				.ExpectEquals("WD_BookingDate not changed.", d => d.WD_BookingDate, new DateTimeOffset(2022, 6, 8, 0, 0, 0, TimeSpan.FromHours(0)))
				.ExpectEquals("WD_SystemLastEditTimeUtc not changed.", d => d.WD_SystemLastEditTimeUtc, order1.WD_SystemLastEditTimeUtc)
				.ExpectEquals("WD_SystemLastEditUser not changed.", d => d.WD_SystemLastEditUser, "A")
				.VerifyAll();
		}

		#endregion

		#region TestUserDescription

		public void TestUserDescription()
		{
			AssertEquals("Update WhsPickLine PickedDate Time from DateTime to DateTimeOffset.", GetNewTestTransformationInstance().UserDescription);
		}

		#endregion

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();
			InsertRefUNLOCOUtcOffset();
		}

		void InsertRefUNLOCOUtcOffset()
		{
			TestConnection.ExecuteNonQuery($@"
IF EXISTS(SELECT NULL FROM RefDatabase_RefUNLOCOUtcOffset WHERE RLO_RL_NKCode = 'AUSYD' AND RLO_StartTimeUtc <= '2022-01-01 00:00:00' AND RLO_EndTimeUtc >= '2023-01-01 00:00:00')
BEGIN
	UPDATE dbo.RefDatabase_RefUNLOCOUtcOffset
		SET RLO_OffsetMinutesFromUtc = 600
		WHERE RLO_RL_NKCode = 'AUSYD' AND RLO_StartTimeUtc <= '2022-06-01 00:00:00' AND RLO_EndTimeUtc >= '2023-07-01 00:00:00'
END
ELSE
BEGIN
	INSERT INTO dbo.RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES
	(newid(), 'AUSYD', '2022-06-01 00:00:00', '2023-07-01 00:00:00', 600)
END

INSERT INTO dbo.RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES
(newid(), 'MAGIC', '2022-01-01 12:00:00', '2022-06-06 12:00:00', 60)

INSERT INTO dbo.RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES
(newid(), 'MAGIC', '2022-06-06 12:00:00', '2023-01-01 12:00:00', 120)
");
		}

		#endregion

		#region Implementation

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateWhsPickLinePickedDateToDateTimeOffset();

		protected override SchemaDateTimeOffsetColumn GetDateTimeOffsetColumnToConvert() => WhsPickLineSchema.WZ_PickedDateTime;

		protected override SqlDbType DateTimeTypeBeforeConversion => SqlDbType.DateTime;

		protected override bool TableHasAutoVersion => true;

		protected override string GetTimeZoneSubQuery(string timeZoneColumnName)
		{
			return $@"SELECT GB_RL_NKHomePort as {timeZoneColumnName}
	FROM
		dbo.WhsDocketLine
		JOIN dbo.WhsDocket ON WE_WD = WD_PK
		JOIN dbo.WhsWarehouse ON WD_WW_Whs = WW_PK
		JOIN dbo.GlbBranch ON WW_GB_RelatedCompanyBranch = GB_PK
	WHERE
		WZ_WE_TransactionLine = WE_PK";
		}

		protected override IndexInfo GetSupportingIndex(TransformationIndexProvider indexProvider)
		{
			return indexProvider.New(WhsPickLineSchema.Instance)
				.Key(WhsPickLineSchema.Constants.PK)
				.Include(WhsPickLineSchema.Constants.WZ_PickedDateTime, "WZ_AutoVersion")
				.GetInfo();
		}

		#endregion
	}
}
