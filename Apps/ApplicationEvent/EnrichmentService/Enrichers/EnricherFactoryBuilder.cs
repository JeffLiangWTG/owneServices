using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace eServices.ApplicationEvent.EnrichmentService.Enrichers;

public static class EnricherFactoryBuilder
{
	public static IHostApplicationBuilder AddEnricherFactory(this IHostApplicationBuilder builder)
	{
		var enricherFactoryOptions = new EnricherFactoryOptions();
		builder.Configuration.GetSection("Enrichers").Bind(enricherFactoryOptions);

		Dictionary<string, EnricherDefinition> enrichers = ParseEnricherDefinitions(enricherFactoryOptions);

		builder.Services.AddSingleton(enrichers!);
		builder.Services.AddSingleton<IEnricherFactory, EnricherFactory>();

		foreach (var app in enrichers)
		{
			if (!app.Value.Includes.All(i => enrichers[i].Type.GetInterface(typeof(IEnricherSingle).FullName!) is not null))
			{
				throw new InvalidOperationException($"Enricher '{app.Key}' includes an enricher that does not implement {nameof(IEnricherSingle)}.");
			}
			builder.Services.AddScoped(app.Value.Type);
		}

		return builder;
	}

	public static Dictionary<string, EnricherDefinition> ParseEnricherDefinitions(EnricherFactoryOptions enricherFactoryOptions)
	{
		return enricherFactoryOptions.ToDictionary(e => e.Key, e =>
			new EnricherDefinition
			{
				Application = e.Key,
				Type = Type.GetType(e.Value.Type, throwOnError: true)!,
				Includes = e.Value.Includes,
				Criteria = e.Value.Criteria.Select((c, p) =>
					new EnricherDefinition.Criterion
					{
						Name = c.Name,
						Parameters = c.Parameters.Select(p => p.Split('.').ToArray()).ToArray(),
						Expression = CSharpScript.EvaluateAsync<Func<string?[], Dictionary<string, string[]>, bool>>("(string[] p, Dictionary<string, string[]> l) => " + c.Expression,
							ScriptOptions.Default.WithImports("System", "System.Text.RegularExpressions", "System.Collections.Generic")).Result
					}).ToList(),
				Lookups = e.Value.Lookups.ToDictionary(l => l.Key, l => l.Value.Order().ToArray())
			}
		);
	}
}
