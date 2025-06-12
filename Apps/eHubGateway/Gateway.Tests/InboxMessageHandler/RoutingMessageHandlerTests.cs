using System;
using System.IO;
using CargoWise.eHub.Common;
using eServices.eHubDataAccess.Integration;
using CargoWise.eHub.Gateway.Routing;
using CargoWise.eHub.Integration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CargoWise.eHub.Gateway.Tests
{
	[TestClass]
	public class RoutingMessageHandlerTests
	{

		private IRoutingRuleService originalRoutingRuleService;
		
		[TestInitialize]
		public void Setup()
		{
			originalRoutingRuleService = MessageHandlerFactory.routingRuleService;
		}

		[TestCleanup]
		public void Clenaup()
		{
			MessageHandlerFactory.routingRuleService = originalRoutingRuleService;
		}

		[TestMethod]
		public void Test_RoutingMessageHandler_ReturnXHMessageHandler()
		{
			var routingRuleService = new Mock<IRoutingRuleService>();
			MessageHandlerFactory.routingRuleService = routingRuleService.Object;

			var resolvedRecipient = It.IsAny<string>();
			routingRuleService.Setup(x => x.IsXHMessage(It.IsAny<eHubGatewayMessage>(), It.IsAny<string>(), out resolvedRecipient)).Returns(true);

			var message = new eHubGatewayMessage
			{
				ApplicationCode = ApplicationCode.CIM,
				ClientID = "eHub",
				SchemaName = "",
				SchemaType = MessageSchemaType.Xml,
				MessageStream = new MemoryStream()
			};

			var handler = MessageHandlerFactory.CreateMessageHandler(message);
			Assert.IsInstanceOfType(handler, typeof(XHMessageHandler));
		}

		[TestMethod]
		public void Test_RoutingMessageHandler_ReturnDefaultHandler()
		{
			var routingRuleService = new Mock<IRoutingRuleService>();
			MessageHandlerFactory.routingRuleService = routingRuleService.Object;

			var resolvedRecipient = It.IsAny<string>();
			routingRuleService.Setup(x => x.IsXHMessage(It.IsAny<eHubGatewayMessage>(), It.IsAny<string>(), out resolvedRecipient)).Returns(false);

			var message = new eHubGatewayMessage
			{
				ApplicationCode = ApplicationCode.CIM,
				ClientID = "eHub",
				SchemaName = "",
				SchemaType = MessageSchemaType.Xml,
				MessageStream = new MemoryStream()
			};

			var handler = MessageHandlerFactory.CreateMessageHandler(message);
			Assert.IsInstanceOfType(handler, typeof(AirMessageHandler));
		}

	}
}
