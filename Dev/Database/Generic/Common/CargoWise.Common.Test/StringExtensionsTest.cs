using System;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	public class StringExtensionsTest : TestCase
	{
		public void TestSplitByLine()
		{
			var testString = @"One Two
Three Four

Five
Six";
			var results = testString.SplitByLine().ToArray();
			AssertEquals(5, results.Length);
			AssertContainsExactElementsInAnyOrder(new[] { "One Two", "Three Four", string.Empty, "Five", "Six" }, results);
			testString = "Seven" + Environment.NewLine + "Ate" + Environment.NewLine + "Nine";
			results = testString.SplitByLine().ToArray();
			AssertEquals(3, results.Length);
			AssertContainsExactElementsInAnyOrder(new[] { "Seven", "Ate", "Nine" }, results);
		}

		public void TestQuoteName()
		{
			AssertEquals("[some name]", "some name".QuoteName());
			AssertEquals("[some [n]]ame]", "some [n]ame".QuoteName());
			AssertEquals("[[some name]]]", "[some name]".QuoteName());
			AssertEquals("[some name]", "some name".QuoteName('['));
			AssertEquals("[some [n]]ame]", "some [n]ame".QuoteName('['));
			AssertEquals("[[some name]]]", "[some name]".QuoteName('['));
			AssertEquals("[some name]", "some name".QuoteName(']'));
			AssertEquals("[some [n]]ame]", "some [n]ame".QuoteName(']'));
			AssertEquals("[[some name]]]", "[some name]".QuoteName(']'));
			AssertEquals("'some name'", "some name".QuoteName('\''));
			AssertEquals("'some ''name'", "some 'name".QuoteName('\''));
			AssertEquals("'''some name'''", "'some name'".QuoteName('\''));
			AssertEquals("(some name)", "some name".QuoteName('('));
			AssertEquals("(some (n)ame)", "some (n)ame".QuoteName('('));
			AssertEquals("((some name))", "(some name)".QuoteName('('));
			AssertEquals("(some name)", "some name".QuoteName(')'));
			AssertEquals("(some (n)ame)", "some (n)ame".QuoteName(')'));
			AssertEquals("((some name))", "(some name)".QuoteName(')'));
			AssertEquals("{some name}", "some name".QuoteName('{'));
			AssertEquals("{some {n}ame}", "some {n}ame".QuoteName('{'));
			AssertEquals("{{some name}}", "{some name}".QuoteName('{'));
			AssertEquals("{some name}", "some name".QuoteName('}'));
			AssertEquals("{some {n}ame}", "some {n}ame".QuoteName('}'));
			AssertEquals("{{some name}}", "{some name}".QuoteName('}'));
			AssertEquals("_some name_", "some name".QuoteName('_'));
			AssertEquals("_some_name_", "some_name".QuoteName('_'));
			AssertEquals("__some name__", "_some name_".QuoteName('_'));
			AssertEquals(null, ((string)null).QuoteName());
			AssertEquals(null, ((string)null).QuoteName('['));
			AssertEquals(null, ((string)null).QuoteName(']'));
			AssertEquals(null, ((string)null).QuoteName('\''));
			AssertEquals(null, ((string)null).QuoteName('('));
			AssertEquals(null, ((string)null).QuoteName(')'));
			AssertEquals(null, ((string)null).QuoteName('{'));
			AssertEquals(null, ((string)null).QuoteName('}'));
			AssertEquals(null, ((string)null).QuoteName('_'));
		}

		public void TestQuoteEscapedName()
		{
			AssertEquals("some name", "some name".QuoteEscapedName());
			AssertEquals("some [n]]ame", "some [n]ame".QuoteEscapedName());
			AssertEquals("[some [n]]ame]]", "[some [n]ame]".QuoteEscapedName());
			AssertEquals("some name", "some name".QuoteEscapedName('['));
			AssertEquals("some [n]]ame", "some [n]ame".QuoteEscapedName('['));
			AssertEquals("[some [n]]ame]]", "[some [n]ame]".QuoteEscapedName('['));
			AssertEquals("some name", "some name".QuoteEscapedName(']'));
			AssertEquals("some [n]]ame", "some [n]ame".QuoteEscapedName(']'));
			AssertEquals("[some [n]]ame]]", "[some [n]ame]".QuoteEscapedName(']'));
			AssertEquals("some name", "some name".QuoteEscapedName('\''));
			AssertEquals("some ''n''ame", "some 'n'ame".QuoteEscapedName('\''));
			AssertEquals("''some name''", "'some name'".QuoteEscapedName('\''));
			AssertEquals("'some [n]ame'", "'some [n]ame'".QuoteEscapedName('_'));
			AssertEquals(null, ((string)null).QuoteEscapedName());
			AssertEquals(null, ((string)null).QuoteEscapedName('['));
			AssertEquals(null, ((string)null).QuoteEscapedName(']'));
			AssertEquals(null, ((string)null).QuoteEscapedName('\''));
			AssertEquals(null, ((string)null).QuoteEscapedName('_'));
		}
	}
}