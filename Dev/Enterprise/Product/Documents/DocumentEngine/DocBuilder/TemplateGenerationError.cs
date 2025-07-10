using Enterprise.DocumentEngine.ReportErrorManagement;

namespace Enterprise.DocumentEngine.DocBuilder
{
	internal class TemplateGenerationError
	{
		internal TemplateGenerationError(string message, ReportProcessingErrorSeverity severity)
		{
			this.message = message;
			this.severity = severity;
		}

		readonly string message;
		readonly ReportProcessingErrorSeverity severity;

		internal ReportProcessingError ToReportProcessingError()
		{
			ReportProcessingError result = new ReportProcessingError(message, severity);
			return result;
		}

		internal string Message
		{
			get { return message; }
		}

		internal ReportProcessingErrorSeverity Severity
		{
			get { return severity; }
		}
	}
}
