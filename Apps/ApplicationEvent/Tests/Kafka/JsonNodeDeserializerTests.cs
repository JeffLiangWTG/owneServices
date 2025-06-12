using System.Text;
using eServices.ApplicationEvent.EnrichmentService.Kafka;

namespace eServices.ApplicationEvent.Tests.Kafka;

public class JsonObjectDeserializerTests
{
	[Test]
	public void JsonObjectDeserializer_Deserialize()
	{
		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Resources.Messages.MessageEventDeliveredToBT));
		var deserializer = new JsonObjectDeserializer();

		var result = deserializer.DeserializeAsync(stream, typeof(JsonObject), null!).Result as JsonObject;

		Assert.That(JsonObject.DeepEquals(result, JsonObject.Parse(Resources.Messages.MessageEventDeliveredToBT)), () => "Deserialized message does not match expected content.");
	}
}