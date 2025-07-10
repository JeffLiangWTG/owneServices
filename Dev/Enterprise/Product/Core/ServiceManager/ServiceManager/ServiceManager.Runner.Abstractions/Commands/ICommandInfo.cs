using System;

namespace ServiceManager.Runner.Abstractions
{
	public interface ICommandInfo
	{
		Guid Id { get; }
		string FormatRequestToLogMessage(RunnerLogMessageStage logMessageStage, params object[] values);
	}
}
