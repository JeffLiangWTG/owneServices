namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class DutyTaxCorrectionCodeListTest : NUnit.Framework.TestCase
	{
		public void TestIs5UARelevant()
		{
			Assert(DutyTaxCorrectionCodeList.Is5UARelevant(DutyTaxCorrectionCodeList.Codes.A));
			Assert(DutyTaxCorrectionCodeList.Is5UARelevant(DutyTaxCorrectionCodeList.Codes.B));
			Assert(!DutyTaxCorrectionCodeList.Is5UARelevant(DutyTaxCorrectionCodeList.Codes.C));
			Assert(!DutyTaxCorrectionCodeList.Is5UARelevant(DutyTaxCorrectionCodeList.Codes.O));
			Assert(!DutyTaxCorrectionCodeList.Is5UARelevant(DutyTaxCorrectionCodeList.Codes.X));
		}
		public void TestIs5ULRelevant()
		{
			Assert(!DutyTaxCorrectionCodeList.Is5ULRelevant(DutyTaxCorrectionCodeList.Codes.A));
			Assert(!DutyTaxCorrectionCodeList.Is5ULRelevant(DutyTaxCorrectionCodeList.Codes.B));
			Assert(DutyTaxCorrectionCodeList.Is5ULRelevant(DutyTaxCorrectionCodeList.Codes.C));
			Assert(!DutyTaxCorrectionCodeList.Is5ULRelevant(DutyTaxCorrectionCodeList.Codes.O));
			Assert(!DutyTaxCorrectionCodeList.Is5ULRelevant(DutyTaxCorrectionCodeList.Codes.X));
		}
	}
}
