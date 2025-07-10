namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class JobComInvoiceHeaderCalcFieldsTest : Customs.Business.Testing.BaseJobComInvoiceHeaderTestForDocumentWrapper
	{
		public void TestCalcBuyingCommission()
		{
			AssertCalcFields(AUChargeCodeList.Codes.BuyingCommission, "BuyingCommission", AUChargeCodeList.Codes.BuyingCommission);
		}

		public void TestCalcOtherCommission()
		{
			AssertCalcFields(AUChargeCodeList.Codes.OtherCommission, "OtherCommission", AUChargeCodeList.Codes.OtherCommission);
		}

		protected override void SetUp()
		{
			base.SetUp();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;
		}
	}
}
