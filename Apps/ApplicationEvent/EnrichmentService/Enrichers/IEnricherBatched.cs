namespace eServices.ApplicationEvent.EnrichmentService.Enrichers;

public interface IEnricherBatched
{
	Task<IEnumerable<JsonObject>> EnrichMessageBatchAsync(JsonObject messageEvent, string criteria, CancellationToken cancellationToken = default);
}