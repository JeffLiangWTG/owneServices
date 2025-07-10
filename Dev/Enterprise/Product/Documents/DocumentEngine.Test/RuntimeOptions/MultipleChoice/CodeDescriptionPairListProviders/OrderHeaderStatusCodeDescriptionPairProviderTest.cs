using CargoWise.Application;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class OrderHeaderStatusCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new OrderHeaderStatusCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			var orderStatuses = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();

			Enterprise.Freight.Integration.Forwarding.IOrderStatusListProvider listProvider = ObjectFactory.Get<Enterprise.Freight.Integration.Forwarding.IOrderStatusListProvider>();
			foreach (CodeDescriptionPair pair in listProvider.GetOrderStatusList())
			{
				AssertCollectionContains(pair, orderStatuses);
			}
		}
	}
}
