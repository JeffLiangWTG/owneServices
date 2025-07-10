using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(AddDurationToDate))]
	sealed class AddDurationToDateTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider() => new AddDurationToDate();

		public void AssertAddDurationToDate(object expected, string macro)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expecting macro to translate", expected, new MacroTranslator(Report).GetValue(macro, Passes.FirstPass));
				Assert("Expected no errors but" + string.Join(System.Environment.NewLine, Report.ErrorManager.ToString()), !Report.ErrorManager.HasErrors);
			});
		}

		[TestDate(1991, 07, 03)]
		public void TestAdd()
		{
			AssertAddDurationToDate(ZDateTime.Now.AddMilliseconds(1), "<AddDurationToDate(\"<Now>\",\"MILLISECONDS\",\"1\")>");
			AssertAddDurationToDate(ZDateTime.Now.AddSeconds(1), "<AddDurationToDate(\"<Now>\",\"SECONDS\",\"1\")>");
			AssertAddDurationToDate(ZDateTime.Now.AddHours(1), "<AddDurationToDate(\"<Now>\",\"HOURS\",\"1\")>");
			AssertAddDurationToDate(ZDateTime.Now.AddMinutes(1), "<AddDurationToDate(\"<Now>\",\"MINUTES\",\"1\")>");
			AssertAddDurationToDate(ZDateTime.Now.AddDays(1), "<AddDurationToDate(\"<Now>\",\"DAYS\",\"1\")>");
			AssertAddDurationToDate(ZDateTime.Now.AddDays(200), "<AddDurationToDate(\"<Now>\",\"DAYS\",\"200\")>");

			AssertAddDurationToDate(new ZDate(2004, 1, 20), "<AddDurationToDate(\"19/01/2004 12:00:00 AM\", \"DAYS\", \"1\")>");
		}

		[TestDate(1991, 07, 03)]
		public void TestAddNegative()
		{
			AssertAddDurationToDate(ZDateTime.Now.AddMilliseconds(-1), "<AddDurationToDate(\"<Now>\",\"MILLISECONDS\",\"-1\")>");
			AssertAddDurationToDate(ZDateTime.Now.AddSeconds(-1), "<AddDurationToDate(\"<Now>\",\"SECONDS\",\"-1\")>");
			AssertAddDurationToDate(ZDateTime.Now.AddHours(-1), "<AddDurationToDate(\"<Now>\",\"HOURS\",\"-1\")>");
			AssertAddDurationToDate(ZDateTime.Now.AddMinutes(-1), "<AddDurationToDate(\"<Now>\",\"MINUTES\",\"-1\")>");
			AssertAddDurationToDate(ZDateTime.Now.AddDays(-1), "<AddDurationToDate(\"<Now>\",\"DAYS\",\"-1\")>");
			AssertAddDurationToDate(ZDateTime.Now.AddDays(-200), "<AddDurationToDate(\"<Now>\",\"DAYS\",\"-200\")>");
		}

		[TestDate(1991, 07, 03)]
		public void TestEmptyDate()
		{
			AssertAddDurationToDate("", "<AddDurationToDate(\"\",\"DAYS\",\"-200\")>");
		}

		[TestDate(1991, 07, 03)]
		public void TestAddZero()
		{
			AssertAddDurationToDate(ZDateTime.Now, "<AddDurationToDate(\"<Now>\",\"DAYS\",\"0\")>");
		}

		[TestDate(1991, 07, 03)]
		public void TestAddNestedMacro()
		{
			AssertAddDurationToDate(ZDateTime.Now.AddDays(3), "<AddDurationToDate(\"<Now>\",\"DAYS\",\"<Add(\"1\", \"2\")>\")>");
		}

		[TestDate(1991, 07, 03)]
		public void TestReportError()
		{
			new MacroTranslator(Report).GetValue("<AddDurationToDate(\"<Now>\",\"OOOOOOOOHHHH\",\"AAAAAAAAAAAAAAAHHHHHH\")>", Passes.FirstPass);
			Assert(Report.ErrorManager.HasErrors);
			AssertContains("Invalid parameters provided:", Report.ErrorManager.ToString());
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("JK_MasterBillIssueDate", new ZDateTime(2004, 1, 19)));
		}
	}
}
