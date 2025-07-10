namespace CargoWise.ResourceStrings.Grammar
{
	public interface IGrammar
	{
		string IndefiniteArticlePrefix(string subject);
		string Pluralize(string singularNoun);
	}
}
