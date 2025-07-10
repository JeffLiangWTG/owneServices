namespace CargoWise.ResourceStrings.Grammar
{
	class DefaultGrammar : IGrammar
	{
		public string IndefiniteArticlePrefix(string subject)
		{
			return string.Empty;
		}

		public string Pluralize(string singluarNoun)
		{
			return singluarNoun;
		}
	}
}
