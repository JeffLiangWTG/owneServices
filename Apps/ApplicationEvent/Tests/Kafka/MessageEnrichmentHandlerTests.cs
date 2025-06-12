using eServices.ApplicationEvent.EnrichmentService.Instrumentation;
using eServices.eHubDataModel.eHubTransactionsCore;
using KafkaFlow;
using KafkaFlow.Producers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;

namespace eServices.ApplicationEvent.Tests.Kafka;

public class MessageEnrichmentHandlerTests
{
	[Test]
	public async Task MessageEventHandler_Invoke_Batched()
	{
		var dbcontext = new Mock<eHubTransactionsContext>(new DbContextOptionsBuilder<eHubTransactionsContext>().Options).Object;
		var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
		new[] { ("OceanMessaging:Inbox:55FDF7AB-540B-455C-BD80-EADC245CD87A:" + nameof(OceanMessagingResources.XPath_Inbound_InboxEdifactMsgType), "IFTSTA"),
				("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_OutboxBatchCount), "2"),
				("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_OutboxMsgType) + ":1", "UniversalEvent"),
				("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_EventType) + ":1", "DEP"),
				("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_Carrier) + ":1", "CHNL"),
				("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_CarrierBookingRef) + ":1", "AMP0489601"),
				("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_MasterBillNumber) + ":1", "AMP0489601"),
				("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_CoLoadCarrierBookingRef) + ":1", ""),
				("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_CoLoadMasterBillNumber) + ":1", ""),
				("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_MessageRef) + ":1", ""),
				("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_DocumentName) + ":1", ""),
				("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_DataType) + ":1", ""),
				("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_DataKey) + ":1", ""),
				("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_OutboxMsgType) + ":2", "UniversalEvent"),
				("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_EventType) + ":2", "DEP"),
				("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_Carrier) + ":2", "CHNL"),
				("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_CarrierBookingRef) + ":2", "AMP0490582"),
				("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_MasterBillNumber) + ":2", "AMP0490582"),
				("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_CoLoadCarrierBookingRef) + ":2", ""),
				("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_CoLoadMasterBillNumber) + ":2", ""),
				("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_MessageRef) + ":2", ""),
				("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_DocumentName) + ":2", ""),
				("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_DataType) + ":2", ""),
				("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_DataKey) + ":2", "") }
			.ToList().ForEach(((string Key, string Value) i) => cache.Set(i.Key, i.Value));
		var configurationData = new Dictionary<string, string?>
		{
			["CacheSizeLimit"] = "2000000000",
			["CacheExpirationDefaultMinutes"] = "30"
		};
		var configuration = new ConfigurationBuilder().AddInMemoryCollection(configurationData).Build();
		var batchedEnrichers = new Dictionary<string, EnricherBatched>()
		{
			["Ocean"] = new(new OceanMessaging(dbcontext, new CacheService(cache, configuration, new FakeLogger<CacheService>(Console.WriteLine))), "Inbound")
		};
		var singleEnrichers = new Dictionary<string, EnricherSingle>
		{
			["eHub"] = new(new EHub(), "Include")
		};
		var enricherFactory = new Mock<IEnricherFactory>();
		enricherFactory.Setup(x => x.GetEnrichers(It.IsAny<JsonObject>(), It.IsAny<IServiceScope>())).Returns((batchedEnrichers, singleEnrichers));
		var services = new ServiceCollection();
		var serviceProvider = services.BuildServiceProvider();
		var producedMessages = new List<(object messageKey, object messageValue)>();
		var messageProducer = new Mock<IMessageProducer>();
		messageProducer.Setup(x => x.ProduceAsync(It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>(), null, null))
			.Callback((object k, object v, IMessageHeaders h, int? p) => producedMessages.Add((k, v)));
		var producerAccessor = new Mock<IProducerAccessor>();
		producerAccessor.Setup(x => x.GetProducer("ApplicationEvents")).Returns(messageProducer.Object);
		var consumerContext = new Mock<IConsumerContext>();
		var message = new Message(null, JsonObject.Parse(Resources.Messages.MessageEventDeliveredFromCMACGM_IFTSTA));
		var messageContext = new Mock<IMessageContext>();
		messageContext.SetupGet(x => x.Message).Returns(message);
		messageContext.SetupGet(x => x.ConsumerContext).Returns(consumerContext.Object);
		var metrics = new Mock<IMetrics>();

		var messageEventHandler = new EnrichmentService.Kafka.MessageEnrichmentHandler(
			serviceProvider,
			enricherFactory.Object,
			producerAccessor.Object,
			new Mock<IHostApplicationLifetime>().Object,
			metrics.Object,
			new FakeLogger<EnrichmentService.Kafka.MessageEnrichmentHandler>());
		await messageEventHandler.Invoke(messageContext.Object, null!);

		Assert.Multiple(() =>
		{
			string[] expectedKeys = ["F7650201-132B-4BED-9774-EA6E32631A61:0", "F7650201-132B-4BED-9774-EA6E32631A61:1"];
			Assert.That(producedMessages.Select(m => m.messageKey), Is.EquivalentTo(expectedKeys));
			var actualJsonItems = producedMessages.Select(m => m.messageValue as JsonObject).ToList();

			AssertJsonAreEqual(actualJsonItems[0]!, Resources.Messages.ApplicationEventOcean1);
			AssertJsonAreEqual(actualJsonItems[1]!, Resources.Messages.ApplicationEventOcean2);

			static void AssertJsonAreEqual(JsonObject actualJson, byte[] expectedBytes)
			{
				var expectedJson = JsonObject.Parse(expectedBytes);
				Assert.That(JsonObject.DeepEquals(actualJson, expectedJson), Is.True, $"Expected: < {expectedJson} >\nBut was: < {actualJson} >");
			}
		});
	}

	[Test]
	public async Task MessageEventHandler_Invoke_SingleOnly()
	{
		var configurationData = new Dictionary<string, string?>
		{ ["Enrichers:Air:AggregateMessageInformationFormat"] = "^(?<MessageType>.*?)_(?<MAWB>.*?)_(?<HAWB>.*?)_(?<Carrier>.*?)_(?<RecipientPIMA>.*)$" };
		var configuration = new ConfigurationBuilder().AddInMemoryCollection(configurationData).Build();
		var batchedEnrichers = new Dictionary<string, EnricherBatched>();
		var singleEnrichers = new Dictionary<string, EnricherSingle>
		{
			["eHub"] = new(new EHub(), "Include"),
			["Air"] = new(new AirMessaging(configuration), "0")
		};
		var enricherFactory = new Mock<IEnricherFactory>();
		enricherFactory.Setup(x => x.GetEnrichers(It.IsAny<JsonObject>(), It.IsAny<IServiceScope>())).Returns((batchedEnrichers, singleEnrichers));
		var services = new ServiceCollection();
		var serviceProvider = services.BuildServiceProvider();
		var producedMessages = new List<(object messageKey, object messageValue)>();
		var messageProducer = new Mock<IMessageProducer>();
		messageProducer.Setup(x => x.ProduceAsync(It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>(), null, null))
			.Callback((object k, object v, IMessageHeaders h, int? p) => producedMessages.Add((k, v)));
		var producerAccessor = new Mock<IProducerAccessor>();
		producerAccessor.Setup(x => x.GetProducer("ApplicationEvents")).Returns(messageProducer.Object);
		var consumerContext = new Mock<IConsumerContext>();
		var message = new Message(null, JsonObject.Parse(Resources.Messages.MessageEventDeliveredToBT));
		var messageContext = new Mock<IMessageContext>();
		messageContext.SetupGet(x => x.Message).Returns(message);
		messageContext.SetupGet(x => x.ConsumerContext).Returns(consumerContext.Object);
		var metrics = new Mock<IMetrics>();
		var logger = new FakeLogger<EnrichmentService.Kafka.MessageEnrichmentHandler>();

		var messageEventHandler = new EnrichmentService.Kafka.MessageEnrichmentHandler(
			serviceProvider,
			enricherFactory.Object,
			producerAccessor.Object,
			new Mock<IHostApplicationLifetime>().Object,
			metrics.Object,
			logger);
		await messageEventHandler.Invoke(messageContext.Object, null!);

		string[] expectedKeys = ["A12E6F60-1950-414A-A949-13FB2B359A2E"];
		Assert.That(producedMessages.Select(m => m.messageKey), Is.EquivalentTo(expectedKeys));
		var actualJson = producedMessages.FirstOrDefault().messageValue as JsonObject;
		var expectedJson = JsonObject.Parse(Resources.Messages.ApplicationEventAir);
		Assert.That(JsonObject.DeepEquals(expectedJson, actualJson), $"Expected: < {expectedJson} >\nBut was: < {actualJson} >");
	}
}
