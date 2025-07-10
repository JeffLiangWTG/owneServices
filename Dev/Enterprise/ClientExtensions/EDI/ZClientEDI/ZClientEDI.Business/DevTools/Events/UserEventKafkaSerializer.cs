using System.IO;
using Confluent.Kafka;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Enterprise.Client.EDI.DevTools.Events
{
	public class UserEventKafkaSerializer : ISerializer<UserEvent>
	{
		public byte[] Serialize(UserEvent data, SerializationContext context)
		{
			var serializer = new JsonSerializer();
			serializer.ContractResolver = new CamelCasePropertyNamesContractResolver();

			using var ms = new MemoryStream();
			using var textWriter = new StreamWriter(ms);
			using var jsonWriter = new JsonTextWriter(textWriter);

			serializer.Serialize(jsonWriter, data);

			jsonWriter.Flush();
			textWriter.Flush();

			return ms.ToArray();
		}
	}
}
