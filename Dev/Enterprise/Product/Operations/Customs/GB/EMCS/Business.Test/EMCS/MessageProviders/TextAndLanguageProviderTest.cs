using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	sealed class TextAndLanguageProviderTest : Customs.Business.Testing.DataProviderTestCase<TextAndLanguageProvider>
	{
		public void TestText()
		{
			var textAndLanguageProvider = new TextAndLanguageProvider("TESTED TEXT");
			AssertEquals("TESTED TEXT", textAndLanguageProvider.Text);
		}

		public void TestLanguage_EmptyIfTextEmpty()
		{
			var textAndLanguageProvider = new TextAndLanguageProvider(ZString.Empty);
			AssertEquals(ZString.Empty, textAndLanguageProvider.Language);
			textAndLanguageProvider = new TextAndLanguageProvider(ZString.Empty, ZString.Empty);
			AssertEquals(ZString.Empty, textAndLanguageProvider.Language);
			textAndLanguageProvider = new TextAndLanguageProvider(ZString.Empty, "XX");
			AssertEquals(ZString.Empty, textAndLanguageProvider.Language);
		}

		public void TestLanguage_NotEmptyIfTextNotEmpty()
		{
			var textAndLanguageProvider = new TextAndLanguageProvider("TEST TEXT", ZString.Empty);
			AssertEquals("en", textAndLanguageProvider.Language);
			textAndLanguageProvider = new TextAndLanguageProvider("TEST TEXT", "ZZ");
			AssertEquals("zz", textAndLanguageProvider.Language);
		}

		protected override TextAndLanguageProvider GetProvider() => new TextAndLanguageProvider("TEST TEXT");
	}
}
