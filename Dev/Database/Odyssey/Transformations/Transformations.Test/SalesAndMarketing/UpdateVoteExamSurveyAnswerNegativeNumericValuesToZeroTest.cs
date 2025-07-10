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
	[TestedType(typeof(UpdateVoteExamSurveyAnswerNegativeNumericValuesToZero))]
	class UpdateVoteExamSurveyAnswerNegativeNumericValuesToZeroTest : DataTransformationTestCase
	{
		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Set negative values to zero for non-negative VoteExamSurveyAnswer (Sub)QuestionOrder columns_1] ON [dbo].[VoteExamSurveyAnswer] ([HZ_QuestionOrder]) WHERE ([HZ_QuestionOrder]<(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Set negative values to zero for non-negative VoteExamSurveyAnswer (Sub)QuestionOrder columns_2] ON [dbo].[VoteExamSurveyAnswer] ([HZ_SubQuestionOrder]) WHERE ([HZ_SubQuestionOrder]<(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override DataTransformation GetNewTestTransformationInstance()
			=> new UpdateVoteExamSurveyAnswerNegativeNumericValuesToZero();

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(VoteExamSurveyAnswerSchema.Constants.TableName, "Constraint_HZ_QuestionOrder");
			DBTransformationTestHelper.DropConstraintIfExists(VoteExamSurveyAnswerSchema.Constants.TableName, "Constraint_HZ_SubQuestionOrder");

			var helper = new TransformationTestDataCreator();
			var company = helper.CreateCompany(Guid.NewGuid(), "ABC", "AU");
			var staff1 = helper.CreateStaff("Staff1", "S1");
			var staff2 = helper.CreateStaff("Staff2", "S2");
			var campaign = helper.CreateGlbCompanyCampaign("Test CMP1", "LCT", company, "CRT00000001", new DateTime(2019, 1, 1));

			var campaignItem1 = helper.CreateGlbCompanyCampaignItem(campaign, staff1);
			var campaignItem2 = helper.CreateGlbCompanyCampaignItem(campaign, staff2);

			var question = helper.CreateVoteExamSurveyQuestion(campaign);

			voteExamSurveyAnswer1 = helper.CreateVoteExamSurveyAnswer(question, campaignItem1, 1, 2);
			voteExamSurveyAnswer2 = helper.CreateVoteExamSurveyAnswer(question, campaignItem2, -1, -1);
		}

		protected override void AssertTransformationResults()
		{
			var testAllNonNegative = GetDataRow(voteExamSurveyAnswer1);

			CombineAssertions("All non-negative column values should not be changed.", () =>
			{
				AssertEquals(testAllNonNegative["HZ_QuestionOrder"], (short)1);
				AssertEquals(testAllNonNegative["HZ_SubQuestionOrder"], (short)2);
			});

			var testAllNegative = GetDataRow(voteExamSurveyAnswer2);

			CombineAssertions("All negative column values should be replaced with 0.", () =>
			{
				AssertEquals(testAllNegative["HZ_QuestionOrder"], (short)0);
				AssertEquals(testAllNegative["HZ_SubQuestionOrder"], (short)0);
			});
		}

		DataRow GetDataRow(Guid pK)
		{
			var dataTable = new DataTable();
			var sql = $"SELECT HZ_QuestionOrder, HZ_SubQuestionOrder FROM dbo.VoteExamSurveyAnswer WHERE HZ_PK = '{pK}'";
			using (var cmd = Db.Connection.Command(sql))
			using (var adapter = cmd.NewDataAdapter())
			{
				adapter.Fill(dataTable);
			}

			return dataTable.Rows[0];
		}

		Guid voteExamSurveyAnswer1;
		Guid voteExamSurveyAnswer2;
	}
}
