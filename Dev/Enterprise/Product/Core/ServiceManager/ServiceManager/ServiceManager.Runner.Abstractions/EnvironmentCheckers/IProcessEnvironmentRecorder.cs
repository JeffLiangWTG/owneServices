using System;

namespace ServiceManager.Runner.Abstractions
{
	public interface IProcessEnvironmentRecorder
	{
		IDisposable MonitorProcess(Microsoft.Extensions.Logging.ILogger logger, IRunCommandInfo runCommandInfo);
	}
}
