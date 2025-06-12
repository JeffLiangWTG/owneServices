using System;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using eServices.eHubDataAccess.Integration;
using CargoWise.eHub.Gateway.Routing;
using CargoWise.eHub.Integration;
using Common.Logging;

namespace CargoWise.eHub.Gateway
{
	public class MessageHandlerFactory
	{
		internal static IRoutingRuleService routingRuleService = new RoutingRuleService();
		static readonly ILog log = LogManager.GetLogger(typeof(MessageHandlerFactory));
		public static Func<IPartyAccessor> GetPartyAccessor = DataAccessFactories.NewPartyAccessorInstance;

		public static MessageHandler CreateMessageHandler(eHubGatewayMessage message)
		{
			return CreateMessageHandler("", message);
		}

		public static MessageHandler CreateMessageHandler(string senderID, eHubGatewayMessage message)
		{
			if (routingRuleService.IsXHMessage(message, senderID, out var resolvedRecipient))
			{
				return new XHMessageHandler()
				{
					ResolvedRecipient = resolvedRecipient
				};
			}

			switch (message.ApplicationCode)
			{
				case ApplicationCode.AgentScavenging:
					switch (message.SchemaName)
					{
						case "http://www.edi.com.au/EnterpriseService/#Billing_1.0":
							return new BillingMessageWithBlacklistingHandler();
						case "http://www.edi.com.au/EnterpriseService/#Billing_1.1":
							return new BillingMessageHandlerV1();
						case "http://www.edi.com.au/EnterpriseService/#Billing_1.2":
							return new BillingMessageHandlerV2();
						case "http://www.edi.com.au/EnterpriseService/#Billing_1.3":
							return new BillingMessageHandlerV3();
						case "http://www.edi.com.au/EnterpriseService/#Billing_1.4":
							return new BillingMessageHandlerV4();
						case "http://www.edi.com.au/EnterpriseService/#Usage_1.1":
							return new UsageMessageHandlerV1();
						case "http://www.edi.com.au/EnterpriseService/#Usage_2.0":
							return new UsageMessageHandlerV2();
						case "http://www.edi.com.au/EnterpriseService/#Usage_2.1":
							return new UsageMessageHandlerV2_1();
						default:
							if (log.IsWarnEnabled) log.WarnFormat("Unknown SchemaName '{0}' for ApplicationCode '{1}' Client ID'{2}' MessageTrackingID '{3}' Content '{4}'.\r\nThis message is to be abandoned.", message.SchemaName, message.ApplicationCode, message.ClientID, message.MessageTrackingID, message.MessageStream.ReadToEnd());
							return new InvalidMessageHandler();
					}
				case ApplicationCode.CIM:
				case ApplicationCode.SGCustomsCMD:
					return new AirMessageHandler();
				case ApplicationCode.AUCustoms:
					return new AUCustomsMessageHandler();
				case ApplicationCode.NZCustoms:
				case ApplicationCode.NZCustomsEBACCA:
					return message.ClientID == "NZCustomsTest" ? new NZCustomsTestMessageHandler() : new NZCustomsMessageHandler();
				case ApplicationCode.USImport:
				case ApplicationCode.USExport:
				case ApplicationCode.AMS:
				case ApplicationCode.AMA:
				case ApplicationCode.USeManifest:
				case ApplicationCode.USVesselStow:
				case ApplicationCode.USExportManifest:
					return new USCustomsInboxMessageHandler();
				case ApplicationCode.HKCustoms:
					return new LicenceCheckInboxOutboxMessageHandler();
				case ApplicationCode.SYS:
				case ApplicationCode.GenericMessageDelivery:
				case ApplicationCode.AUCustomsNEXDOC:
					return new InboxOutboxMessageHandler();
				case ApplicationCode.Telematics:
					if (GetPartyAccessor().IsXHSystem(message.ClientID)) { return new XHMessageHandler(); }
					return new InboxOutboxMessageHandler();
				case ApplicationCode.ZACustoms:
					return new ZACustomsMessageHandler();
				case ApplicationCode.UYCustoms:
					return new XHMessageHandler();
				case ApplicationCode.CustomsWare:
					return new CustomsWareMessageHandler();
				default:
					if ((message.ClientID == "SGCustoms")
						|| (message.ClientID == "USAirAMS")
						|| (message.ClientID == "NEXDOCS")
						|| (message.ClientID == "CACustoms" && message.ApplicationCode == "UDM")
						|| (message.ClientID == "GBCustoms")
						|| (message.ClientID == "GLB_ELEC_INVOICING")
						|| (message.ClientID == "USCustomsEBond" && message.ApplicationCode == "UXB")
						|| message.ApplicationCode == ApplicationCode.USDocumentImagingSystem)
					{
						return new LicenceCheckInboxMessageHandler();
					}
					else if ((message.ClientID == "ITCustoms")
						|| (message.ClientID == "CACustomsDoc" && message.ApplicationCode == "UDM"))
					{
						return new LicenceCheckInboxOutboxMessageHandler();
					}
					if (message.ClientID == "JPCustoms")
					{
						return new JPCustomsInboxMessageHandler();
					}
					if (message.SchemaName == "http://www.wisetechglobal.com/Schemas/Configuration#Configuration")
					{
						if (GetPartyAccessor().IsXHSystem(message.ClientID))
						{
							return new XHMessageHandler();
						}
						return new ConfigurationMessageHandler(message);
					}
					if (message.SchemaName == "http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate")
					{
						return new eHubRegistryUpdateHandler();
					}
					if (message.ClientID == "ZACustoms")
					{
						return new LicenceCheckInboxMessageHandler("Doc");
					}
					if (message.SchemaName == "http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse" && message.ClientID == "eHub")
					{
						return new ITCustomsRequestResponseMessageHandler();
					}
					if (senderID == "FLIGHT_MONITORING_SYSTEM" || senderID == "AIR_BOOKING_ENGINE")
					{
						return new InboxOutboxMessageHandler();
					}
					if (message.ClientID == "FLIGHT_MONITORING_SYSTEM" || message.ClientID == "SCHEDULE_FEED_SERVICE")
					{
						return new InboxOutboxMessageHandler();
					}
					if (senderID == "CONTAINER_TRANSPORT_OPTIMIZATION")
					{
						return new InboxOutboxMessageHandler();
					}
					if ((senderID == "OCM_BookingEngine" || message.ClientID == "OCM_BookingEngine") && message.ApplicationCode == ApplicationCode.OCMBeDirect)
					{
						return new InboxOutboxMessageHandler();
					}
					if (message.SchemaName == "http://cargowise.com/ehub/products/TWCPluginRequest#TWCPluginServiceSendRequest"
					  || (senderID == "CONTAINER_TRACKING" && GetPartyAccessor().IsCW1System(message.ClientID)))
					{
						return new InboxOutboxMessageHandler();
					}
					if (message.ApplicationCode == ApplicationCode.XMS && message.SchemaName != "http://www.edi.com.au/EnterpriseService/#Orders"
						&& GetPartyAccessor().IsCW1System(senderID) && GetPartyAccessor().IsCW1System(message.ClientID))
					{
						return new LegacyXmlE2EMessageHandler();
					}
					if (GetPartyAccessor().IsXHSystem(message.ClientID))
					{
						return new XHMessageHandler();
					}
					if (GetPartyAccessor().IsXHubSystem(message.ClientID))
					{
						return new XHubMessageHandler();
					}
					if (message.ApplicationCode == ApplicationCode.NDM && GetPartyAccessor().IsCW1System(senderID) && GetPartyAccessor().IsCW1System(message.ClientID))
					{
						return new InboxOutboxMessageHandler();
					}
					else
					{
						return new DefaultInboxMessageHandler();
					}
			}
		}
	}
}
