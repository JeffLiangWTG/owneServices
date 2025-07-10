using NUnit.Framework;

namespace CargoWise.ResourceStrings.Grammar
{
	public class EnglishGrammarTest : TestCase
	{
		public void TestIndefiniteArticlePrefix()
		{
			AssertEquals("an ", Grammar.Instance.IndefiniteArticlePrefix("umbrella"));
			AssertEquals("an ", Grammar.Instance.IndefiniteArticlePrefix("Umbrella"));
			AssertEquals("a ", Grammar.Instance.IndefiniteArticlePrefix("UQ"));
			AssertEquals("a ", Grammar.Instance.IndefiniteArticlePrefix("uq"));
			AssertEquals("a ", Grammar.Instance.IndefiniteArticlePrefix("unit"));
			AssertEquals("a ", Grammar.Instance.IndefiniteArticlePrefix("Unit"));
			AssertEquals("an ", Grammar.Instance.IndefiniteArticlePrefix("indicator"));
			AssertEquals("an ", Grammar.Instance.IndefiniteArticlePrefix("Indicator"));
			AssertEquals("a ", Grammar.Instance.IndefiniteArticlePrefix("book"));
			AssertEquals("a ", Grammar.Instance.IndefiniteArticlePrefix("Book"));
			AssertEquals("a ", Grammar.Instance.IndefiniteArticlePrefix("undg"));
			AssertEquals("a ", Grammar.Instance.IndefiniteArticlePrefix("UNDG"));
			AssertEquals("an ", Grammar.Instance.IndefiniteArticlePrefix("hour"));
			AssertEquals("an ", Grammar.Instance.IndefiniteArticlePrefix("Hour"));
			AssertEquals("a ", Grammar.Instance.IndefiniteArticlePrefix("universal"));
			AssertEquals("a ", Grammar.Instance.IndefiniteArticlePrefix("Universal"));
			AssertEquals("a ", Grammar.Instance.IndefiniteArticlePrefix("united"));
			AssertEquals("a ", Grammar.Instance.IndefiniteArticlePrefix("United"));
			AssertEquals("a ", Grammar.Instance.IndefiniteArticlePrefix("european"));
			AssertEquals("a ", Grammar.Instance.IndefiniteArticlePrefix("European"));
		}
	}
}
