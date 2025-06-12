using KafkaFlow.Retry;

namespace eServices.ApplicationEvent.EnrichmentService.Kafka;

public static class KafkaBuilder
{
	private static FaultRetryHandler? faultRetryHandler;

	public static IHostApplicationBuilder AddKafkaFlow(this IHostApplicationBuilder builder)
	{
		var kafkaOptions = new KafkaOptions();
		builder.Configuration.GetSection("Kafka").Bind(kafkaOptions);
		if (kafkaOptions.MessageEvents is null) throw new MissingMemberException(nameof(KafkaOptions), nameof(KafkaOptions.MessageEvents));
		if (kafkaOptions.ApplicationEvents is null) throw new MissingMemberException(nameof(KafkaOptions), nameof(KafkaOptions.ApplicationEvents));

		builder.Services.AddKafka(kafka => kafka
			.UseMicrosoftLog()
			.AddOpenTelemetryInstrumentation()
			.SubscribeGlobalEvents(observers => observers
				.MessageConsumeStarted.Subscribe(context =>
				{
					faultRetryHandler ??= context.MessageContext.DependencyResolver.Resolve<FaultRetryHandler>();
					return Task.CompletedTask;
				})
			)
			.AddCluster(cluster => cluster
				.WithBrokers(kafkaOptions.MessageEvents!.Brokers)
				.WithSecurityInformation(security =>
				{
					security.SecurityProtocol = kafkaOptions.MessageEvents.SecurityProtocol;
					security.SaslMechanism = kafkaOptions.MessageEvents.SaslMechanism;
					security.SaslUsername = kafkaOptions.MessageEvents.Username;
					security.SaslPassword = kafkaOptions.MessageEvents.Password;
				})
				.AddConsumer(consumer => consumer
					.WithName(nameof(KafkaOptions.MessageEvents))
					.Topic(kafkaOptions.MessageEvents.Topic)
					.WithGroupId(kafkaOptions.MessageEvents.GroupId)
					.WithBufferSize(kafkaOptions.MessageEvents.BufferSize)
					.WithWorkersCount(kafkaOptions.MessageEvents.WorkersCount)
					.AddMiddlewares(middlewares => middlewares
						.Add<MessageWorkflowHandler>()
						.RetrySimple(retrySimple => retrySimple
							.HandleAnyException()
							.TryTimes(5)
							.WithTimeBetweenTriesPlan(TimeSpan.FromSeconds(1))
						)
						.RetryForever(retryForever => retryForever
							.Handle(retryContext => faultRetryHandler?.Handle(retryContext) ?? false)
							.WithTimeBetweenTriesPlan(
								TimeSpan.FromSeconds(5),
								TimeSpan.FromSeconds(15),
								TimeSpan.FromSeconds(30),
								TimeSpan.FromSeconds(60))
						)
						.AddSingleTypeDeserializer<JsonObject, JsonObjectDeserializer>()
						.Add<MessageEnrichmentHandler>()
					)
				)
				.EnableAdminMessages(kafkaOptions.MessageEvents.AdminTopic)
				.EnableTelemetry(kafkaOptions.MessageEvents.AdminTopic)
			)
			.AddCluster(cluster => cluster
				.WithBrokers(kafkaOptions.ApplicationEvents!.Brokers)
				.WithSecurityInformation(security =>
				{
					security.SecurityProtocol = kafkaOptions.ApplicationEvents.SecurityProtocol;
					security.SaslMechanism = kafkaOptions.ApplicationEvents.SaslMechanism;
					security.SaslUsername = kafkaOptions.ApplicationEvents.Username;
					security.SaslPassword = kafkaOptions.ApplicationEvents.Password;
				})
				.AddProducer("ApplicationEvents", producer => producer
					.DefaultTopic(kafkaOptions.ApplicationEvents!.Topic)
					.AddMiddlewares(middlewares => middlewares
						.AddSingleTypeSerializer<JsonObject, JsonObjectSerializer>()
					)
				)
			));

		return builder;
	}
}
