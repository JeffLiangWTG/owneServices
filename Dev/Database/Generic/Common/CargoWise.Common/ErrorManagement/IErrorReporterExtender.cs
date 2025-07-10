namespace CargoWise.Common.ErrorManagement
{
	public interface IErrorReporterExtender
	{
		bool ShouldReportAlwaysInReportOnce { get; }
	}
}
