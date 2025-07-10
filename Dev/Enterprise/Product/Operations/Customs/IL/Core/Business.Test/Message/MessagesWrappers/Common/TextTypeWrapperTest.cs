using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class TextTypeWrapperTest : DataProviderTestCase<TextTypeWrapper>
	{
		public void TestValue()
		{
			AssertEquals("Value must be equal to the expected value", "Text", Provider.Value);
		}

		public void TestLanguageID()
		{
			AssertNull("LanguageID", Provider.LanguageID);

			var provider = TextTypeWrapper.NewOrNull("Text", "LangId");
			AssertEquals("LanguageID must be equal to the expected value", "LangId", provider.LanguageID);
		}

		protected override TextTypeWrapper GetProvider()
		{
			return TextTypeWrapper.NewOrNull("Text");
		}
	}
}
