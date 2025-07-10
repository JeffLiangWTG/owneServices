using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	class ZStringExtensionsTest : TestCase
	{
		#region TestTrimOneDoubleQuoteFromEachEnd

		public void TestTrimOneDoubleQuoteFromEachEnd()
		{
			AssertEquals("new ZString().TrimOneDoubleQuoteFromEachEnd()", "", new ZString().TrimOneDoubleQuoteFromEachEnd());
			AssertEquals("new ZString(\").TrimOneDoubleQuoteFromEachEnd()", "\"", new ZString("\"").TrimOneDoubleQuoteFromEachEnd());
			AssertEquals("new ZString(\"\").TrimOneDoubleQuoteFromEachEnd()", "", new ZString("\"\"").TrimOneDoubleQuoteFromEachEnd());
			AssertEquals("new ZString(\"\"\").TrimOneDoubleQuoteFromEachEnd()", "\"", new ZString("\"\"\"").TrimOneDoubleQuoteFromEachEnd());
			AssertEquals("new ZString(\"1\").TrimOneDoubleQuoteFromEachEnd()", "1", new ZString("\"1\"").TrimOneDoubleQuoteFromEachEnd());
			AssertEquals("new ZString(\"1).TrimOneDoubleQuoteFromEachEnd()", "1", new ZString("\"1").TrimOneDoubleQuoteFromEachEnd());
			AssertEquals("new ZString(1\").TrimOneDoubleQuoteFromEachEnd()", "1", new ZString("1\"").TrimOneDoubleQuoteFromEachEnd());
		}

		#endregion

		#region TestWrapInDoubleQuotes

		public void TestWrapInDoubleQuotes()
		{
			AssertEquals("new ZString().WrapInDoubleQuotes()", "\"\"", new ZString().WrapInDoubleQuotes());
			AssertEquals("new ZString(\").WrapInDoubleQuotes()", "\"\"\"", new ZString("\"").WrapInDoubleQuotes());
		}

		#endregion
	}
}
