using Enterprise.DocumentEngine.ReportErrorManagement;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	sealed class TemplateErrorTest : TestCase
	{
		public void TestToReportProcessingError()
		{
			var templateError = new TemplateGenerationError("Test", ReportProcessingErrorSeverity.Warning);
			AssertEquals("templateError.Message", "Test", templateError.Message);
			AssertEquals("templateError.ToReportProcessingError().Message", "Test", templateError.ToReportProcessingError().Message);
			AssertEquals("templateError.ToReportProcessingError().Severity", ReportProcessingErrorSeverity.Warning, templateError.ToReportProcessingError().Severity);
		}
	}
}
