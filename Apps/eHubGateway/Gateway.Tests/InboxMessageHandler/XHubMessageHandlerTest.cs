using System;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

using CargoWise.eHub.Common;
using eServices.eHubDataAccess.Integration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace CargoWise.eHub.Gateway.Tests
{
    [TestClass]
    public class XHubMessageHandlerTest
    {

        [TestMethod]
        public void TestXHubMessageHandler_GetHandler()
        {
            var message = new eHubGatewayMessage
            {
                ClientID = "CLIENT",
            };

            string senderID = "SENDER";
            var mockPartyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
            mockPartyAccessor.Stub(x => x.IsXHubSystem(message.ClientID)).Return(true);

            MessageHandlerFactory.GetPartyAccessor = () => mockPartyAccessor;
            var handler = MessageHandlerFactory.CreateMessageHandler(senderID, message);
            Assert.IsInstanceOfType(handler, typeof(XHubMessageHandler));
        }

        [TestMethod]
        public void TestXHubMessageHandler_Header()
        {
            var messageTrackingID = Guid.NewGuid();

            var headerText = "";
            var stubSendToXHubGateway = new Func<eHubGatewayMessage, XHubMessageHeader, bool>((gatewayMessage, header) =>
            {
                var builder = new StringBuilder();
                var writer = XmlWriter.Create(builder);
                header.WriteHeader(writer, MessageVersion.Soap11);
                writer.Close();
                headerText = builder.ToString().Replace("\n", "").Replace("<Property>", "\n<Property>").Replace('\"', '\'').Trim();
                return true;
            });

            var handler = MockRepository.GenerateMock<XHubMessageHandler>();
            handler.Expect(x => x.SendToXHubGateway(Arg<eHubGatewayMessage>.Is.Anything, Arg<XHubMessageHeader>.Is.Anything)).Callback(stubSendToXHubGateway);
            handler.Expect(x => x.Handle(Arg<string>.Is.Anything, Arg<Guid>.Is.Anything, Arg<eHubGatewayMessage>.Is.Anything)).CallOriginalMethod(OriginalCallOptions.CreateExpectation);
            handler.Handle("SENDER", Guid.NewGuid(), new eHubGatewayMessage()
            {
                ClientID = "CLIENT",
                MessageTrackingID = messageTrackingID,
                ApplicationCode = "GCC",
                EmailSubject = "Subject",
                FileName = "FileName",
                SchemaName = "Schema",
                SchemaType = MessageSchemaType.FlatFile
            });
            Assert.IsTrue(headerText.Contains($"<Property><Name>SourceParty</Name><Namespace>urn:uuid:A2415482-5D5B-4054-8773-DCA6FA605E11</Namespace><Value>SENDER</Value></Property>"));
            Assert.IsTrue(headerText.Contains($"<Property><Name>DestinationParty</Name><Namespace>urn:uuid:A2415482-5D5B-4054-8773-DCA6FA605E11</Namespace><Value>CLIENT</Value></Property>"));
            Assert.IsTrue(headerText.Contains($"<Property><Name>MessageType</Name><Namespace>urn:uuid:A2415482-5D5B-4054-8773-DCA6FA605E11</Namespace><Value>Schema</Value></Property>"));
            Assert.IsTrue(headerText.Contains($"<Property><Name>MessageTrackingID</Name><Namespace>urn:uuid:A2415482-5D5B-4054-8773-DCA6FA605E11</Namespace><Value>{messageTrackingID}</Value></Property>"));
        }
    }
}
