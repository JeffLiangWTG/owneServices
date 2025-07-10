using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DocumentEngineCore.DocumentSupport.Testing
{
	sealed class DocumentSupporterQuestionTest : TestCaseWithFactory
	{
		public void TestDocumentSupporterQuestionDefaults()
		{
			DocumentSupporterQuestion userQuestion = new DocumentSupporterQuestion("Title", "Question");
			AssertEquals(QuestionType.Default, userQuestion.QuestionType);
			AssertEquals(AnswerType.No, userQuestion.DefaultResponse);
			AssertEquals("Title", userQuestion.Title);
			AssertEquals("Question", userQuestion.QuestionText);
		}

		public void TestDocumentSupporterQuestionThrowsArgumentException()
		{
			bool exceptionThrown = false;
			try
			{
				DocumentSupporterQuestion userQuestion = new DocumentSupporterQuestion(ZString.Empty, ZString.Empty);
			}
			catch (ArgumentException)
			{
				exceptionThrown = true;
			}
			Assert("Should have an argumentexception", exceptionThrown);
		}

		public void TestDocumentSupporterQuestion()
		{
			DocumentSupporterQuestion userQuestion = new DocumentSupporterQuestion("Seriously Awesome", "Are Cuckoo Squeakers awesome?", QuestionType.Warning);
			AssertEquals("Are Cuckoo Squeakers awesome?", userQuestion.QuestionText);
			AssertEquals("Seriously Awesome", userQuestion.Title);
			AssertEquals(QuestionType.Warning, userQuestion.QuestionType);
			userQuestion.DefaultResponse = AnswerType.Yes;
			AssertEquals(AnswerType.Yes, userQuestion.DefaultResponse);
		}
	}
}
