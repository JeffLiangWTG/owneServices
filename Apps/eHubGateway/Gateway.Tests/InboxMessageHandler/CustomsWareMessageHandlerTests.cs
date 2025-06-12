using System;
using System.IO;
using System.Text;

using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using eServices.eHubDataAccess.Integration;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Gateway.Tests.InboxMessageHandler
{
	[TestClass]
	public class CustomsWareMessageHandlerTests
	{
		[TestMethod]
		public void TestCustomsWareHandler_GetHandler()
		{
			var customsWareMessage = new eHubGatewayMessage
			{
				ApplicationCode = "CWS",
				ClientID = "CLIENT",
				SchemaName = "http://www.customsware.com/schema/api#InputDocument"
			};

			string senderID = "SENDER";
			var mockPartyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			mockPartyAccessor.Stub(x => x.IsCW1System(senderID)).Return(true);
			mockPartyAccessor.Stub(x => x.IsCW1System(customsWareMessage.ClientID)).Return(true);

			MessageHandlerFactory.GetPartyAccessor = () => mockPartyAccessor;
			var handler = MessageHandlerFactory.CreateMessageHandler(senderID, customsWareMessage);
			Assert.IsInstanceOfType(handler, typeof(CustomsWareMessageHandler));
			Assert.IsInstanceOfType(handler, typeof(DefaultInboxMessageHandler));
		}

		[TestMethod]
		public void TestSendStreamToCustomsWare_Succeed()
		{
			Guid envelopTrackingId = Guid.NewGuid();
			Guid messageTrackingId = Guid.NewGuid();

			var message = new eHubGatewayMessage
			{
				ApplicationCode = "CWS",
				ClientID = "CustomsWare",
				EmailSubject = "EmailSubject1",
				FileName = "FileName1",
				MessageTrackingID = messageTrackingId,
				SchemaName = "http://www.customsware.com/schema/api#InputDocument",
				SchemaType = MessageSchemaType.Xml,
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("Message 1 stream")).CompressAndEncode()
			};

			var senderId = "EDIEDIDAT";

			var mockInboxAccessor = MockRepository.GenerateMock<IInboxAccessor>();

			var handler = MockRepository.GeneratePartialMock<CustomsWareMessageHandler>();
			handler.Stub(x => x.NewInboxAccessor).Return(mockInboxAccessor);

			handler.Handle(senderId, envelopTrackingId, message);

			mockInboxAccessor.AssertWasCalled(x => x.InsertToInbox(senderId, envelopTrackingId, message, true));
			mockInboxAccessor.VerifyAllExpectations();
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidDataException), "Message is empty")]
		public void TestSendStreamToCustomsWare_FailedEmptyMessage()
		{
			Guid envelopTrackingId = Guid.NewGuid();
			Guid messageTrackingId = Guid.NewGuid();

			var message = new eHubGatewayMessage
			{
				ApplicationCode = "CWS",
				ClientID = "CustomsWare",
				EmailSubject = "EmailSubject1",
				FileName = "FileName1",
				MessageTrackingID = messageTrackingId,
				SchemaName = "http://www.customsware.com/schema/api#InputDocument",
				SchemaType = MessageSchemaType.Xml,
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("")).CompressAndEncode()
			};

			var senderId = "EDIEDIDAT";

			var mockInboxAccessor = MockRepository.GenerateMock<IInboxAccessor>();

			var handler = MockRepository.GeneratePartialMock<CustomsWareMessageHandler>();
			handler.Stub(x => x.NewInboxAccessor).Return(mockInboxAccessor);

			handler.Handle(senderId, envelopTrackingId, message);
			handler.Expect(x => x.FailMessage("Message is empty"));

			mockInboxAccessor.VerifyAllExpectations();
			handler.VerifyAllExpectations();
		}
	}
}
