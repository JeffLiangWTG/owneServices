using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Runner.Abstractions
{
	public interface IServiceTaskHandler
	{
		IHostedServiceAttribute HostedServiceAttribute { get; }
		void Run(CancellationToken cancellationToken);
		void InitializeRunningEnvironment(IHostedServiceAttribute hostedServiceAttribute, string taskConfigString, ILogger logger);
		void HandleException(Exception exception, string serviceTaskCode);
	}
}
