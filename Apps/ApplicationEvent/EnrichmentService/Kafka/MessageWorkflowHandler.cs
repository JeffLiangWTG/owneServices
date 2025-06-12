namespace eServices.ApplicationEvent.EnrichmentService.Kafka;

public class MessageWorkflowHandler(
	IErrorReporting errorReporting,
	IMetrics metrics,
	IHostApplicationLifetime hostApplicationLifetime) : IMessageMiddleware
{
	public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
	{
		try
		{
			metrics.AddMsgsRcvd(1);
			await next(context);
		}
		catch (Exception ex) when (
			hostApplicationLifetime.ApplicationStopping.IsCancellationRequested is false
			|| ex is not (OperationCanceledException or AggregateException { InnerException: OperationCanceledException }))
		{
			metrics.AddMsgsFail(1);
			await errorReporting.ReportErrorAsync(ex);
			throw;
		}
	}
}
