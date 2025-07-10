using Enterprise.ServiceManager.Runner.Exceptions;
using Microsoft.Extensions.Logging;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Runner.Abstractions;
using ServiceManager.Shared.CW;

namespace Enterprise.ServiceManager.Runner
{
	class GrpcRunnerErrorReporter : ServiceErrorReporter
	{
		public GrpcRunnerErrorReporter(IRunnerLogger runnerLogger, IServiceTaskLogger serviceTaskLogger)
			: base(new RunnerExceptionHandler())
		{
			this.runnerLogger = runnerLogger ?? throw new ArgumentNullException(nameof(runnerLogger));
			this.serviceTaskLogger = serviceTaskLogger ?? throw new ArgumentNullException(nameof(serviceTaskLogger));
		}

		protected override ILogger CurrentLogger => serviceTaskLogger.TaskLoggerIsActive ? serviceTaskLogger : runnerLogger;

		readonly IRunnerLogger runnerLogger;
		readonly IServiceTaskLogger serviceTaskLogger;
	}
}
