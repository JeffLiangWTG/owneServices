namespace Enterprise.DocumentEngine.ReportErrorManagement
{
	class ReportFilterValidationError : ReportProcessingError
	{
		internal ReportFilterValidationError(string message, ReportProcessingErrorSeverity severity)
			: base(message, severity)
		{
		}
	}
}
