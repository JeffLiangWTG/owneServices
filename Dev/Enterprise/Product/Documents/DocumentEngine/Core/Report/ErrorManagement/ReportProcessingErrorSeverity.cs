namespace Enterprise.DocumentEngine.ReportErrorManagement
{
	using System;
	using System.Collections.Generic;
	using CargoWise.ComponentModel;
	using Enterprise.DocumentEngine.Exceptions;
	using Enterprise.ZArchitecture.Core;

	public enum ReportProcessingErrorSeverity
	{
		Fatal, FatalWithoutErrorReport,
		Error, ErrorWithoutErrorReport,
		Warning, WarningWithoutErrorReport,
		Information, InformationWithoutErrorReport,
	}

	public static class ReportProcessingErrorSeverityExtensions
	{
		public static INotificationType ToNotificationType(this ReportProcessingErrorSeverity severity)
		{
			switch (severity)
			{
				case ReportProcessingErrorSeverity.Fatal:
				case ReportProcessingErrorSeverity.FatalWithoutErrorReport: return NotificationType.Error;
				case ReportProcessingErrorSeverity.Error:
				case ReportProcessingErrorSeverity.ErrorWithoutErrorReport: return NotificationType.Warning;
				case ReportProcessingErrorSeverity.Warning:
				case ReportProcessingErrorSeverity.WarningWithoutErrorReport: return NotificationType.Warning;
				default: return NotificationType.Information;
			}
		}

		public static string ToStringFormatted(this ReportProcessingErrorSeverity severity)
		{
			switch (severity)
			{
				case ReportProcessingErrorSeverity.FatalWithoutErrorReport: return (NoResString)"Fatal Error (without error report)";
				case ReportProcessingErrorSeverity.Fatal: return (NoResString)"Fatal Error";
				case ReportProcessingErrorSeverity.WarningWithoutErrorReport: return (NoResString)"Warning (without error report)";
				case ReportProcessingErrorSeverity.ErrorWithoutErrorReport: return (NoResString)"Error (without error report)";
				case ReportProcessingErrorSeverity.InformationWithoutErrorReport: return (NoResString)"Information (without error report)";
				default: return severity.ToString();
			}
		}

		public static ReportProcessingErrorSeverity GetNonReportableEquivalent(this ReportProcessingErrorSeverity severity)
		{
			switch (severity)
			{
				case ReportProcessingErrorSeverity.Fatal: return ReportProcessingErrorSeverity.FatalWithoutErrorReport;
				case ReportProcessingErrorSeverity.Error: return ReportProcessingErrorSeverity.ErrorWithoutErrorReport;
				case ReportProcessingErrorSeverity.Warning: return ReportProcessingErrorSeverity.WarningWithoutErrorReport;
				case ReportProcessingErrorSeverity.Information: return ReportProcessingErrorSeverity.InformationWithoutErrorReport;
				default: return severity;
			}
		}

		public static readonly List<Type> ExceptionTypesToBeShownToUser = new List<Type>
		{
			typeof(ReportSQLTimeoutException),
			typeof(NotSupportedException),
			typeof(BitmapCreationException),
			typeof(DataProviderException),
			typeof(DocumentEngineException),
			typeof(ExcelInterfaceException),
			typeof(FieldNotFoundException),
			typeof(FormulaProviderException),
			typeof(FormulaProviderNotReadyException),
			typeof(InvalidGroupByColumnException),
			typeof(MaxConcurrentReportConnectionsExceeded),
			typeof(SQLExecutionException),
			typeof(TemplateDefinitionException),
			typeof(FlexCel.Core.FlexCelException),
			typeof(System.Data.SqlClient.SqlException)
		};
	}
}
