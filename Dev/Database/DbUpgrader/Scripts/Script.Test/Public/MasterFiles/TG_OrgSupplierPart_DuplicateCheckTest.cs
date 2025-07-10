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
	[TestedType(typeof(TG_OrgSupplierPart_DuplicateCheck))]
	class TG_OrgSupplierPart_DuplicateCheckTest : DbCreateScriptTest
	{
		const string TriggerMessage = "Relationship between product (OP_PartNum) and owner (OU_OH) must be unique for active (OP_IsActive) products.";

		Guid part10PK;
		Guid part11PK;
		Guid part21PK;

		protected override void SetUp()
		{
			base.SetUp();

			var sql = new SqlQueryBuilder();
			var org = new OrgHeader("TESTORG").AppendInsertAndReturnObject(sql);
			var part10 = new OrgSupplierPart("PART1") { OP_IsActive = false }.AppendInsertAndReturnObject(sql);
			var part11 = new OrgSupplierPart("PART1").AppendInsertAndReturnObject(sql);
			var part21 = new OrgSupplierPart("PART2").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(org, part10, "OWN").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(org, part11, "OWN").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(org, part21, "OWN").AppendInsertAndReturnObject(sql);
			part10PK = part10.PK;
			part11PK = part11.PK;
			part21PK = part21.PK;

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		public void TestUpdate_OP_PartNum()
		{
			AssertExceptionThrown(typeof(SqlException), TriggerMessage,
				() => OrgSupplierPart
				.UpdateWhere(part21PK)
				.Set(p => p.OP_PartNum, "PART1")
				.Post(TestConnection), true);
		}

		public void TestUpdate_OP_IsActive()
		{
			AssertExceptionThrown(typeof(SqlException), TriggerMessage,
				() => OrgSupplierPart
				.UpdateWhere(part10PK)
				.Set(p => p.OP_IsActive, true)
				.Post(TestConnection), true);
		}

		public void TestUpdate_Deactivated()
		{
			AssertNoExceptionThrown(() => OrgSupplierPart
				.UpdateWhere(part11PK)
				.Set(p => p.OP_PartNum, "PART2")
				.Set(p => p.OP_IsActive, false).Post(TestConnection));
			AssertNoExceptionThrown(() => OrgSupplierPart
				.UpdateWhere(part21PK)
				.Set(p => p.OP_PartNum, "PART1")
				.Set(p => p.OP_IsActive, true).Post(TestConnection));

			Assert("no collisions should be detected between deactivated parts", true);
		}

		public void TestUpdate_SelfDuplicate()
		{
			var org = OrgHeader.ShallowLoadFromDB(TestConnection, h => h.OH_Code == "TESTORG").Single();
			var part21 = OrgSupplierPart.ShallowLoadFromDB(TestConnection, p => p.PK == part21PK).Single();

			var sql = new SqlQueryBuilder();
			sql.Append(OrgSupplierPart
				.UpdateWhere(part21PK)
				.Set(p => p.OP_IsActive, false).AsSQL());
			new OrgPartRelation(org, part21, "BTH").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown("triggers only check 2 different duplicate products, self-duplicates are separate issue",
				() => OrgSupplierPart
				.UpdateWhere(part21PK)
				.Set(p => p.OP_IsActive, true).Post(TestConnection));
		}

		public void TestUpdate_CanFixDuplicate()
		{
			var sql = new SqlQueryBuilder();
			sql.Append("DISABLE TRIGGER TG_OrgSupplierPart_DuplicateCheck ON OrgSupplierPart;");
			sql.Append(OrgSupplierPart
				.UpdateWhere(p => p.PK == part10PK || p.PK == part11PK || p.PK == part21PK)
				.Set(p => p.OP_PartNum, "PART1")
				.Set(p => p.OP_IsActive, true).AsSQL() + ";");
			sql.Append("ENABLE TRIGGER TG_OrgSupplierPart_DuplicateCheck ON OrgSupplierPart;");
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			OrgSupplierPart
				.UpdateWhere(part10PK)
				.Set(p => p.OP_IsActive, false).Post(TestConnection);
			OrgSupplierPart
				.UpdateWhere(part21PK)
				.Set(p => p.OP_PartNum, "PART2").Post(TestConnection);

			Assert("duplicates should be fixable can by renaming and deactivating products", true);

			AssertExceptionThrown("making sure that duplicates were fixed under enabled trigger", typeof(SqlException), TriggerMessage,
				() => OrgSupplierPart
				.UpdateWhere(part10PK)
				.Set(p => p.OP_IsActive, true)
				.Post(TestConnection), true);
		}

		public void TestMessageIncludesProductCodes()
		{
			var org = OrgHeader.ShallowLoadFromDB(TestConnection, h => h.OH_Code == "TESTORG").Single();

			var sql = new SqlQueryBuilder();
			var part20 = new OrgSupplierPart("PART2") { OP_IsActive = false }.AppendInsertAndReturnObject(sql);
			var part3 = new OrgSupplierPart("PART3") { OP_IsActive = false }.AppendInsertAndReturnObject(sql);
			new OrgPartRelation(org, part20, "OWN").AppendInsertAndReturnObject(sql);
			new OrgPartRelation(org, part3, "OWN").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var message = AssertExceptionThrown<SqlException>(
				() => OrgSupplierPart
				.UpdateWhere(p => p.PK == part10PK || p.PK == part20.PK || p.PK == part3.PK)
				.Set(p => p.OP_IsActive, true).Post(TestConnection)).Message;

			AssertContains(TriggerMessage, message);
			AssertContains("PART1", message);
			AssertContains("PART2", message);
			AssertNotContains("PART3", message);
		}
	}

	class TG_OrgSupplierPart_DuplicateCheckTest_WithoutSetup : TestCase
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
						var partA = new OrgSupplierPart("PARTA").InsertAndReturnObject(connA);
						new OrgPartRelation(org, partA, "OWN").InsertAndReturnObject(connA);

						using (var connB = Db.NewExtraConnectionToMainDb())
						{
							using (var tranB = connB.BeginTransactionWithManager())
							{
								var partB = new OrgSupplierPart("PARTB").InsertAndReturnObject(connB);
								new OrgPartRelation(org, partB, "OWN").InsertAndReturnObject(connB);

								var exception = AssertExceptionThrown<SqlException>(
									() => OrgSupplierPart
									.UpdateWhere(partB.PK)
									.Set(p => p.OP_PartNum, "PARTA").Post(connB));

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
