using CargoWise.Integration;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Module.Testing
{
	[TestedType(typeof(TranslationFeedbackFilterBusinessObject))]
	public class TranslationFeedbackFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestEnglishNotInLanguageList()
		{
			System.Runtime.Caching.MemoryCache.Default.Remove(nameof(LanguageHelper.GetSecurityLanguageReferences));

			var testLanguage = Factory.New<IRefLocalLanguage>();
			testLanguage.RA_Code = "EN";
			testLanguage.RA_RN_NKCountryCode = "CN";
			testLanguage.RA_Description = "Test Language1";
			Factory.Save();
			var languageList = ((ModuleTextFilter)new TranslationFeedbackFilterBusinessObject()["Language"]).List as ICodeDescriptionPairList;
			Assert(!languageList.ContainsCode(Constants.Languages.English));
			Assert(!languageList.ContainsCode(Constants.Languages.EnglishAmerican));
			Assert(!languageList.ContainsCode(Constants.Languages.EnglishBritish));
			Assert(languageList.ContainsCode(testLanguage.FullLanguageCode));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new TranslationFeedbackFilterBusinessObject();
		}
	}
}
