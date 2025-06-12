using System;
using System.IO;
using System.Text;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using eServices.eHubDataAccess.Integration;
using CargoWise.eServices.USCustoms.Services;
using CargoWise.eHub.Integration;
using Rhino.Mocks;
using CargoWise.Billing.Kafka.API;
using System.Collections.Generic;

namespace CargoWise.eHub.Gateway.Tests.InboxMessageHandler
{
	[TestClass]
	public class USCustomsInboxMessageHandlerTests
	{
		[TestMethod]
		public void TestEnqueueMessageExceptionsAreRethrownAsSystemUnderMaintanance()
		{
			var senderID = "Sender";
			var message = CreateTestMessage("TST");
			var noServiceFoundException = new ArgumentException("Could not find any service with the name '//cargowise.com/eServices/DummyMessageProcessingService' in this database.");

			var inboxAccessor = new Mock<IInboxAccessor>();
            inboxAccessor.Setup(_ => _.EnqueueMessage(It.Is<OutboundMessageQueuer>(q => !q.isProduction), senderID, It.IsAny<Guid>(), message)).Throws(noServiceFoundException);

			var handler = new Mock<USCustomsInboxMessageHandler>(MockBehavior.Loose) { CallBase = true };
			handler.Setup(_ => _.CheckServiceSupported(senderID, message));
			handler.Setup(_ => _.NewInboxAccessor).Returns(inboxAccessor.Object);
			handler.Setup(_ => _.IsEnterpriseLicenceProduction("Sender")).Returns(false);

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

		[TestMethod]
	    public void TestEnqueueMessageOverrideLicenceToProd()
	    {
            var senderID = "TEST_USC";
	        var message = CreateTestMessage("TST");

	        var inboxAccessor = new Mock<IInboxAccessor>();
	        inboxAccessor.Setup(_ => _.EnqueueMessage(It.Is<OutboundMessageQueuer>(q => q.isProduction), senderID, It.IsAny<Guid>(), message));

	        var handler = new Mock<USCustomsInboxMessageHandler>(MockBehavior.Loose) { CallBase = true };
	        handler.Setup(_ => _.CheckServiceSupported(senderID, message));
	        handler.Setup(_ => _.NewInboxAccessor).Returns(inboxAccessor.Object);
	        handler.Setup(_ => _.IsEnterpriseLicenceProduction("Sender")).Returns(false);

	        handler.Object.Handle(senderID, Guid.NewGuid(), message);

	        inboxAccessor.VerifyAll();
	    }

		[TestMethod]
		public void TestEnqueuMessageOverrideSchkRepoOverrideLicenceToProd()
		{
			var senderID = "USC_SHCK_REPO";
			var message = CreateTestMessage("TST");

			var inboxAccessor = new Mock<IInboxAccessor>();
			inboxAccessor.Setup(_=>_.EnqueueMessage(It.Is<OutboundMessageQueuer>(q => q.isProduction), senderID, It.IsAny<Guid>(), message));

			var handler = new Mock<USCustomsInboxMessageHandler>(MockBehavior.Loose) { CallBase = true};
			handler.Setup(_ => _.CheckServiceSupported(senderID, message));
			handler.Setup(_ => _.NewInboxAccessor).Returns(inboxAccessor.Object);
			handler.Setup(_ => _.IsEnterpriseLicenceProduction("Sender")).Returns(false);

			handler.Object.Handle(senderID, Guid.NewGuid(), message);

			inboxAccessor.VerifyAll();
		}

		[TestMethod]
		public void TestCheckServiceSupported()
		{
			var senderID = "TEST_USC";
			
			var messageAMA = CreateTestMessage("AMA");
			var messageUSI = CreateTestMessage("USI");
			var messageMAN = CreateTestMessage("MAN");
			var messageUEM = CreateTestMessage("UEM");

			var registryAccessor = new Mock<IRegistryAccessor>(MockBehavior.Loose);
			var handler = new Mock<USCustomsInboxMessageHandler>(MockBehavior.Loose) { CallBase = true };
			handler.Setup(_ => _.NewRegistrationAccessor).Returns(registryAccessor.Object);

			registryAccessor.Setup(_ => _.SelectRegistryValue(senderID, ApplicationCode.AMA, "Entry Filer Code")).Returns("");
			registryAccessor.Setup(_ => _.SelectRegistryValue(senderID, ApplicationCode.AMA, "Participant Originator Code")).Returns("aaa");
			handler.Object.CheckServiceSupported(senderID, messageAMA);

			registryAccessor.Setup(_ => _.SelectRegistryValue(senderID, ApplicationCode.USImport, "Entry Filer Code")).Returns("");
			registryAccessor.Setup(_ => _.SelectRegistryValue(senderID, ApplicationCode.USImport, "ISF User Data")).Returns("ccc");
			handler.Object.CheckServiceSupported(senderID, messageUSI);

			registryAccessor.Setup(_ => _.SelectRegistryValue(senderID, ApplicationCode.USeManifest, "Entry Filer Code")).Returns("");
			registryAccessor.Setup(_ => _.SelectRegistryValue(senderID, ApplicationCode.USeManifest, "Client Network ID")).Returns("bbb");
			handler.Object.CheckServiceSupported(senderID, messageMAN);

			registryAccessor.Setup(_ => _.SelectRegistryValue(senderID, ApplicationCode.USExportManifest, "Entry Filer Code")).Returns("");
			registryAccessor.Setup(_ => _.SelectRegistryValue(senderID, ApplicationCode.USExportManifest, "Carrier Code")).Returns("ddd");
			handler.Object.CheckServiceSupported(senderID, messageUEM);

			registryAccessor.VerifyAll();
		}

		[TestMethod]
		public void TestCheckServiceNotSupported()
	    {
		    var senderID = "TEST_USC";
		    var message = CreateTestMessage("AAA"); 
			var registryAccessor = new Mock<IRegistryAccessor>();
		    registryAccessor.Setup(_ => _.SelectRegistryValue(senderID, "AAA", "Participant Originator Code")).Returns("RegistryResult");

		    var handler = new Mock<USCustomsInboxMessageHandler>(MockBehavior.Loose) { CallBase = true };
		    handler.Setup(_ => _.NewRegistrationAccessor).Returns(registryAccessor.Object);

		    try
		    {
			    handler.Object.CheckServiceSupported(senderID, message);
				Assert.Fail("ApplicationException is expected.");
		    }
		    catch (ApplicationException e)
		    {
			    Assert.AreEqual(e.Message, "Interchange rejected by eHub because you are not registered with WTG for US Customs messaging. Please register.");
			}
		}

	    static eHubGatewayMessage CreateTestMessage(string applicationCode)
	    {
	        return new eHubGatewayMessage
	        {
	            ApplicationCode = applicationCode,
	            ClientID = "TSTCLIENT",
	            EmailSubject = "EMAIL SUBJECT",
	            FileName = "FILE.NAME",
	            MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE STREAM")).CompressAndEncode(),
				SchemaName = "SF"
	        };
	    }
	}
}
