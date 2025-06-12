using System;
using System.IO;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using eServices.eHubDataAccess.Integration;
using CargoWise.eHub.Integration;

namespace CargoWise.eHub.Gateway.Tests.InboxMessageHandler
{
	[TestClass]
	public class AirMessageHandlerTests
	{
		[TestMethod]
		public void TestEnqueueMessageExceptionsAreRethrownAsSystemUnderMaintanance()
		{
			var senderID = "Sender";
			var message = CreateTestMessage();
			var noServiceFoundException = new ArgumentException("Could not find any service with the name '//cargowise.com/eServices/DummyMessageProcessingService' in this database.");

			var inboxAccessor = new Mock<IInboxAccessor>();
			inboxAccessor.Setup(_ => _.EnqueueMessage(It.IsAny<IMessageQueuerLite>(), senderID, It.IsAny<Guid>(), message)).Throws(noServiceFoundException);

			var handler = new Mock<AirMessageHandler>(MockBehavior.Loose) {CallBase = true};
			handler.Setup(_ => _.NewInboxAccessor).Returns(inboxAccessor.Object);

			try
			{
				handler.Object.Handle(senderID, Guid.NewGuid(), message);
				Assert.Fail("SystemUnderMaintananceException is expected.");
			}
			catch (SystemUnderMaintananceException e)
			{
				Assert.IsTrue(e.Message.StartsWith("eHub Gateway under maintenance. Messages will be resubmitted to eHub automatically on next service task run. Please ignore the following message: Server under maintenance and have not a valid ediEnterprise licence code."));
				Assert.AreEqual(noServiceFoundException, e.InnerException);
			}
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