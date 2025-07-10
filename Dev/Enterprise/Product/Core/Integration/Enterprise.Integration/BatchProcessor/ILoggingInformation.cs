namespace Enterprise.Integration.BatchProcessor
{
	public interface ILoggingInformation
	{
		void Log(string logMsg);
		void ClearLogs();
	}
}
