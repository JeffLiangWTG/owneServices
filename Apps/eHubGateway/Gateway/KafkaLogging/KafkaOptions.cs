using Confluent.Kafka;

namespace CargoWise.eHub.Gateway
{
	public class KafkaOptions
	{
		public string Brokers { get; set; }

		public string Topic { get; set; }

		public Acks Acks { get; set; }

		public string SaslUsername { get; set; }

		public SecurityProtocol SecurityProtocol { get; set; }

		public SaslMechanism SaslMechanism { get; set; }
	}
}
