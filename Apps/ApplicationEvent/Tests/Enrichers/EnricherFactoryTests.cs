using eServices.ApplicationEvent.EnrichmentService.Instrumentation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;

namespace eServices.ApplicationEvent.Tests.Enrichers;

public class EnricherFactoryTests
{
	[Test]
	public void EnricherFactory_GetEnrichers()
	{
		var enricherFactoryOptions = new EnricherFactoryOptions
		{
			["Air"] = new EnricherFactoryOptions.Enricher
			{
				Type = "eServices.ApplicationEvent.EnrichmentService.Enrichers.AirMessaging",
				Criteria = [
					new EnricherFactoryOptions.Enricher.Criterion
					{
						Name = "RegexMatch",
						Parameters = ["MessageEvent.Event.Type", "MessageEvent.Message.RecipientID"],
						Expression = "(p[0] is \"Delivered\" or \"DeliveryFailed\") && Regex.IsMatch(p[1], \"^(ARINC_SP|BT|BT_WithUnsupported)$\", RegexOptions.Compiled)"
					},
					new EnricherFactoryOptions.Enricher.Criterion
					{
						Name = "LookupStartsWith",
						Parameters = ["MessageEvent.Event.Type", "MessageEvent.Message.RecipientID"],
						Expression = "(p[0] is \"Delivered\" or \"DeliveryFailed\") && (Array.Exists(l[\"Providers1\"], i => p[1].StartsWith(i)))"
					},
					new EnricherFactoryOptions.Enricher.Criterion
					{
						Name = "LookupEquals",
						Parameters = ["MessageEvent.Event.Type", "MessageEvent.Message.RecipientID"],
						Expression = "(p[0] is \"Delivered\" or \"DeliveryFailed\") && (Array.Exists(l[\"Providers2\"], i => p[1] == i))"
					}
				],
				Lookups = {
					["Providers1"] = ["ARINC_SP", "BT", "BT_WithUnsupported", "CCSJ"],
					["Providers2"] = ["ARINC_SP", "BT", "BT_WithUnsupported", "Descartes"]
				}
			}
		};
		var enricherDefinitions = EnricherFactoryBuilder.ParseEnricherDefinitions(enricherFactoryOptions);
		var services = new ServiceCollection();
		services.AddTransient(enricherDefinitions["Air"].Type);
		services.AddTransient(_ => new Mock<IConfiguration>().Object);
		var serviceProvider = services.BuildServiceProvider();
		var serviceScope = serviceProvider.CreateScope();
		var metrics = new Mock<IMetrics>();
		var logger = new FakeLogger<EnricherFactory>();

		var enricherFactory = new EnricherFactory(enricherDefinitions, metrics.Object, logger);

		var messageBT = JsonObject.Parse(Resources.Messages.MessageEventDeliveredToBT)!;
		var enrichersBT = enricherFactory.GetEnrichers((JsonObject)messageBT, serviceScope).Single.ToList();

		var messageCCSJ = JsonObject.Parse(Resources.Messages.MessageEventDeliveredToCCSJ)!;
		var enrichersCCSJ = enricherFactory.GetEnrichers((JsonObject)messageCCSJ, serviceScope).Single.ToList();

		var messageDescartes = JsonObject.Parse(Resources.Messages.MessageEventDeliveredToDescartes)!;
		var enrichersDescartes = enricherFactory.GetEnrichers((JsonObject)messageDescartes, serviceScope).Single.ToList();

		Assert.Multiple(() =>
		{
			Assert.That(enrichersBT.Select(e => (e.Key, e.Value.Enricher.GetType().FullName, e.Value.Criteria)),
				Is.EquivalentTo(new[] { ("Air", "eServices.ApplicationEvent.EnrichmentService.Enrichers.AirMessaging", "RegexMatch") }));
			Assert.That(enrichersCCSJ.Select(e => (e.Key, e.Value.Enricher.GetType().FullName, e.Value.Criteria)),
				Is.EquivalentTo(new[] { ("Air", "eServices.ApplicationEvent.EnrichmentService.Enrichers.AirMessaging", "LookupStartsWith") }));
			Assert.That(enrichersDescartes.Select(e => (e.Key, e.Value.Enricher.GetType().FullName, e.Value.Criteria)),
				Is.EquivalentTo(new[] { ("Air", "eServices.ApplicationEvent.EnrichmentService.Enrichers.AirMessaging", "LookupEquals") }));
		});
	}
}
