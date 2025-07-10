using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class LineDefaultQuestionsTest : TestCaseWithFactory
	{
		public void TestGetDefaultAnswer()
		{
			var lineDefaultQuestions = SetUpQuestions();
			AssertDefaultAnswer(lineDefaultQuestions.GetDefaultAnswer(new LineDefaultKey(400, new ZDateTime(2005, 1, 1))), q1, string.Empty);
			AssertDefaultAnswer(lineDefaultQuestions.GetDefaultAnswer(new LineDefaultKey(401, new ZDateTime(2005, 1, 1))), q2, string.Empty);
			AssertEquals("Default answers - not for this Question ID", null, lineDefaultQuestions.GetDefaultAnswer(new LineDefaultKey(402, new ZDateTime(2005, 1, 1))).answer);
			AssertDefaultAnswer(lineDefaultQuestions.GetDefaultAnswer(new LineDefaultKey(400, new ZDateTime(2005, 1, 2))), q1, "The CP Question 400 has changed since recording the answer to the Product or Classification Lookup.\r\nPlease review the CP Question and/or update the Product or Classification CP Question with current answers.");
		}

		void AssertDefaultAnswer((LineDefaultQuestions.DefaultAnswer answer, string warning) data, LineDefaultQuestions.DefaultAnswer expectedAnswer, string warning)
		{
			AssertDefaultAnswer(data, expectedAnswer.CPDecNum, expectedAnswer.AnswerCode, expectedAnswer.CPDecStartDate, warning);
		}

		public static void AssertDefaultAnswer((LineDefaultQuestions.DefaultAnswer answer, string warning) data, ZInt cpDecNum, ZString answerCode, ZDateTime cpDecStartDate, string warning)
		{
			AssertEquals("CPDecNum", cpDecNum, data.answer.CPDecNum);
			AssertEquals("AnswerCode", answerCode, data.answer.AnswerCode);
			AssertEquals("CPDecStartDate", cpDecStartDate, data.answer.CPDecStartDate);
			AssertEquals("Warning", warning, data.warning);
		}

		LineDefaultQuestions SetUpQuestions()
		{
			q1 = new LineDefaultQuestions.DefaultAnswer()
			{
				CPDecNum = 400,
				CPDecStartDate = new ZDateTime(2005, 1, 1),
				AnswerCode = CMRCusEntryCPDec.Answers.YES
			};
			q2 = new LineDefaultQuestions.DefaultAnswer()
			{
				CPDecNum = 401,
				CPDecStartDate = new ZDateTime(2005, 1, 1),
				AnswerCode = CMRCusEntryCPDec.Answers.NO
			};
			return new LineDefaultQuestions(new[] { q1, q2 });
		}

		#region Implementation

		LineDefaultQuestions.DefaultAnswer q1;
		LineDefaultQuestions.DefaultAnswer q2;

		#endregion
	}
}
