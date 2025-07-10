using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class GLLanguageCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new GLLanguageCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			var generatedList = (CodeDescriptionPairList)CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
			CodeDescriptionPairList expectedList = new CodeDescriptionPairList(OLookUpEditType.GLLanguage);
			AssertEquals(expectedList.CodesAsString, generatedList.CodesAsString);
		}
	}
}
