using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CPQASelfHoldingLineAttacheeWrapperTest : TestCaseWithFactory
	{
		public void TestSettingTrueToNeedToGenerateQuestionRemoveExistingQuestions()
		{
			DummyLineAttacheeHolder lineAttacheeHolder = Factory.New<DummyLineAttacheeHolder>();
			lineAttacheeHolder.QuestionsExposed = new CMRCusEntryCPDecCollection(lineAttacheeHolder);
			lineAttacheeHolder.Questions.AddNew();

			CPQASelfHoldingLineAttacheeWrapper testWrapper = new CPQASelfHoldingLineAttacheeWrapper(lineAttacheeHolder);
			AssertEquals("No need to generate questions", false, testWrapper.NeedToGenerateQuestions);
			testWrapper.NeedToGenerateQuestions = false;
			AssertEquals("Still has a question", 1, lineAttacheeHolder.Questions.Count);

			testWrapper.NeedToGenerateQuestions = true;
			AssertEquals("Questions are removed", 0, lineAttacheeHolder.Questions.Count);

			lineAttacheeHolder.IsRiskHistorySupportedExposed = true;
			lineAttacheeHolder.Questions.AddNew();
			testWrapper.NeedToGenerateQuestions = false;
			AssertEquals("Still has a question", 1, lineAttacheeHolder.Questions.Count);
			testWrapper.NeedToGenerateQuestions = true;
			AssertEquals("Still has a question", 1, lineAttacheeHolder.Questions.Count);
		}

		public void TestNeedToGenerateQuestionIsSetToTrueIfQuestionIsZero()
		{
			DummyLineAttacheeHolder lineAttacheeHolder = Factory.New<DummyLineAttacheeHolder>();
			lineAttacheeHolder.QuestionsExposed = new CMRCusEntryCPDecCollection(lineAttacheeHolder);
			lineAttacheeHolder.Questions.AddNew();

			CPQASelfHoldingLineAttacheeWrapper testWrapper = new CPQASelfHoldingLineAttacheeWrapper(lineAttacheeHolder);
			AssertEquals("No need to generate questions", false, testWrapper.NeedToGenerateQuestions);

			lineAttacheeHolder.Questions.RemoveAndDeleteAll();
			testWrapper = new CPQASelfHoldingLineAttacheeWrapper(lineAttacheeHolder);
			AssertEquals("No need to generate questions", true, testWrapper.NeedToGenerateQuestions);
		}

		public void TestGenerateQuestions()
		{
			DummyLineAttacheeHolder lineAttacheeHolder = Factory.New<DummyLineAttacheeHolder>();
			CPQASelfHoldingLineAttacheeWrapper testWrapper = new CPQASelfHoldingLineAttacheeWrapper(lineAttacheeHolder);

			testWrapper.NeedToGenerateQuestions = true;
			testWrapper.GenerateQuestion();
			AssertEquals("NeedToGenerateQuestions is set to false", false, testWrapper.NeedToGenerateQuestions);
		}

		public void TestProperties()
		{
			DummyLineAttachee sourceToDefault = Factory.New<DummyLineAttachee>();
			sourceToDefault.QuestionsExposed = new CMRCusEntryCPDecCollection(sourceToDefault);
			CMRCusEntryCPDec defaultQuestion = sourceToDefault.Questions.AddNew();
			defaultQuestion.ON_CPDecNum = 400;
			defaultQuestion.ON_CPDecStartDate = new ZDateTime(2005, 1, 1);
			defaultQuestion.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;

			CMRCusEntryCPDec unAnsweredQuestion = sourceToDefault.Questions.AddNew();
			unAnsweredQuestion.ON_CPDecNum = 400;
			unAnsweredQuestion.ON_CPDecStartDate = new ZDateTime(2005, 1, 1);

			DummyLineAttacheeHolder lineAttacheeHolder = Factory.New<DummyLineAttacheeHolder>();
			lineAttacheeHolder.SourcesToDefaultExposed = new ICPQALineAttachee[] { sourceToDefault };

			CPQASelfHoldingLineAttacheeWrapper testWrapper = new CPQASelfHoldingLineAttacheeWrapper(lineAttacheeHolder);

			AssertEquals("Headers", 0, testWrapper.Headers.Length);
			AssertEquals("Lines", 1, testWrapper.Lines.Length);
			AssertEquals("Lines", lineAttacheeHolder, testWrapper.Lines[0]);
			AssertEquals("Lines", null, testWrapper.CachedQuestions);
			AssertEquals("Line default questions", 400, testWrapper.DefaultUniqueQuestions.GetDefaultAnswer(new LineDefaultKey(400, new ZDateTime(2005, 1, 1))).answer.CPDecNum);
		}
	}
}
