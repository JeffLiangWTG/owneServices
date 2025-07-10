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
	[TestedType(typeof(UpdateWhsDocketLineFinalisedDateTimeToDateTimeOffset))]
	class UpdateWhsDocketLineFinalisedDateToDateTimeOffsetTest : ConvertDateTimeToDateTimeOffsetTransformTestCase
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

			var receiveLine = new WhsDocketLine(receive, product.PK, 30m, location.PK)
			{
				WE_FinalisedDate = new DateTimeOffset(now, TimeSpan.FromHours(0)),
				WE_StockOnHand = 30m,
				WE_OriginalInventoryStatus = "AVL",
			}
			.InsertAsDateTimeForColumn(dl => dl.WE_FinalisedDate)
			.AppendToInsert(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		protected override void AssertTransformationResults()
		{
			SetupTemplateDBAndRunColumnSync();

			var receive = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketType == "INW").Single();
			var receiveLine = WhsDocketLine.ShallowLoadFromDB(TestConnection, d => d.WE_WD == receive.PK).Single();
			AssertEquals("WE_FinalisedDate should be correct.", new DateTimeOffset(2022, 06, 08, 00, 00, 00, TimeSpan.FromHours(10)), receiveLine.WE_FinalisedDate);
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
			}.AppendInsertAndReturnObject(sql);

			var receiveLine = new WhsDocketLine(receive, product.PK, 30m, location.PK)
			{
				WE_FinalisedDate = new DateTimeOffset(now, TimeSpan.FromHours(0)),
				WE_StockOnHand = 30m,
				WE_OriginalInventoryStatus = "AVL",
			}
			.InsertAsDateTimeForColumn(dl => dl.WE_FinalisedDate)
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
			}.AppendInsertAndReturnObject(sql);
			var receive2 = new WhsDocket(client.PK, whs2.PK, "INW", "REC", "FIN", "R2")
			{
				WD_FinalisedDate = new DateTimeOffset(now, TimeSpan.FromHours(0)),
			}.AppendInsertAndReturnObject(sql);

			var receiveLine1 = new WhsDocketLine(receive1, product.PK, 2m, location1.PK)
			{
				WE_FinalisedDate = new DateTimeOffset(now, TimeSpan.FromHours(0)),
				WE_StockOnHand = 2m,
				WE_OriginalInventoryStatus = "AVL",
			}
			.InsertAsDateTimeForColumn(dl => dl.WE_FinalisedDate)
			.AppendToInsert(sql);

			var receiveLine2 = new WhsDocketLine(receive2, product.PK, 2m, location2.PK)
			{
				WE_FinalisedDate = new DateTimeOffset(now, TimeSpan.FromHours(0)),
				WE_StockOnHand = 2m,
				WE_OriginalInventoryStatus = "AVL",
			}
			.InsertAsDateTimeForColumn(dl => dl.WE_FinalisedDate)
			.AppendToInsert(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			instance.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			SetupTemplateDBAndRunColumnSync();

			WhsDocketLine.AssertFromDB(TestConnection, receiveLine1.PK)
				.ExpectEquals("WE_FinalisedDate was changed.", d => d.WE_FinalisedDate, new DateTimeOffset(2022, 06, 08, 0, 0, 0, TimeSpan.FromHours(10)))
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, receiveLine2.PK)
				.ExpectEquals("WE_FinalisedDate was changed.", d => d.WE_FinalisedDate, new DateTimeOffset(2022, 06, 08, 0, 0, 0, TimeSpan.FromHours(2)))
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
			}.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 30m, location.PK)
			{
				WE_FinalisedDate = new DateTimeOffset(now, TimeSpan.FromHours(0)),
				WE_StockOnHand = 30m,
				WE_OriginalInventoryStatus = "AVL",
			}
			.InsertAsDateTimeForColumn(dl => dl.WE_FinalisedDate)
			.AppendToInsert(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			instance.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			SetupTemplateDBAndRunColumnSync();

			WhsDocket.AssertFromDB(TestConnection, receive.PK)
				.ExpectEquals("WD_FinalisedDate not changed.", d => d.WD_FinalisedDate, new DateTimeOffset(2022, 06, 08, 0, 0, 0, TimeSpan.FromHours(0)))
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, receiveLine.PK)
				.ExpectEquals("WE_FinalisedDate not changed.", d => d.WE_FinalisedDate, new DateTimeOffset(2022, 06, 08, 0, 0, 0, TimeSpan.FromHours(0)))
				.VerifyAll();
		}

		#endregion

		#region TestUserDescription

		public void TestUserDescription()
		{
			AssertEquals("Update Whs Docket Line FinalisedDate Time from DateTime to DateTimeOffset.", GetNewTestTransformationInstance().UserDescription);
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

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateWhsDocketLineFinalisedDateTimeToDateTimeOffset();

		protected override SchemaDateTimeOffsetColumn GetDateTimeOffsetColumnToConvert() => WhsDocketLineSchema.WE_FinalisedDate;

		protected override SqlDbType DateTimeTypeBeforeConversion => SqlDbType.DateTime;

		protected override bool TableHasAutoVersion => true;

		protected override string GetTimeZoneSubQuery(string timeZoneColumnName)
		{
			return $@"SELECT GB_RL_NKHomePort as {timeZoneColumnName}
	FROM
		dbo.WhsDocket
		JOIN dbo.WhsWarehouse ON WD_WW_Whs = WW_PK
		JOIN dbo.GlbBranch ON WW_GB_RelatedCompanyBranch = GB_PK
	WHERE
		WE_WD = WD_PK";
		}

		protected override IndexInfo GetSupportingIndex(TransformationIndexProvider indexProvider)
		{
			return indexProvider.New(WhsDocketLineSchema.Instance)
				.Key(WhsDocketLineSchema.Constants.WE_FinalisedDate)
				.Include(WhsDocketLineSchema.Constants.WE_WD, WhsDocketLineSchema.Constants.WE_StockOnHand, WhsDocketLineSchema.Constants.WE_DocketLineStatus)
				.Where("[WE_FinalisedDate] IS NOT NULL")
				.GetInfo();
		}
		#endregion
	}
}
