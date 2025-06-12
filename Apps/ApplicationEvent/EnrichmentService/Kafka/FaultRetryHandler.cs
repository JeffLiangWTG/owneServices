using System.Data;
using KafkaFlow.Retry;

namespace eServices.ApplicationEvent.EnrichmentService.Kafka;

public class FaultRetryHandler(IMetrics metrics, ILogger<FaultRetryHandler> logger)
{
	public bool Handle(RetryContext context)
	{
		var exception = (context.Exception is var e
			and (AggregateException or DataException { InnerException: not null }))
			? e.InnerException : context.Exception;

		switch (exception)
		{
			case HttpRequestException:
			case { Source: "Confluent.Kafka" }:
				metrics.AddFaults(1);
				logger.RetryingFault(context.Exception);
				return true;
			case OperationCanceledException:
				return false;
			default:
				logger.UnhandledError(context.Exception);
				return false;
		}
	}
}

static partial class FaultRetryHandlerLog
{
	[LoggerMessage(Level = LogLevel.Information, Message = "Handling retryable fault.")]
	public static partial void RetryingFault(this ILogger logger, Exception exception);

	[LoggerMessage(Level = LogLevel.Information, Message = "Unhandled error.")]
	public static partial void UnhandledError(this ILogger logger, Exception exception);
}
