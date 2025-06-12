using System.Text;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace CargoWise.eHub.Gateway
{
	public class KafkaEventSerializer : ISerializer<LogEvent>
	{
		public byte[] Serialize(LogEvent data, SerializationContext context)
		{
			if (data == null)
			{
				return new byte[] {};
			}
			var json = JsonConvert.SerializeObject(data);
			return Encoding.UTF8.GetBytes(json);
		}
	}
}
