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
using Rule = eServices.eHubRoutingRuleEngine.Rule;
using eServices.eHubDataAccess.Integration;

namespace CargoWise.eHub.Gateway.Tests.InboxMessageHandler.Routing
{
	[TestClass]
	public class RoutingRuleServiceEvaluateTests_FPM
	{
		private RoutingRuleService routingRuleService;
		private eHubClient FORWARDING_PORT_MESSAGEClient;
		private eHubTransactionsContext MockContext;
		private ILog MockLogger;
		private IRoutingRuleFactory MockRoutingRuleFactory;
		private IPartyAccessor MockPartyAccessor;

		#region setup

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
					{
						new RoutingClientConfigurationItem()
						{
							ClientId = "FORWARDING_PORT_MESSAGE",
							RouteId = "FORWARDING_PORT_MESSAGE",
							RoutingMethod = RoutingMethod.Standard
						}
					}
				}
			);
			MockPartyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			routingRuleService = new RoutingRuleService(() => MockContext, MockLogger, (_) => MockRoutingRuleFactory,
				() => MockPartyAccessor, routingClientsConfig);

			FORWARDING_PORT_MESSAGEClient = new eHubClient
			{
				CC_ID = "FORWARDING_PORT_MESSAGE",
				eHubRoutingRule = new eHubRoutingRule
				{
					RR_Failed_ErrorCode = "IRJ",
					RR_Failed_ErrorDescription = "Rejected",
					RR_Group_MatchMultiple = false,
					eHubRoutingRules_Group = new List<eHubRoutingRule>
					{
						new eHubRoutingRule
						{
							RR_Condition_Expression =
								"[@UShipmentNamespace,Equal,http://www.cargowise.com/Schemas/Universal/2012/11/SecureContainerRelease/1]",
							eHubClient_Recipient = new eHubClient {CC_ID = "TMINING_XT"}
						}
					},
					eHubRoutingRuleFacts = new List<eHubRoutingRuleFact>
					{
						new eHubRoutingRuleFact
						{
							RX_Name = "UShipmentNamespace",
							RX_Type = "XPATH",
							RX_Query =
								"/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/namespace::*[name()='']"
						},
					}
				}
			};

			var forwardingPortMessageForReading = Rule.GetForReading(FORWARDING_PORT_MESSAGEClient);
			MockRoutingRuleFactory.Stub(x => x.GetForReading("FORWARDING_PORT_MESSAGE")).Return(forwardingPortMessageForReading);
		}

		public eHubGatewayMessage GetMessageFromFile(string fileName, string clientId, string applicationCode)
		{
			var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames()
				.Single(str => str.EndsWith(fileName));

			using (var stream = GetType().Assembly.GetManifestResourceStream(resourceName))
			{
				if (stream == null)
					throw new InvalidOperationException();

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

		#endregion

		[TestMethod]
		public void TestEvaluateFPM_TMINING()
		{
			var propertyFacts = new Dictionary<string, string> { { "SourceParty", "HYEDAUUAT" } };
			var msg = GetMessageFromFile("FPM_XTMessage.txt", "FORWARDING_PORT_MESSAGE", "TST");

			var results = routingRuleService.Evaluate(MockContext, MockRoutingRuleFactory, "FORWARDING_PORT_MESSAGE", propertyFacts, msg);

			StringAssert.Contains("TMINING_XT", results[0].RecipientId);
		}

		[TestMethod]
		public void TestIsXHMessage_ExpectTrue()
		{
			var msg = GetMessageFromFile("FPM_XTMessage.txt", "FORWARDING_PORT_MESSAGE", "TST");

			MockPartyAccessor.Expect(x => x.IsXHSystem("TMINING_XT")).Return(true);

			var results = routingRuleService.IsXHMessage(msg, "HYEDAUUAT", out var resolvedRecipient);

			MockPartyAccessor.VerifyAllExpectations();
			Assert.AreEqual(results, true);
		}

		[TestMethod]
		public void TestIsXHMessage_ExpectFalse()
		{
			var msg = GetMessageFromFile("FPM_XTMessage.txt", "FORWARDING_PORT_MESSAGE", "TST");

			MockPartyAccessor.Expect(x => x.IsXHSystem("TMINING_XT")).Return(false);

			var results = routingRuleService.IsXHMessage(msg, "HYEDAUUAT", out var resolvedRecipient);

			MockPartyAccessor.VerifyAllExpectations();
			Assert.AreEqual(results, false);
		}
	}
}

