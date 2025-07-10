using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	sealed class TextAndLanguageProviderTest : Customs.Business.Testing.DataProviderTestCase<TextAndLanguageProvider>
	{
		public void TestText()
		{
			var textAndLanguageProvider = new TextAndLanguageProvider("TESTED TEXT");
			AssertEquals("TESTED TEXT", textAndLanguageProvider.Text);
		}

		public void TestLanguage_CurrentBranch()
		{
			var textAndLanguageProvider = new TextAndLanguageProvider("UNTESTED TEXT");
			AssertEquals(currentBranchLanguageLowercase, textAndLanguageProvider.Language);
		}

		public void TestLanguage_OrgProxy()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Language = "KO";
			Factory.Save();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = orgHeader.PK;
			var textAndLanguageProvider = new TextAndLanguageProvider("UNTESTED TEXT");
			AssertEquals("ko", textAndLanguageProvider.Language);
		}

		public void TestLanguage_EmptyIfTextEmpty()
		{
			var textAndLanguageProvider = new TextAndLanguageProvider(ZString.Empty);
			AssertEquals(ZString.Empty, textAndLanguageProvider.Language);
		}

		public void TestLanguage_Constructor()
		{
			CombineAssertions(() =>
			{
				var textAndLanguageProvider = new TextAndLanguageProvider("UNTESTED TEXT", null);
				AssertEquals("Null parameter", currentBranchLanguageLowercase, textAndLanguageProvider.Language);
				textAndLanguageProvider = new TextAndLanguageProvider("UNTESTED TEXT", string.Empty);
				AssertEquals("empty language", currentBranchLanguageLowercase, textAndLanguageProvider.Language);
				textAndLanguageProvider = new TextAndLanguageProvider("UNTESTED TEXT", "de");
				AssertEquals("set language", "de", textAndLanguageProvider.Language);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			currentBranchLanguageLowercase = GlbBranch.CurrentBranch.Language.ToLower();
		}
		ZString currentBranchLanguageLowercase;

		protected override TextAndLanguageProvider GetProvider() => new TextAndLanguageProvider("TEST TEXT");
	}
}
