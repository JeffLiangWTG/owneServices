using Enterprise.ServiceManager.Shared;
using ServiceManager.Common.Abstractions;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Runner.Abstractions;

namespace Enterprise.ServiceManager.Runner.Exceptions
{
	class RunnerExceptionHandler : ExceptionHandler
	{
		protected override ExceptionAction EvaluateExceptionAction(Exception ex)
		{
			switch (ex)
			{
				case RunnerInternalException _:
				case GrpcInitializationException _:
					return ExceptionAction.LogError;

				case HostedServiceException hsException:
					return hsException.LogException
						? ExceptionAction.LogError
						: ExceptionAction.Ignore;

				default:
					return ExceptionAction.Report;
			}
		}
	}
}
