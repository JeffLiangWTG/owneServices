using System;
using System.Data;
using System.Linq;
using System.Threading;
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
	[TestedType(typeof(UpdateWhsABCAnalysisDateToToDateTimeOffset))]
	class UpdateWhsABCAnalysisDateToToDateTimeOffsetTest : ConvertDateTimeToDateTimeOffsetTransformTestCase
	{
		protected override void PrepareTestData()
		{
			var branch1 = new GlbBranch("BR1") { GB_RL_NKHomePort = "AUSYD" }.InsertAndReturnObject(TestConnection);
			var sql = new SqlQueryBuilder();

			var whs1 = new WhsWarehouse("WHS", "PRW", branch1.PK).WithDockDoor(TestConnection);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var abcCategory = new WhsABCCategory("key001", whs1.PK, client.PK, product.PK)
			{
				WJ_AnalysisDateTo = new DateTimeOffset(2024, 6, 6, 0, 0, 0, TimeSpan.FromHours(0)),
			}.InsertAsDateTimeForColumn(d => d.WJ_AnalysisDateTo).AppendToInsert(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		protected override void AssertTransformationResults()
		{
			var abcCategory = WhsABCCategory.ShallowLoadFromDB(TestConnection, d => d.WJ_Key == "key001").Single();
			abcCategory.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(WhsABCCategory.WJ_AnalysisDateTo), d => d.WJ_AnalysisDateTo,
					new DateTimeOffset(2024, 6, 6, 0, 0, 0, TimeSpan.FromHours(10)))
				.VerifyAll();
		}

		protected override void InsertDataWithThisTimeZone(string timeZoneUnloco)
		{
			var branch1 = new GlbBranch("BR1") { GB_RL_NKHomePort = timeZoneUnloco }.InsertAndReturnObject(TestConnection);
			var sql = new SqlQueryBuilder();

			var whs1 = new WhsWarehouse("WH0", "PRW", branch1.PK).WithDockDoor(TestConnection);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var abcCategory = new WhsABCCategory("key001", whs1.PK, client.PK, product.PK)
			{
				WJ_AnalysisDateFrom = new DateTimeOffset(2024, 6, 6, 0, 0, 0, TimeSpan.FromHours(0)),
				WJ_AnalysisDateTo = new DateTimeOffset(2024, 6, 10, 0, 0, 0, TimeSpan.FromHours(0)),
			}.InsertAsDateTimeForColumn(d => d.WJ_AnalysisDateTo).AppendToInsert(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		#region TestTwoBranchs

		public void TestTwoBranchs()
		{
			var today = new DateTime(2024, 6, 6, 0, 0, 0);
			var sql = new SqlQueryBuilder();

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var branch1 = new GlbBranch("BR1") { GB_RL_NKHomePort = "AUSYD" }.InsertAndReturnObject(TestConnection);
			var whs1 = new WhsWarehouse("WH8", "PRW", branch1.PK).WithDockDoor(TestConnection);

			var branch2 = new GlbBranch("BR2") { GB_RL_NKHomePort = "MAGIC" }.InsertAndReturnObject(TestConnection);
			var whs2 = new WhsWarehouse("WH9", "PRW", branch2.PK).WithDockDoor(TestConnection);

			var abcCategory1 = new WhsABCCategory("key001", whs1.PK, client.PK, product.PK)
			{
				WJ_AnalysisDateTo = new DateTimeOffset(2024, 6, 6, 0, 0, 0, TimeSpan.FromHours(0)),
			}.InsertAsDateTimeForColumn(d => d.WJ_AnalysisDateTo).AppendToInsert(sql);

			var abcCategory2 = new WhsABCCategory("key002", whs2.PK, client.PK, product.PK)
			{
				WJ_AnalysisDateTo = new DateTimeOffset(2024, 6, 6, 10, 0, 0, TimeSpan.FromHours(0)),
			}.InsertAsDateTimeForColumn(d => d.WJ_AnalysisDateTo).AppendToInsert(sql);

			var abcCategory3 = new WhsABCCategory("key003", whs2.PK, client.PK, product.PK)
			{
				WJ_AnalysisDateTo = new DateTimeOffset(2024, 6, 6, 11, 0, 0, TimeSpan.FromHours(0)),
			}.InsertAsDateTimeForColumn(d => d.WJ_AnalysisDateTo).AppendToInsert(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			instance.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			SetupTemplateDBAndRunColumnSync();

			WhsABCCategory.AssertFromDB(TestConnection, abcCategory1.PK)
				.ExpectEquals("WJ_AnalysisDateTo should be equal.", d => d.WJ_AnalysisDateTo,
					new DateTimeOffset(2024, 6, 6, 0, 0, 0, TimeSpan.FromHours(10)))
				.VerifyAll();

			WhsABCCategory.AssertFromDB(TestConnection, abcCategory2.PK)
				.ExpectEquals("WJ_AnalysisDateTo should be equal.", d => d.WJ_AnalysisDateTo,
					new DateTimeOffset(2024, 6, 6, 10, 0, 0, TimeSpan.FromHours(1)))
				.VerifyAll();

			WhsABCCategory.AssertFromDB(TestConnection, abcCategory3.PK)
				.ExpectEquals("WJ_AnalysisDateTo should be equal.", d => d.WJ_AnalysisDateTo,
					new DateTimeOffset(2024, 6, 6, 11, 0, 0, TimeSpan.FromHours(2)))
				.VerifyAll();
		}

		#endregion

		#region TestNoNeedToUpdate

		public void TestNoNeedToUpdate()
		{
			var sql = new SqlQueryBuilder();

			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var branch1 = new GlbBranch("BR1") { GB_RL_NKHomePort = "AAAAA" }.InsertAndReturnObject(TestConnection);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var whs1 = new WhsWarehouse("WH6", "PRW", branch1.PK).WithDockDoor(TestConnection);

			var abcCategory = new WhsABCCategory("key001", whs1.PK, client.PK, product.PK)
			{
				WJ_AnalysisDateTo = new DateTimeOffset(2024, 6, 10, 1, 0, 0, TimeSpan.FromHours(0)),
			}.InsertAsDateTimeForColumn(d => d.WJ_AnalysisDateTo).AppendToInsert(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			instance.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			SetupTemplateDBAndRunColumnSync();

			WhsABCCategory.AssertFromDB(TestConnection, abcCategory.PK)
				.ExpectEquals("WJ_AnalysisDateTo not changed.", d => d.WJ_AnalysisDateTo,
					new DateTimeOffset(2024, 6, 10, 1, 0, 0, TimeSpan.FromHours(0)))
				.VerifyAll();
		}

		#endregion

		#region TestUserDescription

		public void TestUserDescription()
		{
			AssertEquals("Update Whs ABC Category AnalysisDateTo from DateTime to DateTimeOffset.",
				GetNewTestTransformationInstance().UserDescription);
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
IF EXISTS(SELECT NULL FROM RefDatabase_RefUNLOCOUtcOffset WHERE RLO_RL_NKCode = 'AUSYD' AND RLO_StartTimeUtc <= '2024-01-01 00:00:00' AND RLO_EndTimeUtc >= '2025-01-01 00:00:00')
BEGIN
	UPDATE dbo.RefDatabase_RefUNLOCOUtcOffset
		SET RLO_OffsetMinutesFromUtc = 600
		WHERE RLO_RL_NKCode = 'AUSYD' AND RLO_StartTimeUtc <= '2024-06-01 00:00:00' AND RLO_EndTimeUtc >= '2024-07-01 00:00:00'
END
ELSE
BEGIN
	INSERT INTO dbo.RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES
	(newid(), 'AUSYD', '2024-06-01 00:00:00', '2024-07-01 00:00:00', 600)
END

INSERT INTO dbo.RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES
(newid(), 'MAGIC', '2024-01-01 12:00:00', '2024-06-06 12:00:00', 60)

INSERT INTO dbo.RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES
(newid(), 'MAGIC', '2024-06-06 12:00:00', '2025-01-01 12:00:00', 120)
");
		}

		#endregion

		#region Implementation

		protected override DataTransformation GetNewTestTransformationInstance() =>
			new UpdateWhsABCAnalysisDateToToDateTimeOffset();

		protected override SchemaDateTimeOffsetColumn GetDateTimeOffsetColumnToConvert() => WhsABCCategorySchema.WJ_AnalysisDateTo;

		protected override SqlDbType DateTimeTypeBeforeConversion => SqlDbType.SmallDateTime;

		protected override bool TableHasAutoVersion => true;

		protected override string GetTimeZoneSubQuery(string timeZoneColumnName)
		{
			return $@"SELECT GB_RL_NKHomePort as {timeZoneColumnName}
	FROM
		dbo.WhsWarehouse
		JOIN dbo.GlbBranch ON WW_GB_RelatedCompanyBranch = GB_PK
	WHERE
		WW_PK = WJ_WW_Warehouse";
		}

		protected override IndexInfo GetSupportingIndex(TransformationIndexProvider indexProvider)
		{
			return indexProvider.New(WhsABCCategorySchema.Instance)
				.Key(WhsABCCategorySchema.Constants.WJ_AnalysisDateTo)
				.Include(WhsABCCategorySchema.Constants.WJ_WW_Warehouse, "WJ_AutoVersion")
				.Where("[WJ_AnalysisDateTo] IS NOT NULL")
				.GetInfo();
		}

		#endregion
	}
}
