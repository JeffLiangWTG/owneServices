using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;
using Contains = Enterprise.DocumentEngine.ValueProviders.Macros.Contains;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(Contains))]
	sealed class ContainsTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider() => new Contains();

		public void TestContainsSyntax()
		{
			CombineAssertions(() =>
			{
				AssertNotResponsibleForReplacing(@"Contains(""ABCDE"")");
				AssertNotResponsibleForReplacing(@"<Contains ""ABCDE"">");
				AssertIsResponsibleForReplacing("<Contains(\"      ABCDE      \"))>");
				AssertIsResponsibleForReplacing("<Contains(\"ABCDE\")>");
				AssertIsResponsibleForReplacing("<Contains(ABCDE)>");
			});
		}

		public void TestContainsLogic()
		{
			CombineAssertions(() =>
			{
				AssertIsReplacedWith("Apple contains app", "Y", "<Contains(\"APPLE\", \"APP\")>");
				AssertIsReplacedWith("Orange does not contain app", "N", "<Contains(\"ORANGE\", \"APP\")>");
				AssertIsReplacedWith("Case insensitive second argument", "Y", "<Contains(\"APPLE\", \"App\")>");
				AssertIsReplacedWith("Case insensitive first argument", "Y", "<Contains(\"Apple\", \"APP\")>");
				AssertIsReplacedWith("App does not contain Apple", "N", "<Contains(\"APP\", \"APPLE\")>");
				AssertIsReplacedWith("Contains multiple", "Y", "<Contains(\"APPLEAPPLEAPPLE\", \"APP\")>");
				AssertIsReplacedWith("Same string", "Y", "<Contains(\"TESTTEST\", \"TESTTEST\")>");
				AssertIsReplacedWith("Two empty strings", "Y", "<Contains(\"\", \"\")>");
				AssertIsReplacedWith("One empty strings", "Y", "<Contains(\"APPLE\", \"\")>");
				AssertIsReplacedWith("One character", "Y", "<Contains(\"BLAH\", \"A\")>");
				AssertIsReplacedWith("Two words", "Y", "<Contains(\"Ap pple\", \"Ap\")>");
				AssertIsReplacedWith("Trim outside quotes", "Y", "<Contains(\"Apple\" ,  \"Ap\")>");
				AssertIsReplacedWith("Don't trim inside quotes", "Y", "<Contains(\" App\", \"Ap\")>");
				AssertIsReplacedWith("Handles strings containing commas", "Y", "<Contains(\"A,B,C\", \"B,C\")>");
				AssertIsReplacedWith("Apple+ contains +", "Y", "<Contains(\"APPLE+\", \"+\")>");
			});
		}

		public void TestInvalidArguments()
		{
			CombineAssertions(() =>
			{
				AssertMacroHasError("Unexpected arguments", "<Contains(APPLE\", \"APP\")>");
				AssertMacroHasError("Unexpected arguments", "<Contains(\"BLAH\", AH)>");
				AssertMacroHasError("Unexpected arguments", "<Contains(\"A\", \"B\", \"C\")>");
			});
		}
	}
}
