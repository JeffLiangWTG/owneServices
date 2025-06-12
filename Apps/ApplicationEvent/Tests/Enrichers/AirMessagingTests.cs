using eServices.ApplicationEvent.EnrichmentService.Instrumentation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;

namespace eServices.ApplicationEvent.Tests.Enrichers;

public class AirMessagingTests
{
	[Test]
	public void AirMessaging_Criteria()
	{
		var enricherFactoryOptions = new EnricherFactoryOptions
		{
			["eHub"] = new EnricherFactoryOptions.Enricher
			{
				Type = "eServices.ApplicationEvent.EnrichmentService.Enrichers.EHub",
			},
			["Air"] = new EnricherFactoryOptions.Enricher
			{
				Type = "eServices.ApplicationEvent.EnrichmentService.Enrichers.AirMessaging",
				Includes = ["eHub"],
				Criteria = [
					new EnricherFactoryOptions.Enricher.Criterion
					{
						Parameters = ["MessageEvent.Event.Type", "MessageEvent.Message.RecipientID"],
						Expression = "(p[0] is \"Delivered\" or \"DeliveryFailed\") && Regex.IsMatch(p[1], \"^(ARINC_SP|BT|BT_WithUnsupported)$\", RegexOptions.Compiled)"
					}
				]
			}
		};
		var enricherDefinitions = EnricherFactoryBuilder.ParseEnricherDefinitions(enricherFactoryOptions);
		var message = JsonObject.Parse(Resources.Messages.MessageEventDeliveredToBT)!;
		var services = new ServiceCollection();
		services.AddTransient(enricherDefinitions["eHub"].Type);
		services.AddTransient(enricherDefinitions["Air"].Type);
		services.AddTransient(_ => new Mock<IConfiguration>().Object);
		var serviceProvider = services.BuildServiceProvider();
		var serviceScope = serviceProvider.CreateScope();
		var metrics = new Mock<IMetrics>();
		var logger = new FakeLogger<EnricherFactory>();

		var enricherFactory = new EnricherFactory(enricherDefinitions, metrics.Object, logger);
		var enrichers = enricherFactory.GetEnrichers((JsonObject)message, serviceScope);

		Assert.Multiple(() =>
		{
			Assert.That(enrichers.Single.Select(e => (e.Key, e.Value.Enricher.GetType().FullName, e.Value.Criteria)),
					Is.EquivalentTo(new[]
					{
						("eHub", "eServices.ApplicationEvent.EnrichmentService.Enrichers.EHub", "Include"),
						("Air", "eServices.ApplicationEvent.EnrichmentService.Enrichers.AirMessaging", "0")
					}));
			Assert.That(enrichers.Batched, Is.Empty);
		});
	}

	[Test]
	public async Task AirMessaging_EnrichMessage()
	{
		var configurationData = new Dictionary<string, string?>
		{ ["Enrichers:Air:AggregateMessageInformationFormat"] = "^(?<MessageType>.*?)_(?<MAWB>.*?)_(?<HAWB>.*?)_(?<Carrier>.*?)_(?<RecipientPIMA>.*?)$" };
		var configuration = new ConfigurationBuilder().AddInMemoryCollection(configurationData).Build();
		var message = JsonObject.Parse(Resources.Messages.MessageEventDeliveredToBT)!;

		var enricher = new AirMessaging(configuration);
		var result = await enricher.EnrichMessageSingleAsync((JsonObject)message, "");

		Assert.That(result?.Select(r => (r.Key, r.Value?.ToString())),
			Is.Not.Null.And.EquivalentTo(new[] {
				("MessageType", "FHL"),
				("MAWB", "17284793995"),
				("HAWB", "SHAAA052805"),
				("Carrier", "CV"),
				("RecipientPIMA", "TDVAGT03WILSON"),
				("SenderPIMA", "NDTAKBX") }));
	}

	private static IEnumerable<TestCaseData> AirMessaging_EnrichMessage_Variants_TestCaseSource = [
		new(null, null, Array.Empty<(string, string)>()),
		new("FHL_17284793995_SHAAA052805_CV_TDVAGT03WILSON", null,
			new[] {
				("MessageType", "FHL"),
				("MAWB", "17284793995"),
				("HAWB", "SHAAA052805"),
				("Carrier", "CV"),
				("RecipientPIMA", "TDVAGT03WILSON")
			}),
		new("FHL_17284793995_SHAAA052805_CV_TDVAGT03WILSON", "RHKAGT021330119/HKG850",
			new[] {
				("MessageType", "FHL"),
				("MAWB", "17284793995"),
				("HAWB", "SHAAA052805"),
				("Carrier", "CV"),
				("RecipientPIMA", "TDVAGT03WILSON"),
				("SenderPIMA", "RHKAGT021330119/HKG85")
			}),
		new("FHL_17284793995__CV_TDVAGT03WILSON", "REUFFW87CGWAIT/ORD02",
			new[] {
				("MessageType", "FHL"),
				("MAWB", "17284793995"),
				("Carrier", "CV"),
				("RecipientPIMA", "TDVAGT03WILSON"),
				("SenderPIMA", "REUFFW87CGWAIT/ORD02")
			})
	];

	[TestCaseSource(nameof(AirMessaging_EnrichMessage_Variants_TestCaseSource))]
	public async Task AirMessaging_EnrichMessage_Variants(string? fileName, string? email, (string, string)[] expectedResults)
	{
		var configurationData = new Dictionary<string, string?>
		{ ["Enrichers:Air:AggregateMessageInformationFormat"] = "^(?<MessageType>.*?)_(?<MAWB>.*?)_(?<HAWB>.*?)_(?<Carrier>.*?)_(?<RecipientPIMA>.*?)$" };
		var configuration = new ConfigurationBuilder().AddInMemoryCollection(configurationData).Build();
		var message = JsonObject.Parse(Resources.Messages.MessageEventDeliveredToBT)!;
		message["MessageEvent"]!["eHub"]!["Outbox"]!["FileName"] = fileName;
		message["MessageEvent"]!["eHub"]!["Outbox"]!["EmailSubject"] = email;

		var enricher = new AirMessaging(configuration);
		var result = await enricher.EnrichMessageSingleAsync((JsonObject)message, "");

		Assert.That(result?.Select(r => (r.Key, r.Value?.ToString())),
			Is.Not.Null.And.EquivalentTo(expectedResults));
	}
}