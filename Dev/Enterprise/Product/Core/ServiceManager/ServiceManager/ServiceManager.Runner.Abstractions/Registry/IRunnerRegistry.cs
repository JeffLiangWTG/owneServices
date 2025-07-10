using System;
using System.Diagnostics;
using ServiceManager.Shared.Abstractions;

namespace ServiceManager.Runner.Abstractions;

public interface IRunnerRegistrySettings : ISharedRegistrySettings
{
	public bool EnableStackTraceInUserContextSwitcher { get; }
	public bool ServiceTaskRunnerConnectionPoolingEnabled { get; }
	public ProcessPriorityClass RunnerProcessPriorityValue { get; }
	public TimeSpan ServiceTaskUnloadTimeout { get; }
}
