using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.SalesAndMarketing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.SalesAndMarketing
{
	[TestedType(typeof(UpdateGlbCompanyCampaignBudgetItemNegativeValuesToZero))]
	class UpdateGlbCompanyCampaignBudgetItemNegativeValuesToZeroTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateGlbCompanyCampaignBudgetItemNegativeValuesToZero();
		}

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(GlbCompanyCampaignBudgetItemSchema.Constants.TableName, "Constraint_G9_ExchangeRate");
			DBTransformationTestHelper.DropConstraintIfExists(GlbCompanyCampaignBudgetItemSchema.Constants.TableName, "Constraint_G9_FlatAmount");
			DBTransformationTestHelper.DropConstraintIfExists(GlbCompanyCampaignBudgetItemSchema.Constants.TableName, "Constraint_G9_PerUnitAmount");

			var helper = new TransformationTestDataCreator();
			var companyPK = helper.CreateCompany(Guid.NewGuid(), "ABC", "AU");
			var campaignPK = helper.CreateGlbCompanyCampaign("testGlbCompanyCampaign", "LCT", companyPK, "CRT00000001");

			testGlbCompanyCampaignBudgetItem1PK = helper.CreateGlbCompanyCampaignBudgetItem(campaignPK, 1, 2, 3);
			testGlbCompanyCampaignBudgetItem2PK = helper.CreateGlbCompanyCampaignBudgetItem(campaignPK, -1, -2, -3);
		}

		protected override void AssertTransformationResults()
		{
			var testAllNonNegative = GetDataRow(testGlbCompanyCampaignBudgetItem1PK);
			CombineAssertions("All non-negative column values should not be changed.", () =>
			{
				AssertEquals((decimal)1, testAllNonNegative["G9_ExchangeRate"]);
				AssertEquals((decimal)2, testAllNonNegative["G9_FlatAmount"]);
				AssertEquals((decimal)3, testAllNonNegative["G9_PerUnitAmount"]);
			});

			var testAllNegative = GetDataRow(testGlbCompanyCampaignBudgetItem2PK);
			CombineAssertions("All negative column values should be replaced with 0.", () =>
			{
				AssertEquals((decimal)0, testAllNegative["G9_ExchangeRate"]);
				AssertEquals((decimal)0, testAllNegative["G9_FlatAmount"]);
				AssertEquals((decimal)0, testAllNegative["G9_PerUnitAmount"]);
			});
		}

		DataRow GetDataRow(Guid pK)
		{
			var dataTable = new DataTable();
			var sql = $"SELECT G9_ExchangeRate, G9_FlatAmount, G9_PerUnitAmount FROM dbo.GlbCompanyCampaignBudgetItem WHERE G9_PK = '{pK}'";
			using (var cmd = Db.Connection.Command(sql))
			using (var adapter = cmd.NewDataAdapter())
			{
				adapter.Fill(dataTable);
			}

			return dataTable.Rows[0];
		}

		Guid testGlbCompanyCampaignBudgetItem1PK;
		Guid testGlbCompanyCampaignBudgetItem2PK;
	}
}
