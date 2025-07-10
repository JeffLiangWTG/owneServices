using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	[TestedType(typeof(TranslationSearchCriteria))]
	sealed class TranslationSearchCriteriaTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TranslationSearchCriteria();
		}

		#endregion

		public void TestTranslationSearchCriteriaDefaultTargetLanguage()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.EnglishAmerican))
			{
				var criteria = new TranslationSearchCriteria();
				AssertEquals("", criteria.TargetLanguage);
			}

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				var criteria = new TranslationSearchCriteria();
				AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, criteria.TargetLanguage);
			}
		}

		public void TestLanguagesList()
		{
			System.Runtime.Caching.MemoryCache.Default.Remove(nameof(LanguageHelper.GetSecurityLanguageReferences));

			var criteria = new TranslationSearchCriteria();
			var testLanguage = Factory.New<IRefLocalLanguage>();
			testLanguage.RA_Code = "EN";
			testLanguage.RA_RN_NKCountryCode = "CN";
			testLanguage.RA_Description = "Test Language1";
			Factory.Save();

			Assert("CustomLanguage should be included in the language list", criteria.Languages.ContainsCode(testLanguage.FullLanguageCode));
		}
	}
}
