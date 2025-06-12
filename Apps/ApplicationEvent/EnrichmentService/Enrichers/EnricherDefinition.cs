namespace eServices.ApplicationEvent.EnrichmentService.Enrichers;

public class EnricherDefinition
{
	public required string Application { get; set; }
	public required Type Type { get; set; }
	public List<string> Includes { get; set; } = [];
	public List<Criterion> Criteria { get; set; } = [];
	public Dictionary<string, string[]> Lookups { get; set; } = [];

	public class Criterion
	{
		public string? Name { get; set; }
		public string[][] Parameters { get; set; } = [];
		public required Func<string?[], Dictionary<string, string[]>, bool> Expression { get; set; }
	}
}
