namespace eServices.ApplicationEvent.EnrichmentService.Kafka;

public class KafkaOptions
{
	public ConsumerOptions? MessageEvents { get; set; }
	public ProducerOptions? ApplicationEvents { get; set; }

	public class ConsumerOptions : Options
	{
		public required string GroupId { get; set; }
		public required int BufferSize { get; set; }
		public required int WorkersCount { get; set; }
		public required int RetryIntervalSeconds { get; set; }
		public required string AdminTopic { get; set; }
	}

	public class ProducerOptions : Options
	{
	}

	public class Options
	{
		public required List<string> Brokers { get; set; }
		public required string Topic { get; set; }
		public SecurityProtocol? SecurityProtocol { get; set; }
		public SaslMechanism? SaslMechanism { get; set; }
		public string? Username { get; set; }
		public string? Password { get; set; }
	}
}
