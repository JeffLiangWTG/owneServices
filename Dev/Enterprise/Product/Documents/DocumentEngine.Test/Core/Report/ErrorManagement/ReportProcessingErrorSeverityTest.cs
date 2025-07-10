using Enterprise.DocumentEngine.Exceptions;

namespace Enterprise.DocumentEngine.ReportErrorManagement.Testing
{
	sealed class ReportProcessingErrorSeverityTest : NUnit.Framework.TestCase
	{
		public void TestToStringFormatted()
		{
			AssertEquals("Error", ReportProcessingErrorSeverity.Error.ToStringFormatted());
			AssertEquals("Error (without error report)", ReportProcessingErrorSeverity.ErrorWithoutErrorReport.ToStringFormatted());
			AssertEquals("Warning", ReportProcessingErrorSeverity.Warning.ToStringFormatted());
			AssertEquals("Warning (without error report)", ReportProcessingErrorSeverity.WarningWithoutErrorReport.ToStringFormatted());
			AssertEquals("Fatal Error", ReportProcessingErrorSeverity.Fatal.ToStringFormatted());
			AssertEquals("Fatal Error (without error report)", ReportProcessingErrorSeverity.FatalWithoutErrorReport.ToStringFormatted());
			AssertEquals("Information", ReportProcessingErrorSeverity.Information.ToStringFormatted());
			AssertEquals("Information (without error report)", ReportProcessingErrorSeverity.InformationWithoutErrorReport.ToStringFormatted());
		}

		public void TestGetNonReportableEquivalent()
		{
			AssertEquals(ReportProcessingErrorSeverity.FatalWithoutErrorReport, ReportProcessingErrorSeverity.Fatal.GetNonReportableEquivalent());
			AssertEquals(ReportProcessingErrorSeverity.ErrorWithoutErrorReport, ReportProcessingErrorSeverity.Error.GetNonReportableEquivalent());
			AssertEquals(ReportProcessingErrorSeverity.WarningWithoutErrorReport, ReportProcessingErrorSeverity.Warning.GetNonReportableEquivalent());
			AssertEquals(ReportProcessingErrorSeverity.InformationWithoutErrorReport, ReportProcessingErrorSeverity.Information.GetNonReportableEquivalent());

			AssertEquals(ReportProcessingErrorSeverity.FatalWithoutErrorReport, ReportProcessingErrorSeverity.FatalWithoutErrorReport.GetNonReportableEquivalent());
			AssertEquals(ReportProcessingErrorSeverity.ErrorWithoutErrorReport, ReportProcessingErrorSeverity.ErrorWithoutErrorReport.GetNonReportableEquivalent());
			AssertEquals(ReportProcessingErrorSeverity.WarningWithoutErrorReport, ReportProcessingErrorSeverity.WarningWithoutErrorReport.GetNonReportableEquivalent());
			AssertEquals(ReportProcessingErrorSeverity.InformationWithoutErrorReport, ReportProcessingErrorSeverity.InformationWithoutErrorReport.GetNonReportableEquivalent());
		}

		public void TestExceptionTypesToBeShownToUser()
		{
			AssertCollectionContains(typeof(ReportSQLTimeoutException), ReportProcessingErrorSeverityExtensions.ExceptionTypesToBeShownToUser);
			AssertCollectionContains(typeof(System.NotSupportedException), ReportProcessingErrorSeverityExtensions.ExceptionTypesToBeShownToUser);
			AssertCollectionContains(typeof(BitmapCreationException), ReportProcessingErrorSeverityExtensions.ExceptionTypesToBeShownToUser);
			AssertCollectionContains(typeof(DataProviderException), ReportProcessingErrorSeverityExtensions.ExceptionTypesToBeShownToUser);
			AssertCollectionContains(typeof(DocumentEngineException), ReportProcessingErrorSeverityExtensions.ExceptionTypesToBeShownToUser);
			AssertCollectionContains(typeof(ExcelInterfaceException), ReportProcessingErrorSeverityExtensions.ExceptionTypesToBeShownToUser);
			AssertCollectionContains(typeof(FieldNotFoundException), ReportProcessingErrorSeverityExtensions.ExceptionTypesToBeShownToUser);
			AssertCollectionContains(typeof(FormulaProviderException), ReportProcessingErrorSeverityExtensions.ExceptionTypesToBeShownToUser);
			AssertCollectionContains(typeof(FormulaProviderNotReadyException), ReportProcessingErrorSeverityExtensions.ExceptionTypesToBeShownToUser);
			AssertCollectionContains(typeof(InvalidGroupByColumnException), ReportProcessingErrorSeverityExtensions.ExceptionTypesToBeShownToUser);
			AssertCollectionContains(typeof(MaxConcurrentReportConnectionsExceeded), ReportProcessingErrorSeverityExtensions.ExceptionTypesToBeShownToUser);
			AssertCollectionContains(typeof(SQLExecutionException), ReportProcessingErrorSeverityExtensions.ExceptionTypesToBeShownToUser);
			AssertCollectionContains(typeof(TemplateDefinitionException), ReportProcessingErrorSeverityExtensions.ExceptionTypesToBeShownToUser);
			AssertCollectionContains(typeof(FlexCel.Core.FlexCelException), ReportProcessingErrorSeverityExtensions.ExceptionTypesToBeShownToUser);
			AssertCollectionContains(typeof(FlexCel.Core.FlexCelException), ReportProcessingErrorSeverityExtensions.ExceptionTypesToBeShownToUser);
			AssertCollectionContains(typeof(System.Data.SqlClient.SqlException), ReportProcessingErrorSeverityExtensions.ExceptionTypesToBeShownToUser);
		}
	}
}
