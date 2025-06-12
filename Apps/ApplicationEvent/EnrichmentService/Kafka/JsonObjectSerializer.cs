namespace eServices.ApplicationEvent.EnrichmentService.Kafka;

public class JsonObjectSerializer : ISerializer
{
	public async Task SerializeAsync(object message, Stream output, ISerializerContext context)
	{
		if (message is JsonObject json)
		{
			using var writer = new Utf8JsonWriter(output);
			await Task.Run(() => json.WriteTo(writer));
		}
	}
}