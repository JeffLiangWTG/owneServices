using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	internal sealed class CMRLodgementQuestionFindBoxListProvider : FindBoxListProvider
	{
		public CMRLodgementQuestionFindBoxListProvider(CMRLodgementQuestionCollection collection)
			: base(collection)
		{
		}

		protected override void AddCodeEqualsFilter(ZQuery query, string code)
		{
			var parser = new CMRLodgementQuestionParser(code);
			query.IsNoResultQuery = !parser.IsCompleteCode;
			if (parser.IsCompleteCode)
			{
				query.AddToFilter(GetQuestionIDFilter(parser.QuestionID), JoinCondition.And);
			}
		}

		protected override void AddCodeStartsWithFilter(ZQuery query, string code)
		{
			var parser = new CMRLodgementQuestionParser(code);
			query.IsNoResultQuery = parser.IsCompleteCode;
			if (!parser.IsEmptyCode)
			{
				query.AddToFilter(GetQuestionIDFilter(parser.QuestionID), JoinCondition.And);
			}
		}

		protected override string GetCodePropertyName(ZGuid pK) => CMRLodgementQuestion.Schema.QuestionIDAndStartDate;

		protected override string GetDescriptionPropertyName(Type typeOfElements) => CMRLodgementQuestion.Schema.QuestionIDAndStartDate;

		ZQuery GetQuestionIDFilter(ZInt questionID) => new ZQuery(CMRLodgementQuestionSchema.CQ_LodgementQuestionIdentifier, SQLComparisonOperator.Equal, questionID);
	}
}
