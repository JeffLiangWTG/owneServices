using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	public class PriceQuestionCodeListPartialTest : TestCaseWithFactory
	{
		public void TestMandatoryQuestionsForMethodOneFOr5SM()
		{
			var questions = PriceQuestionCodeList.MandatoryQuestionsForMethodOneFor5SM();
			var question = questions.GetEnumerator();
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._5A, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._6A, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._6B, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._7A, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._7B, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._8A, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._8B, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._8C, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._8D, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._9A, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._9B, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._10A, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._10B, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._10C, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._10D, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._11A, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._11B, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._11C, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._11D, question.Current);
		}

		public void TestMandatoryQuestionsForMethodOneForIMP()
		{
			var questions = PriceQuestionCodeList.MandatoryQuestionsForMethodOneForIMP();
			var question = questions.GetEnumerator();
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._7A, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._8A, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._8B, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._9A, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._9B, question.Current);
		}

		public void TestSubQuestionsOf5A()
		{
			var questions = PriceQuestionCodeList.SubQuestionsOf5A();
			var question = questions.GetEnumerator();
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._5B, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._5C, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._5D, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._5EA, question.Current);
			question.MoveNext();
			AssertEquals(PriceQuestionCodeList.Codes._5EB, question.Current);
		}
	}
}
