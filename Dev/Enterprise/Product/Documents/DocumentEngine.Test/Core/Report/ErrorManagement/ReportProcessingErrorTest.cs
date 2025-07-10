using Enterprise.DocumentEngine.FlexCelInterface;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.ReportErrorManagement.Testing
{
	sealed class ReportProcessingErrorTest : TestCase
	{
		public void TestIncrementOccurrences()
		{
			IReportProcessingError reportProcessingError = new ReportProcessingError("Invalid expression", new CellReference("Test sheet", "A23"), ReportProcessingErrorSeverity.Error);
			AssertEquals("reportProcessingError.Occurrences", 1, reportProcessingError.Occurrences);
			reportProcessingError.IncrementOccurrences();
			AssertEquals("reportProcessingError.Occurrences", 2, reportProcessingError.Occurrences);
			reportProcessingError.IncrementOccurrences();
			AssertEquals("reportProcessingError.Occurrences", 3, reportProcessingError.Occurrences);
		}

		public void TestSetCurrentStateValues()
		{
			IReportProcessingError reportProcessingError = new ReportProcessingError("Invalid expression", new CellReference("Test Sheet", "A23"), ReportProcessingErrorSeverity.Error);
			AssertEquals("reportProcessingError.OuterContent", null, reportProcessingError.OuterContent);
			AssertEquals("reportProcessingError.InnerMacro", null, reportProcessingError.InnerMacro);
			AssertEquals("reportProcessingError.SheetName", "Test Sheet", reportProcessingError.SheetName);
			AssertEquals("reportProcessingError.CellName", "A23", reportProcessingError.CellName);

			reportProcessingError.SetCurrentStatus("Fred", "Flintstone", new CellReference("Flat Sheet", "B1"));
			AssertEquals("reportProcessingError.OuterContent", "Fred", reportProcessingError.OuterContent);
			AssertEquals("reportProcessingError.InnerMacro", "Flintstone", reportProcessingError.InnerMacro);
			AssertEquals("reportProcessingError.SheetName", "Test Sheet", reportProcessingError.SheetName);
			AssertEquals("reportProcessingError.CellName", "A23", reportProcessingError.CellName);

			reportProcessingError = new ReportProcessingError("Invalid expression", ReportProcessingErrorSeverity.Error);
			reportProcessingError.SetCurrentStatus("Barney", "Rubble", new CellReference("Fitted Sheet", "B2"));
			AssertEquals("reportProcessingError.OuterContent", "Barney", reportProcessingError.OuterContent);
			AssertEquals("reportProcessingError.InnerMacro", "Rubble", reportProcessingError.InnerMacro);
			AssertEquals("reportProcessingError.SheetName", "Fitted Sheet", reportProcessingError.SheetName);
			AssertEquals("reportProcessingError.CellName", "B2", reportProcessingError.CellName);
		}

		public void TestConstruct()
		{
			ReportProcessingError reportProcessingError = new ReportProcessingError("Invalid expression", new CellReference("Test sheet", "A23"), ReportProcessingErrorSeverity.Error);
			AssertNotNull("reportProcessingError should not be null", reportProcessingError);
			AssertEquals("reportProcessingError.Sheet", "Test sheet", reportProcessingError.SheetName);
			AssertEquals("reportProcessingError.CellName", "A23", reportProcessingError.CellName);
			AssertEquals("reportProcessingError.Message", "Invalid expression", reportProcessingError.Message);
			AssertEquals("reportProcessingError.Severity", ReportProcessingErrorSeverity.Error, reportProcessingError.Severity);
		}

		public void TestToString()
		{
			ReportProcessingError reportProcessingError = new ReportProcessingError("Invalid Expression", new CellReference("Other Sheet", 5, 3), ReportProcessingErrorSeverity.Error);
			((IReportProcessingError)reportProcessingError).SetTemplatePath("ZeusDrivesACRX.xls");
			AssertEquals("reportProcessingError.ToString", @"Error on Sheet: [Other Sheet] at Cell D6: [Invalid Expression] in File: [ZeusDrivesACRX.xls]", reportProcessingError.ToString());
		}
	}
}
