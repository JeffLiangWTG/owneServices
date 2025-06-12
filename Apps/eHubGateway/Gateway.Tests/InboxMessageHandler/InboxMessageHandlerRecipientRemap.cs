using CargoWise.eHub.Common;
using eServices.eHubDataAccess.Integration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Gateway.Tests
{
	[TestClass]
	public class InboxMessageHandlerRecipientRemapTests
	{
		[TestMethod]
		public void InboxMessageHandlerRecipientRemap_GetHandler()
		{
			var message1 = new eHubGatewayMessage { ClientID = "SHIPPING_INSTRUCTION" };
			var message2 = new eHubGatewayMessage { ClientID = "OceanCarrierMessaging" };

			var mockPartyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			mockPartyAccessor.Stub(x => x.IsXHubSystem(message1.ClientID)).Return(false);
			mockPartyAccessor.Stub(x => x.IsXHubSystem(message2.ClientID)).Return(false);
			MessageHandlerFactory.GetPartyAccessor = () => mockPartyAccessor;

			var handler1 = MessageHandlerFactory.CreateMessageHandler(message1);
			var handler2 = MessageHandlerFactory.CreateMessageHandler(message2);

			Assert.AreEqual("SHIPPING_INSTRUCTION", message1.ClientID);
			Assert.IsInstanceOfType(handler1, typeof(DefaultInboxMessageHandler));

			Assert.AreEqual("OceanCarrierMessaging", message2.ClientID);
			Assert.IsInstanceOfType(handler2, typeof(DefaultInboxMessageHandler));
		}

		[TestMethod]
		public void TestDefaultInboxMessageHandler_eHubTestMessage()
		{
			var legacyMessage = new eHubGatewayMessage
			{
				ApplicationCode = "XMS",
				ClientID = "CLIENT",
				SchemaName = "http://www.edi.com.au/EnterpriseService/#Orders"
			};

			string senderID = "SENDER";
			var mockPartyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			mockPartyAccessor.Stub(x => x.IsCW1System(senderID)).Return(true);
			mockPartyAccessor.Stub(x => x.IsCW1System(legacyMessage.ClientID)).Return(true);

			MessageHandlerFactory.GetPartyAccessor = () => mockPartyAccessor;
			var handler = MessageHandlerFactory.CreateMessageHandler(senderID, legacyMessage);
			Assert.IsInstanceOfType(handler, typeof(DefaultInboxMessageHandler));
			Assert.IsNotInstanceOfType(handler, typeof(LegacyXmlE2EMessageHandler));
		}
	}
}
