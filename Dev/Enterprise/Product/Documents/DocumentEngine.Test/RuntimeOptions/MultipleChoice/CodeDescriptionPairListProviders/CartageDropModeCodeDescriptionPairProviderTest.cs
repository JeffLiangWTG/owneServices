using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class CartageDropModeCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new CartageDropModeCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			CodeDescriptionPairList list = new LCLAIREquipmentNeededList();
			list.AddRangeOverwriteIfExists(new FCLEquipmentNeededList());

			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), list);
		}
	}
}
