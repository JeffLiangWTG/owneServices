using CargoWise.EntityFramework.Testing;

namespace Enterprise.ResourceStrings.Business.Testing
{
	sealed class TranslationSearchCriteriaValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSourceTextValidation()
		{
			var criteria = new TranslationSearchCriteria();
			criteria.SearchSource = true;
			criteria.SourceText = "Hello";
			AssertNoErrors(criteria.SourceTextInfo);
			criteria.SourceText = "";
			AssertHasErrors(criteria.SourceTextInfo);
			criteria.SearchSource = false;
			AssertNoErrors(criteria.SourceTextInfo);
			criteria.UseRegularExpressions = true;
			criteria.SourceText = "(What?";
			AssertNoErrors(criteria.SourceTextInfo);
			criteria.SearchSource = true;
			AssertHasErrors(criteria.SourceTextInfo);
			criteria.UseRegularExpressions = false;
			AssertNoErrors(criteria.SourceTextInfo);
		}

		public void TestTargetTextValidation()
		{
			var criteria = new TranslationSearchCriteria();
			criteria.SearchTarget = true;
			criteria.TargetText = "Hello";
			AssertNoErrors(criteria.TargetTextInfo);
			criteria.TargetText = "";
			AssertHasErrors(criteria.TargetTextInfo);
			criteria.SearchTarget = false;
			AssertNoErrors(criteria.TargetTextInfo);
			criteria.UseRegularExpressions = true;
			criteria.TargetText = "(What?";
			AssertNoErrors(criteria.TargetTextInfo);
			criteria.SearchTarget = true;
			AssertHasErrors(criteria.TargetTextInfo);
			criteria.UseRegularExpressions = false;
			AssertNoErrors(criteria.TargetTextInfo);
		}

		public void TestSearchSourceOrTargetValidation()
		{
			var criteria = new TranslationSearchCriteria();
			criteria.SearchSource = criteria.SearchTarget = false;
			AssertHasErrors(criteria.SearchSourceInfo);
			AssertHasErrors(criteria.SearchTargetInfo);
			criteria.SearchSource = true;
			AssertNoErrors(criteria.SearchSourceInfo);
			AssertNoErrors(criteria.SearchTargetInfo);
			criteria.SearchSource = criteria.SearchTarget = false;
			AssertHasErrors(criteria.SearchSourceInfo);
			AssertHasErrors(criteria.SearchTargetInfo);
			criteria.SearchTarget = true;
			AssertNoErrors(criteria.SearchSourceInfo);
			AssertNoErrors(criteria.SearchTargetInfo);
			criteria.SearchSource = criteria.SearchTarget = false;
			AssertHasErrors(criteria.SearchSourceInfo);
			AssertHasErrors(criteria.SearchTargetInfo);
			criteria.SearchSource = criteria.SearchTarget = true;
			AssertNoErrors(criteria.SearchSourceInfo);
			AssertNoErrors(criteria.SearchTargetInfo);
		}

		public void TestTargetLanguageValidation()
		{
			var criteria = new TranslationSearchCriteria();
			criteria.SearchTarget = true;
			criteria.TargetLanguage = "";
			AssertHasError(criteria.TargetLanguageInfo, "Please enter a Language.");
			criteria.TargetLanguage = Core.SharedConstants.Languages.ChineseSimplified;
			AssertNoErrors(criteria.TargetLanguageInfo);
			criteria.TargetLanguage = "ENG";
			AssertHasError(criteria.TargetLanguageInfo, "Enter a valid Language.");
			criteria.TargetLanguage = "INV";
			AssertHasErrors(criteria.TargetLanguageInfo);
		}
	}
}
