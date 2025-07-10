using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
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
	[TestedType(typeof(PopulateWhsDocketStartedReceivingTimeUtcColumn))]
	class PopulateWhsDocketStartedReceivingTimeUtcColumnTest : DataTransformationTestCase
	{
		const string TriggerName = "TG_WhsDocket_StartedReceivingTimeUtc";
		const string StoredFromPKName = "PopulateWhsDocketStartedReceivingTimeUtcColumn.WD_StartedReceivingTimeUtc.FromPK";
		const string StoredToPKName = "PopulateWhsDocketStartedReceivingTimeUtcColumn.WD_StartedReceivingTimeUtc.ToPK";
		const string TransformationHasRunToCompletion = "PopulateWhsDocketStartedReceivingTimeUtcColumn.WD_StartedReceivingTimeUtc.TransformationRunToCompletion";
		const string LastProcessedChunkPKName = "PopulateWhsDocketStartedReceivingTimeUtcColumn.WD_StartedReceivingTimeUtc.LastProcessedChunkPK";

		#region Basic Test

		protected override void PrepareTestData()
		{
			var today = DateTime.Today;

			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);

			var arrivalDate = new DateTimeOffset(2022, 6, 8, 12, 01, 0, TimeSpan.FromHours(10));
			var asnLineCreateTime = new DateTime(2022, 6, 9, 12, 01, 0);

			var receive1 = new WhsDocketOld_V02(client.PK, whs.PK, "INW", "REC", "FIN", "R1")
			{
				WD_ArrivalDate = arrivalDate,
				WD_FinalisedDate = today,
				WD_StartedReceiving = true
			}.AppendInsertAndReturnObject(sql);

			var receive2 = new WhsDocketOld_V02(client.PK, whs.PK, "INW", "REC", "ENT", "R2")
			{
				WD_ArrivalDate = arrivalDate,
				WD_StartedReceiving = true
			}.AppendInsertAndReturnObject(sql);
			var asnLine = new WhsAsnLine(receive2, component1.PK, 10m)
			{
				WN_SystemCreateTimeUtc = asnLineCreateTime
			}.AppendInsertAndReturnObject(sql);

			var receive3 = new WhsDocketOld_V02(client.PK, whs.PK, "INW", "REC", "ENT", "R3")
			{
				WD_StartedReceiving = false
			}.AppendInsertAndReturnObject(sql);

			var order = new WhsDocketOld_V02(client.PK, whs.PK, "ORD", "ORD", "ENT", "Order1")
			{
				WD_ArrivalDate = DateTime.UtcNow,
				WD_StartedReceiving = false
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		protected override void AssertTransformationResults()
		{
			var column = WhsDocketSchema.WD_StartedReceivingTimeUtc;
			AssertEquals("Column WD_StartedReceivingTimeUtc should get created.", true, DbObjectCreator.ColumnExists(TestConnection, column.TableName, column.Name));

			var countOfTriggeredRows = WhsDocket.CountInDB(TestConnection, p => p.WD_StartedReceivingTimeUtc != null);
			AssertEquals("Transformation should populate WD_StartedReceivingTimeUtc from WD_StartedReceiving when WD_StartedReceiving = 1 AND WD_DocketType = 'INW'.", 2, countOfTriggeredRows);

			var receive1 = WhsDocket.ShallowLoadFromDB(TestConnection, r => r.WD_DocketID == "R1").Single();
			var receive2 = WhsDocket.ShallowLoadFromDB(TestConnection, r => r.WD_DocketID == "R2").Single();

			WhsDocket.AssertFromDB(TestConnection, receive1.PK)
				.ExpectEquals("WD_StartedReceivingTimeUtc should be updated to WD_ArrivalDate.", l => l.WD_StartedReceivingTimeUtc?.ToString("yyyy-MM-dd HH:mm"), "2022-06-08 02:01")
				.VerifyAll();

			WhsDocket.AssertFromDB(TestConnection, receive2.PK)
				.ExpectEquals("WD_StartedReceivingTimeUtc should be updated to WN_SystemCreateTimeUtc.", l => l.WD_StartedReceivingTimeUtc?.ToString("yyyy-MM-dd HH:mm"), "2022-06-09 12:01")
				.VerifyAll();
		}

		#endregion

		#region Additional Test

		public void TestWhsDocketStartedReceivingTimeUtcColumnIsCreated()
		{
			AssertEquals("Column WD_StartedReceivingTimeUtc should not exist.", false, DbObjectCreator.ColumnExists(TestConnection, WhsDocketSchema.Constants.TableName, WhsDocketSchema.Constants.WD_StartedReceivingTimeUtc));

			var column = WhsDocketSchema.WD_StartedReceivingTimeUtc;
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("Column WD_StartedReceivingTimeUtc should get created.", true, DbObjectCreator.ColumnExists(TestConnection, WhsDocketSchema.Constants.TableName, WhsDocketSchema.Constants.WD_StartedReceivingTimeUtc));
			AssertEquals("Column WD_StartedReceivingTimeUtc should be created with correct data type.", "smalldatetime", DbObjectCreator.GetColumnType(TestConnection, column.TableName, WhsDocketSchema.Constants.WD_StartedReceivingTimeUtc));
		}

		public void TestTriggerIsCreatedCorrectly()
		{
			var expectedTriggerDefinition = @"
CREATE TRIGGER dbo.TG_WhsDocket_StartedReceivingTimeUtc
	ON dbo.WhsDocket
	FOR INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	IF UPDATE (WD_StartedReceiving)
	BEGIN
		UPDATE wd
		SET
			wd.WD_StartedReceivingTimeUtc = CASE WHEN i.WD_StartedReceiving = 1 THEN GetUtcDate() ELSE NULL END,
			wd.WD_SystemLastEditUser = wd.WD_SystemLastEditUser,
			wd.WD_SystemLastEditTimeUtc = wd.WD_SystemLastEditTimeUtc
		FROM
			WhsDocket wd
			JOIN inserted i ON wd.WD_PK = i.WD_PK
		WHERE
			i.WD_DocketType = 'INW'
	END
END";

			AssertEquals("Trigger should not exist.", false, DbObjectCreator.TriggerExists(TestConnection, WhsDocketSchema.Constants.TableName, TriggerName));

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, WhsDocketSchema.Constants.TableName, TriggerName));
			AssertEquals("Trigger Definition is correct", expectedTriggerDefinition, DbObjectCreator.GetTriggerDefinition(TestConnection, TriggerName));
		}

		public void TestPopulateStartedReceivingTimeUtcFromTrigger_Insert()
		{
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			PrepareTestData();

			var countOfTriggeredRows = WhsDocket.CountInDB(Db.Connection, p => p.WD_StartedReceivingTimeUtc != null);
			AssertEquals("Trigger should populate WD_StartedReceivingTimeUtc when WD_StartedReceiving = 1 AND WD_DocketType = 'INW'.", 2, countOfTriggeredRows);
		}

		public void TestPopulateStartedReceivingTimeUtcFromTrigger_Update()
		{
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);

			var arrivalDate = new DateTimeOffset(2022, 6, 8, 12, 01, 0, TimeSpan.FromHours(10));
			var receive1 = new WhsDocketOld_V02(client.PK, whs.PK, "INW", "REC", "FIN", "R1")
			{
				WD_ArrivalDate = arrivalDate,
				WD_FinalisedDate = today,
				WD_StartedReceiving = false
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var countOfTriggeredRows = WhsDocket.CountInDB(Db.Connection, p => p.WD_StartedReceivingTimeUtc != null);
			AssertEquals("Trigger should not populate WD_StartedReceivingTimeUtc when WD_StartedReceiving = 0 AND WD_DocketType = 'INW'.", 0, countOfTriggeredRows);

			WhsDocketOld_V02.UpdateWhere(receive1.PK).Set(l => l.WD_StartedReceiving, true).Post(TestConnection);

			countOfTriggeredRows = WhsDocket.CountInDB(Db.Connection, p => p.WD_StartedReceivingTimeUtc != null);
			AssertEquals("Trigger should populate WD_StartedReceivingTimeUtc when updating WD_StartedReceiving to 1.", 1, countOfTriggeredRows);

			WhsDocketOld_V02.UpdateWhere(receive1.PK).Set(l => l.WD_StartedReceiving, false).Post(TestConnection);

			WhsDocket.AssertFromDB(TestConnection, receive1.PK)
				.ExpectEquals("WD_StartedReceivingTimeUtc should be updated to null when updating WD_StartedReceiving to 0.", l => l.WD_StartedReceivingTimeUtc, null)
				.VerifyAll();
		}

		public void TestStatusExtendedProperty_SetToFinishedOnceTransformCompleted()
		{
			AssertNull("Status should be null before transformation.", ExtProperty.Database.Select(TestConnection, TransformationHasRunToCompletion));

			var transform = GetNewTestTransformationInstance();

			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("Status should be updated to Finished once Transform is complete.", bool.TrueString, ExtProperty.Database.Select(TestConnection, TransformationHasRunToCompletion));
		}

		public void TestStatusExtendedProperty_DeletesAllExtendedPropertiesWhenFinished()
		{
			PrepareTestData();

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("Extended property 'StoredFromPKName' should be removed once Transform is complete.", null, ExtProperty.Database.Select(TestConnection, StoredFromPKName));
			AssertEquals("Extended property 'StoredToPKName' should be removed once Transform is complete.", null, ExtProperty.Database.Select(TestConnection, StoredToPKName));
			AssertEquals("Extended property 'LastProcessedChunkPKName' should be removed once Transform is complete.", null, ExtProperty.Database.Select(TestConnection, LastProcessedChunkPKName));
		}

		public void TestStartedReceivingTimeUtcCanBeUpdatedToArrivalDateWithCorrectTimeZone_Transformation()
		{
			var arrivalDate = new DateTimeOffset(2022, 6, 8, 12, 01, 01, TimeSpan.FromHours(10));
			PrepareDateForTestingArrivalDateTimeZone(arrivalDate);

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var receive1 = WhsDocket.ShallowLoadFromDB(TestConnection, r => r.WD_DocketID == "R1").Single();
			WhsDocket.AssertFromDB(TestConnection, receive1.PK)
				.ExpectEquals("WD_StartedReceivingTimeUtc should be updated to WD_ArrivalDate.", l => l.WD_StartedReceivingTimeUtc?.ToString("yyyy-MM-dd HH:mm:ss"), "2022-06-08 02:01:00")
				.VerifyAll();
		}

		public void TestStartedReceivingTimeUtcCanBeUpdatedToBookingDateWithCorrectTimeZone_Transformation()
		{
			PrepareDateForTestingArrivalDateTimeZone(null);

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var receive1 = WhsDocket.ShallowLoadFromDB(TestConnection, r => r.WD_DocketID == "R1").Single();
			WhsDocket.AssertFromDB(TestConnection, receive1.PK)
				.ExpectEquals("WD_StartedReceivingTimeUtc should be updated to WD_BookingDate.", l => l.WD_StartedReceivingTimeUtc?.ToString("yyyy-MM-dd HH:mm:ss"), "2022-06-06 02:01:00")
				.VerifyAll();
		}

		public void TestMultipleAsnLinesOrderBySystemCreateTimeUtcAsc()
		{
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);

			var arrivalDate = new DateTimeOffset(2022, 6, 8, 12, 01, 01, TimeSpan.FromHours(10));
			var receive1 = new WhsDocketOld_V02(client.PK, whs.PK, "INW", "REC", "FIN", "R1")
			{
				WD_ArrivalDate = arrivalDate,
				WD_FinalisedDate = DateTime.Today,
				WD_StartedReceiving = true
			}.AppendInsertAndReturnObject(sql);
			var asnLine1 = new WhsAsnLine(receive1, component1.PK, 10m)
			{
				WN_SystemCreateTimeUtc = new DateTime(2022, 6, 8, 12, 01, 01)
			}.AppendInsertAndReturnObject(sql);
			var asnLine2 = new WhsAsnLine(receive1, component1.PK, 10m)
			{
				WN_SystemCreateTimeUtc = new DateTime(2022, 6, 7, 12, 01, 01)
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			WhsDocket.AssertFromDB(TestConnection, receive1.PK)
				.ExpectEquals("WD_StartedReceivingTimeUtc should be updated to WD_ArrivalDate.", l => l.WD_StartedReceivingTimeUtc?.ToString("yyyy-MM-dd HH:mm:ss"), "2022-06-07 12:01:00")
				.VerifyAll();
		}

		public void TestMultiBatchShouldAllBeProcessed()
		{
			PrepareTestAdditionalBatch();

			var countOfRowsToPopulate = WhsDocketOld_V02.CountInDB(Db.Connection, p => p.WD_StartedReceiving && p.WD_DocketType == "INW");
			var countOfOrders = WhsDocketOld_V02.CountInDB(Db.Connection, p => p.WD_StartedReceiving && p.WD_DocketType == "ORD");
			var countOfReceiveNotToPopulate = WhsDocketOld_V02.CountInDB(Db.Connection, p => !p.WD_StartedReceiving && p.WD_DocketType == "INW");
			AssertEquals("countOfRowsToPopulate should be 3000.", 3000, countOfRowsToPopulate);
			AssertEquals("countOfOrders should be 5.", 5, countOfOrders);
			AssertEquals("countOfReceiveNotToPopulate should be 3.", 3, countOfReceiveNotToPopulate);

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("The count of rows that are populated should be 3000.", 3000, WhsDocket.CountInDB(Db.Connection, p => p.WD_StartedReceivingTimeUtc != null));
		}

		public void TestStartedReceiveTimeUtcFallbackToBookingDate()
		{
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);

			var bookingDate = new DateTimeOffset(2022, 6, 8, 12, 01, 01, TimeSpan.FromHours(10));
			var receive1 = new WhsDocketOld_V02(client.PK, whs.PK, "INW", "REC", "FIN", "R1")
			{
				WD_ArrivalDate = null,
				WD_FinalisedDate = DateTime.Today,
				WD_BookingDate = bookingDate,
				WD_StartedReceiving = true
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			WhsDocket.AssertFromDB(TestConnection, receive1.PK)
				.ExpectEquals("WD_StartedReceivingTimeUtc should be updated to WD_BookingDate.", l => l.WD_StartedReceivingTimeUtc?.ToString("yyyy-MM-dd HH:mm:ss"), "2022-06-08 02:01:00")
				.VerifyAll();
		}

		#endregion

		void PrepareDateForTestingArrivalDateTimeZone(DateTimeOffset? arrivalDate)
		{
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);

			var bookingDate = new DateTimeOffset(2022, 6, 6, 12, 01, 01, TimeSpan.FromHours(10));
			var receive1 = new WhsDocketOld_V02(client.PK, whs.PK, "INW", "REC", "FIN", "R1")
			{
				WD_ArrivalDate = arrivalDate,
				WD_BookingDate = bookingDate,
				WD_FinalisedDate = DateTime.Today,
				WD_StartedReceiving = true
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		void PrepareTestAdditionalBatch()
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var whsDockets = new List<WhsDocketOld_V02>();
			var whsAsnLines = new List<WhsAsnLine>();

			var arrivalDate = new DateTimeOffset(2022, 6, 8, 12, 01, 0, TimeSpan.FromHours(10));
			var asnLineCreateTimeUTC = new DateTime(2022, 6, 7, 12, 01, 01);

			WhsDocketOld_V02 receive3;
			WhsDocketOld_V02 order;
			for (int i = 0; i < 1500; i++)
			{
				var receive1 = new WhsDocketOld_V02(client.PK, whs.PK, "INW", "REC", "FIN", $"RA{i}")
				{
					WD_ArrivalDate = arrivalDate,
					WD_FinalisedDate = today,
					WD_StartedReceiving = true
				};
				whsDockets.Add(receive1);

				var receive2 = new WhsDocketOld_V02(client.PK, whs.PK, "INW", "REC", "ENT", $"RB{i}")
				{
					WD_ArrivalDate = arrivalDate,
					WD_StartedReceiving = true
				};
				var asnLine = new WhsAsnLine(receive2, component1.PK, 10m)
				{
					WN_SystemCreateTimeUtc = asnLineCreateTimeUTC
				};
				whsDockets.Add(receive2);
				whsAsnLines.Add(asnLine);

				if (i % 500 == 0)
				{
					receive3 = new WhsDocketOld_V02(client.PK, whs.PK, "INW", "REC", "ENT", $"RC{i}")
					{
						WD_StartedReceiving = false
					};
					whsDockets.Add(receive3);
				}

				if (i % 300 == 0)
				{
					order = new WhsDocketOld_V02(client.PK, whs.PK, "ORD", "ORD", "ENT", $"O{i}")
					{
						WD_ArrivalDate = DateTime.UtcNow,
						WD_StartedReceiving = true
					};
					whsDockets.Add(order);
				}
			}

			TestConnection.ExecuteNonQuery(WhsDocketOld_V02.GetBulkInsertStatement(whsDockets));
			TestConnection.ExecuteNonQuery(WhsAsnLine.GetBulkInsertStatement(whsAsnLines));
		}

		#region SetUp

		protected override void SetUp()
		{
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, WhsDocketSchema.Constants.TableName, "WD_StartedReceiving", "BIT", "0");
			new DbColumnDependencyRemover(WhsDocketSchema.Constants.TableName, WhsDocketSchema.Constants.WD_StartedReceivingTimeUtc).DropRelateObjects(TestConnection);
			TestConnection.ExecuteNonQuery("ALTER TABLE WhsDocket DROP COLUMN WD_StartedReceivingTimeUtc");
		}

		#endregion

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateWhsDocketStartedReceivingTimeUtcColumn();
	}
}
