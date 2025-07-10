using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class WhsPickMethodCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new WhsPickMethodCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), WarehouseDataRegistry.Instance.PickMethod.Value.GetCodeDescriptionPairList());
		}
	}
}
