using KafkaFlow.Producers;

namespace eServices.ApplicationEvent.EnrichmentService.Kafka;

public class MessageEnrichmentHandler(
	IServiceProvider serviceProvider,
	IEnricherFactory enricherFactory,
	IProducerAccessor producerAccessor,
	IHostApplicationLifetime hostApplicationLifetime,
	IMetrics metrics,
	ILogger<MessageEnrichmentHandler> logger) : IMessageMiddleware
{
	private readonly CancellationToken cancellationToken = hostApplicationLifetime.ApplicationStopping;

	public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
	{
		using var activity = metrics.MsgActivitySource?.StartActivity(Metrics.Names.Activity_MsgsHandle);
		activity?.AddTag("ConsumerOffset", new { context.ConsumerContext.Topic, context.ConsumerContext.Partition, context.ConsumerContext.Offset });

		try
		{
			logger.ReceivedMessage(context.Message.Value);

			if (context.Message.Value is not JsonObject message)
			{
				metrics.AddMsgsSkip(1);
				return;
			}

			var trackingId = message["MessageEvent"]?["Message"]?["TrackingID"]?.GetValue<string>();
			activity?.AddTag("TrackingId", trackingId);
			using var _ = logger.BeginScope(new[] { new KeyValuePair<string, object?>("TrackingId", trackingId) });

			logger.ReceivedMessage(message);

			cancellationToken.ThrowIfCancellationRequested();
			using var serviceScope = serviceProvider.CreateScope();
			var enrichers = enricherFactory.GetEnrichers(message, serviceScope);

			if (enrichers.Batched.Count == 0 && enrichers.Single.Count == 0)
			{
				metrics.AddMsgsSkip(1);
				return;
			}

			activity?.AddTag("Enrichers", enrichers.Batched.Keys.Concat(enrichers.Single.Keys).ToArray());
			activity?.AddEvent(new ActivityEvent(Metrics.Names.Activity_MsgsEnrichersSelected));

			var batchedEnricherTasks = enrichers.Batched.Select(async e =>
				(e.Key, Value: await e.Value.Enricher.EnrichMessageBatchAsync(message, e.Value.Criteria, cancellationToken))).ToList();
			var batchedEnrichments = (await Task.WhenAll(batchedEnricherTasks))
				.SelectMany(s => s.Value.Select(b => (s.Key, Value: b))).ToList();

			var singleEnricherTasks = enrichers.Single.Select(async e =>
				(e.Key, Value: await e.Value.Enricher.EnrichMessageSingleAsync(message, e.Value.Criteria, cancellationToken))).ToList();
			var singleEnrichments = (await Task.WhenAll(singleEnricherTasks))
				.Where(s => s.Value is not null && s.Value.Count > 0).ToList();

			activity?.AddEvent(new ActivityEvent(Metrics.Names.Activity_MsgsEnrichmentFinished));

			if (batchedEnrichments.Count == 0 && singleEnrichments.Count == 0)
			{
				metrics.AddMsgsSkip(1);
				return;
			}

			var applicationEvent = new JsonObject
			{
				["@timestamp"] = message["@timestamp"]?.DeepClone(),
				["ApplicationEvent"] = new JsonObject
				{
					["Event"] = message["MessageEvent"]?["Event"]?.DeepClone(),
					["Message"] = message["MessageEvent"]?["Message"]?.DeepClone()
				}
			};

			foreach (var (Key, Value) in singleEnrichments)
			{
				applicationEvent["ApplicationEvent"]![Key] = Value;
			}

			activity?.AddEvent(new ActivityEvent(Metrics.Names.Activity_MsgsAppEventSending));

			var producer = producerAccessor.GetProducer("ApplicationEvents");

			if (batchedEnrichments.Count == 0)
			{
				logger.SendingMessage(applicationEvent);

				var deliveryResult = await producer.ProduceAsync(trackingId, applicationEvent);

				activity?.AddEvent(new ActivityEvent(Metrics.Names.Activity_MsgsAppEventSent,
					tags: new() { ["ProducerOffset"] = new { deliveryResult.Topic, deliveryResult.Partition.Value, deliveryResult.Offset } }));
				metrics.AddMsgsSent(1, [.. enrichers.Single.Keys]);
			}
			else
			{
				foreach (var (Pos, Key, Value) in batchedEnrichments.Select((e, p) => (Pos: p, e.Key, e.Value)))
				{
					var batchedEvent = applicationEvent.DeepClone();
					batchedEvent["ApplicationEvent"]![Key] = Value;

					logger.SendingMessage(batchedEvent);

					var deliveryResult = await producer.ProduceAsync($"{trackingId}:{Pos}", batchedEvent);

					activity?.AddEvent(new ActivityEvent(Metrics.Names.Activity_MsgsAppEventSent,
						tags: new() { ["ProducerOffset"] = new { deliveryResult.Topic, deliveryResult.Partition.Value, deliveryResult.Offset } }));
					metrics.AddMsgsSent(1, [Key, .. enrichers.Single.Keys]);
				}
			}
		}
		catch (Exception ex)
		{
			activity?.AddException(ex);
			throw;
		}
	}
}

static partial class MessageEventHandlerLog
{
	[LoggerMessage(Level = LogLevel.Trace, Message = "Received message:\n{Message}")]
	public static partial void ReceivedMessage(this ILogger logger, object message);

	[LoggerMessage(Level = LogLevel.Trace, Message = "Sending message:\n{Message}")]
	public static partial void SendingMessage(this ILogger logger, object message);
}
