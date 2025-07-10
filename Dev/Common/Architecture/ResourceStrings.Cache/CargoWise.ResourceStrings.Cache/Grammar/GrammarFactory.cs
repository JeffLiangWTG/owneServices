using CargoWiseOne.ResourceStrings;

namespace CargoWise.ResourceStrings.Grammar
{
	static class GrammarFactory
	{
		internal static IGrammar GetGrammar(string language)
		{
			if (Res.IsEnglish(language))
			{
				return new EnglishGrammar();
			}
			else
			{
				return new DefaultGrammar();
			}
		}
	}
}
