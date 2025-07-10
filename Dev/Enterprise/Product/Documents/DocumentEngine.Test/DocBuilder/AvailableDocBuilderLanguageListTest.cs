using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Languages = Enterprise.Core.SharedConstants.Languages;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	sealed class AvailableDocBuilderLanguageListTest : TestCaseWithFactory
	{
		public void TestList()
		{
			var testLanguage = Factory.New<IRefLocalLanguage>();
			testLanguage.RA_Code = "EN";
			testLanguage.RA_RN_NKCountryCode = "CN";
			testLanguage.RA_Description = "Test Language1";
			Factory.Save();

			var originalSecurityStatus = new Dictionary<string, bool>();
			try
			{
				var testLanguageCheckPoint = Env.Security.DocBuilderLanguages.ChildCheckPoints.FirstOrDefault(p => p.Code == "DocBuilderLanguage_" + testLanguage.FullLanguageCode)
					?? new SecurityCheckpoint("DocBuilderLanguage_" + testLanguage.FullLanguageCode, testLanguage.RA_DescriptionMultilingual, Env.Security.DocBuilderLanguages, Env.Security.SecurityInstance);
				if (!Env.Security.DocBuilderLanguagesLookup.ContainsKey(testLanguage.FullLanguageCode))
				{
					Env.Security.DocBuilderLanguagesLookup.Add(testLanguage.FullLanguageCode, testLanguageCheckPoint);
					MemoryCache.Default.Remove(nameof(LanguageHelper.GetSecurityLanguageReferences));
				}

				foreach (var item in Env.Security.DocBuilderLanguagesLookup)
				{
					originalSecurityStatus[item.Key] = item.Value.IsAllowed;
					if (item.Key == Languages.Spanish || item.Key == Languages.SpanishLatin || item.Key == Languages.Turkish)
					{
						item.Value.IsAllowed = false;
					}
				}

				var allLanguages = new CodeDescriptionPairList(OLookUpEditType.Language);
				var expected = Env.Licence.LanguagePackLookup.Keys
					.Except(new[] { Languages.Spanish, Languages.SpanishLatin, Languages.Turkish })
					.Concat(new[] { Languages.EnglishAmerican, Languages.EnglishBritish, testLanguage.FullLanguageCode.ToString() })
					.Select(code => allLanguages[code]);
				AssertContainsExactElementsInAnyOrder(expected, new AvailableDocBuilderLanguageList(Factory));
			}
			finally
			{
				foreach (var item in Env.Security.DocBuilderLanguagesLookup)
				{
					item.Value.IsAllowed = originalSecurityStatus[item.Key];
				}
				Env.Security.DocBuilderLanguagesLookup.Remove(testLanguage.FullLanguageCode);
				MemoryCache.Default.Remove(nameof(LanguageHelper.GetSecurityLanguageReferences));
			}
		}
	}
}
