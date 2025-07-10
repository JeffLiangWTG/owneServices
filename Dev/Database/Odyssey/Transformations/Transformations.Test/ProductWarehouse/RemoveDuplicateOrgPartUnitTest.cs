using System;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ProductWarehouse
{
	[TestedType(typeof(RemoveDuplicateOrgPartUnit))]
	public class RemoveDuplicateOrgPartUnitTest : RegistryDataTransformationTestCase
	{
		const string TriggerName = "TG_OrgPartUnit_PreventDuplicateUnit";

		protected override void PrepareTestData()
		{
			var sql = new SqlQueryBuilder();
			var product1 = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			new OrgPartUnit(product1, 10m, "UNT", "BOX").AppendInsertAndReturnObject(sql);
			new OrgPartUnit(product1, 0m, "UNT", "BOX").AppendInsertAndReturnObject(sql);

			var product2 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			new OrgPartUnit(product2, 20m, "UNT", "BOX").AppendInsertAndReturnObject(sql);
			new OrgPartUnit(product2, 30m, "UNT", "BOX").AppendInsertAndReturnObject(sql);

			var product3 = new OrgSupplierPart("P3") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			new OrgPartUnit(product3, 40m, "UNT", "BOX").AppendInsertAndReturnObject(sql);
			new OrgPartUnit(product3, 50m, "UNT", "BOX").AppendInsertAndReturnObject(sql);

			DBTransformationTestHelper.DropIndexIfExists(OrgPartUnitSchema.Constants.TableName, "NR_UX__OF_ParentPackType_OF_PackType_OF_OP");
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertEquals("All Units have successful insert in DB", 6, OrgPartUnit.CountInDB(Db.Connection));
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals("All the duplicate Units should have been deleted.", 3, OrgPartUnit.CountInDB(Db.Connection));
		}

		public void TestTransformation_RemoveDuplicateUnitWithQuantityInParentIsZero()
		{
			var date = new DateTime(2023, 8, 1);
			var sql = new SqlQueryBuilder();
			var product = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			new OrgPartUnit(product, 10m, "UNT", "BOX").AppendInsertAndReturnObject(sql);
			new OrgPartUnit(product, 0m, "UNT", "BOX").AppendInsertAndReturnObject(sql);

			DBTransformationTestHelper.DropIndexIfExists(OrgPartUnitSchema.Constants.TableName, "NR_UX__OF_ParentPackType_OF_PackType_OF_OP");
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertEquals("2 Units with same Product, PackType and ParentPackType Created", 2, OrgPartUnit.CountInDB(TestConnection, r => r.OF_OP == product.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals("Unit with QuantityInParent Zero is deleted.", 1, OrgPartUnit.CountInDB(TestConnection, r => r.OF_OP == product.PK));
			var unit = OrgPartUnit.ShallowLoadFromDB(TestConnection, r => r.OF_OP == product.PK).Single();
			AssertEquals("Unit with QuantityInParent Zero is deleted.", 10m, unit.OF_QuantityInParent);
		}

		public void TestTransformation_RemoveDuplicateUnitWithLeastValueBiggerThanZero()
		{
			var date = new DateTime(2023, 8, 1);
			var sql = new SqlQueryBuilder();
			var product = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			new OrgPartUnit(product, 40m, "UNT", "BOX") { OF_Cubic = 2, OF_Depth = 3, OF_Height = 4, OF_SystemCreateTimeUtc = date.AddHours(6) }.AppendInsertAndReturnObject(sql);
			new OrgPartUnit(product, 50m, "UNT", "BOX") { OF_Cubic = 2, OF_Depth = 3, OF_Height = 4, OF_Weight = 5, OF_SystemCreateTimeUtc = date.AddHours(5) }.AppendInsertAndReturnObject(sql);

			DBTransformationTestHelper.DropIndexIfExists(OrgPartUnitSchema.Constants.TableName, "NR_UX__OF_ParentPackType_OF_PackType_OF_OP");
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertEquals("2 Units with same Product, PackType and ParentPackType Created", 2, OrgPartUnit.CountInDB(TestConnection, r => r.OF_OP == product.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals("Unit with least NonZeroCol is deleted.", 1, OrgPartUnit.CountInDB(TestConnection, r => r.OF_OP == product.PK));
			var unit = OrgPartUnit.ShallowLoadFromDB(TestConnection, r => r.OF_OP == product.PK).Single();
			AssertEquals("Unit with least NonZeroCol is deleted.", 50m, unit.OF_QuantityInParent);
		}

		public void TestTransformation_RemoveDuplicateUnitCreateLater()
		{
			var date = new DateTime(2023, 8, 1);
			var sql = new SqlQueryBuilder();
			var product = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			new OrgPartUnit(product, 20m, "UNT", "BOX") { OF_SystemCreateTimeUtc = date.AddHours(4) }.AppendInsertAndReturnObject(sql);
			new OrgPartUnit(product, 30m, "UNT", "BOX") { OF_SystemCreateTimeUtc = date.AddHours(3) }.AppendInsertAndReturnObject(sql);

			DBTransformationTestHelper.DropIndexIfExists(OrgPartUnitSchema.Constants.TableName, "NR_UX__OF_ParentPackType_OF_PackType_OF_OP");
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertEquals("2 Units with same Product, PackType and ParentPackType Created", 2, OrgPartUnit.CountInDB(TestConnection, r => r.OF_OP == product.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals("Unit with Created Date later is deleted.", 1, OrgPartUnit.CountInDB(TestConnection, r => r.OF_OP == product.PK));
			var unit = OrgPartUnit.ShallowLoadFromDB(TestConnection, r => r.OF_OP == product.PK).Single();
			AssertEquals("Unit with Created Date later is deleted.", 30m, unit.OF_QuantityInParent);
		}

		public void TestBatchingWorksProperly()
		{
			var date = new DateTime(2023, 8, 1);
			var sql = new SqlQueryBuilder();
			for (var i = 0; i < 1000; i++)
			{
				var product1 = new OrgSupplierPart("P1" + i) { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
				new OrgPartUnit(product1, 10m, "UNT", "BOX").AppendInsertAndReturnObject(sql);
				new OrgPartUnit(product1, 0m, "UNT", "BOX").AppendInsertAndReturnObject(sql);

				var product2 = new OrgSupplierPart("P2" + i) { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
				new OrgPartUnit(product2, 20m, "UNT", "BOX") { OF_SystemCreateTimeUtc = date.AddHours(i + 2) }.AppendInsertAndReturnObject(sql);
				new OrgPartUnit(product2, 30m, "UNT", "BOX") { OF_SystemCreateTimeUtc = date.AddHours(i + 1) }.AppendInsertAndReturnObject(sql);

				var product3 = new OrgSupplierPart("P3" + i) { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
				new OrgPartUnit(product3, 40m, "UNT", "BOX") { OF_Cubic = 2, OF_Depth = 3, OF_Height = 4, OF_SystemCreateTimeUtc = date.AddHours(6) }.AppendInsertAndReturnObject(sql);
				new OrgPartUnit(product3, 50m, "UNT", "BOX") { OF_Cubic = 2, OF_Depth = 3, OF_Height = 4, OF_Weight = 5, OF_SystemCreateTimeUtc = date.AddHours(5) }.AppendInsertAndReturnObject(sql);
			}

			DBTransformationTestHelper.DropIndexIfExists(OrgPartUnitSchema.Constants.TableName, "NR_UX__OF_ParentPackType_OF_PackType_OF_OP");
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertEquals("Precondition", 6000, OrgPartUnit.CountInDB(TestConnection));

			GetNewTestTransformationInstance().Run();

			AssertEquals("Duplicated unit should been delete", 3000, OrgPartUnit.CountInDB(TestConnection));
		}

		public void TestTransformation_TempTriggerWorkedCorrectly()
		{
			DBTransformationTestHelper.DropIndexIfExists(OrgPartUnitSchema.Constants.TableName, "NR_UX__OF_ParentPackType_OF_PackType_OF_OP");
			var sql = new SqlQueryBuilder();
			var product1 = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);

			new OrgPartUnit(product1, 10m, "UNT", "BOX").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertEquals("Precondition", 1, OrgPartUnit.CountInDB(TestConnection));

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, OrgPartUnitSchema.Constants.TableName, TriggerName));

			sql.Clear();
			new OrgPartUnit(product1, 20m, "UNT", "BOX").AppendInsertAndReturnObject(sql);
			var ex = AssertExceptionThrown<SqlException>(() =>
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			});

			AssertContains("Cannot insert a new Unit Conversion as there is already an entry for the same Package & Parent Package.", ex.ToString(), ignoreCase: true);
		}

		public void TestTransformation_TempTriggerCreatedAndDroppedCorrectly()
		{
			var expectedTriggerDefinition = @"
CREATE TRIGGER dbo.TG_OrgPartUnit_PreventDuplicateUnit
	ON dbo.OrgPartUnit
	AFTER INSERT, UPDATE
AS
BEGIN
	IF (@@ROWCOUNT = 0) RETURN
	SET NOCOUNT ON;

	IF
	(
		EXISTS
		(
			SELECT
				NULL
			FROM
			(
				SELECT
					COUNT(*) as UniqueCount
				FROM
					dbo.OrgPartUnit
				WHERE
					OF_OP IN (SELECT OF_OP FROM inserted)
				GROUP BY
					OF_OP,
					OF_PackType,
					OF_ParentPackType
			) as UniqueCounts
			WHERE
				UniqueCount > 1
		)
	)
	BEGIN
		RAISERROR('Cannot insert a new Unit Conversion as there is already an entry for the same Package & Parent Package.', 16, 1)
		ROLLBACK
	END
END
";

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, OrgPartUnitSchema.Constants.TableName, TriggerName));
			AssertEquals("Trigger Definition is correct", expectedTriggerDefinition, GetTriggerDefinition(TriggerName));

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);

			AssertEquals("Trigger should be dropped.", false, DbObjectCreator.TriggerExists(TestConnection, OrgPartUnitSchema.Constants.TableName, TriggerName));
		}

		public void TestUserDescription()
		{
			AssertEquals("Remove duplicate OrgPartUnit.", GetNewTestTransformationInstance().UserDescription);
		}

		string GetTriggerDefinition(string triggerName)
		{
			return TestConnection.ExecuteScalar<string>($@"
SELECT
	TrgDefinition	= def.definition
FROM
	sys.triggers trg
	JOIN sys.sql_modules AS def ON def.object_id = trg.object_id
WHERE trg.name = '{triggerName}'
");
		}

		#region Implementation

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RemoveDuplicateOrgPartUnit();
		}

		#endregion
	}
}
