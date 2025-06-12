namespace eServices.ApplicationEvent.EnrichmentService.Enrichers;

public interface IEnricherSingle
{
	Task<JsonObject> EnrichMessageSingleAsync(JsonObject messageEvent, string criteria, CancellationToken cancellationToken = default);
}