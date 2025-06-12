using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Gateway.Routing;
using eServices.eHubDataModel.eHubTransactions;
using eServices.eHubRoutingRuleEngine;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using eServices.eHubDataAccess.Integration;

namespace CargoWise.eHub.Gateway.Tests.InboxMessageHandler.Routing
{
	[TestClass]
	public class RoutingRuleServiceEvaluateTests_OCM
	{
		private RoutingRuleService routingRuleService;
		private eHubClient OCM_MutipleRecipientsCopyingClient;
		private eHubClient SHIPPING_INSTRUCTIONClient;
		private eHubTransactionsContext MockContext;
		private ILog MockLogger;
		private IRoutingRuleFactory MockRoutingRuleFactory;
		private IPartyAccessor MockPartyAccessor;

		[TestInitialize]
		public void SetUp()
		{
			MockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			MockLogger = MockRepository.GenerateMock<ILog>();
			MockRoutingRuleFactory = MockRepository.GenerateMock<IRoutingRuleFactory>();
			var routingClientsConfig = new RoutingClientsConfiguration(
				enabled: true,
				new List<RoutingClientConfigurationItem>()
				{
					{ new RoutingClientConfigurationItem(){ ClientId = "SHIPPING_INSTRUCTION" , RouteId = "SHIPPING_INSTRUCTION" , RoutingMethod =  RoutingMethod.OCM } }
				}
				);
			MockPartyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			routingRuleService = new RoutingRuleService(() => MockContext, MockLogger, (_) => MockRoutingRuleFactory, () => MockPartyAccessor, routingClientsConfig);

			OCM_MutipleRecipientsCopyingClient = new eHubClient();
			OCM_MutipleRecipientsCopyingClient.CC_ID = "OCM_MutipleRecipientsCopying";
			OCM_MutipleRecipientsCopyingClient.eHubRoutingRule = new eHubRoutingRule();
			OCM_MutipleRecipientsCopyingClient.eHubRoutingRule.RR_Group_MatchMultiple = false;
			OCM_MutipleRecipientsCopyingClient.eHubRoutingRule.eHubRoutingRules_Group = new List<eHubRoutingRule>();

			var eHubRoutingRule1 = new eHubRoutingRule();
			eHubRoutingRule1.RR_Condition_Expression = "[@SourceParty,IsMatch,^DFO.*$] && [@SCACUniShip,IsMatch,^(ONEY|HLCU)$] && [@Port,IsMatch,^(CNNGB|CNNBO|CNNBG)$]";
			eHubRoutingRule1.RR_Result_Value = "CarrierBookingAgent";
			OCM_MutipleRecipientsCopyingClient.eHubRoutingRule.eHubRoutingRules_Group.Add(eHubRoutingRule1);
			OCM_MutipleRecipientsCopyingClient.eHubRoutingRule.eHubRoutingRuleFacts = new List<eHubRoutingRuleFact>();

			var eHubRoutingRule2 = new eHubRoutingRule();
			eHubRoutingRule2.RR_Result_Value = "@CarrierBookingOffice";
			eHubRoutingRule2.eHubRoutingRule_Computes = new List<eHubRoutingRuleFact>();

			var routingRule = new eHubRoutingRule();
			routingRule.RR_Condition_Expression = "[@OperationalPortCode,NotEqual,]";
			routingRule.eHubRoutingRule_Group = eHubRoutingRule2;
			routingRule.RR_Result_Value = "@OperationalPortCode";
			routingRule.RR_Group_Ordering = 1000;

			var computedFact = new eHubRoutingRuleFact();
			computedFact.RX_Name = "Port";
			computedFact.RX_Type = "COMPUTED";
			computedFact.eHubRoutingRule_Compute = routingRule;
			OCM_MutipleRecipientsCopyingClient.eHubRoutingRule.eHubRoutingRuleFacts.Add(computedFact);

			var fact1 = new eHubRoutingRuleFact();
			fact1.RX_Name = "OperationalPortCode";
			fact1.RX_Type = "XPATH";
			fact1.RX_Query = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][*[local-name()='Key']/text()='OperationalPort_Code']/*[local-name()='Value']";
			OCM_MutipleRecipientsCopyingClient.eHubRoutingRule.eHubRoutingRuleFacts.Add(fact1);

			var fact2 = new eHubRoutingRuleFact();
			fact2.RX_Name = "CarrierBookingOffice";
			fact2.RX_Type = "XPATH";
			fact2.RX_Query = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='CarrierBookingOffice']";
			OCM_MutipleRecipientsCopyingClient.eHubRoutingRule.eHubRoutingRuleFacts.Add(fact2);

			var fact3 = new eHubRoutingRuleFact();
			fact3.RX_Name = "SCACUniShip";
			fact3.RX_Type = "XPATHNAVFUNC";
			fact3.RX_Query = "translate(/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][*[local-name()='AddressType']/text()='ShippingLineAddress']/*[local-name()='RegistrationNumberCollection']/*[local-name()='RegistrationNumber'][*[local-name()='Type']/text()='CCC' and *[local-name()='CountryOfIssue']/text()='US']/*[local-name()='Value'], 'abcdefghijklmnopqrstuvwxyz', 'ABCDEFGHIJKLMNOPQRSTUVWXYZ')";
			OCM_MutipleRecipientsCopyingClient.eHubRoutingRule.eHubRoutingRuleFacts.Add(fact3);

			var fact4 = new eHubRoutingRuleFact();
			fact4.RX_Name = "SourceParty";
			fact4.RX_Type = "PROPERTY";
			fact4.RX_Query = "http://schemas.microsoft.com/BizTalk/2003/system-properties#SourceParty";
			OCM_MutipleRecipientsCopyingClient.eHubRoutingRule.eHubRoutingRuleFacts.Add(fact4);

			SHIPPING_INSTRUCTIONClient = new eHubClient();
			SHIPPING_INSTRUCTIONClient.CC_ID = "SHIPPING_INSTRUCTION";
			SHIPPING_INSTRUCTIONClient.eHubRoutingRule = new eHubRoutingRule();
			SHIPPING_INSTRUCTIONClient.eHubRoutingRule.RR_Failed_ErrorCode = "IRJ";
			SHIPPING_INSTRUCTIONClient.eHubRoutingRule.RR_Failed_ErrorDescription = "Rejected";
			SHIPPING_INSTRUCTIONClient.eHubRoutingRule.RR_Group_MatchMultiple = false;
			SHIPPING_INSTRUCTIONClient.eHubRoutingRule.eHubRoutingRules_Group = new List<eHubRoutingRule>();

			var eHubRoutingRule3 = new eHubRoutingRule();
			eHubRoutingRule3.RR_Condition_Expression = "[@CarrierHandlingAgent,Equal,C1H1]";
			eHubRoutingRule3.eHubClient_Recipient = new eHubClient {CC_ID = "HANDLING_AGENT"};
			SHIPPING_INSTRUCTIONClient.eHubRoutingRule.eHubRoutingRules_Group.Add(eHubRoutingRule3);

			var eHubRoutingRule4 = new eHubRoutingRule();
			eHubRoutingRule4.RR_Condition_Expression = "[@CarrierHandlingAgent,Equal,C1HA]";
			eHubRoutingRule4.eHubClient_Recipient = new eHubClient {CC_ID = "HANDLING_AGENT_1"};
			SHIPPING_INSTRUCTIONClient.eHubRoutingRule.eHubRoutingRules_Group.Add(eHubRoutingRule4);

			var eHubRoutingRule5 = new eHubRoutingRule();
			eHubRoutingRule5.RR_Condition_Expression = "[@CarrierBookingAgent,Equal,C1BA]";
			eHubRoutingRule5.eHubClient_Recipient = new eHubClient {CC_ID = "BOOKING_AGENT"};
			SHIPPING_INSTRUCTIONClient.eHubRoutingRule.eHubRoutingRules_Group.Add(eHubRoutingRule5);

			var eHubRoutingRule6 = new eHubRoutingRule();
			eHubRoutingRule6.RR_Condition_Expression = "[@CarrierBookingAgent,Equal,C1B1] && ([@CarrierAgent,Equal,DefaultCarrier] || [@CarrierAgent,Equal,])";
			eHubRoutingRule6.eHubClient_Recipient = new eHubClient {CC_ID = "BOOKING_AGENT_1"};
			SHIPPING_INSTRUCTIONClient.eHubRoutingRule.eHubRoutingRules_Group.Add(eHubRoutingRule6);

			var eHubRoutingRule7 = new eHubRoutingRule();
			eHubRoutingRule7.RR_Condition_Expression = "[@CarrierBookingAgent,Equal,C1B1] && [@CarrierAgent,Equal,CarrierBookingAgent]";
			eHubRoutingRule7.eHubClient_Recipient = new eHubClient {CC_ID = "BOOKING_AGENT_2"};
			SHIPPING_INSTRUCTIONClient.eHubRoutingRule.eHubRoutingRules_Group.Add(eHubRoutingRule7);
			SHIPPING_INSTRUCTIONClient.eHubRoutingRule.eHubRoutingRuleFacts = new List<eHubRoutingRuleFact>();

			var fact5 = new eHubRoutingRuleFact();
			fact5.RX_Name = "CarrierHandlingAgent";
			fact5.RX_Type = "XPATH";
			fact5.RX_Query = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][*[local-name()='AddressType']/text()='CarrierHandlingAgent']/*[local-name()='RegistrationNumberCollection']/*[local-name()='RegistrationNumber'][*[local-name()='Type']/text()='C1C']/*[local-name()='Value']";
			SHIPPING_INSTRUCTIONClient.eHubRoutingRule.eHubRoutingRuleFacts.Add(fact5);

			var fact6 = new eHubRoutingRuleFact();
			fact6.RX_Name = "CarrierBookingAgent";
			fact6.RX_Type = "XPATH";
			fact6.RX_Query = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][*[local-name()='AddressType']/text()='CarrierBookingAgent']/*[local-name()='RegistrationNumberCollection']/*[local-name()='RegistrationNumber'][*[local-name()='Type']/text()='C1C']/*[local-name()='Value']";
			SHIPPING_INSTRUCTIONClient.eHubRoutingRule.eHubRoutingRuleFacts.Add(fact6);

			var fact7 = new eHubRoutingRuleFact();
			fact7.RX_Name = "CarrierAgent";
			fact7.RX_Type = "INJECTED";
			SHIPPING_INSTRUCTIONClient.eHubRoutingRule.eHubRoutingRuleFacts.Add(fact7);

			var ocmMultipleRecipientsCopyingRule = Rule.GetForReading(OCM_MutipleRecipientsCopyingClient);
			var shippingInstructionRule = Rule.GetForReading(SHIPPING_INSTRUCTIONClient);

			MockRoutingRuleFactory.Stub(x => x.GetForReading("OCM_MutipleRecipientsCopying")).Return(ocmMultipleRecipientsCopyingRule);
			MockRoutingRuleFactory.Stub(x => x.GetForReading("SHIPPING_INSTRUCTION")).Return(shippingInstructionRule);
		}

		[TestMethod]
		public void TestEvaluateOCM_MutipleRecipientsCopying()
		{
			var propertyFacts = new Dictionary<string, string> { { "SourceParty", "DFODAUUAT" } };
			var msg = GetMessageFromFile("SqlMessage_MutipleRecipientsCopying.txt", "SHIPPING_INSTRUCTION", "TST");

			var results = routingRuleService.EvaluateOCM(MockContext, MockRoutingRuleFactory, "SHIPPING_INSTRUCTION", propertyFacts, msg);

			StringAssert.Contains("BOOKING_AGENT_1", results[0].RecipientId);
			StringAssert.Contains("BOOKING_AGENT_2", results[1].RecipientId);
		}

		[TestMethod]
		public void TestEvaluateOCM_ShippingOrder()
		{
			var propertyFacts = new Dictionary<string, string> { { "SourceParty", "HYEDAUUAT" } };
			var msg = GetMessageFromFile("SqlMessage_ShippingOrder.txt", "SHIPPING_INSTRUCTION", "TST");

			var results = routingRuleService.EvaluateOCM(MockContext, MockRoutingRuleFactory, "SHIPPING_INSTRUCTION", propertyFacts, msg);

			StringAssert.Contains("BOOKING_AGENT_1", results[0].RecipientId);
		}

		[TestMethod]
		public void TestEvaluateOCM_eManifest()
		{
			var propertyFacts = new Dictionary<string, string> { { "SourceParty", "HYEDAUUAT" } };
			var msg = GetMessageFromFile("SqlMessage_eManifest.txt", "SHIPPING_INSTRUCTION", "TST");

			var results = routingRuleService.EvaluateOCM(MockContext, MockRoutingRuleFactory, "SHIPPING_INSTRUCTION", propertyFacts, msg);

			StringAssert.Contains("HANDLING_AGENT_1", results[0].RecipientId);
		}

		[TestMethod]
		public void TestEvaluateOCM_VGM_Handling()
		{
			var propertyFacts = new Dictionary<string, string> { { "SourceParty", "HYEDAUUAT" } };
			var msg = GetMessageFromFile("SqlMessage_VGM_Handling.txt", "SHIPPING_INSTRUCTION", "TST");

			var results = routingRuleService.EvaluateOCM(MockContext, MockRoutingRuleFactory, "SHIPPING_INSTRUCTION", propertyFacts, msg);

			StringAssert.Contains("HANDLING_AGENT", results[0].RecipientId);
		}

		[TestMethod]
		public void TestEvaluateOCM_VGM_Booking()
		{
			var propertyFacts = new Dictionary<string, string> { { "SourceParty", "HYEDAUUAT" } };
			var msg = GetMessageFromFile("SqlMessage_VGM_Booking.txt", "SHIPPING_INSTRUCTION", "TST");

			var results = routingRuleService.EvaluateOCM(MockContext, MockRoutingRuleFactory, "SHIPPING_INSTRUCTION", propertyFacts, msg);

			StringAssert.Contains("BOOKING_AGENT", results[0].RecipientId);
		}

		[TestMethod]
		public void TestIsXHMessage_ExpectTrue()
		{
			var msg = GetMessageFromFile("SqlMessage_VGM_Booking.txt", "SHIPPING_INSTRUCTION", "TST");

			MockPartyAccessor.Expect(x => x.IsXHSystem("BOOKING_AGENT")).Return(true);

			var results = routingRuleService.IsXHMessage(msg, "HYEDAUUAT", out var resolvedRecipient);

			MockPartyAccessor.VerifyAllExpectations();
			Assert.AreEqual(results, true);
		}

		[TestMethod]
		public void TestIsXHMessage_ExpectFalse()
		{
			var msg = GetMessageFromFile("SqlMessage_VGM_Booking.txt", "SHIPPING_INSTRUCTION", "TST");
			MockPartyAccessor.Expect(x => x.IsXHSystem("BOOKING_AGENT")).Return(false);

			var results = routingRuleService.IsXHMessage(msg, "HYEDAUUAT", out var resolvedRecipient);

			MockPartyAccessor.VerifyAllExpectations();
			Assert.AreEqual(results, false);
		}

		public eHubGatewayMessage GetMessageFromFile(string fileName, string clientId, string applicationCode)
		{
			var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames()
				.Single(str => str.EndsWith(fileName));

			using (var stream = GetType().Assembly.GetManifestResourceStream(resourceName))
			{
				if (stream == null) throw new InvalidOperationException();

				return new eHubGatewayMessage
				{
					ApplicationCode = applicationCode,
					ClientID = clientId,
					EmailSubject = "EMAIL SUBJECT",
					FileName = "FILE.NAME",
					MessageStream = stream.CompressAndEncode()
				};
			}
		}
	}
}
