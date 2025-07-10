using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class LineDefaultQuestionGeneratorTest : TestCaseWithFactory
	{
		public void TestLineDefaultKey()
		{
			LineDefaultKey defaultKey = new LineDefaultKey(400, new ZDateTime(2005, 1, 1));
			AssertEquals("Default Key ID", 400, defaultKey.QuestionID);
			AssertEquals("Default Key Start Date", new ZDateTime(2005, 1, 1), defaultKey.StartDate);
		}

		public void TestFilterValidDefaultQuestions()
		{
			var now = ZDateTime.Now;

			var sourceToDefault = Factory.New<DummyLineAttachee>();
			sourceToDefault.QuestionsExposed = new CMRCusEntryCPDecCollection(sourceToDefault);

			var question1 = sourceToDefault.Questions.AddNew();
			question1.ON_CPDecNum = 1;
			question1.ON_CPDecStartDate = now.AddDays(-10);
			question1.ON_CPDecEndDate = now.AddDays(10);
			question1.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;

			var question2 = sourceToDefault.Questions.AddNew();
			question2.ON_CPDecNum = 2;
			question2.ON_CPDecStartDate = now.AddDays(-10);
			question2.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;

			var question3 = sourceToDefault.Questions.AddNew();
			question3.ON_CPDecNum = 3;
			question3.ON_CPDecEndDate = now.AddDays(10);
			question3.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;

			var question4 = sourceToDefault.Questions.AddNew();
			question4.ON_CPDecNum = 4;
			question4.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;

			var question5 = sourceToDefault.Questions.AddNew();
			question5.ON_CPDecNum = 5;
			question5.ON_CPDecStartDate = now.AddDays(1);
			question5.ON_CPDecEndDate = now.AddDays(10);
			question5.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;

			var question6 = sourceToDefault.Questions.AddNew();
			question6.ON_CPDecNum = 6;
			question6.ON_CPDecStartDate = now.AddDays(-10);
			question6.ON_CPDecEndDate = now.AddDays(-1);
			question6.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;

			DummyAttachee.SourcesToDefaultExposed = new ICPQALineAttachee[] { sourceToDefault };
			var result = new LineDefaultQuestionGenerator().GenerateUniqueDefaultQuestions(DummyAttachee, now);
			AssertDefaultAnswer(result.GetDefaultAnswer(new LineDefaultKey(1, now)), question1, "The CP Question 1 has changed since recording the answer to the Product or Classification Lookup.\r\nPlease review the CP Question and/or update the Product or Classification CP Question with current answers.");
			AssertDefaultAnswer(result.GetDefaultAnswer(new LineDefaultKey(2, now)), question2, "The CP Question 2 has changed since recording the answer to the Product or Classification Lookup.\r\nPlease review the CP Question and/or update the Product or Classification CP Question with current answers.");
			AssertDefaultAnswer(result.GetDefaultAnswer(new LineDefaultKey(3, now)), question3, "The CP Question 3 has changed since recording the answer to the Product or Classification Lookup.\r\nPlease review the CP Question and/or update the Product or Classification CP Question with current answers.");
			AssertDefaultAnswer(result.GetDefaultAnswer(new LineDefaultKey(4, now)), question4, "The CP Question 4 has changed since recording the answer to the Product or Classification Lookup.\r\nPlease review the CP Question and/or update the Product or Classification CP Question with current answers.");
			AssertNull("Start date is in the future", result.GetDefaultAnswer(new LineDefaultKey(005, now)).answer);
			AssertNull("Expired", result.GetDefaultAnswer(new LineDefaultKey(6, now)).answer);
		}

		void AssertDefaultAnswer((LineDefaultQuestions.DefaultAnswer answer, string warning) data, CMRCusEntryCPDec expectedAnswer, string warning)
		{
			LineDefaultQuestionsTest.AssertDefaultAnswer(data, expectedAnswer.ON_CPDecNum, expectedAnswer.ON_AnswerCode, expectedAnswer.ON_CPDecStartDate, warning);
		}

		public void TestDefaultQuestionsOnlyHasAnsweredQuestions()
		{
			DummyLineAttachee sourceToDefault = Factory.New<DummyLineAttachee>();
			sourceToDefault.QuestionsExposed = new CMRCusEntryCPDecCollection(sourceToDefault);
			CMRCusEntryCPDec question1 = sourceToDefault.Questions.AddNew();
			question1.ON_CPDecNum = 400;
			question1.ON_CPDecStartDate = new ZDateTime(2005, 1, 1);
			question1.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;

			CMRCusEntryCPDec question2 = sourceToDefault.Questions.AddNew();
			question2.ON_CPDecNum = 401;
			question2.ON_CPDecStartDate = new ZDateTime(2005, 1, 1);
			AssertEquals("question2 is not answered", false, question2.IsAnswered);

			DummyAttachee.SourcesToDefaultExposed = new ICPQALineAttachee[] { sourceToDefault };
			LineDefaultQuestions result = new LineDefaultQuestionGenerator().GenerateUniqueDefaultQuestions(DummyAttachee, ZDateTime.Now);
			AssertEquals("There should be one answer", 400, result.GetDefaultAnswer(new LineDefaultKey(400, new ZDateTime(2005, 1, 1))).answer.CPDecNum);
			AssertNull("401 was not answered", result.GetDefaultAnswer(new LineDefaultKey(401, new ZDateTime(2005, 1, 1))).answer);
		}

		public void TestRemoveAllQuestionsWithDifferentAnswers()
		{
			DummyLineAttachee sourceToDefault1 = Factory.New<DummyLineAttachee>();
			sourceToDefault1.QuestionsExposed = new CMRCusEntryCPDecCollection(sourceToDefault1);
			CMRCusEntryCPDec question1 = sourceToDefault1.Questions.AddNew();
			question1.ON_CPDecNum = 400;
			question1.ON_CPDecStartDate = new ZDateTime(2005, 1, 1);
			question1.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;

			CMRCusEntryCPDec question2 = sourceToDefault1.Questions.AddNew();
			question2.ON_CPDecNum = 401;
			question2.ON_CPDecStartDate = new ZDateTime(2005, 1, 1);
			question2.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;

			DummyLineAttachee sourceToDefault2 = Factory.New<DummyLineAttachee>();
			sourceToDefault2.QuestionsExposed = new CMRCusEntryCPDecCollection(sourceToDefault2);
			CMRCusEntryCPDec question3 = sourceToDefault2.Questions.AddNew();
			question3.ON_CPDecNum = 400;
			question3.ON_CPDecStartDate = new ZDateTime(2005, 1, 1);
			question3.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;

			CMRCusEntryCPDec question4 = sourceToDefault2.Questions.AddNew();
			question4.ON_CPDecNum = 401;
			question4.ON_CPDecStartDate = new ZDateTime(2005, 1, 1);
			question4.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;

			CMRCusEntryCPDec question5 = sourceToDefault2.Questions.AddNew();
			question5.ON_CPDecNum = 402;
			question5.ON_CPDecStartDate = new ZDateTime(2005, 1, 1);
			question5.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;

			DummyAttachee.SourcesToDefaultExposed = new ICPQALineAttachee[] { sourceToDefault1, sourceToDefault2 };
			LineDefaultQuestions result = new LineDefaultQuestionGenerator().GenerateUniqueDefaultQuestions(DummyAttachee, ZDateTime.Now);
			AssertNull("400 has different answered", result.GetDefaultAnswer(new LineDefaultKey(400, new ZDateTime(2005, 1, 1))).answer);
			AssertEquals("Result should have a question with same answers", 401, result.GetDefaultAnswer(new LineDefaultKey(401, new ZDateTime(2005, 1, 1))).answer.CPDecNum);
			AssertEquals("Result should have a question with same answers", 402, result.GetDefaultAnswer(new LineDefaultKey(402, new ZDateTime(2005, 1, 1))).answer.CPDecNum);
		}

		public void TestDefaultPermits()
		{
			DummyLineAttachee sourceToDefault1 = Factory.New<DummyLineAttachee>();
			sourceToDefault1.QuestionsExposed = new CMRCusEntryCPDecCollection(sourceToDefault1);
			CMRCusEntryCPDec question1 = sourceToDefault1.Questions.AddNew();
			question1.ON_CPDecNum = 400;
			question1.ON_CPDecStartDate = new ZDateTime(2005, 1, 1);
			question1.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			question1.ON_Permit = ZString.Empty;

			CMRCusEntryCPDec question2 = sourceToDefault1.Questions.AddNew();
			question2.ON_CPDecNum = 401;
			question2.ON_CPDecStartDate = new ZDateTime(2005, 1, 1);
			question2.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			question2.ON_Permit = "L401";

			DummyLineAttachee sourceToDefault2 = Factory.New<DummyLineAttachee>();
			sourceToDefault2.QuestionsExposed = new CMRCusEntryCPDecCollection(sourceToDefault2);
			CMRCusEntryCPDec question3 = sourceToDefault2.Questions.AddNew();
			question3.ON_CPDecNum = 400;
			question3.ON_CPDecStartDate = new ZDateTime(2005, 1, 1);
			question3.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			question3.ON_Permit = "P400";

			CMRCusEntryCPDec question4 = sourceToDefault2.Questions.AddNew();
			question4.ON_CPDecNum = 401;
			question4.ON_CPDecStartDate = new ZDateTime(2005, 1, 1);
			question4.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			question4.ON_Permit = "P401";

			DummyAttachee.SourcesToDefaultExposed = new ICPQALineAttachee[] { sourceToDefault1, sourceToDefault2 };
			LineDefaultQuestions result = new LineDefaultQuestionGenerator().GenerateUniqueDefaultQuestions(DummyAttachee, ZDateTime.Now);
			AssertEquals("P400", result.GetDefaultAnswer(new LineDefaultKey(400, new ZDateTime(2005, 1, 1))).answer.Permit);
			AssertEquals("L401", result.GetDefaultAnswer(new LineDefaultKey(401, new ZDateTime(2005, 1, 1))).answer.Permit);
		}

		public void TestListHasUniqueQuestions()
		{
			DummyLineAttachee sourceToDefault1 = Factory.New<DummyLineAttachee>();
			sourceToDefault1.QuestionsExposed = new CMRCusEntryCPDecCollection(sourceToDefault1);
			CMRCusEntryCPDec question1 = sourceToDefault1.Questions.AddNew();
			question1.ON_CPDecNum = 400;
			question1.ON_CPDecStartDate = new ZDateTime(2005, 1, 1);
			question1.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			question1.ON_Permit = "P1";

			CMRCusEntryCPDec question2 = sourceToDefault1.Questions.AddNew();
			question2.ON_CPDecNum = 401;
			question2.ON_CPDecStartDate = new ZDateTime(2005, 1, 1);
			question2.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			question2.ON_Permit = "P2";

			DummyLineAttachee sourceToDefault2 = Factory.New<DummyLineAttachee>();
			sourceToDefault2.QuestionsExposed = new CMRCusEntryCPDecCollection(sourceToDefault2);
			CMRCusEntryCPDec question3 = sourceToDefault2.Questions.AddNew();
			question3.ON_CPDecNum = 400;
			question3.ON_CPDecStartDate = new ZDateTime(2016, 1, 1);
			question3.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			question3.ON_Permit = "P3";

			CMRCusEntryCPDec question4 = sourceToDefault2.Questions.AddNew();
			question4.ON_CPDecNum = 401;
			question4.ON_CPDecStartDate = new ZDateTime(2005, 1, 1);
			question4.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			question4.ON_Permit = "P4";

			CMRCusEntryCPDec question5 = sourceToDefault2.Questions.AddNew();
			question5.ON_CPDecNum = 402;
			question5.ON_CPDecStartDate = new ZDateTime(2005, 1, 1);
			question5.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
			question5.ON_Permit = "P5";

			DummyAttachee.SourcesToDefaultExposed = new ICPQALineAttachee[] { sourceToDefault1, sourceToDefault2 };
			LineDefaultQuestions result = new LineDefaultQuestionGenerator().GenerateUniqueDefaultQuestions(DummyAttachee, ZDateTime.Now);
			AssertEquals("Result should have a question that has been superseeded", "P3", result.GetDefaultAnswer(new LineDefaultKey(400, new ZDateTime(2005, 1, 1))).answer.Permit);
			AssertEquals("Result should have a question 2", "P2", result.GetDefaultAnswer(new LineDefaultKey(401, new ZDateTime(2005, 1, 1))).answer.Permit);
			AssertEquals("Result should have a question 5", "P5", result.GetDefaultAnswer(new LineDefaultKey(402, new ZDateTime(2005, 1, 1))).answer.Permit);
		}

		#region Implementation

		DummyLineAttachee DummyAttachee
		{
			get
			{
				if (fDummyAttachee == null)
				{
					fDummyAttachee = Factory.New<DummyLineAttachee>();
				}
				return fDummyAttachee;
			}
		}
		DummyLineAttachee fDummyAttachee;

		#endregion
	}
}
