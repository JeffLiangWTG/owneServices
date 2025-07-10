using System;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW;

namespace Enterprise.ServiceManager.Host
{
	class HostServiceErrorReporter : ServiceErrorReporter
	{
		public HostServiceErrorReporter(IHostLogger hostLogger, IExceptionHandler exceptionHandler)
			: base(exceptionHandler)
		{
			this.hostLogger = hostLogger ?? throw new ArgumentNullException(nameof(hostLogger));
		}

		protected override ILogger CurrentLogger => hostLogger;
		readonly IHostLogger hostLogger;
	}
}
