using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace eServices.ApplicationEvent.Tests.Enrichers;

public class EnricherFactoryBuilderTests
{
	[Test]
	public void EnricherFactoryBuilder_AddEnricherFactory()
	{
		var configurationData = new Dictionary<string, string?>
		{
			["Enrichers:Air:Type"] = "eServices.ApplicationEvent.EnrichmentService.Enrichers.AirMessaging",
			["Enrichers:Air:Criteria:0:Parameters:0"] = "MessageEvent.Event.Type",
			["Enrichers:Air:Criteria:0:Parameters:1"] = "MessageEvent.Event.RecipientID",
			["Enrichers:Air:Criteria:0:Expression"] = "(p[0] is \"Delivered\" or \"DeliveryFailed\") && Regex.IsMatch(p[1], \"^(ARINC_SP|BT|BT_WithUnsupported)$\", RegexOptions.Compiled)"
		};
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configurationData).Build().GetSection("Enrichers");
		var configurationManager = new Mock<IConfigurationManager>();
		configurationManager.Setup(x => x.GetSection("Enrichers")).Returns(configuration);
		var services = new ServiceCollection();
		var hostBuilder = new Mock<IHostApplicationBuilder>();
		hostBuilder.SetupGet(x => x.Configuration).Returns(configurationManager.Object);
		hostBuilder.SetupGet(x => x.Services).Returns(services);

		var result = EnricherFactoryBuilder.AddEnricherFactory(hostBuilder.Object);

		Assert.Multiple(() =>
		{
			Assert.That(result, Is.SameAs(hostBuilder.Object));
			Assert.That(services, Has.One.Matches<ServiceDescriptor>(s => s.ServiceType == typeof(Dictionary<string, EnricherDefinition>)));
			Assert.That(services, Has.One.Matches<ServiceDescriptor>(s => s.ServiceType == typeof(IEnricherFactory)));
			Assert.That(services, Has.One.Matches<ServiceDescriptor>(s => s.ServiceType == typeof(AirMessaging)));
		});
	}
}
