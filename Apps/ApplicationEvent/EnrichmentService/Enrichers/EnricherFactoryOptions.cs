using static eServices.ApplicationEvent.EnrichmentService.Enrichers.EnricherFactoryOptions;

namespace eServices.ApplicationEvent.EnrichmentService.Enrichers;

public class EnricherFactoryOptions : Dictionary<string, Enricher>
{
	public class Enricher
	{
		public required string Type { get; set; }
		public List<string> Includes { get; set; } = [];
		public List<Criterion> Criteria { get; set; } = [];
		public Dictionary<string, List<string>> Lookups { get; set; } = [];

		public class Criterion
		{
			public string? Name { get; set; }
			public List<string> Parameters { get; set; } = [];
			public required string Expression { get; set; }
		}
	}
}
