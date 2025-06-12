namespace CargoWise.eHub.MessageEvent.KafkaProducerService.Options
{
	public class KafkaOptions
	{
		public const string Section = "Kafka";

		public bool EnableIdempotence { get; set; } = false;
		public string? BootstrapServers { get; set; }
		public string? SaslUsername { get; set; }
		public string? SaslPassword { get; set; }
		public string? Topic { get; set; }
		public int TransactionTimeoutSecond { get; set; } = 60;
		public string? TransactionalId { get; set; }
	}
}
