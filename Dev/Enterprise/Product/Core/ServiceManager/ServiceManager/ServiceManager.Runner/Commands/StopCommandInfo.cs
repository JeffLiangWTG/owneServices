using System;
using ServiceManager.Runner.Abstractions;

namespace Enterprise.ServiceManager.Runner
{
	[Serializable]
	public class StopCommandInfo : IStopCommandInfo
	{
		public StopCommandInfo()
		{
			Id = Guid.NewGuid();
		}

		public Guid Id { get; }

		public string FormatRequestToLogMessage(RunnerLogMessageStage logMessageStage, params object[] values)
		{
			switch (logMessageStage)
			{
				case RunnerLogMessageStage.ReceivedCommand:
					return $"[{Id}] Received {this}.";
				case RunnerLogMessageStage.PreparingExecution:
					return $"[{Id}] Preparing for Stopping Runner.";
				default:
					throw new ArgumentOutOfRangeException(nameof(logMessageStage));
			}
		}

		public override string ToString()
		{
			return "command [Stop]";
		}
	}
}
