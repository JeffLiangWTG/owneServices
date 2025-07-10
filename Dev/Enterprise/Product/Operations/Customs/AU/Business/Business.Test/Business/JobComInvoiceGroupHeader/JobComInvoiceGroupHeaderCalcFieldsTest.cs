namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class JobComInvoiceGroupHeaderCalcFieldsTest : Customs.Business.Testing.BaseJobComInvoiceGroupHeaderTestForDocumentWrapper
	{
		public void TestCalcBuyingCommission()
		{
			AssertCalcFields(AUChargeCodeList.Codes.BuyingCommission, "CalcBuyingCommission", "CalcBuyingCommissionCurrency");
		}

		public void TestCalcOtherCommission()
		{
			AssertCalcFields(AUChargeCodeList.Codes.OtherCommission, "CalcOtherCommission", "CalcOtherCommissionCurrency");
		}

		protected override void SetUp()
		{
			base.SetUp();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
		}
	}
}
