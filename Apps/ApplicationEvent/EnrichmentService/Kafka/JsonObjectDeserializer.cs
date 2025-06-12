namespace eServices.ApplicationEvent.EnrichmentService.Kafka;

public class JsonObjectDeserializer : IDeserializer
{
	public async Task<object> DeserializeAsync(Stream input, Type type, ISerializerContext context)
		=> (await JsonObject.ParseAsync(input))?.AsObject()!;
}
