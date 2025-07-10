using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using NUnit.Framework;

namespace CargoWise.ResourceStrings.Grammar
{
	class GrammarFactoryTest : TestCase
	{
		public void TestFactoryReturnsValueForAllLanguages()
		{
			foreach (var language in DataFile.GetAvailableLanguages())
			{
				AssertNotNull(GrammarFactory.GetGrammar(language));
				if (!Res.IsEnglish(language))
				{
					AssertNotEquals(typeof(EnglishGrammar), GrammarFactory.GetGrammar(language).GetType());
				}
			}

			AssertType(typeof(EnglishGrammar), GrammarFactory.GetGrammar(Res.DefaultLanguage));
			AssertType(typeof(EnglishGrammar), GrammarFactory.GetGrammar(SharedConstants.Languages.EnglishAmerican));
			AssertType(typeof(EnglishGrammar), GrammarFactory.GetGrammar(SharedConstants.Languages.EnglishBritish));
			AssertType(typeof(EnglishGrammar), Grammar.Instance);

			AssertEquals(string.Empty, GrammarFactory.GetGrammar("GRM").IndefiniteArticlePrefix("Word"));
		}
	}
}
