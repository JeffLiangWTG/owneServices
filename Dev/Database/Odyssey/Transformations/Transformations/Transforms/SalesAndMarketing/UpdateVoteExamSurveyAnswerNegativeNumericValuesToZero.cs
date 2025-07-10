using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.SalesAndMarketing
{
	public class UpdateVoteExamSurveyAnswerNegativeNumericValuesToZero : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Set negative values to zero for non-negative VoteExamSurveyAnswer (Sub)QuestionOrder columns";

		#region ITransformationIndexProvider.IndexProvider

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(VoteExamSurveyAnswerSchema.Instance)
					.Key(VoteExamSurveyAnswerSchema.Constants.HZ_QuestionOrder)
					.Where("[HZ_QuestionOrder]<(0)")
					.GetInfo();

				indexProvider.New(VoteExamSurveyAnswerSchema.Instance)
					.Key(VoteExamSurveyAnswerSchema.Constants.HZ_SubQuestionOrder)
					.Where("[HZ_SubQuestionOrder]<(0)")
					.GetInfo();

				return indexProvider;
			}
		}

		#endregion

		protected override void OfflinePostUpgradeTransform()
		{
			var updateVoteExamSurveyAnswerNegativeNumericValues = @"
UPDATE dbo.VoteExamSurveyAnswer
SET HZ_QuestionOrder = 0,
HZ_SystemLastEditUser = '~BP',
HZ_SystemLastEditTimeUtc = GetUtcDate() 
WHERE HZ_QuestionOrder < 0;
							
UPDATE dbo.VoteExamSurveyAnswer
SET HZ_SubQuestionOrder = 0,
HZ_SystemLastEditUser = '~BP',
HZ_SystemLastEditTimeUtc = GetUtcDate() 
WHERE HZ_SubQuestionOrder < 0;
";
			Db.Connection.ExecuteNonQuery(updateVoteExamSurveyAnswerNegativeNumericValues);
		}
	}
}
