using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class LanguageCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new LanguageCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			AssertEquals(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList().GetType(), new CodeDescriptionPairList(OLookUpEditType.Language).GetType());
		}
	}
}
