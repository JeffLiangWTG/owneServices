using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles
{
	[TestedType(typeof(TG_OrgPartRelation_DuplicateCheck))]
	class TG_OrgPartRelation_DuplicateCheckTest : DbCreateScriptTest
	{
		const string TriggerMessage = "Relationship between product (OP_PartNum) and owner (OU_OH) must be unique for active (OP_IsActive) products.";

		Guid part11PK;
		Guid part12PK;
		Guid part21PK;
		Guid part22PK;

		protected override void SetUp()
		{
			base.SetUp();

			var sql = new SqlQueryBuilder();
			var org1 = new OrgHeader("TESTORG1").AppendInsertAndReturnObject(sql);
			new OrgHeader("TESTORG2").AppendInsertAndReturnObject(sql);
			var part11 = new OrgSupplierPart("PART1").AppendInsertAndReturnObject(sql);
			var part12 = new OrgSupplierPart("PART1").AppendInsertAndReturnObject(sql);
			var part21 = new OrgSupplierPart("PART2").AppendInsertAndReturnObject(sql);
			var part22 = new OrgSupplierPart("PART2").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(org1, part11, "OWN").AppendInsertAndReturnObject(sql);
			part11PK = part11.PK;
			part12PK = part12.PK;
			part21PK = part21.PK;
			part22PK = part22.PK;

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		public void TestInsert()
		{
			var org1 = OrgHeader.ShallowLoadFromDB(TestConnection, h => h.OH_Code == "TESTORG1").Single();
			var part12 = OrgSupplierPart.ShallowLoadFromDB(TestConnection, p => p.PK == part12PK).Single();

			AssertExceptionThrown(typeof(SqlException), TriggerMessage, () => new OrgPartRelation(org1, part12, "BTH").Insert(TestConnection), true);
		}

		public void TestInsert_DuplicateAmongNewRows()
		{
			var org1 = OrgHeader.ShallowLoadFromDB(TestConnection, h => h.OH_Code == "TESTORG1").Single();
			var part21 = OrgSupplierPart.ShallowLoadFromDB(TestConnection, p => p.PK == part21PK).Single();
			var part22 = OrgSupplierPart.ShallowLoadFromDB(TestConnection, p => p.PK == part22PK).Single();

			var sql = new SqlQueryBuilder();
			new OrgPartRelation(org1, part21, "OWN").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(org1, part22, "BTH").AppendInsertAndReturnObject(sql);

			AssertExceptionThrown(typeof(SqlException), TriggerMessage, () => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends()), true);
		}

		public void TestInsert_SelfDuplicte()
		{
			var org1 = OrgHeader.ShallowLoadFromDB(TestConnection, h => h.OH_Code == "TESTORG1").Single();
			var part11 = OrgSupplierPart.ShallowLoadFromDB(TestConnection, p => p.PK == part11PK).Single();
			AssertNoExceptionThrown("triggers only check 2 different duplicate products, self-duplicates are separate issue", () => new OrgPartRelation(org1, part11, "BTH").Insert(TestConnection));
		}

		public void TestInsert_Deactivated()
		{
			var org1 = OrgHeader.ShallowLoadFromDB(TestConnection, h => h.OH_Code == "TESTORG1").Single();
			var part11 = OrgSupplierPart.ShallowLoadFromDB(TestConnection, p => p.PK == part11PK).Single();
			var part12 = OrgSupplierPart.ShallowLoadFromDB(TestConnection, p => p.PK == part12PK).Single();

			var sql = new SqlQueryBuilder();
			sql.Append(OrgSupplierPart
				.UpdateWhere(part11PK)
				.Set(p => p.OP_IsActive, false).AsSQL());
			new OrgPartRelation(org1, part11, "BTH").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(org1, part12, "BTH").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("no collisions should be detected between deactivated parts", true);
		}

		public void TestUpdate_OU_OP()
		{
			var org1 = OrgHeader.ShallowLoadFromDB(TestConnection, h => h.OH_Code == "TESTORG1").Single();
			var part21 = OrgSupplierPart.ShallowLoadFromDB(TestConnection, p => p.PK == part21PK).Single();
			var part12 = OrgSupplierPart.ShallowLoadFromDB(TestConnection, p => p.PK == part12PK).Single();

			var newRel = new OrgPartRelation(org1, part21, "BTH").InsertAndReturnObject(TestConnection);
			AssertExceptionThrown(typeof(SqlException), TriggerMessage,
				() => OrgPartRelation
				.UpdateWhere(newRel.PK)
				.Set(r => r.OU_OP, part12)
				.Post(TestConnection), true);
		}

		public void TestUpdate_OU_OH()
		{
			var org1 = OrgHeader.ShallowLoadFromDB(TestConnection, h => h.OH_Code == "TESTORG1").Single();
			var org2 = OrgHeader.ShallowLoadFromDB(TestConnection, h => h.OH_Code == "TESTORG2").Single();
			var part12 = OrgSupplierPart.ShallowLoadFromDB(TestConnection, p => p.PK == part12PK).Single();

			var newRel = new OrgPartRelation(org2, part12, "BTH").InsertAndReturnObject(TestConnection);
			AssertExceptionThrown(typeof(SqlException), TriggerMessage,
				() => OrgPartRelation
				.UpdateWhere(newRel.PK)
				.Set(r => r.OU_OH, org1)
				.Post(TestConnection), true);
		}

		public void TestUpdate_OU_Relationship()
		{
			var org1 = OrgHeader.ShallowLoadFromDB(TestConnection, h => h.OH_Code == "TESTORG1").Single();
			var part12 = OrgSupplierPart.ShallowLoadFromDB(TestConnection, p => p.PK == part12PK).Single();

			var newRel = new OrgPartRelation(org1, part12, "WCN").InsertAndReturnObject(TestConnection);
			AssertExceptionThrown(typeof(SqlException), TriggerMessage,
				() => OrgPartRelation
				.UpdateWhere(newRel.PK)
				.Set(r => r.OU_Relationship, "BTH")
				.Post(TestConnection), true);
		}

		public void TestUpdate_SelfDuplicate()
		{
			var org1 = OrgHeader.ShallowLoadFromDB(TestConnection, h => h.OH_Code == "TESTORG1").Single();
			var org2 = OrgHeader.ShallowLoadFromDB(TestConnection, h => h.OH_Code == "TESTORG2").Single();
			var part11 = OrgSupplierPart.ShallowLoadFromDB(TestConnection, p => p.PK == part11PK).Single();

			var newRel = new OrgPartRelation(org2, part11, "BTH").InsertAndReturnObject(TestConnection);
			AssertNoExceptionThrown("triggers only check 2 different duplicate products, self-duplicates are separate issue", () =>  OrgPartRelation
				.UpdateWhere(newRel.PK)
				.Set(r => r.OU_OH, org1)
				.Post(TestConnection));
		}

		public void TestUpdate_Deactivated()
		{
			var org1 = OrgHeader.ShallowLoadFromDB(TestConnection, h => h.OH_Code == "TESTORG1").Single();
			var org2 = OrgHeader.ShallowLoadFromDB(TestConnection, h => h.OH_Code == "TESTORG2").Single();
			var part11 = OrgSupplierPart.ShallowLoadFromDB(TestConnection, p => p.PK == part11PK).Single();
			var part12 = OrgSupplierPart.ShallowLoadFromDB(TestConnection, p => p.PK == part12PK).Single();
			var part21 = OrgSupplierPart.ShallowLoadFromDB(TestConnection, p => p.PK == part21PK).Single();

			OrgSupplierPart
				.UpdateWhere(part11PK)
				.Set(p => p.OP_IsActive, false).Post(TestConnection);
			var newRel = new OrgPartRelation(org2, part21, "WCN").InsertAndReturnObject(TestConnection);
			OrgPartRelation
				.UpdateWhere(newRel.PK)
				.Set(p => p.OU_OP, part11)
				.Set(p => p.OU_OH, org1)
				.Set(p => p.OU_Relationship, "BTH").Post(TestConnection);
			OrgPartRelation
				.UpdateWhere(newRel.PK)
				.Set(p => p.OU_OP, part21)
				.Set(p => p.OU_OH, org2)
				.Set(p => p.OU_Relationship, "WCN").Post(TestConnection);
			OrgPartRelation
				.UpdateWhere(newRel.PK)
				.Set(p => p.OU_OP, part12)
				.Set(p => p.OU_OH, org1)
				.Set(p => p.OU_Relationship, "BTH").Post(TestConnection);

			Assert("no collisions should be detected between deactivated parts", true);
		}

		public void TestMessageIncludesProductCodes()
		{
			var part3 = new OrgSupplierPart("PART3").InsertAndReturnObject(TestConnection);

			var org1 = OrgHeader.ShallowLoadFromDB(TestConnection, h => h.OH_Code == "TESTORG1").Single();
			var part12 = OrgSupplierPart.ShallowLoadFromDB(TestConnection, p => p.PK == part12PK).Single();
			var part21 = OrgSupplierPart.ShallowLoadFromDB(TestConnection, p => p.PK == part21PK).Single();
			var part22 = OrgSupplierPart.ShallowLoadFromDB(TestConnection, p => p.PK == part22PK).Single();

			var message = AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery($@"
				insert into dbo.OrgPartRelation(OU_PK, OU_OP, OU_OH, OU_Relationship, OU_SystemCreateTimeUtc, OU_SystemCreateUser, OU_SystemLastEditTimeUtc, OU_SystemLastEditUser) values
					('{Guid.NewGuid()}', '{part12PK}', '{org1.PK}', 'OWN', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					('{Guid.NewGuid()}', '{part21PK}', '{org1.PK}', 'OWN', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					('{Guid.NewGuid()}', '{part22PK}', '{org1.PK}', 'OWN', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					('{Guid.NewGuid()}', '{part3.PK}', '{org1.PK}', 'OWN', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
			")).Message;

			AssertContains(TriggerMessage, message);
			AssertContains("PART1", message);
			AssertContains("PART2", message);
			AssertNotContains("PART3", message);
		}
	}

	class TG_OrgPartRelation_DuplicateCheckTest_WithoutSetup : TestCase
	{
		const string TriggerMessage = "Relationship between product (OP_PartNum) and owner (OU_OH) must be unique for active (OP_IsActive) products.";

		[UseSnapshotProtection]
		public void TestTriggerShouldReadUncommittedData()
		{
			CombineAssertions(() =>
			{
				using (var connA = Db.NewExtraConnectionToMainDb())
				{
					var org = OrgHeader.ShallowLoadFromDB(connA).First();

					// TG_OrgPartRelation_UpdateOrgPartRelationUnitsPerClientUQ calls a stored proc that updates OrgPartRelation
					// On an empty database, this can result in index/table locks rather than row locks which will prevent insertion of the rest of the test data.
					connA.ExecuteNonQuery("DROP TRIGGER TG_OrgPartRelation_UpdateOrgPartRelationUnitsPerClientUQ");

					using (var tranA = connA.BeginTransactionWithManager())
					{
						var partA = new OrgSupplierPart("PART").InsertAndReturnObject(connA);
						new OrgPartRelation(org, partA, "OWN").Insert(connA);

						using (var connB = Db.NewExtraConnectionToMainDb())
						{
							using (var tranB = connB.BeginTransactionWithManager())
							{
								var partB = new OrgSupplierPart("PART").InsertAndReturnObject(connB);
								var exception = AssertExceptionThrown<SqlException>(() => new OrgPartRelation(org, partB, "OWN").Insert(connB));
								AssertContains(TriggerMessage, exception?.Message);
								tranB.RollbackTransaction();
							}
						}

						tranA.RollbackTransaction();
					}
				}
			});
		}
	}
}
