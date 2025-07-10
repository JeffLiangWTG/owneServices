using CargoWise.Application;
using Enterprise.Integration.Accounting;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class PurchaseOrderGoodsReceivedNotesCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new PurchaseOrderGoodsReceivedNotesCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			PurchaseOrderGoodsReceivedNotesCodeDescriptionPairProvider testPairProvider = new PurchaseOrderGoodsReceivedNotesCodeDescriptionPairProvider();
			var list = testPairProvider.GetCodeDescriptionPairList();
			AssertContainsExactElementsInAnyOrder(list, ObjectFactory.Get<IAccounting>().GoodsReceivedStatusCodesList);
		}
	}
}
