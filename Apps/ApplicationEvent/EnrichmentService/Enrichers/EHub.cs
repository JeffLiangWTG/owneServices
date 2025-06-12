namespace eServices.ApplicationEvent.EnrichmentService.Enrichers;

public class EHub : IEnricherSingle
{
	public Task<JsonObject> EnrichMessageSingleAsync(JsonObject messageEvent, string criteria, CancellationToken cancellationToken = default)
	{
		var enrichedData = new JsonObject();

		if ((messageEvent["MessageEvent"]?["eHub"]?["Inbox"]?["PK"]
			?? messageEvent["MessageEvent"]?["eHub"]?["Outbox"]?["InboxPK"])
				is var pkNode and not null)
		{
			enrichedData["InboxPK"] = pkNode.ToString();
		}

		return Task.FromResult(enrichedData);
	}
}
