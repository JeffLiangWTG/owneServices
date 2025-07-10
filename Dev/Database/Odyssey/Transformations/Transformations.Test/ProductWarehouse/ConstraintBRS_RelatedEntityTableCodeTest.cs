using System;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ProductWarehouse
{
	[TestedType(typeof(ConstraintBRS_RelatedEntityTableCode))]
	sealed class ConstraintBRS_RelatedEntityTableCodeTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new ConstraintBRS_RelatedEntityTableCode();

		protected override void PrepareTestData()
		{
			Db.Connection.ExecuteNonQuery(DeleteDefaultOdysseyBarcodeRulesets);

			AssertEquals("Precondition: Odyssey default rows are deleted", 0, BarcodeRuleSet.CountInDB(TestConnection));
			AssertEquals("Precondition: Odyssey default rows are deleted", 0, BarcodeRule.CountInDB(TestConnection));
			AssertEquals("Precondition: Odyssey default rows are deleted", 0, BarcodeRuleComponent.CountInDB(TestConnection));
			AssertEquals("Precondition: Odyssey default rows are deleted", 0, BarcodeValidationRule.CountInDB(TestConnection));

			Db.Connection.ExecuteNonQuery("ALTER TABLE BarcodeRuleSet DROP CONSTRAINT IF EXISTS Constraint_RelatedEntityTableCode");

			var sql = new SqlQueryBuilder();

			var buyerOrg = new OrgHeader("Buyer").AppendInsertAndReturnObject(sql);
			AppendBarcodeRulesetRulesAndValidation(sql, "WHS", "", null, buyerOrg.PK);
			AppendBarcodeRulesetRulesAndValidation(sql, "WHS", "OP", Guid.NewGuid(), buyerOrg.PK);
			AppendBarcodeRulesetRulesAndValidation(sql, "WHS", "ZZZ", Guid.NewGuid(), buyerOrg.PK);
			AppendBarcodeRulesetRulesAndValidation(sql, "WHS", "VV", Guid.NewGuid(), buyerOrg.PK);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(4, BarcodeRuleSet.CountInDB(TestConnection, row => row.BRS_OH_Buyer != null));
			AssertEquals(4, BarcodeRule.CountInDB(TestConnection));
			AssertEquals(4, BarcodeRuleComponent.CountInDB(TestConnection));
			AssertEquals(4, BarcodeValidationRule.CountInDB(TestConnection));

			AssertEquals(1, BarcodeRuleSet.CountInDB(TestConnection, row => row.BRS_RelatedEntityTableCode == ""));
			AssertEquals("Invalid TableCodes are converted to OP", 3, BarcodeRuleSet.CountInDB(TestConnection, row => row.BRS_RelatedEntityTableCode == "OP"));

			AssertEquals(1, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM BarcodeValidationRule JOIN BarcodeRuleSet ON BRS_PK = BVR_BRS_RuleSet AND BRS_RelatedEntityTableCode = ''"));
			AssertEquals(3, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM BarcodeValidationRule JOIN BarcodeRuleSet ON BRS_PK = BVR_BRS_RuleSet AND BRS_RelatedEntityTableCode = 'OP'"));

			AssertEquals(1, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM BarcodeRule JOIN BarcodeRuleSet ON BRS_PK = BRU_BRS_RuleSet AND BRS_RelatedEntityTableCode = ''"));
			AssertEquals(3, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM BarcodeRule JOIN BarcodeRuleSet ON BRS_PK = BRU_BRS_RuleSet AND BRS_RelatedEntityTableCode = 'OP'"));

			AssertEquals(1, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM BarcodeRuleComponent JOIN BarcodeRule ON BRU_PK = BRC_BRU_Rule JOIN BarcodeRuleSet ON BRS_PK = BRU_BRS_RuleSet AND BRS_RelatedEntityTableCode = ''"));
			AssertEquals(3, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM BarcodeRuleComponent JOIN BarcodeRule ON BRU_PK = BRC_BRU_Rule JOIN BarcodeRuleSet ON BRS_PK = BRU_BRS_RuleSet AND BRS_RelatedEntityTableCode = 'OP'"));
		}

		public void TestIsOfflinePostUpgrade()
		{
			PrepareTestData();
			AssertEquals("Precondition", 4, BarcodeRuleSet.CountInDB(TestConnection));
			AssertEquals("Precondition", 1, BarcodeRuleSet.CountInDB(TestConnection, row => row.BRS_RelatedEntityTableCode == ""));
			AssertEquals("Precondition", 1, BarcodeRuleSet.CountInDB(TestConnection, row => row.BRS_RelatedEntityTableCode == "OP"));

			var transform = GetNewTestTransformationInstance();

			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			AssertEquals("No modification", 1, BarcodeRuleSet.CountInDB(TestConnection, row => row.BRS_RelatedEntityTableCode == "OP"));

			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			AssertEquals("No modification", 1, BarcodeRuleSet.CountInDB(TestConnection, row => row.BRS_RelatedEntityTableCode == "OP"));

			transform.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertEquals("Invalid data deleted", 3, BarcodeRuleSet.CountInDB(TestConnection, row => row.BRS_RelatedEntityTableCode == "OP"));

			AssertTransformationResults();
		}

		static void AppendBarcodeRulesetRulesAndValidation(SqlQueryBuilder sql, string module, string relatedEntityTableCode, Guid? relatedEntityId, Guid buyerOrgPK)
		{
			var ruleset = new BarcodeRuleSet(module) { BRS_RelatedEntityTableCode = relatedEntityTableCode, BRS_RelatedEntityId = relatedEntityId, BRS_OH_Buyer = buyerOrgPK }.AppendInsertAndReturnObject(sql);
			var rule = new BarcodeRule(ruleset, "ProductID", 1) { BRU_Terminator = "." }.AppendInsertAndReturnObject(sql);
			new BarcodeRuleComponent(rule, "ANS", "PRC") { BRC_ApplicationID = "ProductID" }.AppendInsertAndReturnObject(sql);
			var validation = new BarcodeValidationRule(ruleset, "ANY", "PRC").AppendInsertAndReturnObject(sql);
		}

		public override string[] expectedIndex =>
		[
			"NONCLUSTERED INDEX [IX_ConstraintBRS_RelatedEntityTableCode_Invalid_RelatedEntityTableCode] ON [dbo].[BarcodeRuleSet] ([BRS_RelatedEntityTableCode]) INCLUDE ([BRS_SystemLastEditTimeUtc], [BRS_SystemLastEditUser]) WHERE ([BRS_RelatedEntityTableCode]<>'' AND [BRS_RelatedEntityTableCode]<>'OP') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
		];

		const string DeleteDefaultOdysseyBarcodeRulesets = @"
DELETE BarcodeRuleComponent
DELETE BarcodeRule
DELETE BarcodeRuleSet
";
	}
}
