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
	[TestedType(typeof(UpdateGlbCompanyCampaignNegativeValuesToZero))]
	class UpdateGlbCompanyCampaignNegativeValuesToZeroTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateGlbCompanyCampaignNegativeValuesToZero();
		}

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(GlbCompanyCampaignSchema.Constants.TableName, "Constraint_G0_BatchCountDefault");
			DBTransformationTestHelper.DropConstraintIfExists(GlbCompanyCampaignSchema.Constants.TableName, "Constraint_G0_LastSentBatchNumber");
			DBTransformationTestHelper.DropConstraintIfExists(GlbCompanyCampaignSchema.Constants.TableName, "Constraint_G0_QuestionsPerWebPage");

			var helper = new TransformationTestDataCreator();
			var companyPK = helper.CreateCompany(Guid.NewGuid(), "ABC", "AU");

			testGlbCompanyCampaign1PK = helper.CreateGlbCompanyCampaign("testGlbCompanyCampaign", "LCT", companyPK, "CRT00000001", 1, 2, 3);
			testGlbCompanyCampaign2PK = helper.CreateGlbCompanyCampaign("testGlbCompanyCampaign", "LCT", companyPK, "CRT00000002", -1, -2, -3);
		}

		protected override void AssertTransformationResults()
		{
			var testAllNonNegative = GetDataRow(testGlbCompanyCampaign1PK);
			CombineAssertions("All non-negative column values should not be changed.", () =>
			{
				AssertEquals(1, testAllNonNegative["G0_BatchCountDefault"]);
				AssertEquals(2, testAllNonNegative["G0_LastSentBatchNumber"]);
				AssertEquals(3, testAllNonNegative["G0_QuestionsPerWebPage"]);
			});

			var testAllNegative = GetDataRow(testGlbCompanyCampaign2PK);
			CombineAssertions("All negative column values should be replaced with 0.", () =>
			{
				AssertEquals(0, testAllNegative["G0_BatchCountDefault"]);
				AssertEquals(0, testAllNegative["G0_LastSentBatchNumber"]);
				AssertEquals(0, testAllNegative["G0_QuestionsPerWebPage"]);
			});
		}

		DataRow GetDataRow(Guid pK)
		{
			var dataTable = new DataTable();
			var sql = $"SELECT G0_BatchCountDefault, G0_LastSentBatchNumber, G0_QuestionsPerWebPage FROM dbo.GlbCompanyCampaign WHERE G0_PK = '{pK}'";
			using (var cmd = Db.Connection.Command(sql))
			using (var adapter = cmd.NewDataAdapter())
			{
				adapter.Fill(dataTable);
			}

			return dataTable.Rows[0];
		}

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Set negative values to zero for non-negative GlbCompanyCampaign columns_1] ON [dbo].[GlbCompanyCampaign] ([G0_BatchCountDefault]) WHERE ([G0_BatchCountDefault]<(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Set negative values to zero for non-negative GlbCompanyCampaign columns_2] ON [dbo].[GlbCompanyCampaign] ([G0_LastSentBatchNumber]) WHERE ([G0_LastSentBatchNumber]<(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Set negative values to zero for non-negative GlbCompanyCampaign columns_3] ON [dbo].[GlbCompanyCampaign] ([G0_QuestionsPerWebPage]) WHERE ([G0_QuestionsPerWebPage]<(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
		};

		Guid testGlbCompanyCampaign1PK;
		Guid testGlbCompanyCampaign2PK;
	}
}
