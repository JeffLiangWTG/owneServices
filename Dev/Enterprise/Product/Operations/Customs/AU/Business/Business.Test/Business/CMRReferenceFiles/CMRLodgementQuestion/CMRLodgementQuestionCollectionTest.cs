using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRLodgementQuestionCollection))]
	sealed class CMRLodgementQuestionCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAdditionalFilter()
		{
			var question1 = CMRLodgementQuestion.New(Factory);
			question1.CQ_LodgementQuestionIdentifier = 1;
			question1.CQ_LodgementQuestionType = CMRCusEntryCPDec.LodgementQuestionTypes.GeneralLodgementQuestion;

			var question2 = CMRLodgementQuestion.New(Factory);
			question2.CQ_LodgementQuestionIdentifier = 2;
			question2.CQ_LodgementQuestionType = CMRCusEntryCPDec.LodgementQuestionTypes.CommunityProtectionQuestion;

			var questionsCollection = new CMRLodgementQuestionCollection(Factory);
			questionsCollection.Load();
			AssertEquals("Collection should not contain a GLQ question", false, questionsCollection.Contains(question1.PK));
			AssertEquals("Collection should  contain a CPQ question", true, questionsCollection.Contains(question2.PK));
		}

		public void TestAdditionalFilterNotMatchedError()
		{
			const string AdditionalFilterErrorString = "This question cannot be selected because it is a General Lodgement Question not a Community Protection Question.";
			var questionsCollection = new CMRLodgementQuestionCollection(Factory);
			AssertEquals("AdditionalFilterNotMatchedError", AdditionalFilterErrorString, questionsCollection.GetAllNotificationsWhenAdditionalFilterNotMet(null));
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CMRLodgementQuestionCollection(Factory);
	}
}
