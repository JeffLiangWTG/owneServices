using ServiceManager.Logging.CW;
using ServiceManager.Runner.Abstractions;

namespace Enterprise.ServiceManager.Runner
{
	class ProcessEnvironmentRecorderProvider : IProcessEnvironmentRecorder
	{
		public IDisposable MonitorProcess(Microsoft.Extensions.Logging.ILogger logger, IRunCommandInfo runCommandInfo)
		{
			return new ProcessEnvironmentRecorder(logger.ToCW1Logger()).StartRun(runCommandInfo.Code);
		}
	}
}
