using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class TranslateLanguageCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new TranslateLanguageCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			var generatedList = (CodeDescriptionPairList)CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
			var expectedList = new AvailableDocBuilderLanguageList(new BusinessObjectFactory());
			expectedList.RemoveCode(Core.Constants.Languages.EnglishAmerican);
			expectedList.RemoveCode(Core.Constants.Languages.EnglishBritish);
			AssertEquals(expectedList.Count, generatedList.Count);
			AssertEquals(expectedList.CodesAsString, generatedList.CodesAsString);
		}
	}
}
