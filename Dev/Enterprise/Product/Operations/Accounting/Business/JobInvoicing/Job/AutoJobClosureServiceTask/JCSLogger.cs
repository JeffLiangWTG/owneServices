using CargoWise.Common;
using Enterprise.Accounting.Business.JobInvoicing.AutoJobClosureServiceTask.Diagnostic;
using Enterprise.Integration;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface IJCSLogger
	{
		void LogInformation(string logText);

		void LogError(string logText);

		void LogDebug(string logText);

		void LogDiagnostic(string logText);

		DisposableAction SetLogPrefix(string prefix);
	}

	public class JCSLogger : IJCSLogger
	{
		public JCSLogger(ILogger serviceLogger)
		{
			ServiceLogger = serviceLogger;
		}
		ILogger ServiceLogger { get; }

		void IJCSLogger.LogInformation(string logText) => ServiceLogger.Log(LogType.Information, $"{logPrefix}{logText}");

		void IJCSLogger.LogError(string logText) => ServiceLogger.Log(LogType.Error, $"{logPrefix}{logText}");

		void IJCSLogger.LogDebug(string logText)
		{
			if (ServiceLogger is IDiagnosticLogger digLogger)
			{
				return;
			}

			ServiceLogger.Log(LogType.Debug, $"{logPrefix}{logText}");
		}

		void IJCSLogger.LogDiagnostic(string logText)
		{
			ServiceLogger.Log(LogType.Debug, $"{logPrefix}{logText}");
		}

		DisposableAction IJCSLogger.SetLogPrefix(string prefix) => new DisposableAction(() => logPrefix = prefix, () => logPrefix = string.Empty);

		string logPrefix = "";
	}
}
