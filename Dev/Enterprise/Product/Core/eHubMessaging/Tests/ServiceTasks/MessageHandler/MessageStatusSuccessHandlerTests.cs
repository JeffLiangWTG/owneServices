using System;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.ZArchitecture;
using Moq;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.DownloadHandler
{
	class MessageStatusSuccessHandlerTests : TestCaseWithFactory
	{
		public void TestMessageStatusSuccessHandler()
		{
			var message = new Mock<IeHubMessage>();
			Guid trackingID = new Guid("728e874f-8f09-4860-a2a0-082eb62743a5");
			message.Setup(m => m.TrackingID).Returns(trackingID);
			message.Setup(m => m.SenderID).Returns("SenderID");
			message.Setup(m => m.RecipientID).Returns("RecipientID");
			message.Setup(m => m.SchemaName).Returns("MessageStatusSuccess");
			message.Setup(m => m.MessageStream).Returns(new VirtualMemoryStream());

			var interchange = Factory.New<EDIInterchange>();
			var message1 = EDIMessageTestFactory.New(Factory);
			message1.EM_Status = EDIMessage.Status.Sent;
			interchange.ContainedMessages.Add(message1);
			var message2 = EDIMessageTestFactory.New(Factory);
			message2.EM_Status = EDIMessage.Status.Sent;
			interchange.ContainedMessages.Add(message2);

			var handler = new Mock<MessageStatusSuccessHandler>() { CallBase = true };
			handler.Setup(m => m.FindOutgoingInterchange(trackingID)).Returns(interchange);

			var notification = new NotificationBuffer();
			handler.Object.SaveMessage(message.Object, TestHelpers.ValidCompanyForTest(handler.Object.FactoryProvider.Current), notification);

			AssertEquals(EDIInterchange.Status.Sent, interchange.EI_Status);
			AssertEquals(EDIMessage.Status.Sent, interchange.ContainedMessages[0].EM_Status);
			AssertEquals(EDIMessage.Status.Sent, interchange.ContainedMessages[1].EM_Status);
			AssertEquals(notification.AsString, "");
			handler.VerifyAll();
		}
	}
}
