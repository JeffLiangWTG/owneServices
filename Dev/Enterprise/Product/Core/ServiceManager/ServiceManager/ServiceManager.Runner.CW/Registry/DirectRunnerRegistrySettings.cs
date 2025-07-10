using System.Diagnostics;
using Enterprise.Registry.Business;
using ServiceManager.Runner.Abstractions;
using ServiceManager.Shared.CW;

namespace Enterprise.ServiceManager.Runner;

class DirectRunnerRegistrySettings : DirectSharedRegistrySettings, IRunnerRegistrySettings
{
	public bool EnableStackTraceInUserContextSwitcher => SystemDataRegistry.Instance.EnableStackTraceInUserContextSwitcher.Value;
	public bool ServiceTaskRunnerConnectionPoolingEnabled => SystemDataRegistry.Instance.ServiceTaskRunnerConnectionPoolingEnabled.Value;
	public ProcessPriorityClass RunnerProcessPriorityValue => SystemDataRegistry.Instance.RunnerProcessPriorityValue;
	public TimeSpan ServiceTaskUnloadTimeout => SystemDataRegistry.Instance.ServiceTaskUnloadTimeout;
}
