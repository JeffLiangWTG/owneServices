using System;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Moq;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.DownloadHandler
{
	class MessageStatusAcknowledgeHandlerTests : TestCaseWithFactory
	{
		public void TestMessageStatusAcknowledgeHandler_GetHandler()
		{
			var trackingId = Guid.NewGuid();
			var message = CreateMessageStatusAcknowledgeMessage(trackingId);
			var handler = HandlerFactory.GetHandler(message.SchemaName);
			AssertType<MessageStatusAcknowledgmentHandler>("Should be MessageStatusAcknowledgmentHandler", handler);
		}

		public void TestMessageStatusAcknowledgeHandler_Sent()
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_Status = EDIMessageStatusList.Codes.Sent;

			var interchange = Factory.New<EDIInterchange>();
			interchange.ContainedMessages.Add(message);
			var trackingId = interchange.PK.ToGuid();
			var eHubMessage = CreateMessageStatusAcknowledgeMessage(trackingId);
			var handler = new Mock<MessageStatusAcknowledgmentHandler>() { CallBase = true };
			handler.Setup(m => m.FindOutgoingInterchange(trackingId)).Returns(interchange);

			var notification = new NotificationBuffer();
			handler.Object.SaveMessage(eHubMessage, TestHelpers.ValidCompanyForTest(handler.Object.FactoryProvider.Current), notification);

			AssertEquals(EDIInterchangeStatusList.Codes.Acknowledged, interchange.EI_Status);
			AssertEquals(EDIMessageStatusList.Codes.Sent, interchange.ContainedMessages[0].EM_Status);
			AssertEquals(notification.AsString, "");

			handler.VerifyAll();
		}

		public void TestMessageStatusAcknowledgeHandler_Pending()
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_Status = EDIMessageStatusList.Codes.Sent;

			var interchange = Factory.New<EDIInterchange>();
			interchange.ContainedMessages.Add(message);
			var trackingId = interchange.PK.ToGuid();
			var eHubMessage = CreateMessageStatusAcknowledgeMessage(trackingId);
			var handler = new Mock<MessageStatusAcknowledgmentHandler>() { CallBase = true };
			handler.Setup(m => m.FindOutgoingInterchange(trackingId)).Returns(interchange);

			var notification = new NotificationBuffer();
			handler.Object.SaveMessage(eHubMessage, TestHelpers.ValidCompanyForTest(handler.Object.FactoryProvider.Current), notification);

			AssertEquals(EDIInterchangeStatusList.Codes.Acknowledged, interchange.EI_Status);
			AssertEquals(EDIMessageStatusList.Codes.Sent, interchange.ContainedMessages[0].EM_Status);
			AssertEquals(notification.AsString, "");
		}

		static IeHubMessage CreateMessageStatusAcknowledgeMessage(Guid trackingId)
		{
			var eHubMessage = new Mock<IeHubMessage>();
			eHubMessage.Setup(m => m.TrackingID).Returns(trackingId);
			eHubMessage.Setup(m => m.SenderID).Returns("SenderID");
			eHubMessage.Setup(m => m.RecipientID).Returns("RecipientID");
			eHubMessage.Setup(m => m.SchemaName).Returns("http://cargowise.com/ehub/core/2014/09#AcknowledgementReceived");
			eHubMessage.Setup(m => m.MessageStream).Returns((System.IO.Stream)null);
			return eHubMessage.Object;
		}
	}
}
