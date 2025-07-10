using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class OriginalManufacturerNameENStrategyTest : TestCaseWithFactory
	{
		public void TestOriginalManufacturerNameENStrategy()
		{
			var strategy = new OriginalManufacturerNameCNStrategy();
			Assert("IsMandatory", !strategy.IsMandatory);
			Assert("IsMergeKey", strategy.IsMergeKey);
			Assert("Applicable for Both", strategy.IsApplicable(EnteringOrExiting.Both));
			Assert("Applicable for Entering", strategy.IsApplicable(EnteringOrExiting.Entering));
			Assert("NOT Applicable for Exiting", !strategy.IsApplicable(EnteringOrExiting.Exiting));
		}
	}
}
