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
	[TestedType(typeof(UpdateWhsDocketFinalisedDateTimeToDateTimeOffset))]
	class UpdateWhsDocketFinalisedDateTimeToDateTimeOffsetTest : ConvertDateTimeToDateTimeOffsetTransformTestCase
	{
		#region AssertTransformationResults

		protected override void PrepareTestData()
		{
			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "AUSYD" }.InsertAndReturnObject(TestConnection);

			var sql = new SqlQueryBuilder();

			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("PRD").AppendInsertAndReturnObject(sql);

			var now = new DateTime(2022, 06, 08, 0, 0, 0);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1")
			{
				WD_FinalisedDate = new DateTimeOffset(now, TimeSpan.FromHours(0)),
				WD_GS_NKFinalizedBy = "Bob",
			}
			.InsertAsDateTimeForColumn(dl => dl.WD_FinalisedDate)
			.AppendToInsert(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		protected override void AssertTransformationResults()
		{
			SetupTemplateDBAndRunColumnSync();

			var receive = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketType == "INW").Single();
			AssertEquals("WD_FinalisedDate should be correct.", new DateTimeOffset(2022, 06, 08, 00, 00, 00, TimeSpan.FromHours(10)), receive.WD_FinalisedDate);
		}

		protected override void InsertDataWithThisTimeZone(string timeZoneUnloco)
		{
			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = timeZoneUnloco }.InsertAndReturnObject(TestConnection);

			var sql = new SqlQueryBuilder();

			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("PRD").AppendInsertAndReturnObject(sql);

			var now = new DateTime(2022, 06, 08, 0, 0, 0);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1")
			{
				WD_FinalisedDate = new DateTimeOffset(now, TimeSpan.FromHours(0)),
				WD_GS_NKFinalizedBy = "Bob",
			}
			.InsertAsDateTimeForColumn(dl => dl.WD_FinalisedDate)
			.AppendToInsert(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
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

			var receive1 = new WhsDocket(client.PK, whs1.PK, "INW", "REC", "FIN", "R1")
			{
				WD_FinalisedDate = new DateTimeOffset(now, TimeSpan.FromHours(0)),
			}
			.InsertAsDateTimeForColumn(d => d.WD_FinalisedDate)
			.AppendToInsert(sql);

			var receive2 = new WhsDocket(client.PK, whs2.PK, "INW", "REC", "FIN", "R2")
			{
				WD_FinalisedDate = new DateTimeOffset(now, TimeSpan.FromHours(0)),
			}
			.InsertAsDateTimeForColumn(d => d.WD_FinalisedDate)
			.AppendToInsert(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			instance.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			SetupTemplateDBAndRunColumnSync();

			WhsDocket.AssertFromDB(TestConnection, receive1.PK)
				.ExpectEquals("WE_FinalisedDate was changed.", d => d.WD_FinalisedDate, new DateTimeOffset(2022, 06, 08, 0, 0, 0, TimeSpan.FromHours(10)))
				.VerifyAll();

			WhsDocket.AssertFromDB(TestConnection, receive2.PK)
				.ExpectEquals("WE_FinalisedDate was changed.", d => d.WD_FinalisedDate, new DateTimeOffset(2022, 06, 08, 0, 0, 0, TimeSpan.FromHours(2)))
				.VerifyAll();
		}

		#endregion

		#region TestNoNeedToUpdate

		public void TestNoNeedToUpdate()
		{
			var sql = new SqlQueryBuilder();

			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "AAAAA" }.InsertAndReturnObject(TestConnection);

			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("PRD").AppendInsertAndReturnObject(sql);

			var now = new DateTime(2022, 06, 08, 0, 0, 0);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1")
			{
				WD_FinalisedDate = new DateTimeOffset(now, TimeSpan.FromHours(0)),
				WD_GS_NKFinalizedBy = "Bob",
			}
			.InsertAsDateTimeForColumn(d => d.WD_FinalisedDate)
			.AppendToInsert(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			instance.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			SetupTemplateDBAndRunColumnSync();

			WhsDocket.AssertFromDB(TestConnection, receive.PK)
				.ExpectEquals("WD_FinalisedDate not changed.", d => d.WD_FinalisedDate, new DateTimeOffset(2022, 06, 08, 0, 0, 0, TimeSpan.FromHours(0)))
				.VerifyAll();
		}

		#endregion

		#region TestOrders

		public void TestDifferentDocketTypes()
		{
			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "AUSYD" }.InsertAndReturnObject(TestConnection);

			var sql = new SqlQueryBuilder();

			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("PRD").AppendInsertAndReturnObject(sql);

			var now = new DateTime(2022, 06, 08, 0, 0, 0);

			var pick = new WhsPick(whs, "P1", "NEW").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1")
			{
				WD_FinalisedDate = new DateTimeOffset(now, TimeSpan.FromHours(0)),
				WD_GS_NKFinalizedBy = "Bob",
				WD_WP = pick.PK,
			}
			.InsertAsDateTimeForColumn(d => d.WD_FinalisedDate)
			.AppendToInsert(sql);

			var workOrder = new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "FIN", "O2")
			{
				WD_FinalisedDate = new DateTimeOffset(now, TimeSpan.FromHours(0)),
				WD_GS_NKFinalizedBy = "Bob",
				WD_WP = pick.PK,
			}
			.InsertAsDateTimeForColumn(d => d.WD_FinalisedDate)
			.AppendToInsert(sql);

			var dynamicWorkOrder = new WhsDocket(client.PK, whs.PK, "DWO", "ASS", "FIN", "O3")
			{
				WD_FinalisedDate = new DateTimeOffset(now, TimeSpan.FromHours(0)),
				WD_GS_NKFinalizedBy = "Bob",
				WD_WP = pick.PK,
			}
			.InsertAsDateTimeForColumn(d => d.WD_FinalisedDate)
			.AppendToInsert(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "TIN")
			{
				WD_FinalisedDate = new DateTimeOffset(now, TimeSpan.FromHours(0)),
				WD_GS_NKFinalizedBy = "Bob",
			}
			.InsertAsDateTimeForColumn(d => d.WD_FinalisedDate)
			.AppendToInsert(sql);

			var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "FIN", "A1")
			{
				WD_FinalisedDate = new DateTimeOffset(now, TimeSpan.FromHours(0)),
				WD_GS_NKFinalizedBy = "Bob",
			}
			.InsertAsDateTimeForColumn(d => d.WD_FinalisedDate)
			.AppendToInsert(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			instance.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			SetupTemplateDBAndRunColumnSync();

			WhsDocket.AssertFromDB(TestConnection, order.PK)
				.ExpectEquals("WD_FinalisedDate was changed.", d => d.WD_FinalisedDate, new DateTimeOffset(2022, 06, 08, 0, 0, 0, TimeSpan.FromHours(10)))
				.VerifyAll();

			WhsDocket.AssertFromDB(TestConnection, workOrder.PK)
				.ExpectEquals("WD_FinalisedDate was changed.", d => d.WD_FinalisedDate, new DateTimeOffset(2022, 06, 08, 0, 0, 0, TimeSpan.FromHours(10)))
				.VerifyAll();

			WhsDocket.AssertFromDB(TestConnection, dynamicWorkOrder.PK)
				.ExpectEquals("WD_FinalisedDate was changed.", d => d.WD_FinalisedDate, new DateTimeOffset(2022, 06, 08, 0, 0, 0, TimeSpan.FromHours(10)))
				.VerifyAll();

			WhsDocket.AssertFromDB(TestConnection, transfer.PK)
				.ExpectEquals("WD_FinalisedDate was changed.", d => d.WD_FinalisedDate, new DateTimeOffset(2022, 06, 08, 0, 0, 0, TimeSpan.FromHours(10)))
				.VerifyAll();

			WhsDocket.AssertFromDB(TestConnection, adjustment.PK)
				.ExpectEquals("WD_FinalisedDate was changed.", d => d.WD_FinalisedDate, new DateTimeOffset(2022, 06, 08, 0, 0, 0, TimeSpan.FromHours(10)))
				.VerifyAll();
		}

		#endregion

		#region TestUserDescription

		public void TestUserDescription()
		{
			AssertEquals("Update Whs Docket FinalisedDate Time from DateTime to DateTimeOffset.", GetNewTestTransformationInstance().UserDescription);
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
(newid(), 'MAGIC', '2022-01-01 12:00:00', '2023-01-01 12:00:00', 120)
");
		}

		#endregion

		#region Implementation

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateWhsDocketFinalisedDateTimeToDateTimeOffset();

		protected override SchemaDateTimeOffsetColumn GetDateTimeOffsetColumnToConvert() => WhsDocketSchema.WD_FinalisedDate;

		protected override SqlDbType DateTimeTypeBeforeConversion => SqlDbType.DateTime;

		protected override bool TableHasAutoVersion => true;

		protected override string GetTimeZoneSubQuery(string timeZoneColumnName)
		{
			return $@"SELECT GB_RL_NKHomePort as {timeZoneColumnName}
	FROM
		dbo.WhsWarehouse
		JOIN dbo.GlbBranch ON WW_GB_RelatedCompanyBranch = GB_PK
	WHERE
		WD_WW_Whs = WW_PK";
		}

		protected override IndexInfo GetSupportingIndex(TransformationIndexProvider indexProvider) => null;

		#endregion
	}
}
