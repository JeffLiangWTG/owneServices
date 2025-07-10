using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ValuationQuestion))]
	sealed class ValuationQuestionTest : CusCodeDataTest<ValuationQuestion>
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return valuationQuestion;
		}
		protected override BusinessObject GetNewBusinessObject() => valuationQuestion;

		public void TestCY_Data()
		{
			AssertEquals(50, valuationQuestion.CY_DataInfo.MaxLength);
			AssertEquals(false, valuationQuestion.CY_DataAllowWesternEuropeanCharactersOnly);
		}

		public void TestCY_DataAllowWesternEuropeanCharactersOnly()
		{
			valuationQuestion.CY_Code = PriceQuestionCodeList.Codes._5EB;
			valuationQuestion.CY_Data = "기타 사유";
			AssertNoErrors(valuationQuestion.CY_DataInfo);
		}

		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			valuationQuestion = invoice.ValuationQuestions.AddNew();
			Factory.Save();
		}
		ValuationQuestion valuationQuestion;
	}
}
