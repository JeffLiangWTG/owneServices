using CargoWiseOne.ResourceStrings;

namespace CargoWise.ResourceStrings.Grammar
{
	public static class Grammar
	{
		public static IGrammar Instance => GrammarFactory.GetGrammar(Res.CurrentLanguage);
	}
}
