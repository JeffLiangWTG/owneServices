using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.DataModification.Public.Warehouse
{
	[TestedType(typeof(PopulateWhsDocketUnloadCompletedTime))]
	class PopulateWhsDocketUnloadCompletedTimeTest : DataTransformationTestCase
	{
		const string TriggerName = "TG_WhsDocket_SetUnloadCompletedTime";

		public void TestUnloadCompletedTimeColumnIsCreated()
		{
			var column = WhsDocketSchema.WD_UnloadCompletedTime;
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("Column WD_UnloadCompletedTime should get created.", true, DbObjectCreator.ColumnExists(TestConnection, WhsDocketSchema.Constants.TableName, WhsDocketSchema.Constants.WD_UnloadCompletedTime));
			AssertEquals("Column WD_UnloadCompletedTime should be created with correct data type.", "datetimeoffset", DbObjectCreator.GetColumnType(TestConnection, column.TableName, WhsDocketSchema.Constants.WD_UnloadCompletedTime));
		}

		public void TestTriggerIsCreatedCorrectly()
		{
			var transform = GetNewTestTransformationInstance();
			var expectedTriggerDefinition = @"
CREATE TRIGGER dbo.TG_WhsDocket_SetUnloadCompletedTime
	ON dbo.WhsDocket
	FOR INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	IF UPDATE (WD_FinalisedDate)
	BEGIN
		UPDATE wd
		SET
			wd.WD_UnloadCompletedTime = i.WD_FinalisedDate,
			wd.WD_SystemLastEditUser = i.WD_SystemLastEditUser,
			wd.WD_SystemLastEditTimeUtc = i.WD_SystemLastEditTimeUtc
		FROM
			WhsDocket wd
			JOIN inserted i ON wd.WD_PK = i.WD_PK
		WHERE
			i.WD_DocketType = 'INW' AND
			i.WD_FinalisedDate IS NOT NULL AND
			wd.WD_UnloadCompletedTime IS NULL
	END
END";

			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, WhsDocketSchema.Constants.TableName, TriggerName));
			AssertEquals("Trigger Definition is correct", expectedTriggerDefinition, DbObjectCreator.GetTriggerDefinition(TestConnection, TriggerName));

			PrepareBatch();

			AssertEquals("Precondition: 3200 WhsDockets created for test", 3200, WhsDocket.CountInDB(Db.Connection));

			var countOfUpdatedRows = WhsDocket.CountInDB(Db.Connection, p => p.WD_UnloadCompletedTime == p.WD_FinalisedDate);
			AssertEquals("Trigger should populate all valid rows.", 3124, countOfUpdatedRows);

			var countOfAlreadySetRows = WhsDocket.CountInDB(Db.Connection, p => p.WD_UnloadCompletedTime < p.WD_FinalisedDate);
			AssertEquals("Trigger should skip rows with already set WD_UnloadCompletedTime value.", 12, countOfAlreadySetRows);

			var countOfSkippedRows = WhsDocket.CountInDB(Db.Connection, p => p.WD_UnloadCompletedTime == null && p.WD_FinalisedDate == null);
			AssertEquals("Trigger should skip unfinalised rows.", 64, countOfSkippedRows);
		}

		public void TestNoneDocketRecord()
		{
			var countOfAllRows = WhsDocket.CountInDB(Db.Connection);
			AssertEquals("Count of all rows.", 0, countOfAllRows);
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals("Count of all rows.", 0, countOfAllRows);
		}

		public void TestTransform_Batching()
		{
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, WhsDocketSchema.Constants.TableName, WhsDocketSchema.Constants.WD_UnloadCompletedTime, "DATETIMEOFFSET(0)");

			PrepareBatch();

			AssertEquals("Precondition: 3200 WhsDockets created for test", 3200, WhsDocket.CountInDB(Db.Connection));

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var countOfUpdatedRows = WhsDocket.CountInDB(Db.Connection, p => p.WD_UnloadCompletedTime == p.WD_FinalisedDate);
			AssertEquals("Trigger should populate all valid rows.", 3124, countOfUpdatedRows);

			var countOfAlreadySetRows = WhsDocket.CountInDB(Db.Connection, p => p.WD_UnloadCompletedTime < p.WD_FinalisedDate);
			AssertEquals("Trigger should skip rows with already set WD_UnloadCompletedTime value.", 12, countOfAlreadySetRows);

			var countOfSkippedRows = WhsDocket.CountInDB(Db.Connection, p => p.WD_UnloadCompletedTime == null && p.WD_FinalisedDate == null);
			AssertEquals("Trigger should skip unfinalised rows.", 64, countOfSkippedRows);
		}

		protected override void AssertTransformationResults()
		{
			var column = WhsDocketSchema.WD_UnloadCompletedTime;
			AssertEquals("Column WD_UnloadCompletedTime should get created.", true, DbObjectCreator.ColumnExists(TestConnection, column.TableName, column.Name));

			var receiveFinalised = WhsDocket.ShallowLoadFromDB(TestConnection, p1 => p1.WD_DocketID == "R1").Single();
			receiveFinalised
				.BuildAssertion(TestConnection)
				.ExpectEquals("WD_UnloadCompletedTime has been set to WD_FinalisedDate.", p1 => p1.WD_UnloadCompletedTime, receiveFinalised.WD_FinalisedDate)
				.VerifyAll();

			var receiveUnfinalised = WhsDocket.ShallowLoadFromDB(TestConnection, p1 => p1.WD_DocketID == "R2").Single();
			receiveUnfinalised
				.BuildAssertion(TestConnection)
				.ExpectEquals("WD_UnloadCompletedTime has NOT been set on unfinalised receive.", p2 => p2.WD_UnloadCompletedTime, null)
				.VerifyAll();

			var transfer = WhsDocket.ShallowLoadFromDB(TestConnection, p1 => p1.WD_DocketID == "T1").Single();
			transfer
				.BuildAssertion(TestConnection)
				.ExpectEquals("WD_UnloadCompletedTime has NOT been set on transfer.", p2 => p2.WD_UnloadCompletedTime, null)
				.VerifyAll();

			var adjustment = WhsDocket.ShallowLoadFromDB(TestConnection, p1 => p1.WD_DocketID == "A1").Single();
			adjustment
				.BuildAssertion(TestConnection)
				.ExpectEquals("WD_UnloadCompletedTime has NOT been set on adjustment.", p2 => p2.WD_UnloadCompletedTime, null)
				.VerifyAll();

			var order = WhsDocket.ShallowLoadFromDB(TestConnection, p1 => p1.WD_DocketID == "O1").Single();
			order
				.BuildAssertion(TestConnection)
				.ExpectEquals("WD_UnloadCompletedTime has NOT been set on order.", p2 => p2.WD_UnloadCompletedTime, null)
				.VerifyAll();

			var workOrder = WhsDocket.ShallowLoadFromDB(TestConnection, p1 => p1.WD_DocketID == "W1").Single();
			workOrder
				.BuildAssertion(TestConnection)
				.ExpectEquals("WD_UnloadCompletedTime has NOT been set on workOrder.", p2 => p2.WD_UnloadCompletedTime, null)
				.VerifyAll();
		}

		protected override void PrepareTestData()
		{
			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "Row1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			// Finalised Receive
			var receiveFinalised = new WhsDocketOld_V01(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "Bob" }.AppendInsertAndReturnObject(sql);
			new WhsDocketLine(receiveFinalised, product.PK, 60m, location.PK)
			{
				WE_StockOnHand = 60m,
				WE_OriginalInventoryStatus = "AVL",
				WE_UnloadedTime = new DateTimeOffset(now),
				WE_GS_NKUnloadedBy = "Bob"
			}.AppendInsertAndReturnObject(sql);

			// Unfinalised Receive
			new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R2").AppendInsertAndReturnObject(sql);

			// Transfer
			new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "T1").AppendInsertAndReturnObject(sql);

			// Adjustment
			new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "ENT", "A1").AppendInsertAndReturnObject(sql);

			// Order
			new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);

			// Work Order
			new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "ENT", "W1").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateWhsDocketUnloadCompletedTime();

		void PrepareBatch()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "Row1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var now = DateTimeOffset.Now;
			var unloadTime = now.AddHours(-1);

			var dockets = new List<WhsDocket>();
			var docketLines = new List<WhsDocketLine>();

			for (var i = 0; i < 3200; i++)
			{
				var notFinalised = i % 50 == 0;
				WhsDocket receive;
				if (notFinalised)
				{
					receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", $"R1{i}");
				}
				else if (i % 220 == 0)
				{
					receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", $"R1{i}")
					{
						WD_FinalisedDate = now,
						WD_GS_NKFinalizedBy = "Me",
						WD_UnloadCompletedTime = unloadTime,
					};
				}
				else
				{
					receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", $"R1{i}")
					{
						WD_FinalisedDate = now,
						WD_GS_NKFinalizedBy = "Me",
						WD_UnloadCompletedTime = null,
					};
				}

				var receiveLine = new WhsDocketLine(receive, product.PK, 60m, location.PK)
				{
					WE_StockOnHand = 60m,
					WE_OriginalInventoryStatus = notFinalised ? "REC" : "AVL",
					WE_CurrentInventoryStatus = notFinalised ? "REC" : "AVL",
					WE_UnloadedTime = now,
					WE_GS_NKUnloadedBy = "Bob"
				};

				dockets.Add(receive);
				docketLines.Add(receiveLine);
			}

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			TestConnection.ExecuteNonQuery(WhsDocket.GetBulkInsertStatement(dockets));
			TestConnection.ExecuteNonQuery(WhsDocketLine.GetBulkInsertStatement(docketLines));
		}

		protected override void SetUp()
		{
			new DbColumnDependencyRemover(WhsDocketSchema.Constants.TableName, WhsDocketSchema.Constants.WD_UnloadCompletedTime).DropRelateObjects(Db.Connection);
			TestConnection.ExecuteNonQuery("ALTER TABLE WhsDocket DROP COLUMN WD_UnloadCompletedTime");
		}
	}
}
