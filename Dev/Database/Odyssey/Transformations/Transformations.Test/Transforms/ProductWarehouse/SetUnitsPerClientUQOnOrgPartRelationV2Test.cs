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
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.ProductWarehouse
{
	[TestedType(typeof(SetUnitsPerClientUQOnOrgPartRelationV2))]
	class SetUnitsPerClientUQOnOrgPartRelationTestV2 : DataTransformationTestCase
	{
		public void TestBatching()
		{
			var sql1 = new SqlQueryBuilder();
			var products = new OrgSupplierPart[2100];

			// add multiple batches of products that will get filtered out
			for (var index = 0; index < 2100; index++)
			{
				products[index] = new OrgSupplierPart($"P{index}") { OP_StockKeepingUnit = string.Empty }.AppendInsertAndReturnObject(sql1);
			}

			var product1 = new OrgSupplierPart("OTH") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql1);
			TestConnection.ExecuteNonQuery(sql1.ToStringWithNewLineBetweenAppends());

			var newProduct1 = OrgSupplierPart.UpdateWhere(product1.PK).Set(l => l.PK, Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")).Post(TestConnection);

			var sql2 = new SqlQueryBuilder();
			var client1 = new OrgHeader("C1").AppendInsertAndReturnObject(sql2);
			new OrgPartUnit(newProduct1, 10, "UNT", "BOX").AppendInsertAndReturnObject(sql2);
			var partRelation = new OrgPartRelation(client1, newProduct1, "OWN") { OU_ClientUQ = "BOX", OU_SystemLastEditTimeUtc = null }.AppendInsertAndReturnObject(sql2);
			TestConnection.ExecuteNonQuery(sql2.ToStringWithNewLineBetweenAppends());

			var transformation = GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			OrgPartRelation
				.AssertFromDB(TestConnection, partRelation.PK)
				.ExpectEquals(nameof(OrgPartRelation.OU_UnitsPerClientUQ), r => r.OU_UnitsPerClientUQ, 0.1m)
				.VerifyAll();
		}

		public void TestEmptyUQs()
		{
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);

			// Both empty
			var product1 = new OrgSupplierPart("P1") { OP_StockKeepingUnit = string.Empty }.AppendInsertAndReturnObject(sql);
			new OrgPartUnit(product1, 10, "UNT", "BOX").AppendInsertAndReturnObject(sql);
			var partRelation1 = new OrgPartRelation(client, product1, "OWN") { OU_ClientUQ = string.Empty, OU_SystemLastEditTimeUtc = null }.AppendInsertAndReturnObject(sql);

			// OP_StockKeepingUnit empty
			var product2 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = string.Empty }.AppendInsertAndReturnObject(sql);
			new OrgPartUnit(product2, 20, "CAS", "BOX").AppendInsertAndReturnObject(sql);
			var partRelation2 = new OrgPartRelation(client, product2, "OWN") { OU_ClientUQ = "BOX", OU_SystemLastEditUser = "B", OU_SystemLastEditTimeUtc = new DateTime(2024, 6, 16, 0, 0, 0) }.AppendInsertAndReturnObject(sql);

			// OP_StockKeepingUnit
			var product3 = new OrgSupplierPart("P3") { OP_StockKeepingUnit = "BOX" }.AppendInsertAndReturnObject(sql);
			new OrgPartUnit(product3, 10, "UNT", "BOX").AppendInsertAndReturnObject(sql);
			var partRelation3 = new OrgPartRelation(client, product3, "OWN") { OU_ClientUQ = string.Empty, OU_SystemLastEditTimeUtc = null }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var transformation = GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			OrgPartRelation
				.AssertFromDB(TestConnection, partRelation1.PK)
				.ExpectEquals(nameof(OrgPartRelation.OU_UnitsPerClientUQ), r => r.OU_UnitsPerClientUQ, 0m)
				.VerifyAll();

			OrgPartRelation
				.AssertFromDB(TestConnection, partRelation1.PK)
				.ExpectEquals(nameof(OrgPartRelation.OU_UnitsPerClientUQ), r => r.OU_UnitsPerClientUQ, 0m)
				.VerifyAll();

			OrgPartRelation
				.AssertFromDB(TestConnection, partRelation1.PK)
				.ExpectEquals(nameof(OrgPartRelation.OU_UnitsPerClientUQ), r => r.OU_UnitsPerClientUQ, 0m)
				.VerifyAll();
		}

		public void TestRunCancellationAndExtProperty()
		{
			PrepareTestData();
			var logger = new List<string>();
			var cancellationToken = new CancellationToken(true);
			var transformation = GetNewTestTransformationInstance();

			AssertNull($"ExtProperty '{StoredFromPKName}' should be empty", ExtProperty.Database.Select(Db.Connection, StoredFromPKName));
			AssertNull($"ExtProperty '{StoredTotalUpdatedCount}' should be empty", ExtProperty.Database.Select(Db.Connection, StoredTotalUpdatedCount));
			AssertExceptionThrown<OperationCanceledException>(() => ((IOnlineTransformation)transformation).Run(s => logger.Add(s), cancellationToken));
			AssertContainsExactElementsInExactOrder(new[] { "Processed 2 Products." }, logger);

			AssertEquals($"ExtProperty '{StoredFromPKName}' should be max PK", "ffffffff-ffff-ffff-ffff-ffffffffffff", ExtProperty.Database.Select(Db.Connection, StoredFromPKName));
			AssertEquals($"ExtProperty '{StoredTotalUpdatedCount}' should be 2", "2", ExtProperty.Database.Select(Db.Connection, StoredTotalUpdatedCount));

			transformation.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			AssertNull($"ExtProperty '{StoredFromPKName}' should be cleared", ExtProperty.Database.Select(Db.Connection, StoredFromPKName));
			AssertNull($"ExtProperty '{StoredTotalUpdatedCount}' should be cleared", ExtProperty.Database.Select(Db.Connection, StoredTotalUpdatedCount));
		}

		protected override void PrepareTestData()
		{
			var sql = new SqlQueryBuilder();
			var client1 = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
			var product1 = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			new OrgPartUnit(product1, 10, "UNT", "BOX").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(client1, product1, "OWN") { OU_ClientUQ = "BOX", OU_SystemLastEditTimeUtc = null }.AppendInsertAndReturnObject(sql);

			var client2 = new OrgHeader("C2").AppendInsertAndReturnObject(sql);
			var product2 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "CAS" }.AppendInsertAndReturnObject(sql);
			new OrgPartUnit(product2, 20, "CAS", "BOX").AppendInsertAndReturnObject(sql);
			new OrgPartUnit(product2, 10, "UNT", "BOX").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(client2, product2, "OWN") { OU_ClientUQ = "BOX", OU_SystemLastEditUser = "B", OU_SystemLastEditTimeUtc = new DateTime(2024, 6, 16, 0, 0, 0) }.AppendInsertAndReturnObject(sql);

			new OrgPartRelation(client2, product1, "OWN") { OU_ClientUQ = "CAS", OU_SystemLastEditUser = "B", OU_SystemLastEditTimeUtc = new DateTime(2024, 6, 16, 0, 0, 0) }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		protected override void AssertTransformationResults()
		{
			var product1 = OrgSupplierPart.ShallowLoadFromDB(TestConnection, p => p.OP_PartNum == "P1").Single();
			var client1 = OrgHeader.ShallowLoadFromDB(TestConnection, o => o.OH_Code == "C1").Single();
			var clientProductRelationAfterTransformation1 = OrgPartRelation.ShallowLoadFromDB(TestConnection, p => (p.OU_OP == product1.PK && p.OU_OH == client1.PK)).Single();

			var product2 = OrgSupplierPart.ShallowLoadFromDB(TestConnection, p => p.OP_PartNum == "P2").Single();
			var client2 = OrgHeader.ShallowLoadFromDB(TestConnection, o => o.OH_Code == "C2").Single();
			var clientProductRelationAfterTransformation2 = OrgPartRelation.ShallowLoadFromDB(TestConnection, p => (p.OU_OP == product2.PK && p.OU_OH == client2.PK)).Single();

			var clientProductRelationAfterTransformation3 = OrgPartRelation.ShallowLoadFromDB(TestConnection, p => (p.OU_OP == product1.PK && p.OU_OH == client2.PK)).Single();

			AssertEquals("Transform should set OU_UnitsPerClientUQ to 0.1", 0.1m, clientProductRelationAfterTransformation1.OU_UnitsPerClientUQ);
			AssertNotNull("Transform should set OU_SystemLastEditTimeUtc to non null", clientProductRelationAfterTransformation1.OU_SystemLastEditTimeUtc);
			AssertEquals("Transform should set OU_SystemLastEditUser to A", "A", clientProductRelationAfterTransformation1.OU_SystemLastEditUser);

			AssertEquals("Transform should set OU_UnitsPerClientUQ to 0.05", 0.05m, clientProductRelationAfterTransformation2.OU_UnitsPerClientUQ);
			AssertGreaterThan("Transform should set OU_SystemLastEditTimeUtc to be greater than 2024-06-16 00:00:00", (DateTime)clientProductRelationAfterTransformation2.OU_SystemLastEditTimeUtc, new DateTime(2024, 6, 16, 0, 0, 0));
			AssertEquals("Transform should set OU_SystemLastEditUser to B", "B", clientProductRelationAfterTransformation2.OU_SystemLastEditUser);

			AssertEquals("Transform should keep OU_UnitsPerClientUQ unchanged (0)", 0m, clientProductRelationAfterTransformation3.OU_UnitsPerClientUQ);
			AssertEquals("Transform should keep OU_SystemLastEditTimeUtc unchanged (2024-06-16 00:00:00)", clientProductRelationAfterTransformation3.OU_SystemLastEditTimeUtc, new DateTime(2024, 6, 16, 0, 0, 0));
			AssertEquals("Transform should keep OU_SystemLastEditUser unchanged (B)", "B", clientProductRelationAfterTransformation3.OU_SystemLastEditUser);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DbObjectCreator.DropTriggerIfExists(Db.Connection, "TG_OrgPartRelation_UpdateOrgPartRelationUnitsPerClientUQ");
			DbObjectCreator.DropTriggerIfExists(Db.Connection, "TG_OrgPartUnit_UpdateOrgPartRelationUnitsPerClientUQ");
			DbObjectCreator.DropTriggerIfExists(Db.Connection, "TG_OrgSupplierPart_UpdateOrgPartRelationUnitsPerClientUQ");
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new SetUnitsPerClientUQOnOrgPartRelationV2();

		const string StoredFromPKName = "SetUnitsPerClientUQOnOrgPartRelation.NewFromPK";
		const string StoredTotalUpdatedCount = "SetUnitsPerClientUQOnOrgPartRelation.NewTotalUpdatedCount";
	}
}
