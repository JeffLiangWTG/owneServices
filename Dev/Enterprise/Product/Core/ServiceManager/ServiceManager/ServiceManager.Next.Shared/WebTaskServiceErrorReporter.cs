using Microsoft.Extensions.Logging;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW;

namespace CargoWise.ServiceManager.Next.Shared;

public class WebTaskServiceErrorReporter(ILogger<WebTaskServiceErrorReporter> logger, IExceptionHandler exceptionHandler)
	: ServiceErrorReporter(exceptionHandler)
{
	protected override ILogger CurrentLogger => logger;
}
