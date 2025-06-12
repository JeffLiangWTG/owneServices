namespace eServices.ApplicationEvent.EnrichmentService.Enrichers;

public interface IEnricherFactory
{
	(IDictionary<string, EnricherBatched> Batched, IDictionary<string, EnricherSingle> Single) GetEnrichers(JsonObject messageEvent, IServiceScope serviceScope);
}

public class EnricherFactory : IEnricherFactory
{
	private readonly Dictionary<string, EnricherDefinition> enrichers;
	private readonly IMetrics metrics;

	public EnricherFactory(
		Dictionary<string, EnricherDefinition> enrichers,
		IMetrics metrics,
		ILogger<EnricherFactory> logger)
	{
		this.enrichers = enrichers;
		this.metrics = metrics;

		logger.ConfiguredEnrichers(string.Join(',', enrichers.Keys));
	}

	public (IDictionary<string, EnricherBatched> Batched, IDictionary<string, EnricherSingle> Single)
		GetEnrichers(JsonObject messageEvent, IServiceScope serviceScope)
	{
		var matchedBatchEnrichers = new Dictionary<string, EnricherBatched>();
		var matchedSingleEnrichers = new Dictionary<string, EnricherSingle>();
		var includes = new List<string>();
		using var activity = metrics.MsgActivitySource?.StartActivity(Metrics.Names.Activity_EnricherFactoryGet);

		foreach (var enricher in enrichers)
		{
			if (enricher.Value.Criteria.Select((c, p) => (c, p)).FirstOrDefault(s =>
			{
				var parameters = s.c.Parameters.Select(p =>
				{
					JsonNode node = messageEvent;
					for (int i = 0; node != null && i < p.Length; i++) { node = node?[p[i]]!; }
					return node?.GetValue<string>() ?? string.Empty;
				}).ToArray();
				var result = s.c.Expression(parameters, enricher.Value.Lookups);
				activity?.AddEvent(new ActivityEvent(Metrics.Names.Activity_EnricherFactoryCriterion,
					tags: new()
					{
						["Enricher"] = enricher.Key,
						["CriterionId"] = s.p,
						["Parameters"] = parameters.ToArray(),
						["Result"] = result
					}));
				return result;
			}) is var criteria and { c: not null })
			{
				activity?.AddEvent(new ActivityEvent(Metrics.Names.Activity_EnricherFactoryMatched,
					tags: new() { ["Enricher"] = enricher.Key }));
				var enricherService = serviceScope.ServiceProvider.GetRequiredService(enricher.Value.Type);
				switch (enricherService)
				{
					case IEnricherBatched batched:
						matchedBatchEnrichers.Add(enricher.Key, new(batched, criteria.c.Name ?? criteria.p.ToString()));
						break;
					case IEnricherSingle single:
						matchedSingleEnrichers.Add(enricher.Key, new(single, criteria.c.Name ?? criteria.p.ToString()));
						break;
					default:
						throw new InvalidOperationException($"Enricher '{enricher.Key}' of type {enricher.GetType()} does not implement {nameof(IEnricherBatched)} or {nameof(IEnricherSingle)}.");
				}
				includes.AddRange(enricher.Value.Includes);
			}
		}

		foreach (var include in includes.Distinct().Except(matchedSingleEnrichers.Keys))
		{
			matchedSingleEnrichers.Add(include, new((IEnricherSingle)serviceScope.ServiceProvider.GetRequiredService(enrichers[include].Type), "Include"));
		}
		activity?.AddTag("MatchedEnrichersCount", matchedBatchEnrichers.Count + matchedSingleEnrichers.Count);
		return (matchedBatchEnrichers, matchedSingleEnrichers);
	}
}

static partial class EncricherFactoryLog
{
	[LoggerMessage(
		EventId = 51002,
		Level = LogLevel.Information,
		Message = "Configured enrichers: [{Enrichers}]")]
	public static partial void ConfiguredEnrichers(this ILogger logger, string enrichers);
}

public record struct EnricherSingle(IEnricherSingle Enricher, string Criteria);

public record struct EnricherBatched(IEnricherBatched Enricher, string Criteria);