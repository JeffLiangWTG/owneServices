using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using eServices.eHubDataAccess.Integration;
using CargoWise.eHub.Gateway.Routing;
using eServices.eHubDataModel.eHubTransactions;
using eServices.eHubRoutingRuleEngine;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CargoWise.eHub.Gateway.Tests.InboxMessageHandler.Routing
{
	[TestClass]
	public class RoutingRuleServiceTests
	{
		[TestMethod]
		public void Test_IsXHMessage_NoResult_ReturnFalse()
		{

			var mockContext = new Mock<eHubTransactionsContext>();
			var mockLogger = new Mock<ILog>();

			var routingClientsConfig = new RoutingClientsConfiguration(
				enabled: true,
				new List<RoutingClientConfigurationItem>()
					{
						{ new RoutingClientConfigurationItem(){ ClientId = "TSTCLIENT" , RouteId = "TSTCLIENT" , RoutingMethod =  RoutingMethod.Standard } }
					});

			var MockPartyAccessor = new Mock<IPartyAccessor>();
			var mockRuleFactory = new Mock<IRoutingRuleFactory>();

			var mockRule = new Mock<IRule>();
			mockRule.Setup(x => x.Evaluate(It.IsAny<eHubTransactionsContext>(), It.IsAny<IFactResolver[]>(), It.IsAny<ILog>()))
				.Returns(new Collection<Result>());

			mockRuleFactory.Setup(x => x.GetForReading("TSTCLIENT")).Returns(mockRule.Object);

			var routingRuleService = new RoutingRuleService(() => mockContext.Object, mockLogger.Object, (_) => mockRuleFactory.Object, () => MockPartyAccessor.Object, routingClientsConfig);

			var message = CreateTestMessage();

			Assert.IsFalse(routingRuleService.IsXHMessage(message, "TSTCLIENT", out var resolvedRecipient));
			mockLogger.Verify(x => x.WarnFormat(It.Is<string>((s) => s.StartsWith("eHubGateway failed to route the message. Routing rule engine returned no results for Sender")), It.IsAny<Object[]>()));
		}

		[TestMethod]
		public void Test_IsXHMessage_ErrorResult_ReturnFalse()
		{

			var mockContext = new Mock<eHubTransactionsContext>();
			var mockLogger = new Mock<ILog>();

			var routingClientsConfig = new RoutingClientsConfiguration(
				enabled: true,
				new List<RoutingClientConfigurationItem>()
					{
						{ new RoutingClientConfigurationItem(){ ClientId = "TSTCLIENT" , RouteId = "TSTCLIENT" , RoutingMethod =  RoutingMethod.Standard } }
					});

			var MockPartyAccessor = new Mock<IPartyAccessor>();
			var mockRuleFactory = new Mock<IRoutingRuleFactory>();

			var resultToReturn = new Collection<Result>();
			resultToReturn.Add(new Result(new eHubClient(), "", "Some Description for the Error", ""));

			var mockRule = new Mock<IRule>();
			mockRule.Setup(x => x.Evaluate(It.IsAny<eHubTransactionsContext>(), It.IsAny<IFactResolver[]>(), It.IsAny<ILog>()))
				.Returns(resultToReturn);

			mockRuleFactory.Setup(x => x.GetForReading("TSTCLIENT")).Returns(mockRule.Object);

			var routingRuleService = new RoutingRuleService(() => mockContext.Object, mockLogger.Object, (_) => mockRuleFactory.Object, () => MockPartyAccessor.Object, routingClientsConfig);

			var message = CreateTestMessage();

			Assert.IsFalse(routingRuleService.IsXHMessage(message, "TSTCLIENT", out var resolvedRecipient));
			mockLogger.Verify(x => x.WarnFormat(It.IsAny<string>(), It.Is<Object[]>(arr => arr.Any(o=>o.ToString().Contains("Some Description for the Error")))));
		}

		[TestMethod]
		[ExpectedException(typeof(Exception), "An exception occured during the RouteRule execution for the message.")]
		public void Test_IsXHMessage_EvaluationException_Throws()
		{

			var mockContext = new Mock<eHubTransactionsContext>();
			var mockLogger = new Mock<ILog>();

			var routingClientsConfig = new RoutingClientsConfiguration(
				enabled: true,
				new List<RoutingClientConfigurationItem>()
					{
						{ new RoutingClientConfigurationItem(){ ClientId = "TSTCLIENT" , RouteId = "TSTCLIENT" , RoutingMethod =  RoutingMethod.Standard } }
					});

			var MockPartyAccessor = new Mock<IPartyAccessor>();
			var mockRuleFactory = new Mock<IRoutingRuleFactory>();

			var mockRule = new Mock<IRule>();
			mockRule.Setup(x => x.Evaluate(It.IsAny<eHubTransactionsContext>(), It.IsAny<IFactResolver[]>(), It.IsAny<ILog>()))
				.Throws(new Exception("Some Exception"));

			mockRuleFactory.Setup(x => x.GetForReading("TSTCLIENT")).Returns(mockRule.Object);

			var routingRuleService = new RoutingRuleService(() => mockContext.Object, mockLogger.Object, (_) => mockRuleFactory.Object, () => MockPartyAccessor.Object, routingClientsConfig);

			var message = CreateTestMessage();

			routingRuleService.IsXHMessage(message, "TSTCLIENT", out var resolvedRecipient);
		}

		[TestMethod]
		[ExpectedException(typeof(Exception), "An exception occured during the RouteRule execution for the message.")]
		public void Test_IsXHMessage_RuleFactoryException_Throws()
		{

			var mockContext = new Mock<eHubTransactionsContext>();
			var mockLogger = new Mock<ILog>();

			var routingClientsConfig = new RoutingClientsConfiguration(
				enabled: true,
				new List<RoutingClientConfigurationItem>()
					{
						{ new RoutingClientConfigurationItem(){ ClientId = "TSTCLIENT" , RouteId = "TSTCLIENT" , RoutingMethod =  RoutingMethod.Standard } }
					});

			var MockPartyAccessor = new Mock<IPartyAccessor>();
			var mockRuleFactory = new Mock<IRoutingRuleFactory>();

			mockRuleFactory.Setup(x => x.GetForReading("TSTCLIENT")).Throws(new Exception());

			var routingRuleService = new RoutingRuleService(() => mockContext.Object, mockLogger.Object, (_) => mockRuleFactory.Object, () => MockPartyAccessor.Object, routingClientsConfig);

			var message = CreateTestMessage();

			routingRuleService.IsXHMessage(message, "TSTCLIENT", out var resolvedRecipient);
		}

		static eHubGatewayMessage CreateTestMessage()
		{
			return new eHubGatewayMessage
			{
				ApplicationCode = "TST",
				ClientID = "TSTCLIENT",
				EmailSubject = "EMAIL SUBJECT",
				FileName = "FILE.NAME",
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE STREAM")).CompressAndEncode()
			};
		}

	}
}
