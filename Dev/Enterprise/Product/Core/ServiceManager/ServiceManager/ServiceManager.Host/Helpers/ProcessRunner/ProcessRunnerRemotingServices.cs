using System;
using Enterprise.ServiceManager.Shared;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	public class ProcessRunnerRemotingServices : IProcessRunnerRemotingServices
	{
		public ProcessRunnerRemotingServices(IHostLogger hostLogger, IErrorReporterProxy errorReporterProxy)
		{
			this.hostLogger	= hostLogger ?? throw new ArgumentNullException(nameof(hostLogger));
			this.errorReporterProxy = errorReporterProxy ?? throw new ArgumentNullException(nameof(errorReporterProxy));
		}

		public IRunnerCommandQueueProvider CreateRunnerCommandQueueProxy(int? grpcPort)
		{
			return new RunnerCommandQueueProvider(hostLogger, grpcPort ?? throw new ArgumentNullException($"{nameof(grpcPort)}: Grpc Server not initialized"), errorReporterProxy);
		}

		readonly IHostLogger hostLogger;
		readonly IErrorReporterProxy errorReporterProxy;
	}
}
