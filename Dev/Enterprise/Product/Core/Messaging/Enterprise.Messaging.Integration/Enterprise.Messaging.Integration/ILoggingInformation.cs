namespace Enterprise.Messaging.Integration
{
	public interface ILoggingInformation
	{
		void ClearLogs();
		void DebugLog(string logMsg);
		void Log(string logMsg);
		void LogError(string logMsg);
		void LogWarning(string logMsg);
	}
}
