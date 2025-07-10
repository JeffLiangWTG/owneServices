using System.Threading;

namespace Enterprise.Integration.BatchProcessor
{
	public interface IBatchDirector
	{
		void SetInformationLogger(ILoggingInformation logger);
		bool ValidateEnvironment();
		void InitialiseBatchProcesses();
		void DoMainProcessingLoop(CancellationToken token);
		int TimerIntervalInMilliseconds { get; }
		string BatchProcessorName { get; set; }
	}
}
