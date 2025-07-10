using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class ValuationQuestionsProviderTest : TestCaseWithFactory
	{
		public void TestPopulateQuestionAndAnswer()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryWithFullGOVCBR934Data();
			entry.Declaration.JE_MessageType = KRJobMessageTypeList.Codes.ValuationDeclaration;
			var entryLine = entry.MergedLines.AddNew();
			var invoice = entry.Declaration.Invoices[0];
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			#region 934
			SetQuestion(invoice);
			invoice.ValuationQuestion7EA = PricingCodeList.Codes._07;
			invoice.ValuationQuestion7EB = "기타";
			Factory.Save();

			var import934 = new Import934HeaderCreator().Create(entry);
			AssertEquals(5, import934.FormAData.Questions.Length);
			var questions934 = import934.FormAData.Questions.ToArray();

			AssertEquals(PricingCodeList.Codes._07, questions934.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._7E).AnswerCode);
			AssertEquals("기타", questions934.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._7E).AnswerOtherDescription);

			AssertOtherQuestions(questions934);
			#endregion

			#region 5SM
			SetQuestion(invoice);
			invoice.ValuationQuestion5EA = PricingCodeList.Codes._99;
			invoice.ValuationQuestion5EB = "기타법률";
			Factory.Save();

			var import5SM = new Import5SMHeaderCreator().Create(entry);
			AssertEquals(5, import5SM.FormCData.Questions.Length);
			var questions5SM = import5SM.FormCData.Questions.ToArray();

			AssertEquals(PricingCodeList.Codes._99, questions5SM.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._5E).AnswerCode);
			AssertEquals("기타법률", questions5SM.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._5E).AnswerOtherDescription);

			AssertOtherQuestions(questions5SM);
			#endregion
		}

		void SetQuestion(JobComInvoiceHeader invoice)
		{
			invoice.ValuationQuestions.RemoveAndDeleteAll();
			invoice.ValuationQuestion5A = YesNoList.Codes.Yes;
			invoice.ValuationQuestion5B = "01";
			invoice.ValuationQuestion5C = YesNoList.Codes.Yes;
			invoice.ValuationQuestion5D = YesNoList.Codes.Yes;
		}

		void AssertOtherQuestions(QuestionAndAnswer[] questions)
		{
			AssertEquals(YesNoList.Codes.Yes, questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._5A).AnswerCode);
			AssertEquals("01", questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._5B).AnswerCode);
			AssertEquals(YesNoList.Codes.Yes, questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._5C).AnswerCode);
			AssertEquals(YesNoList.Codes.Yes, questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._5D).AnswerCode);
		}
	}
}
