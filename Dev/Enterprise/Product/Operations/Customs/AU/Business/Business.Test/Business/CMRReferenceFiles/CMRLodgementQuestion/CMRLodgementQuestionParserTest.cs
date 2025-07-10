using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRLodgementQuestionParserTest : TestCaseWithFactory
	{
		public void TestIsCompleteCode()
		{
			var parser = new CMRLodgementQuestionParser("400-01-JAN-2006");
			AssertEquals("IsCompleteCode", true, parser.IsCompleteCode);

			parser = new CMRLodgementQuestionParser("400");
			AssertEquals("IsCompleteCode", false, parser.IsCompleteCode);
		}

		public void TestIsEmptyCode()
		{
			var parser = new CMRLodgementQuestionParser("400-01-JAN-2006");
			AssertEquals("IsCompleteCode", false, parser.IsEmptyCode);

			parser = new CMRLodgementQuestionParser(ZString.Empty);
			AssertEquals("IsCompleteCode", true, parser.IsEmptyCode);
		}

		public void TestProperties()
		{
			var parser = new CMRLodgementQuestionParser("400-01-JAN-2006");
			AssertEquals("QuestionID", 400, parser.QuestionID);
			AssertEquals("StartDate", new ZDateTime(2006, 1, 1), parser.StartDate);
		}
	}
}
