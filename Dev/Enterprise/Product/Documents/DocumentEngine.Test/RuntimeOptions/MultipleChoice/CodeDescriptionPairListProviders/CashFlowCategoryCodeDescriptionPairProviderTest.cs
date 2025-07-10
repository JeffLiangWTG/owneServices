using CargoWise.Application;
using Enterprise.Integration.Accounting;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class CashFlowCategoryCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new CashFlowCategoryCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			CashFlowCategoryCodeDescriptionPairProvider testPairProvider = new CashFlowCategoryCodeDescriptionPairProvider();
			var list = testPairProvider.GetCodeDescriptionPairList();
			AssertContainsExactElementsInAnyOrder(list, ObjectFactory.Get<IAccounting>().CashFlowCategoryCodeDescriptionList);
		}
	}
}
