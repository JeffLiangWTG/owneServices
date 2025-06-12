using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Gateway.Routing;
using eServices.eHubDataModel.Common;
using eServices.eHubDataModel.eHubTransactions;
using eServices.eHubRoutingRuleEngine;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using eServices.eHubDataAccess.Integration;

namespace CargoWise.eHub.Gateway.Tests.InboxMessageHandler.Routing
{
	[TestClass]
	public class RoutingRuleServiceEvaluateTests
	{
		private RoutingRuleService routingRuleService;
		private IPartyAccessor MockPartyAccessor;
		private eHubTransactionsContext mockContext;
		private ILog mockLogger;
		private IRoutingRuleFactory ruleFactory;

		[TestInitialize]
		public void SetUp()
		{
			mockContext = GetMockContext();
			mockLogger = MockRepository.GenerateMock<ILog>();

			var routingClientsConfig = new RoutingClientsConfiguration(
				enabled: true,
				new List<RoutingClientConfigurationItem>()
					{
						{ new RoutingClientConfigurationItem(){ ClientId = "GLB_ELEC_INVOICING" , RouteId = "GLB_ELEC_INVOICING" , RoutingMethod =  RoutingMethod.Standard } }
					});

			MockPartyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			ruleFactory = new RoutingRuleFactory(mockContext, mockLogger);
			routingRuleService = new RoutingRuleService(() => mockContext, mockLogger, (_) => ruleFactory, () => MockPartyAccessor, routingClientsConfig);
		}

		[TestMethod]
		public void TestEvaluate_InjectedAndMessageFactResolversResolved_RecipientIsGEITaiwan()
		{
			var propertyFacts = new Dictionary<string, string> { { "SourceParty", "WTLDTWAY2" } };
			var msg = GetMessageFromFile("GLB_ELEC_INVOICING_Taiwan.txt", "WTLDTWAY2", "TST");

			var results = routingRuleService.Evaluate(mockContext, ruleFactory, "GLB_ELEC_INVOICING", propertyFacts, msg);

			StringAssert.Contains("GEI_TAIWAN", results[0].RecipientId);
		}

		[TestMethod]
		public void TestEvaluate_InjectedAndMessageFactResolversResolved_RecipientIsGEIFiji()
		{
			var propertyFacts = new Dictionary<string, string> { { "SourceParty", "WTLDTWAY2" } };
			var msg = GetMessageFromFile("GLB_ELEC_INVOICING_Fiji.txt", "WTLDTWAY2", "TST");

			var results = routingRuleService.Evaluate(mockContext, ruleFactory, "GLB_ELEC_INVOICING", propertyFacts, msg);

			StringAssert.Contains("GEI_FIJI", results[0].RecipientId);
		}

		[TestMethod]
		public void TestEvaluate_NoRoutingRuleFound_ReturnErrorCodeAndDescription()
		{
			var propertyFacts = new Dictionary<string, string> { { "SourceParty", "WTLDTWAY2" } };
			var msg = GetMessageFromFile("GLB_ELEC_INVOICING_NoMessagingSystem.txt", "WTLDTWAY2", "TST");

			var results = routingRuleService.Evaluate(mockContext, ruleFactory, "GLB_ELEC_INVOICING", propertyFacts, msg);

			StringAssert.Contains("IRJ", results[0].ErrorCode);
			StringAssert.Contains("You are not registered with this service. Please contact WTG to register.", results[0].ErrorDescription);
		}

		[TestMethod]
		public void TestEvaluate_MessageFactResolverWithPropertyMessageType_RecipientWTLDTWAY2()
		{
			var propertyFacts = new Dictionary<string, string> { { "SourceParty", "WTLDTWAY2" } };
			var msg = GetMessageFromFile("GLB_ELEC_INVOICING_PropertyMessageTypeTest.txt", "WTLDTWAY2", "TST");

			var results = routingRuleService.Evaluate(mockContext, ruleFactory, "GLB_ELEC_INVOICING", propertyFacts, msg);

			StringAssert.Contains("WTLDTWAY2", results[0].RecipientId);
		}


		[TestMethod]
		public void TestIsXHMessage_ExpectTrue()
		{
			var msg = GetMessageFromFile("GLB_ELEC_INVOICING_PropertyMessageTypeTest.txt", "GLB_ELEC_INVOICING", "TST");

			MockPartyAccessor.Expect(x => x.IsXHSystem("WTLDTWAY2")).Return(true);

			var results = routingRuleService.IsXHMessage(msg, "HYEANYANY", out var resolvedRecipient);

			MockPartyAccessor.VerifyAllExpectations();
			Assert.AreEqual(results, true);
		}

		[TestMethod]
		public void TestIsXHMessage_ExpectFalse()
		{
			var msg = GetMessageFromFile("GLB_ELEC_INVOICING_Fiji.txt", "GLB_ELEC_INVOICING", "TST");

			MockPartyAccessor.Expect(x => x.IsXHSystem("GEI_FIJI")).Return(false);

			var results = routingRuleService.IsXHMessage(msg, "HYEANYANY", out var resolvedRecipient);

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

		public eHubTransactionsContext GetMockContext()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var testRules = new TestDbSet<eHubRoutingRule>();
			var testFacts = new TestDbSet<eHubRoutingRuleFact>();
			var testClients = new TestDbSet<eHubClient>();
			mockContext.Stub(x => x.eHubRoutingRules).Return(testRules);
			mockContext.Stub(x => x.eHubRoutingRuleFacts).Return(testFacts);
			mockContext.Stub(x => x.eHubClients).Return(testClients);
			mockContext.Stub(x => x.SqlQuery<DateTime>(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything)).Return(new[] { DateTime.UtcNow });

			var geiTaiwanClient = new eHubClient { CC_ID = "GEI_TAIWAN", CC_FriendlyName = "Global Electronic Invoicing - Taiwan", CC_OwnerCategory = "Service Provider", CC_SystemCategory = "Third Party" };
			var geiFijiClient = new eHubClient { CC_ID = "GEI_FIJI", CC_FriendlyName = "Global Electronic Invoicing - Fiji", CC_OwnerCategory = "Service Provider", CC_SystemCategory = "Third Party" };
			var wiseTechTestClient = new eHubClient { CC_ID = "WTLDTWAY2", CC_FriendlyName = "WiseTech Test (Internal) Licenses", CC_OwnerCategory = "Client", CC_SystemCategory = "Enterprise" };

			var globalElectronicInvoicingService = new eHubClient
			{
				CC_ID = "GLB_ELEC_INVOICING",
				CC_RR = Guid.NewGuid(),
				eHubRoutingRule = testRules.Add(new eHubRoutingRule
				{
					RR_Group_MatchMultiple = false,
					RR_LastUpdateUTC = DateTime.UtcNow,
					eHubRoutingRules_Group = testRules.AddRange(new[]
					{
						new eHubRoutingRule
						{
							RR_Condition_Expression = "[@MessagingSystem,Equal,PropertyMessageTypeTest] && [@MessageType,Equal,http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing#GlobalElectronicInvoicing]",
							eHubClient_Recipient = wiseTechTestClient,
							RR_Group_Ordering = 300
						},
						new eHubRoutingRule
						{
							RR_Condition_Expression = "[@MessagingSystem,Equal,Taiwan electronic invoicing system]",
							eHubClient_Recipient = geiTaiwanClient,
							RR_Group_Ordering = 200
						},
						new eHubRoutingRule
						{
							RR_Condition_Expression = "[@MessagingSystem,Equal,Fiji electronic invoicing system]",
							eHubClient_Recipient = geiFijiClient,
							RR_Group_Ordering = 100
						}
					}).ToList(),
					eHubRoutingRuleFacts = new List<eHubRoutingRuleFact>
					{
						testFacts.Add(new eHubRoutingRuleFact{ RX_Name = "MessagingSystem", RX_Type = "XPATH", RX_Query = "/*[local-name()='GlobalElectronicInvoicing']/*[local-name()='Header']/*[local-name()='ElectronicInvoiceBatchRequest']/*[local-name()='MessagingSystem']" }),
						testFacts.Add(new eHubRoutingRuleFact{ RX_Name = "SourceParty", RX_Type = "PROPERTY", RX_Query = "http://schemas.microsoft.com/BizTalk/2003/system-properties#SourceParty" }),
						testFacts.Add(new eHubRoutingRuleFact{ RX_Name = "MessageType", RX_Type = "PROPERTY", RX_Query = "http://schemas.microsoft.com/BizTalk/2003/system-properties#MessageType" })
					},
					RR_Failed_ErrorCode = "IRJ",
					RR_Failed_ErrorDescription = "You are not registered with this service. Please contact WTG to register.",
					RR_Group_Ordering = 1000
				})
			};

			testClients.AddRange(new[] { geiTaiwanClient, geiFijiClient, globalElectronicInvoicingService, wiseTechTestClient });

			return mockContext;
		}
	}
}
