using CargoWise.eHub.Common;
using eServices.eHubDataAccess.Integration;
using CargoWise.eHub.Gateway.OpenAPIs;
using CargoWise.eHub.Gateway.OpenAPIs.XHGateway;
using CargoWise.eHub.Integration;
using CargoWise.Billing.Kafka.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace CargoWise.eHub.Gateway.Tests
{
    [TestClass]
    public class XHMessageHandlerTests
    {
        [TestMethod]
        public void TestXHMessageHandler_GetHandler()
        {
            var message = new eHubGatewayMessage
            {
                ClientID = "CLIENT",
            };

            string senderID = "SENDER";
            var mockPartyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
            mockPartyAccessor.Stub(x => x.IsXHSystem(message.ClientID)).Return(true);

            MessageHandlerFactory.GetPartyAccessor = () => mockPartyAccessor;
            var handler = MessageHandlerFactory.CreateMessageHandler(senderID, message);
            Assert.IsInstanceOfType(handler, typeof(XHMessageHandler));
        }

        [TestMethod]
        public void TestXHMessageHandler_Headers()
        {
            var clientMock = MockRepository.GenerateStub<IClient>();
            clientMock.Stub(x => x.SendMessageAsync(Arg<OpenAPIs.XHGateway.Message>.Is.Anything)).Callback<OpenAPIs.XHGateway.Message>(msg =>
            {
                Assert.IsTrue(msg.Properties["custom.SourceParty"] == "TestSenderId");
                Assert.IsTrue(msg.Properties["custom.ApplicationCode"] == "TestApplicationCode");
                Assert.IsTrue(msg.Properties["custom.DestinationParty"] == "TestClientID");
                Assert.IsTrue(msg.Properties["custom.EmailSubject"] == "TestEmailSubject");
                Assert.IsTrue(msg.Properties["custom.FileName"] == "TestFileName");
                Assert.IsTrue(msg.Properties["custom.MessageTrackingID"] == "0000000a-0014-001e-2832-3c46505a646e");
                Assert.IsTrue(msg.Properties["custom.MessageType"] == "TestSchemaName");
                Assert.IsTrue(msg.Properties["custom.SchemaType"] == "Xml");
                Assert.IsTrue(msg.Properties["custom.IsXHubMessage"] == "true");
                return true;
            }).Return(Task.CompletedTask);

            var clientFactory = MockRepository.GenerateMock<XHMessageHandler>();
            var handler = new XHMessageHandler(url => clientMock);

            handler.Handle("TestSenderId", new Guid(110, 120, 130, 140, 150, 160, 170, 180, 190, 200, 210), new eHubGatewayMessage()
            {
                ApplicationCode = "TestApplicationCode",
                ClientID = "TestClientID",
                EmailSubject = "TestEmailSubject",
                FileName = "TestFileName",
                MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("!! Message Body !!")),
                MessageTrackingID = new Guid(10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110),
                SchemaName = "TestSchemaName",
                SchemaType = MessageSchemaType.Xml
            });

        }

		[TestMethod]
		public void TestXHMessageHandler_ThrowingTaskCanceledException()
		{
			var clientMock = MockRepository.GenerateMock<IClient>();
			clientMock.Stub(x => x.SendMessageAsync(Arg<OpenAPIs.XHGateway.Message>.Is.Anything)).Throw(new TaskCanceledException("A task was canceled"));
			
			var handler = new XHMessageHandler(url => clientMock);

			try
			{
				handler.Handle("TestSenderId", new Guid(110, 120, 130, 140, 150, 160, 170, 180, 190, 200, 210), new eHubGatewayMessage()
				{
					ApplicationCode = "TestApplicationCode",
					ClientID = "TestClientID",
					EmailSubject = "TestEmailSubject",
					FileName = "TestFileName",
					MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("!! Message Body !!")),
					MessageTrackingID = new Guid(10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110),
					SchemaName = "TestSchemaName",
					SchemaType = MessageSchemaType.Xml
				});
				Assert.Fail("XHGatewayException is expected.");
			}
			catch (XHGatewayException e)
			{
				Assert.IsTrue(e.Message.StartsWith("A task was canceled"));
			}
			catch (Exception)
			{
				Assert.Fail("XHGatewayException is expected.");
			}
		}

		[TestMethod]
		public void TestEHubStreamedService_ThrowingSystemUnderMaintananceException()
		{
			Guid envelopTrackingId = Guid.NewGuid();
			var sendStreamRequest = new SendStreamRequest { SendStreamRequestTrackingID = envelopTrackingId };
			var messages = new List<eHubGatewayMessage>();

			var message = new eHubGatewayMessage
			{
				ApplicationCode = "TestApplicationCode",
				ClientID = "TestClientID",
				EmailSubject = "TestEmailSubject",
				FileName = "TestFileName",
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("!! Message Body !!")),
				MessageTrackingID = new Guid(10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110),
				SchemaName = "TestSchemaName",
				SchemaType = MessageSchemaType.Xml
			};

			messages.Add(message);
			sendStreamRequest.Messages = messages.ToArray();

			var serviceMock = MockRepository.GeneratePartialMock<eHubStreamedServiceMockWithHandle>();
			serviceMock.SenderId = message.ClientID;
			var handlerMock = MockRepository.GeneratePartialMock<XHMessageHandler>();
			handlerMock.Stub(x => x.CheckIfTooManyUnprocessedMessages(serviceMock.SenderId, message.ClientID)).Do(new Action<string, string>(delegate { }));
			handlerMock.Stub(x => x.Handle(serviceMock.SenderId, envelopTrackingId, message)).Throw(new XHGatewayException("A task was canceled", 500, string.Empty, null, null));
			serviceMock.Stub(x => x.GetHandler(message)).Return(handlerMock);

			var kafkaClientMock = MockRepository.GenerateMock<BillingKafkaClient>("hosts", null);
			var kafkaClientMockLazy = new Lazy<BillingKafkaClient>(() => kafkaClientMock);
			kafkaClientMock.Expect(x => x.Dispose()).Repeat.Never();
			serviceMock.Expect(x => x.CreateBillingKafkaClient()).Return(kafkaClientMockLazy);
			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);

			try
			{
				serviceMock.SendStream(sendStreamRequest);
				Assert.Fail("SystemUnderMaintananceException is expected.");
			}
			catch (SystemUnderMaintananceException e)
			{
				Assert.IsTrue(e.Message.Contains("A task was canceled"));
			}
			catch (Exception)
			{
				Assert.Fail("SystemUnderMaintananceException is expected.");
			}

			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);
			serviceMock.VerifyAllExpectations();
			kafkaClientMock.VerifyAllExpectations();
		}

		[TestMethod]
        public void TestUYCustoms_GetHandler()
        {
            var message = new eHubGatewayMessage()
            {
                ApplicationCode = ApplicationCode.UYCustoms
            };

            var handler = MessageHandlerFactory.CreateMessageHandler(message);
            Assert.IsInstanceOfType(handler, typeof(XHMessageHandler));
        }

        [TestMethod]
        public void TestTelematicsMessage_GetHandler()
        {
            var message = new eHubGatewayMessage()
            {
                ApplicationCode = ApplicationCode.Telematics,
                ClientID = "CLIENT"
            };

            var mockPartyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
            mockPartyAccessor.Stub(x => x.IsXHSystem(message.ClientID)).Return(true);

            MessageHandlerFactory.GetPartyAccessor = () => mockPartyAccessor;
            var handler = MessageHandlerFactory.CreateMessageHandler("SenderID", message);
            Assert.IsInstanceOfType(handler, typeof(XHMessageHandler));
        }

    }
}
