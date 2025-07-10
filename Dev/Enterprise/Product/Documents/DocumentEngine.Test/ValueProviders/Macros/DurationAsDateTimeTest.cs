using System;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(DurationAsDateTime))]
	sealed class DurationAsDateTimeTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider() => new DurationAsDateTime();

		public void AssertDurationAsDateTime(object expected, string macro)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expecting macro to translate", expected, new MacroTranslator(Report).GetValue(macro, Passes.FirstPass));
				Assert("Expected no errors but" + string.Join(System.Environment.NewLine, Report.ErrorManager.ToString()), !Report.ErrorManager.HasErrors);
			});
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<DurationAsDateTime()>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<DurationAsDateTime(:)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<DurationAsDateTime(\"\")>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<DurationAsDateTime(\":\")>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<DurationAsDateTime(\"40\")>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<DurationAsDateTime(\"12:\")>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<DurationAsDateTime(\":40\")>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<DurationAsDateTime(\"0:40\")>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<DurationAsDateTime(\"12:10\")>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<DurationAsDateTime(\"12:ABC\")>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<  DurationAsDateTime(  \"12:ABC        \"    )            >", Passes.FirstPass));
		}

		[TestDate(1991, 07, 03)]
		public void TestMacro()
		{
			ZDateTime date = ZDateTime.DefaultDurationEpoch;
			AssertDurationAsDateTime(date, "<DurationAsDateTime(\":\")>");
			AssertDurationAsDateTime(date, "<DurationAsDateTime(\"0:0\")>");
			AssertDurationAsDateTime(date.AddMinutes(1), "<DurationAsDateTime(\":1\")>");
			AssertDurationAsDateTime(date.AddHours(100), "<DurationAsDateTime(\"100:\")>");
			AssertDurationAsDateTime(date.AddMinutes(1), "<DurationAsDateTime(\"0:1\")>");
			AssertDurationAsDateTime(date.AddHours(100), "<DurationAsDateTime(\"100:0\")>");
			AssertDurationAsDateTime(date.AddHours(1).AddMinutes(1), "<DurationAsDateTime(\"1:1\")>");
			AssertDurationAsDateTime(date.AddHours(1).AddMinutes(40), "<DurationAsDateTime(\"01:40\")>");
			AssertDurationAsDateTime(date.AddHours(1).AddMinutes(40), "<DurationAsDateTime(\"001:40\")>");
			AssertDurationAsDateTime(date.AddHours(1).AddMinutes(40), "<DurationAsDateTime(\"1:040\")>");
			AssertDurationAsDateTime(date.AddHours(25).AddMinutes(40), "<DurationAsDateTime(\"25:40\")>");
		}

		[TestDate(1991, 07, 03)]
		public void TestNestedMacro()
		{
			AssertDurationAsDateTime((ZDateTime)TimeSpan.FromHours(3), "<DurationAsDateTime(\"<Add(\"1\", \"2\")>:0\")>");
		}

		[TestDate(1991, 07, 03)]
		public void TestReportError()
		{
			MacroTranslator macro = new MacroTranslator(Report);
			macro.GetValue("<DurationAsDateTime(\"1000:40\")>", Passes.FirstPass);
			Assert(Report.ErrorManager.HasErrors);
			AssertContains("Incorrect format, should be", Report.ErrorManager.ToString());

			Report.ErrorManager.ClearErrors();
			macro.GetValue("<DurationAsDateTime(\"40\")>", Passes.FirstPass);
			Assert(Report.ErrorManager.HasErrors);
			AssertContains("Incorrect format, should be", Report.ErrorManager.ToString());

			Report.ErrorManager.ClearErrors();
			macro.GetValue("<DurationAsDateTime(\"ABC:DEF\")>", Passes.FirstPass);
			Assert(Report.ErrorManager.HasErrors);
			AssertContains("Incorrect format, should be", Report.ErrorManager.ToString());
		}
	}
}
