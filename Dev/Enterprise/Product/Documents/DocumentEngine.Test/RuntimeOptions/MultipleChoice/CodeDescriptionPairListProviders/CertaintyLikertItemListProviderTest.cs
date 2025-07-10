using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class CertaintyLikertItemListProviderTest : CodeDescriptionPairListProviderTest
	{
		public override void TestIsReturningCorrectCollection()
		{
			var expectedList = new CertaintyLikertItemList();
			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), expectedList);
		}

		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new CertaintyLikertItemListProvider();
		}
	}
}
