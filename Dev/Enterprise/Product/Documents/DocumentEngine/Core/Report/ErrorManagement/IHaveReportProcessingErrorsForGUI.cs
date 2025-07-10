namespace Enterprise.DocumentEngine.ReportErrorManagement
{
	public interface IHaveReportProcessingErrorsForGUI
	{
		IReportProcessingError[] GetErrors();
	}
}
