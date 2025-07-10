using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CMRCusEntryCPDecLookupsTest : TestCaseWithFactory
	{
		public void TestON_AnswerCode_List()
		{
			AssertNotNull("Answer code list", lookups.ON_AnswerCode_List);
			AssertEquals("Containes Yes", CMRCusEntryCPDec.Answers.YES, lookups.ON_AnswerCode_List.GetCodeFromDescription("Yes"));
			AssertEquals("Containes No", CMRCusEntryCPDec.Answers.NO, lookups.ON_AnswerCode_List.GetCodeFromDescription("No"));
		}

		public void TestCPQuestions()
		{
			ZDateTime currentDate = new ZDateTime(2005, 12, 22);
			CMRLodgementQuestion question = Factory.New<CMRLodgementQuestion>();
			question.CQ_LodgementQuestionIdentifier = 400;
			question.CQ_LodgementQuestionStartDate = currentDate;
			question.CQ_LodgementQuestionEndDate = ZDateTime.Empty;
			question.CQ_LodgementQuestionName = "TESTTEST";
			question.CQ_LodgementQuestionText = "QUESTION TEXT";

			AssertNotNull("CPQuestion Collection", lookups.CPQuestions);
			Assert("Should contain Question", lookups.CPQuestions.Contains(question.PK));
		}

		CMRCusEntryCPDec cPDec;
		CMRCusEntryCPDecLookups lookups;

		protected override void SetUp()
		{
			base.SetUp();
			cPDec = Factory.New<CMRCusEntryCPDec>();
			lookups = new CMRCusEntryCPDecLookups(cPDec);
		}
	}
}
