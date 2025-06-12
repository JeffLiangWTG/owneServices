using eServices.ApplicationEvent.EnrichmentService.Instrumentation;
using eServices.eHubDataModel.eHubTransactionsCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;

namespace eServices.ApplicationEvent.Tests.Enrichers;

public class OceanMessagingTests
{
	private static IEnumerable<TestCaseData> OceanMessaging_Criteria_TestCaseSource = [
		new(nameof(Resources.Messages.MessageEventDeliveredFromINTTRA_IFTMBC_XUE),
			new[]
			{
				("eHub", "eServices.ApplicationEvent.EnrichmentService.Enrichers.EHub", "Include"),
				("OceanInbound", "eServices.ApplicationEvent.EnrichmentService.Enrichers.OceanMessaging", "Inbound")
			}),
		new(nameof(Resources.Messages.MessageEventProcessedToINTTRA),
			new[]
			{
				("eHub", "eServices.ApplicationEvent.EnrichmentService.Enrichers.EHub", "Include"),
				("OceanInbound", "eServices.ApplicationEvent.EnrichmentService.Enrichers.OceanMessaging", "Outbound")
			}),
		new(nameof(Resources.Messages.MessageEventProcessedToCMACGM_SI_DeliveryNotification),
			new[]
			{
				("eHub", "eServices.ApplicationEvent.EnrichmentService.Enrichers.EHub", "Include"),
				("OceanInbound", "eServices.ApplicationEvent.EnrichmentService.Enrichers.OceanMessaging", "DeliveryNotification")
			}),
	];

	[TestCaseSource(nameof(OceanMessaging_Criteria_TestCaseSource))]
	public void OceanMessaging_Criteria(string message, (string, string, string)[] expectedResults)
	{
		var enricherFactoryOptions = new EnricherFactoryOptions
		{
			["eHub"] = new EnricherFactoryOptions.Enricher
			{
				Type = "eServices.ApplicationEvent.EnrichmentService.Enrichers.EHub",
			},
			["OceanInbound"] = new EnricherFactoryOptions.Enricher
			{
				Type = "eServices.ApplicationEvent.EnrichmentService.Enrichers.OceanMessaging",
				Includes = ["eHub"],
				Criteria = [
					new EnricherFactoryOptions.Enricher.Criterion
					{
						Name = "Inbound",
						Parameters = ["MessageEvent.Event.Type", "MessageEvent.Message.SenderID", "MessageEvent.Message.RecipientID"],
						Expression = "(p[0] is \"Processed\" or \"Delivered\" or \"DeliveryFailed\") && Array.Exists(l[\"Providers\"], i => p[1] == i) && (p[2] is not \"CONTAINER_TRACKING\")"
					},
					new EnricherFactoryOptions.Enricher.Criterion
					{
						Name = "Outbound",
						Parameters = ["MessageEvent.Event.Type", "MessageEvent.Message.RecipientID"],
						Expression = "(p[0] is \"Processed\" or \"Delivered\" or \"DeliveryFailed\") && Array.Exists(l[\"Providers\"], i => p[1].Length > i.Length && p[1].StartsWith(i) && p[1][i.Length] is '_')"
					},
					new EnricherFactoryOptions.Enricher.Criterion
					{
						Name = "OutboundError",
						Parameters = ["MessageEvent.Event.Type", "MessageEvent.Message.RecipientID"],
						Expression = "(p[0] is \"DeliveryFailed\") && (p[1] is \"SHIPPING_INSTRUCTION\")"
					},
					new EnricherFactoryOptions.Enricher.Criterion
					{
						Name = "DeliveryNotification",
						Parameters = ["MessageEvent.Event.Type", "MessageEvent.Message.SenderID"],
						Expression = "(p[0] is \"Processed\" or \"Delivered\" or \"DeliveryFailed\") && (p[1] is \"SHIPPING_INSTRUCTION\")"
					}
				],
				Lookups = {
					["Providers"] = [
						"CARGOSMART",
						"CARGOWISE",
						"CargoWorldNetwork",
						"CAROTRANS",
						"CARRIER_EMAIL",
						"CHARTERLINK",
						"CMACGM",
						"CMLOG",
						"CROWLEY",
						"CWTG",
						"EASIPASS",
						"ECULINE",
						"EMIRATESLINE",
						"EVERGREEN",
						"EVERRICH",
						"FAMOUS",
						"GREATASIA",
						"GTNEXUS",
						"HAPAG_LLOYD",
						"HYUNDAI",
						"ICARGOALLIANCE",
						"ICL",
						"INTTRA",
						"KMTC",
						"MAERSK",
						"MARFRET",
						"MSC",
						"NGBEDI",
						"ODYSSEY",
						"ONE",
						"PENAVICO",
						"PIL",
						"PORTRIX",
						"SINOLINE",
						"SINOTRANS",
						"SINOTRANS_EASTERN",
						"SINOTRANS_MINGZHOU",
						"SMLINE",
						"SOUTHEAST",
						"SWIRE",
						"TRANSOCEAN",
						"TSLINE",
						"TURKON",
						"UNIWILL",
						"VANGUARD",
						"WANHAI",
						"WECC",
						"WORLDEX",
						"WWA",
						"YANGMING",
						"ZIM"
					]
				}
			}
		};
		var enricherDefinitions = EnricherFactoryBuilder.ParseEnricherDefinitions(enricherFactoryOptions);
		var messageJson = JsonObject.Parse(Resources.Messages.ResourceManager.GetString(message)!)!;
		var services = new ServiceCollection();
		services.AddTransient(enricherDefinitions["eHub"].Type);
		services.AddTransient(enricherDefinitions["OceanInbound"].Type);
		services.AddTransient(_ => new Mock<eHubTransactionsContext>(new DbContextOptionsBuilder<eHubTransactionsContext>().Options).Object);
		services.AddTransient<IMemoryCache>(_ => new MemoryCache(new MemoryCacheOptions()));
		var configurationData = new Dictionary<string, string?>
		{
			["CacheSizeLimit"] = "2000000000",
			["CacheExpirationDefaultMinutes"] = "30"
		};
		services.AddTransient<IConfiguration>(_ => new ConfigurationBuilder().AddInMemoryCollection(configurationData).Build());
		services.AddTransient<CacheService>();
		services.AddLogging();
		var serviceProvider = services.BuildServiceProvider();
		var serviceScope = serviceProvider.CreateScope();
		var metrics = new Mock<IMetrics>();
		var logger = new FakeLogger<EnricherFactory>();

		var enricherFactory = new EnricherFactory(enricherDefinitions, metrics.Object, logger);
		var enrichers = enricherFactory.GetEnrichers((JsonObject)messageJson, serviceScope);

		Assert.That(enrichers.Single.Select(e => (e.Key, e.Value.Enricher.GetType().FullName, e.Value.Criteria))
			.Concat(enrichers.Batched.Select(e => (e.Key, e.Value.Enricher.GetType().FullName, e.Value.Criteria))),
			Is.EquivalentTo(expectedResults));
	}

	private static IEnumerable<TestCaseData> OceanMessaging_EnrichMessage_Variants_TestCaseSource = [
		new(nameof(Resources.Messages.MessageEventDeliveredFromINTTRA_IFTMBC_XUE),
			"Inbound",
			new[] { ("OceanMessaging:Inbox:C6D41BAB-AE8B-43C2-88D9-F2ABCC3F7C48:" + nameof(OceanMessagingResources.XPath_Inbound_InboxEdifactMsgType), "IFTMBC"),
					("OceanMessaging:Outbox:C56E2534-1414-4491-89F3-6FC5CB9C3B4E:" + nameof(OceanMessagingResources.XPath_Inbound_OutboxBatchCount), "1"),
					("OceanMessaging:Outbox:C56E2534-1414-4491-89F3-6FC5CB9C3B4E:" + nameof(OceanMessagingResources.XPath_Inbound_OutboxMsgType) + ":1", "UniversalEvent"),
					("OceanMessaging:Outbox:C56E2534-1414-4491-89F3-6FC5CB9C3B4E:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_EventType) + ":1", "MAA"),
					("OceanMessaging:Outbox:C56E2534-1414-4491-89F3-6FC5CB9C3B4E:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_Carrier) + ":1", "MFUS"),
					("OceanMessaging:Outbox:C56E2534-1414-4491-89F3-6FC5CB9C3B4E:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_CarrierBookingRef) + ":1", ""),
					("OceanMessaging:Outbox:C56E2534-1414-4491-89F3-6FC5CB9C3B4E:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_MasterBillNumber) + ":1", ""),
					("OceanMessaging:Outbox:C56E2534-1414-4491-89F3-6FC5CB9C3B4E:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_CoLoadCarrierBookingRef) + ":1", "BK24139704"),
					("OceanMessaging:Outbox:C56E2534-1414-4491-89F3-6FC5CB9C3B4E:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_CoLoadMasterBillNumber) + ":1", "BL24137282"),
					("OceanMessaging:Outbox:C56E2534-1414-4491-89F3-6FC5CB9C3B4E:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_MessageRef) + ":1", "C2401588574-V2"),
					("OceanMessaging:Outbox:C56E2534-1414-4491-89F3-6FC5CB9C3B4E:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_DocumentName) + ":1", "Booking Request"),
					("OceanMessaging:Outbox:C56E2534-1414-4491-89F3-6FC5CB9C3B4E:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_DataType) + ":1", "ForwardingConsol"),
					("OceanMessaging:Outbox:C56E2534-1414-4491-89F3-6FC5CB9C3B4E:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_DataKey) + ":1", "C2401588574-V2") },
			new[] { new[] { ("EventType", "MAA"),
					("MessageType", "IFTMBC"),
					("Carrier", "MFUS"),
					("CoLoadCarrierBookingRef", "BK24139704"),
					("CoLoadMasterBillNumber", "BL24137282"),
					("DocumentName", "Booking Request"),
					("MessageRef", "C2401588574-V2"),
					("DataType", "ForwardingConsol"),
					("DataKey", "C2401588574-V2"),
					("Direction", "Response"),
					("TrackingID", "756BDA7A-B540-46F0-91E2-12A33297C558"),
					("FileName", "/outbound/808273_IFTMBC_4463751466_7377958321_.txt") } })
		{ TestName = nameof(Resources.Messages.MessageEventDeliveredFromINTTRA_IFTMBC_XUE) },
		new(nameof(Resources.Messages.MessageEventDeliveredFromINTTRA_IFTMBC_XUS),
			"Inbound",
			new[] { ("OceanMessaging:Inbox:C6D41BAB-AE8B-43C2-88D9-F2ABCC3F7C48:" + nameof(OceanMessagingResources.XPath_Inbound_InboxEdifactMsgType), "IFTMBC"),
					("OceanMessaging:Outbox:6FB348F8-221D-456F-9EF7-1B4A430D0EFF:" + nameof(OceanMessagingResources.XPath_Inbound_OutboxBatchCount), "1"),
					("OceanMessaging:Outbox:6FB348F8-221D-456F-9EF7-1B4A430D0EFF:" + nameof(OceanMessagingResources.XPath_Inbound_OutboxMsgType) + ":1", "UniversalShipment"),
					("OceanMessaging:Outbox:6FB348F8-221D-456F-9EF7-1B4A430D0EFF:" + nameof(OceanMessagingResources.XPath_Inbound_XUS_EventType) + ":1", "MAA"),
					("OceanMessaging:Outbox:6FB348F8-221D-456F-9EF7-1B4A430D0EFF:" + nameof(OceanMessagingResources.XPath_Inbound_XUS_Carrier) + ":1", "MFUS"),
					("OceanMessaging:Outbox:6FB348F8-221D-456F-9EF7-1B4A430D0EFF:" + nameof(OceanMessagingResources.XPath_Inbound_XUS_CarrierBookingRef) + ":1", ""),
					("OceanMessaging:Outbox:6FB348F8-221D-456F-9EF7-1B4A430D0EFF:" + nameof(OceanMessagingResources.XPath_Inbound_XUS_MasterBillNumber) + ":1", ""),
					("OceanMessaging:Outbox:6FB348F8-221D-456F-9EF7-1B4A430D0EFF:" + nameof(OceanMessagingResources.XPath_Inbound_XUS_CoLoadCarrierBookingRef) + ":1", "BK24139704"),
					("OceanMessaging:Outbox:6FB348F8-221D-456F-9EF7-1B4A430D0EFF:" + nameof(OceanMessagingResources.XPath_Inbound_XUS_CoLoadMasterBillNumber) + ":1", "BL24137282"),
					("OceanMessaging:Outbox:6FB348F8-221D-456F-9EF7-1B4A430D0EFF:" + nameof(OceanMessagingResources.XPath_Inbound_XUS_MessageRef) + ":1", "C2401588574-V2"),
					("OceanMessaging:Outbox:6FB348F8-221D-456F-9EF7-1B4A430D0EFF:" + nameof(OceanMessagingResources.XPath_Inbound_XUS_DocumentName) + ":1", "Booking Request"),
					("OceanMessaging:Outbox:6FB348F8-221D-456F-9EF7-1B4A430D0EFF:" + nameof(OceanMessagingResources.XPath_Inbound_XUS_DataType) + ":1", "ForwardingConsol"),
					("OceanMessaging:Outbox:6FB348F8-221D-456F-9EF7-1B4A430D0EFF:" + nameof(OceanMessagingResources.XPath_Inbound_XUS_DataKey) + ":1", "C2401588574-V2") },
			new[] { new[] { ("EventType", "MAA"),
					("MessageType", "IFTMBC"),
					("Carrier", "MFUS"),
					("CoLoadCarrierBookingRef", "BK24139704"),
					("CoLoadMasterBillNumber", "BL24137282"),
					("DocumentName", "Booking Request"),
					("MessageRef", "C2401588574-V2"),
					("DataType", "ForwardingConsol"),
					("DataKey", "C2401588574-V2"),
					("Direction", "Response"),
					("TrackingID", "D06063FF-A0DF-4C67-959B-21A5B7E22DAE"),
					("FileName", "/outbound/808273_IFTMBC_4463751466_7377958321_.txt") } })
		{ TestName = nameof(Resources.Messages.MessageEventDeliveredFromINTTRA_IFTMBC_XUS) },
		new(nameof(Resources.Messages.MessageEventProcessedToINTTRA),
			"Outbound",
			new[] { ("OceanMessaging:Outbox:7C48CD50-AD23-413C-89C1-DE6341F137BA:" + nameof(OceanMessagingResources.XPath_Outbound_MessageType), "IFTMIN"),
					("OceanMessaging:Outbox:7C48CD50-AD23-413C-89C1-DE6341F137BA:" + nameof(OceanMessagingResources.XPath_Outbound_InterchangeNum), "13614558"),
					("OceanMessaging:Inbox:c7092fa4-a0b4-4556-927a-f5c28f516ba6:" + nameof(OceanMessagingResources.XPath_Outbound_IsCoLoad), "true"),
					("OceanMessaging:Inbox:c7092fa4-a0b4-4556-927a-f5c28f516ba6:" + nameof(OceanMessagingResources.XPath_Outbound_Carrier_CoLoad), "CMDU"),
					("OceanMessaging:Inbox:c7092fa4-a0b4-4556-927a-f5c28f516ba6:" + nameof(OceanMessagingResources.XPath_Outbound_CarrierBookingRef), "LHV3515156"),
					("OceanMessaging:Inbox:c7092fa4-a0b4-4556-927a-f5c28f516ba6:" + nameof(OceanMessagingResources.XPath_Outbound_MasterBillNumber), "LHV3515156"),
					("OceanMessaging:Inbox:c7092fa4-a0b4-4556-927a-f5c28f516ba6:" + nameof(OceanMessagingResources.XPath_Outbound_CoLoadCarrierBookingRef), ""),
					("OceanMessaging:Inbox:c7092fa4-a0b4-4556-927a-f5c28f516ba6:" + nameof(OceanMessagingResources.XPath_Outbound_CoLoadMasterBillNumber), ""),
					("OceanMessaging:Outbox:7C48CD50-AD23-413C-89C1-DE6341F137BA:" + nameof(OceanMessagingResources.XPath_Outbound_MessageRef), "CMA0001651847"),
					("OceanMessaging:Inbox:c7092fa4-a0b4-4556-927a-f5c28f516ba6:" + nameof(OceanMessagingResources.XPath_Outbound_DocumentName), "Shipping Instruction"),
					("OceanMessaging:Inbox:c7092fa4-a0b4-4556-927a-f5c28f516ba6:" + nameof(OceanMessagingResources.XPath_Outbound_Purpose), "ORG"),
					("OceanMessaging:Inbox:c7092fa4-a0b4-4556-927a-f5c28f516ba6:" + nameof(OceanMessagingResources.XPath_Outbound_SubmissionVersion), "1"),
					("OceanMessaging:Inbox:c7092fa4-a0b4-4556-927a-f5c28f516ba6:" + nameof(OceanMessagingResources.XPath_Outbound_DataType), "ForwardingConsol"),
					("OceanMessaging:Inbox:c7092fa4-a0b4-4556-927a-f5c28f516ba6:" + nameof(OceanMessagingResources.XPath_Outbound_DataKey), "CSFR24210715"),
					("OceanMessaging:Inbox:c7092fa4-a0b4-4556-927a-f5c28f516ba6:" + nameof(OceanMessagingResources.XPath_Outbound_OperationalPort), "FRFOS"),
					("OceanMessaging:Inbox:c7092fa4-a0b4-4556-927a-f5c28f516ba6:" + nameof(OceanMessagingResources.XPath_Outbound_TrackingID),
						"01069045-c204-4f1a-ab26-d0535131360a") },
			new[] { new[] { ("MessageType", "IFTMIN"),
					("InterchangeNum", "13614558"),
					("Carrier", "CMDU"),
					("CarrierBookingRef", "LHV3515156"),
					("MasterBillNumber", "LHV3515156"),
					("MessageRef", "CMA0001651847"),
					("DocumentName", "Shipping Instruction"),
					("Purpose", "ORG"),
					("SubmissionVersion", "1"),
					("DataType", "ForwardingConsol"),
					("DataKey", "CSFR24210715"),
					("OperationalPort", "FRFOS"),
					("TrackingID", "01069045-C204-4F1A-AB26-D0535131360A"),
					("Direction", "Request"),
					("FileName", "INFTMIN_DDAOBRS_13614558") } })
		{ TestName = nameof(Resources.Messages.MessageEventProcessedToINTTRA) },
		new(nameof(Resources.Messages.MessageEventProcessedToCMACGM_BK),
			"Outbound",
			new[] { ("OceanMessaging:Outbox:016DEADF-3A1D-4545-B64E-7899346D9E56:" + nameof(OceanMessagingResources.XPath_Outbound_MessageType), "IFTMBF"),
					("OceanMessaging:Outbox:016DEADF-3A1D-4545-B64E-7899346D9E56:" + nameof(OceanMessagingResources.XPath_Outbound_InterchangeNum), "3444738"),
					("OceanMessaging:Inbox:16850c20-592a-44a0-a968-e92b0d5c56ea:" + nameof(OceanMessagingResources.XPath_Outbound_IsCoLoad), ""),
					("OceanMessaging:Inbox:16850c20-592a-44a0-a968-e92b0d5c56ea:" + nameof(OceanMessagingResources.XPath_Outbound_Carrier), "CMDU"),
					("OceanMessaging:Inbox:16850c20-592a-44a0-a968-e92b0d5c56ea:" + nameof(OceanMessagingResources.XPath_Outbound_CarrierBookingRef), ""),
					("OceanMessaging:Inbox:16850c20-592a-44a0-a968-e92b0d5c56ea:" + nameof(OceanMessagingResources.XPath_Outbound_MasterBillNumber), ""),
					("OceanMessaging:Inbox:16850c20-592a-44a0-a968-e92b0d5c56ea:" + nameof(OceanMessagingResources.XPath_Outbound_CoLoadCarrierBookingRef), "LHV3515156"),
					("OceanMessaging:Inbox:16850c20-592a-44a0-a968-e92b0d5c56ea:" + nameof(OceanMessagingResources.XPath_Outbound_CoLoadMasterBillNumber), "LHV3515156"),
					("OceanMessaging:Outbox:016DEADF-3A1D-4545-B64E-7899346D9E56:" + nameof(OceanMessagingResources.XPath_Outbound_MessageRef), "CMA0001707388"),
					("OceanMessaging:Inbox:16850c20-592a-44a0-a968-e92b0d5c56ea:" + nameof(OceanMessagingResources.XPath_Outbound_DocumentName), "Booking Request"),
					("OceanMessaging:Inbox:16850c20-592a-44a0-a968-e92b0d5c56ea:" + nameof(OceanMessagingResources.XPath_Outbound_Purpose), "ORG"),
					("OceanMessaging:Inbox:16850c20-592a-44a0-a968-e92b0d5c56ea:" + nameof(OceanMessagingResources.XPath_Outbound_SubmissionVersion), "1"),
					("OceanMessaging:Inbox:16850c20-592a-44a0-a968-e92b0d5c56ea:" + nameof(OceanMessagingResources.XPath_Outbound_DataType), "ForwardingConsol"),
					("OceanMessaging:Inbox:16850c20-592a-44a0-a968-e92b0d5c56ea:" + nameof(OceanMessagingResources.XPath_Outbound_DataKey), "C11020169"),
					("OceanMessaging:Inbox:16850c20-592a-44a0-a968-e92b0d5c56ea:" + nameof(OceanMessagingResources.XPath_Outbound_OperationalPort), "ESALG"),
					("OceanMessaging:Inbox:16850c20-592a-44a0-a968-e92b0d5c56ea:" + nameof(OceanMessagingResources.XPath_Outbound_TrackingID),
						"870055a9-d284-437b-9bb5-218a7c7d5b12") },
			new[] { new[] { ("MessageType", "IFTMBF"),
					("InterchangeNum", "3444738"),
					("Carrier", "CMDU"),
					("CoLoadCarrierBookingRef", "LHV3515156"),
					("CoLoadMasterBillNumber", "LHV3515156"),
					("MessageRef", "CMA0001707388"),
					("DocumentName", "Booking Request"),
					("Purpose", "ORG"),
					("SubmissionVersion", "1"),
					("DataType", "ForwardingConsol"),
					("DataKey", "C11020169"),
					("OperationalPort", "ESALG"),
					("TrackingID", "870055A9-D284-437B-9BB5-218A7C7D5B12"),
					("Direction", "Request"),
					("FileName", "IFTMBF_GEOGESPRO_3444738") } })
		{ TestName = nameof(Resources.Messages.MessageEventProcessedToCMACGM_BK) },
		new(nameof(Resources.Messages.MessageEventProcessedToCMACGM_VM),
			"Outbound",
			new[] { ("OceanMessaging:Outbox:F3789D54-4AF2-4F01-A58F-45F2F9766E1A:" + nameof(OceanMessagingResources.XPath_Outbound_MessageType), "VERMAS"),
					("OceanMessaging:Outbox:F3789D54-4AF2-4F01-A58F-45F2F9766E1A:" + nameof(OceanMessagingResources.XPath_Outbound_InterchangeNum), "3444739"),
					("OceanMessaging:Inbox:6a28b660-3eee-44e3-894e-b70ff864a389:" + nameof(OceanMessagingResources.XPath_Outbound_IsCoLoad), ""),
					("OceanMessaging:Inbox:6a28b660-3eee-44e3-894e-b70ff864a389:" + nameof(OceanMessagingResources.XPath_Outbound_Carrier), "CMDU"),
					("OceanMessaging:Inbox:6a28b660-3eee-44e3-894e-b70ff864a389:" + nameof(OceanMessagingResources.XPath_Outbound_CarrierBookingRef), "SHZ6702274"),
					("OceanMessaging:Inbox:6a28b660-3eee-44e3-894e-b70ff864a389:" + nameof(OceanMessagingResources.XPath_Outbound_MasterBillNumber), "CMDUSHZ6702274"),
					("OceanMessaging:Inbox:6a28b660-3eee-44e3-894e-b70ff864a389:" + nameof(OceanMessagingResources.XPath_Outbound_CoLoadCarrierBookingRef), ""),
					("OceanMessaging:Inbox:6a28b660-3eee-44e3-894e-b70ff864a389:" + nameof(OceanMessagingResources.XPath_Outbound_CoLoadMasterBillNumber), ""),
					("OceanMessaging:Outbox:F3789D54-4AF2-4F01-A58F-45F2F9766E1A:" + nameof(OceanMessagingResources.XPath_Outbound_MessageRef), "CMA00003444739"),
					("OceanMessaging:Inbox:6a28b660-3eee-44e3-894e-b70ff864a389:" + nameof(OceanMessagingResources.XPath_Outbound_DocumentName), "Verified Gross Container Weight"),
					("OceanMessaging:Inbox:6a28b660-3eee-44e3-894e-b70ff864a389:" + nameof(OceanMessagingResources.XPath_Outbound_Purpose), "ORG"),
					("OceanMessaging:Inbox:6a28b660-3eee-44e3-894e-b70ff864a389:" + nameof(OceanMessagingResources.XPath_Outbound_SubmissionVersion), "1"),
					("OceanMessaging:Inbox:6a28b660-3eee-44e3-894e-b70ff864a389:" + nameof(OceanMessagingResources.XPath_Outbound_DataType), "ForwardingConsol"),
					("OceanMessaging:Inbox:6a28b660-3eee-44e3-894e-b70ff864a389:" + nameof(OceanMessagingResources.XPath_Outbound_DataKey), "C2401948840-V1"),
					("OceanMessaging:Inbox:6a28b660-3eee-44e3-894e-b70ff864a389:" + nameof(OceanMessagingResources.XPath_Outbound_OperationalPort), "CNYTN"),
					("OceanMessaging:Inbox:6a28b660-3eee-44e3-894e-b70ff864a389:" + nameof(OceanMessagingResources.XPath_Outbound_TrackingID),
						"1607c0a4-b531-47e9-936d-83c6f9720811") },
			new[] { new[] { ("MessageType", "VERMAS"),
					("InterchangeNum", "3444739"),
					("Carrier", "CMDU"),
					("CarrierBookingRef", "SHZ6702274"),
					("MasterBillNumber", "CMDUSHZ6702274"),
					("MessageRef", "CMA00003444739"),
					("DocumentName", "Verified Gross Container Weight"),
					("Purpose", "ORG"),
					("SubmissionVersion", "1"),
					("DataType", "ForwardingConsol"),
					("DataKey", "C2401948840-V1"),
					("OperationalPort", "CNYTN"),
					("TrackingID", "1607C0A4-B531-47E9-936D-83C6F9720811"),
					("Direction", "Request"),
					("FileName", "VERMAS_DFOCN0ZSN_3444739") } })
		{ TestName = nameof(Resources.Messages.MessageEventProcessedToCMACGM_VM) },
		new(nameof(Resources.Messages.MessageEventProcessedToCMACGM_SI_DeliveryNotification),
			"DeliveryNotification",
			new[] { ("OceanMessaging:Outbox:59245A69-8390-4836-A2AC-E4F22E1D12F9:" + nameof(OceanMessagingResources.XPath_DeliveryNotification_EventType), "ISN"),
					("OceanMessaging:Outbox:59245A69-8390-4836-A2AC-E4F22E1D12F9:" + nameof(OceanMessagingResources.XPath_DeliveryNotification_InterchangeNum), "3444737"),
					("OceanMessaging:Outbox:59245A69-8390-4836-A2AC-E4F22E1D12F9:" + nameof(OceanMessagingResources.XPath_DeliveryNotification_MessageRef), "CMA0001651847"),
					("OceanMessaging:Inbox:0beba829-5833-42c8-a082-a3ef55ef1985:" + nameof(OceanMessagingResources.XPath_DeliveryNotification_InboxMsgType), "DeliveryNotificationMessage"),
					("OceanMessaging:Inbox:0beba829-5833-42c8-a082-a3ef55ef1985:" + nameof(OceanMessagingResources.XPath_DeliveryNotification_IsCoLoad), ""),
					("OceanMessaging:Inbox:0beba829-5833-42c8-a082-a3ef55ef1985:" + nameof(OceanMessagingResources.XPath_DeliveryNotification_Carrier), "CMDU"),
					("OceanMessaging:Inbox:0beba829-5833-42c8-a082-a3ef55ef1985:" + nameof(OceanMessagingResources.XPath_DeliveryNotification_CarrierBookingRef), "LHV3515156"),
					("OceanMessaging:Inbox:0beba829-5833-42c8-a082-a3ef55ef1985:" + nameof(OceanMessagingResources.XPath_DeliveryNotification_MasterBillNumber), "LHV3515156"),
					("OceanMessaging:Inbox:0beba829-5833-42c8-a082-a3ef55ef1985:" + nameof(OceanMessagingResources.XPath_DeliveryNotification_CoLoadCarrierBookingRef), ""),
					("OceanMessaging:Inbox:0beba829-5833-42c8-a082-a3ef55ef1985:" + nameof(OceanMessagingResources.XPath_DeliveryNotification_CoLoadMasterBillNumber), ""),
					("OceanMessaging:Inbox:0beba829-5833-42c8-a082-a3ef55ef1985:" + nameof(OceanMessagingResources.XPath_DeliveryNotification_DocumentName), "Shipping Instruction"),
					("OceanMessaging:Inbox:0beba829-5833-42c8-a082-a3ef55ef1985:" + nameof(OceanMessagingResources.XPath_DeliveryNotification_Purpose), "ORG"),
					("OceanMessaging:Inbox:0beba829-5833-42c8-a082-a3ef55ef1985:" + nameof(OceanMessagingResources.XPath_DeliveryNotification_SubmissionVersion), "1"),
					("OceanMessaging:Inbox:0beba829-5833-42c8-a082-a3ef55ef1985:" + nameof(OceanMessagingResources.XPath_DeliveryNotification_DataType), "ForwardingConsol"),
					("OceanMessaging:Inbox:0beba829-5833-42c8-a082-a3ef55ef1985:" + nameof(OceanMessagingResources.XPath_DeliveryNotification_DataKey), "CSFR24210715"),
					("OceanMessaging:Inbox:0beba829-5833-42c8-a082-a3ef55ef1985:" + nameof(OceanMessagingResources.XPath_DeliveryNotification_OperationalPort), "FRFOS"),
					("OceanMessaging:Inbox:0beba829-5833-42c8-a082-a3ef55ef1985:" + nameof(OceanMessagingResources.XPath_DeliveryNotification_TrackingID),
						"01069045-c204-4f1a-ab26-d0535131360a") },
			new[] { new[] { ("EventType", "ISN"),
					("InterchangeNum", "3444737"),
					("MessageRef", "CMA0001651847"),
					("Carrier", "CMDU"),
					("CarrierBookingRef", "LHV3515156"),
					("MasterBillNumber", "LHV3515156"),
					("DocumentName", "Shipping Instruction"),
					("Purpose", "ORG"),
					("SubmissionVersion", "1"),
					("DataType", "ForwardingConsol"),
					("DataKey", "CSFR24210715"),
					("OperationalPort", "FRFOS"),
					("TrackingID", "01069045-C204-4F1A-AB26-D0535131360A"),
					("Direction", "Request") } })
		{ TestName = nameof(Resources.Messages.MessageEventProcessedToCMACGM_SI_DeliveryNotification) },
		new(nameof(Resources.Messages.MessageEventDeliveryFailedToSHIPPING_INSTRUCTION),
			"OutboundError",
			new[] { ("OceanMessaging:Inbox:4F76A60D-5573-4B4F-803A-2988AD67A4DA:" + nameof(OceanMessagingResources.XPath_Outbound_Carrier), ""),
					("OceanMessaging:Inbox:4F76A60D-5573-4B4F-803A-2988AD67A4DA:" + nameof(OceanMessagingResources.XPath_Outbound_IsCoLoad), ""),
					("OceanMessaging:Inbox:4F76A60D-5573-4B4F-803A-2988AD67A4DA:" + nameof(OceanMessagingResources.XPath_Outbound_CarrierBookingRef), "V00166565"),
					("OceanMessaging:Inbox:4F76A60D-5573-4B4F-803A-2988AD67A4DA:" + nameof(OceanMessagingResources.XPath_Outbound_MasterBillNumber), "V00166565"),
					("OceanMessaging:Inbox:4F76A60D-5573-4B4F-803A-2988AD67A4DA:" + nameof(OceanMessagingResources.XPath_Outbound_CoLoadCarrierBookingRef), ""),
					("OceanMessaging:Inbox:4F76A60D-5573-4B4F-803A-2988AD67A4DA:" + nameof(OceanMessagingResources.XPath_Outbound_CoLoadMasterBillNumber), ""),
					("OceanMessaging:Inbox:4F76A60D-5573-4B4F-803A-2988AD67A4DA:" + nameof(OceanMessagingResources.XPath_Outbound_DocumentName), "BL Data"),
					("OceanMessaging:Inbox:4F76A60D-5573-4B4F-803A-2988AD67A4DA:" + nameof(OceanMessagingResources.XPath_Outbound_Purpose), "MAA"),
					("OceanMessaging:Inbox:4F76A60D-5573-4B4F-803A-2988AD67A4DA:" + nameof(OceanMessagingResources.XPath_Outbound_SubmissionVersion), "1"),
					("OceanMessaging:Inbox:4F76A60D-5573-4B4F-803A-2988AD67A4DA:" + nameof(OceanMessagingResources.XPath_Outbound_DataType), "BillOfLading"),
					("OceanMessaging:Inbox:4F76A60D-5573-4B4F-803A-2988AD67A4DA:" + nameof(OceanMessagingResources.XPath_Outbound_DataKey), "V00166565"),
					("OceanMessaging:Inbox:4F76A60D-5573-4B4F-803A-2988AD67A4DA:" + nameof(OceanMessagingResources.XPath_Outbound_OperationalPort), ""),
					("OceanMessaging:Inbox:4F76A60D-5573-4B4F-803A-2988AD67A4DA:" + nameof(OceanMessagingResources.XPath_Outbound_TrackingID),
						"588a862a-d9a7-46af-8765-bba330fe6a44") },
			new[] { new[] { ("CarrierBookingRef", "V00166565"),
					("MasterBillNumber", "V00166565"),
					("DocumentName", "BL Data"),
					("Purpose", "MAA"),
					("SubmissionVersion", "1"),
					("DataType", "BillOfLading"),
					("DataKey", "V00166565"),
					("TrackingID", "588A862A-D9A7-46AF-8765-BBA330FE6A44"),
					("Direction", "Request") } })
		{ TestName = nameof(Resources.Messages.MessageEventDeliveryFailedToSHIPPING_INSTRUCTION) },
		new(nameof(Resources.Messages.MessageEventDeliveredFromCARGOSMART_IFTSTA),
			"Inbound",
			new[] { ("OceanMessaging:Inbox:1FC8B19E-542C-439F-B137-75BCE9905605:" + nameof(OceanMessagingResources.XPath_Inbound_InboxEdifactMsgType), "IFTSTA"),
					("OceanMessaging:Outbox:4FDF1BFF-C866-4836-85E3-E92808757CD7:" + nameof(OceanMessagingResources.XPath_Inbound_OutboxBatchCount), "1"),
					("OceanMessaging:Outbox:4FDF1BFF-C866-4836-85E3-E92808757CD7:" + nameof(OceanMessagingResources.XPath_Inbound_OutboxMsgType) + ":1", "UniversalEvent"),
					("OceanMessaging:Outbox:4FDF1BFF-C866-4836-85E3-E92808757CD7:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_EventType) + ":1", "FUL"),
					("OceanMessaging:Outbox:4FDF1BFF-C866-4836-85E3-E92808757CD7:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_Carrier) + ":1", "OOLU"),
					("OceanMessaging:Outbox:4FDF1BFF-C866-4836-85E3-E92808757CD7:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_CarrierBookingRef) + ":1", "2155923440"),
					("OceanMessaging:Outbox:4FDF1BFF-C866-4836-85E3-E92808757CD7:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_MasterBillNumber) + ":1", "2155923440"),
					("OceanMessaging:Outbox:4FDF1BFF-C866-4836-85E3-E92808757CD7:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_CoLoadCarrierBookingRef) + ":1", ""),
					("OceanMessaging:Outbox:4FDF1BFF-C866-4836-85E3-E92808757CD7:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_CoLoadMasterBillNumber) + ":1", ""),
					("OceanMessaging:Outbox:4FDF1BFF-C866-4836-85E3-E92808757CD7:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_MessageRef) + ":1", ""),
					("OceanMessaging:Outbox:4FDF1BFF-C866-4836-85E3-E92808757CD7:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_DocumentName) + ":1", ""),
					("OceanMessaging:Outbox:4FDF1BFF-C866-4836-85E3-E92808757CD7:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_DataType) + ":1", ""),
					("OceanMessaging:Outbox:4FDF1BFF-C866-4836-85E3-E92808757CD7:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_DataKey) + ":1", "") },
			new[] { new[] { ("EventType", "FUL"),
					("MessageType", "IFTSTA"),
					("Carrier", "OOLU"),
					("CarrierBookingRef", "2155923440"),
					("MasterBillNumber", "2155923440"),
					("Direction", "Response"),
					("TrackingID", "0F6A88C7-DF8B-430C-8C03-016BC81664AD"),
					("FileName", "/out/CARGOWISE_IFTSTA_EDI2025030706105939-67.edi") } })
		{ TestName = nameof(Resources.Messages.MessageEventDeliveredFromCARGOSMART_IFTSTA) },
		new(nameof(Resources.Messages.MessageEventDeliveredFromCMACGM_IFTSTA),
			"Inbound",
			new[] { ("OceanMessaging:Inbox:55FDF7AB-540B-455C-BD80-EADC245CD87A:" + nameof(OceanMessagingResources.XPath_Inbound_InboxEdifactMsgType), "IFTSTA"),
					("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_OutboxBatchCount), "4"),
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
					("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_DataKey) + ":2", ""),
					("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_OutboxMsgType) + ":3", "UniversalEvent"),
					("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_EventType) + ":3", "DEP"),
					("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_Carrier) + ":3", "CHNL"),
					("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_CarrierBookingRef) + ":3", "AMQ0323368"),
					("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_MasterBillNumber) + ":3", "AMQ0323368"),
					("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_CoLoadCarrierBookingRef) + ":3", ""),
					("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_CoLoadMasterBillNumber) + ":3", ""),
					("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_MessageRef) + ":3", ""),
					("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_DocumentName) + ":3", ""),
					("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_DataType) + ":3", ""),
					("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_DataKey) + ":3", ""),
					("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_OutboxMsgType) + ":4", "UniversalEvent"),
					("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_EventType) + ":4", "DEP"),
					("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_Carrier) + ":4", "CHNL"),
					("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_CarrierBookingRef) + ":4", "CNB0277335"),
					("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_MasterBillNumber) + ":4", "CNB0277335"),
					("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_CoLoadCarrierBookingRef) + ":4", ""),
					("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_CoLoadMasterBillNumber) + ":4", ""),
					("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_MessageRef) + ":4", ""),
					("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_DocumentName) + ":4", ""),
					("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_DataType) + ":4", ""),
					("OceanMessaging:Outbox:C0B7D98B-4FDE-4C5A-AAEC-3FAD7B8C3AA4:" + nameof(OceanMessagingResources.XPath_Inbound_XUE_DataKey) + ":4", "") },
			new[] { new[] { ("EventType", "DEP"),
					("MessageType", "IFTSTA"),
					("Carrier", "CHNL"),
					("CarrierBookingRef", "AMP0489601"),
					("MasterBillNumber", "AMP0489601"),
					("Direction", "Response"),
					("TrackingID", "F7650201-132B-4BED-9774-EA6E32631A61"),
					("FileName", "/OUT/IFTSTA/3430886126") },
					new[] { ("EventType", "DEP"),
					("MessageType", "IFTSTA"),
					("Carrier", "CHNL"),
					("CarrierBookingRef", "AMP0490582"),
					("MasterBillNumber", "AMP0490582"),
					("Direction", "Response"),
					("TrackingID", "F7650201-132B-4BED-9774-EA6E32631A61"),
					("FileName", "/OUT/IFTSTA/3430886126") },
					new[] { ("EventType", "DEP"),
					("MessageType", "IFTSTA"),
					("Carrier", "CHNL"),
					("CarrierBookingRef", "AMQ0323368"),
					("MasterBillNumber", "AMQ0323368"),
					("Direction", "Response"),
					("TrackingID", "F7650201-132B-4BED-9774-EA6E32631A61"),
					("FileName", "/OUT/IFTSTA/3430886126") },
					new[] { ("EventType", "DEP"),
					("MessageType", "IFTSTA"),
					("Carrier", "CHNL"),
					("CarrierBookingRef", "CNB0277335"),
					("MasterBillNumber", "CNB0277335"),
					("Direction", "Response"),
					("TrackingID", "F7650201-132B-4BED-9774-EA6E32631A61"),
					("FileName", "/OUT/IFTSTA/3430886126") } })
		{ TestName = nameof(Resources.Messages.MessageEventDeliveredFromCMACGM_IFTSTA) }
	];

	[TestCaseSource(nameof(OceanMessaging_EnrichMessage_Variants_TestCaseSource))]
	public async Task OceanMessaging_EnrichMessage_Variants(string message, string criteria, (string Key, string Value)[] inputCache, (string, string)[][] expectedResults)
	{
		var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
		inputCache.ToList().ForEach(s => cache.Set(s.Key, s.Value));
		var messageJson = JsonObject.Parse(Resources.Messages.ResourceManager.GetString(message)!)!;
		var dbcontext = new Mock<eHubTransactionsContext>(new DbContextOptionsBuilder<eHubTransactionsContext>().Options).Object;
		var configurationData = new Dictionary<string, string?>
		{
			["CacheSizeLimit"] = "2000000000",
			["CacheExpirationDefaultMinutes"] = "30"
		};
		var configuration = new ConfigurationBuilder().AddInMemoryCollection(configurationData).Build();
		var logger = new FakeLogger<CacheService>(Console.WriteLine);

		var enricher = new OceanMessaging(dbcontext, new CacheService(cache, configuration, logger));
		var result = (await enricher.EnrichMessageBatchAsync((JsonObject)messageJson, criteria)).ToList();

		Assert.That(result.Count, Is.EqualTo(expectedResults.Length));
		for (int i = 0; i < result.Count; i++)
		{
			Assert.That(result[i].Select(r => (r.Key, r.Value?.ToString())),
				Is.Not.Null.And.EquivalentTo(expectedResults[i]));
		}
	}
}
