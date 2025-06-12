using eServices.ApplicationEvent.EnrichmentService.Kafka;

namespace eServices.ApplicationEvent.Tests.Kafka;

public class JsonObjectSerializerTests
{
	[Test]
	public void JsonObjectSerializer_Serialize()
	{
		using var stream = new MemoryStream();
		var message = JsonObject.Parse(Resources.Messages.ApplicationEventAir)!;
		var serializer = new JsonObjectSerializer();

		serializer.SerializeAsync(message, stream, null!).Wait();
		stream.Position = 0;

		Assert.That(JsonObject.DeepEquals(JsonObject.Parse(stream), message), () => "Serialized message does not match expected content.");
	}
}