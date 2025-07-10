using System;

namespace Enterprise.ZArchitecture.Core
{
	public class ProcessedEventArgs : EventArgs
	{
		public ProcessedEventArgs(int percentageComplete)
		{
			PercentageComplete = percentageComplete;
			ProcessedCount = 1;
		}

		public ProcessedEventArgs(int percentageComplete, int processedCount, int failureCount, string logEntry)
		{
			PercentageComplete = percentageComplete;
			ProcessedCount = processedCount;
			FailureCount = failureCount;
			LogEntry = logEntry;
		}

		public int PercentageComplete;
		public int ProcessedCount;
		public int FailureCount;
		public string LogEntry;
	}

	public delegate void ProcessedEventHandler(object sender, ProcessedEventArgs e);

	public class ProcessFileEventArgs : EventArgs
	{
		public ProcessFileEventArgs(string unmappedFileName)
		{
			UnmappedFileName = unmappedFileName;
		}

		public string UnmappedFileName;
	}

	public delegate void ProcessFileEventHandler(object sender, ProcessFileEventArgs e);
}
