namespace CargoWise.eHub.MessageEvent.KafkaProducerService.Options
{
	public class ServiceBrokerOptions
	{
		public const string Section = "ServiceBroker";

		public int BatchSize { get; set; } = 1;
		public int TimeoutSecond { get; set; } = 60 * 10;
		public int RetryIntervalSeconds { get; set; } = 5;
		public string? DbPassword { get; set; }
	}
}
