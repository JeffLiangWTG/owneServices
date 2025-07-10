using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class MeetsPricePromiseStrategyTest : TestCaseWithFactory
	{
		public void TestADDCVDElementStrategy()
		{
			var strategy = new MeetsPricePromiseStrategy();
			Assert("IsMandatory", !strategy.IsMandatory);
			Assert("IsMergeKey", strategy.IsMergeKey);
			Assert("Applicable for Both", strategy.IsApplicable(EnteringOrExiting.Both));
			Assert("Applicable for Entering", strategy.IsApplicable(EnteringOrExiting.Entering));
			Assert("NOT Applicable for Exiting", !strategy.IsApplicable(EnteringOrExiting.Exiting));
		}

		public void TestList()
		{
			var strategy = new MeetsPricePromiseStrategy();
			Assert("ProvideList", strategy.ProvideList);
			Assert("GetList", strategy.GetList(Factory, EnteringOrExiting.Both).ContainsCode(ConfirmationTypeList.Codes.Yes));
			Assert("GetList", strategy.GetList(Factory, EnteringOrExiting.Both).ContainsCode(ConfirmationTypeList.Codes.No));
		}
	}
}
