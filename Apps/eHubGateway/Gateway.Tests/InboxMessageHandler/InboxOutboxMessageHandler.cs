using System;
using CargoWise.eHub.Common;
using eServices.eHubDataAccess.Integration;
using CargoWise.eHub.Integration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Gateway.Tests
{
	[TestClass]
	public class InboxOutboxMessageHandlerTests
	{
		[TestMethod]
		public void InboxOutboxMessageHandler_GetHandler()
		{
			var ioMsg = new eHubGatewayMessage
			{
				ApplicationCode = "TRX",
				ClientID = "SENDER",
				SchemaName = "HKCustoms",
				SchemaType = MessageSchemaType.FlatFile
			};
			// TODO: SYS will be enabled again after we have a load test
			//var sysMsg = new eHubGatewayMessage
			//{
			//    ApplicationCode = "SYS",
			//    ClientID = "EDIAUSSYD",
			//    SchemaName = "http://www.cargowise.com/Schemas/System#SystemInterchange",
			//    SchemaType = MessageSchemaType.FlatFile
			//};

			var ioHandler = MessageHandlerFactory.CreateMessageHandler(ioMsg);
			//var sysHandler = MessageHandlerFactory.CreateMessageHandler(sysMsg);

			Assert.IsInstanceOfType(ioHandler, typeof(InboxOutboxMessageHandler));
			//Assert.IsInstanceOfType(sysHandler, typeof(InboxOutboxMessageHandler));
		}

		[TestMethod]
		public void InboxOutboxMessageHandler_Handle()
		{
			string senderID = "SENDER";
			Guid envelopeID = Guid.NewGuid();
			var message = new eHubGatewayMessage();
			var mockInboxAccessor = MockRepository.GenerateMock<IInboxAccessor>();
			var handler = MockRepository.GeneratePartialMock<InboxOutboxMessageHandler>();
			handler.Stub(x => x.GetInboxAccessor()).Return(mockInboxAccessor);

			handler.Handle(senderID, envelopeID, message);

			mockInboxAccessor.AssertWasCalled(x => x.InsertToInboxAndOutbox(senderID, envelopeID, message));
		}

		[TestMethod]
		public void InboxOutboxMessageHandler_HandleTelematricsMessage()
		{
			var ioMsg = new eHubGatewayMessage
			{
				ApplicationCode = "TEL",
				ClientID = "SENDER",
				SchemaName = "TELEMATIC",
				SchemaType = MessageSchemaType.Xml
			};

			var mockPartyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			mockPartyAccessor.Stub(x => x.IsXHSystem(ioMsg.ClientID)).Return(false);
			MessageHandlerFactory.GetPartyAccessor = () => mockPartyAccessor;
			var ioHandler = MessageHandlerFactory.CreateMessageHandler(ioMsg);

			Assert.IsInstanceOfType(ioHandler, typeof(InboxOutboxMessageHandler));
		}

		[TestMethod]
		public void InboxOutboxMessageHandler_GetHandler_GMD()
		{
			var ioMsg = new eHubGatewayMessage
			{
				ApplicationCode = "GMD"
			};

			var ioHandler = MessageHandlerFactory.CreateMessageHandler(ioMsg);

			Assert.IsInstanceOfType(ioHandler, typeof(InboxOutboxMessageHandler));
		}

		[TestMethod]
		public void InboxOutboxMessageHandler_GetHandler_NDM()
		{
			// Arrange
			var message = new eHubGatewayMessage
			{
				ApplicationCode = "NDM",
				ClientID = "ClientID"
			};
			var senderId = "SenderID";
			var mockPartyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			mockPartyAccessor.Stub(x => x.IsXHSystem(message.ClientID)).Return(false);
			mockPartyAccessor.Stub(x => x.IsXHubSystem(message.ClientID)).Return(false);
			mockPartyAccessor.Stub(x => x.IsCW1System(message.ClientID)).Return(true);
			mockPartyAccessor.Stub(x => x.IsCW1System(senderId)).Return(true);
			MessageHandlerFactory.GetPartyAccessor = () => mockPartyAccessor;
			// Act
			var handler = MessageHandlerFactory.CreateMessageHandler(senderId, message);
			// Assert
			Assert.IsInstanceOfType(handler, typeof(InboxOutboxMessageHandler));
		}

		[TestMethod]
		public void InboxOutboxMessageHandler_GetHandler_TaiwanCustoms()
		{
			var ioMsg = new eHubGatewayMessage
			{
				SchemaName = "http://cargowise.com/ehub/products/TWCPluginRequest#TWCPluginServiceSendRequest"
			};

			var ioHandler = MessageHandlerFactory.CreateMessageHandler(ioMsg);

			Assert.IsInstanceOfType(ioHandler, typeof(InboxOutboxMessageHandler));
		}

		[TestMethod]
		public void InboxOutboxMessageHandler_HandleSenderFMS()
		{
			var ioMsg = new eHubGatewayMessage();

			var ioHandler = MessageHandlerFactory.CreateMessageHandler("FLIGHT_MONITORING_SYSTEM", ioMsg);

			Assert.IsInstanceOfType(ioHandler, typeof(InboxOutboxMessageHandler));
		}

		[TestMethod]
		public void InboxOutboxMessageHandler_HandleRecipientFMS()
		{
			var ioMsg = new eHubGatewayMessage
			{
				ClientID = "FLIGHT_MONITORING_SYSTEM"
			};

			var ioHandler = MessageHandlerFactory.CreateMessageHandler(ioMsg);

			Assert.IsInstanceOfType(ioHandler, typeof(InboxOutboxMessageHandler));
		}

		[TestMethod]
		public void InboxOutboxMessageHandler_HandleRecipientSFS()
		{
			var ioMsg = new eHubGatewayMessage()
			{
				ClientID = "SCHEDULE_FEED_SERVICE"
			};

			var ioHandler = MessageHandlerFactory.CreateMessageHandler(ioMsg);

			Assert.IsInstanceOfType(ioHandler, typeof(InboxOutboxMessageHandler));
		}

		[TestMethod]
		public void InboxOutboxMessageHandler_HandleSenderABE()
		{
			var ioMsg = new eHubGatewayMessage();

			var ioHandler = MessageHandlerFactory.CreateMessageHandler("AIR_BOOKING_ENGINE", ioMsg);

			Assert.IsInstanceOfType(ioHandler, typeof(InboxOutboxMessageHandler));
		}


		[TestMethod]
		public void InboxOutboxMessageHandler_HandleTelematicsMessage()
		{
			var message = new eHubGatewayMessage()
			{
				ApplicationCode = ApplicationCode.Telematics,
				ClientID = "CLIENT"
			};

			var mockPartyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			mockPartyAccessor.Stub(x => x.IsXHSystem(message.ClientID)).Return(false);

			MessageHandlerFactory.GetPartyAccessor = () => mockPartyAccessor;
			var handler = MessageHandlerFactory.CreateMessageHandler("SenderID", message);
			Assert.IsInstanceOfType(handler, typeof(InboxOutboxMessageHandler));
		}

		[TestMethod]
		public void InboxOutboxMessageHandler_VTP_HandleSender()
		{
			var message = new eHubGatewayMessage();

			var handler = MessageHandlerFactory.CreateMessageHandler("CONTAINER_TRANSPORT_OPTIMIZATION", message);

			Assert.IsInstanceOfType(handler, typeof(InboxOutboxMessageHandler));
		}

		[TestMethod]
		public void InboxOutboxMessageHandler_OCMBE_HandleSender()
		{
			var message = new eHubGatewayMessage
			{
				ApplicationCode = ApplicationCode.OCMBeDirect
			};
			var handler = MessageHandlerFactory.CreateMessageHandler("OCM_BookingEngine", message);

			Assert.IsInstanceOfType(handler, typeof(InboxOutboxMessageHandler));
		}

		[TestMethod]
		public void InboxOutboxMessageHandler_OCMBE_HandleRecipient()
		{
			var message = new eHubGatewayMessage
			{
				ApplicationCode = ApplicationCode.OCMBeDirect,
				ClientID = "OCM_BookingEngine"
			};

			var handler = MessageHandlerFactory.CreateMessageHandler(message);

			Assert.IsInstanceOfType(handler, typeof(InboxOutboxMessageHandler));
		}
	}
}
