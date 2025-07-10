using Enterprise.DocumentEngine.ValueProviders.Macros;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(DateDiff))]
	sealed class DateDiffTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider() => new DateDiff();

		public void TestDateDiffSyntax()
		{
			CombineAssertions(() =>
			{
				Assert("Need angle braces", !ValueProviderToTest.IsResponsibleForReplacing(@"DateDiff(""ABCDE"")", Passes.FirstPass));
				Assert("Need curly braces", !ValueProviderToTest.IsResponsibleForReplacing(@"<DateDiff ""ABCDE"">", Passes.FirstPass));
				Assert("Should replace", ValueProviderToTest.IsResponsibleForReplacing("<DateDiff(\"      ABCDE      \"))>", Passes.FirstPass));
				Assert("Should replace", ValueProviderToTest.IsResponsibleForReplacing("<DateDiff(\"ABCDE\")>", Passes.FirstPass));
				Assert("Should replace", ValueProviderToTest.IsResponsibleForReplacing("<DateDiff(ABCDE)>", Passes.FirstPass));
			});
		}

		public void TestDateDiffUnits()
		{
			CombineAssertions(() =>
			{
				AssertIsReplacedWith("Replace with days", "3", "<DateDiff(\"2024-02-27\", \"2024-03-01\", \"DAYS\")>");
				AssertIsReplacedWith("Replace with days", "3", "<DateDiff(\" 2024-02-27	\", \" 2024-03-01 \", \"	DAYS\")>");
				AssertIsReplacedWith("Replace with hours", "28", "<DateDiff(\"2023-02-22 06:26 AM\", \"2023-02-23 10:26 AM\", \"HOURS\")>");
				AssertIsReplacedWith("Replace with minutes", "79", "<DateDiff(\"2023-02-22 06:26 AM\", \"2023-02-22 07:45 AM\", \"MINUTES\")>");
				AssertIsReplacedWith("Replace with seconds", "145", "<DateDiff(\"2023-02-22 06:26:05 AM\", \"2023-02-22 06:28:30 AM\", \"SECONDS\")>");
				AssertIsReplacedWith("Replace with milliseconds", "48", "<DateDiff(\"2023-02-22 06:26:05.174 AM\", \"2023-02-22 06:26:05.222 AM\", \"MILLISECONDS\")>");
			});
		}

		public void TestInvalidDate()
		{
			CombineAssertions(() =>
			{
				AssertMacroHasError("Invalid start date", "<DateDiff(\"avcdtest\", \"2023-02-22\", \"DAYS\")>");
				AssertMacroHasError("Invalid end date", "<DateDiff(\"2023-02-22\", \"bl13854\", \"HOURS\")>");
				AssertMacroHasError("Invalid time unit", "<DateDiff(\"2023-02-22\", \"2021-02-22\", \"TESTS\")>");
				AssertMacroHasError("Start date must be within quotation marks", "<DateDiff(2020-02-22\", \"2023-02-22\", \"DAYS\")>");
				AssertMacroHasError("End date must be within quotation marks", "<DateDiff(\"2023-02-22\", \"2024-04-17, \"HOURS\")>");
				AssertMacroHasError("Time unit must be within quotation marks", "<DateDiff(\"2023-02-22\", \"2021-02-22\", SECONDS)>");
				AssertMacroHasError("Unexpected number of arguments", "<DateDiff(\"2023-02-22\", \"2021-02-22\", \"SECONDS\", \"MINUTES\")>");
			});
		}
		public void TestNegativeDifference()
		{
			CombineAssertions(() =>
			{
				AssertIsReplacedWith("Replace with negative days difference", "-7", "<DateDiff(\"2023-02-22\", \"2023-02-15\", \"DAYS\")>");
				AssertIsReplacedWith("Replace with negative hours difference", "-6", "<DateDiff(\"March 01 2008 8:00 AM\", \"2008-03-01 2:00 AM\", \"HOURS\")>");
				AssertIsReplacedWith("Replace with negative minutes difference", "-9", "<DateDiff(\"2019-04-05 5:15 PM\", \"2019-04-05 5:06 PM\", \"MINUTES\")>");
				AssertIsReplacedWith("Replace with negative seconds difference", "-275", "<DateDiff(\"March 01 2008 8:17:03 PM\", \"2008-03-01 8:12:28 PM\", \"SECONDS\")>");
				AssertIsReplacedWith("Replace with negative milliseconds difference", "-101", "<DateDiff(\"2020-07-16 19:46:15.123\", \"2020-07-16 19:46:15.022\", \"MILLISECONDS\")>");
			});
		}

		public void TestLargeDifference()
		{
			CombineAssertions(() =>
			{
				AssertIsReplacedWith("Should not overflow milliseconds", "31536000000", "<DateDiff(\"2022-07-16\", \"2023-07-16\", \"MILLISECONDS\")>");
				AssertIsReplacedWith("Early / late dates should be valid", "3652058", "<DateDiff(\"0001-01-01\", \"9999-12-31\", \"DAYS\")>");
			});
		}

		public void TestRounding()
		{
			CombineAssertions(() =>
			{
				AssertIsReplacedWith("Should round down days", "2", "<DateDiff(\"2023-02-22 23:59\", \"2023-02-25\", \"DAYS\")>");
				AssertIsReplacedWith("Should round down minutes", "3", "<DateDiff(\"2023-02-22 19:47\", \"2023-02-22 23:00\", \"HOURS\")>");
				AssertIsReplacedWith("Should round down minutes", "4", "<DateDiff(\"2023-02-22 23:52:48\", \"2023-02-22 23:57\", \"MINUTES\")>");
				AssertIsReplacedWith("Should round down seconds", "9", "<DateDiff(\"2023-02-22 23:57:48.762\", \"2023-02-22 23:57:58\", \"SECONDS\")>");
			});
		}
	}
}
