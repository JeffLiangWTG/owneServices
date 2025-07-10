using System;

namespace Enterprise.ArchiveManager.Integration
{
	public interface IArchiveLogger
	{
		void LogInfo(string code, string message);

		void LogInfo(string message);

		void LogWarning(string code, string message);

		void LogWarning(string message);

		void LogError(string code, string message);

		void LogError(string message);

		void LogAndReportError(string key, string systemCode, string message, Exception exception);
	}
}
