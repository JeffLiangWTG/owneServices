using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using eServices.Configuration.Framework;
using eServices.Configuration.Schemas;
using eServices.eHubDataAccess.Integration;
using eServices.eHubDataModel.Common;
using eServices.eHubDataModel.eHubTransactions;
using Common.Logging.Simple;
using KellermanSoftware.CompareNetObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Gateway.Tests.InboxMessageHandler
{
    [TestClass]
    public class ConfigurationMessageHandlerTests
    {
        [TestMethod]
        public void ConfigurationMessageHandler_MessageHandlerFactory()
        {
            var message = new eHubGatewayMessage
            {
                ApplicationCode = "UDM",
                ClientID = "eHub",
                SchemaName = "http://www.wisetechglobal.com/Schemas/Configuration#Configuration",
                SchemaType = MessageSchemaType.Xml,
                MessageStream = ConfigurationMessage.SerializeToStream(new ConfigurationMessage()).CompressAndEncode()
            };

            var mockPartyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
            mockPartyAccessor.Stub(x => x.IsXHSystem(message.ClientID)).Return(false);

            MessageHandlerFactory.GetPartyAccessor = () => mockPartyAccessor;
            var handler = MessageHandlerFactory.CreateMessageHandler("SenderID", message);
            Assert.IsInstanceOfType(handler, typeof(ConfigurationMessageHandler));
        }

        [TestMethod]
        public void ConfigurationMessageHandler_Handle()
        {
            Func<int, Guid> formatGuid = i => new Guid(Enumerable.Repeat((byte)((i % 16) * 0x11), 16).ToArray());
            var guidQueue = new Queue<Guid>(Enumerable.Range(5, 2).Select(i => formatGuid(i)));
            ConfigurationMessageHandler.InternalNewGuid = () => guidQueue.Dequeue();

            var eHubClients = new TestDbSet<eHubClient>
            {
                new eHubClient {CC_PK = formatGuid(1), CC_ID = "eHub"},
                new eHubClient {CC_PK = formatGuid(2), CC_ID = "AAABBBCCC"}
            };
            var eHubInboxMessages = new TestDbSet<eHubInboxMessage>();
            var eHubOutboxMessages = new TestDbSet<eHubOutboxMessage>();
            var eHubMessageTypes = new TestDbSet<eHubMessageType>() { new eHubMessageType { DT_PK = formatGuid(3), DT_Code = "MessageStatusSuccess" } };
            var stubContext = MockRepository.GenerateStub<eHubTransactionsContext>();
            stubContext.eHubClients = eHubClients;
            stubContext.eHubInboxMessages = eHubInboxMessages;
            stubContext.eHubOutboxMessages = eHubOutboxMessages;
            stubContext.eHubMessageTypes = eHubMessageTypes;

            var stubConfigurationHandler = MockRepository.GenerateMock<IConfigurationHandler, IDbContextInjectable>();

            var message = new eHubGatewayMessage
            {
                ClientID = "eHub",
                ApplicationCode = "UDM",
                SchemaName = "http://www.wisetechglobal.com/Schemas/Configuration#Configuration",
                SchemaType = MessageSchemaType.Xml,
                MessageTrackingID = formatGuid(4),
                MessageStream = ConfigurationMessage.SerializeToStream(new ConfigurationMessage()).CompressAndEncode()
            };

            var handler = new ConfigurationMessageHandler(() => stubContext,
                configurationMessage => stubConfigurationHandler,
                new TraceLoggerFactoryAdapter().GetLogger(typeof(ConfigurationMessageHandlerTests).Name));

            handler.Handle("AAABBBCCC", formatGuid(4), message);

            ((IDbContextInjectable)stubConfigurationHandler).AssertWasCalled(x => x.SetDbContext(stubContext));
            stubConfigurationHandler.AssertWasCalled(x => x.AddOrUpdate(Arg<ConfigurationMessage>.Is.Anything));
            stubContext.AssertWasCalled(x => x.SaveChanges());

            var expectedInboxMessages = new TestDbSet<eHubInboxMessage>
            {
                new eHubInboxMessage
                {
                    EI_PK = formatGuid(5),
                    EI_MessageTrackingID = formatGuid(4).ToString(),
                    EI_EnvelopeTrackingID = string.Empty,
                    EI_CC_Sender = formatGuid(2),
                    EI_CC_Recipient = formatGuid(1),
                    EI_MessageType = message.SchemaName,
                    EI_IsFlatFile = false,
                    EI_ApplicationCode = message.ApplicationCode,
                    EI_EmailSubjectOverride = string.Empty,
                    EI_FileNameOverride = string.Empty,
                    EI_Status = 3,
                    EI_Content = message.MessageStream.ReadToEnd()
                }
            };
            var compInboxConfig = new ComparisonConfig();
            compInboxConfig.MembersToIgnore.Add("EI_InsertUTC");
            compInboxConfig.MembersToIgnore.Add("EI_LastUpdateUTC");
			var compInbox = new CompareLogic(compInboxConfig);
            var diffInbox = compInbox.Compare(expectedInboxMessages, eHubInboxMessages);
            Assert.IsTrue(diffInbox.AreEqual, diffInbox.DifferencesString);

            var expectedOutboxMessages = new TestDbSet<eHubOutboxMessage>
            {
                new eHubOutboxMessage
                {
                    OI_PK = formatGuid(6),
                    OI_CC_Sender = formatGuid(1),
                    OI_CC_Recipient = formatGuid(2),
                    OI_MessageTrackingID = formatGuid(4).ToString(),
                    OI_Status = 0,
                    OI_DT_Target = formatGuid(3),
                    OI_Content = "H4sIAAAAAAAEAOy9B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/It7kTZu+butiefH/BAAA//+kn3eSCwAAAA=="
                }
            };
            var compOutboxConfig = new ComparisonConfig();
            compOutboxConfig.MembersToIgnore.Add("OI_InsertUTC");
            var compOutbox = new CompareLogic(compOutboxConfig);
            var diffOutbox = compOutbox.Compare(expectedOutboxMessages, eHubOutboxMessages);
            Assert.IsTrue(diffOutbox.AreEqual, diffOutbox.DifferencesString);
        }

        [TestMethod]
        public void TestGetACAS_BRCustomsMessageHandler()
        {
            const string SystemID = "HYECM2";
            const string Company1 = "DAU";
            const string StaffID = "DUS";

            const string SY4F17N1 =
                "S/Z0Dl6yTi4DSjV9uftYUPncLuAI4cHnbf337puuxGLFIQ3ku8hacqKvLYp4cT+Zh5pAgxzM5Gs2xzpY5UC0StXDRIOpcmERfuFxD5dgSSsqa3vo+hpCSrysKCAKriLUxZrhG9zbsQ85oYZxFZQlML55zurSowRpZNg2fLShpJs=";

            string xmlString =
                $@"<Configuration xmlns='http://www.wisetechglobal.com/Schemas/Configuration' Name='ACAS_BR' Version='1.0'> 
	<Group Type='System' Reference='{SystemID}' >
	<Group Type='Company' Reference='{Company1}' >
	<Group Type='Staff' Reference='{StaffID}' >
		<Certificate>
			<File Filename='TBK0461.pfx'>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</File>
			<Passphrase>{SY4F17N1}</Passphrase>
		</Certificate>
	</Group>
	</Group>
	</Group>
</Configuration>";

            var message = new eHubGatewayMessage
            {
                ApplicationCode = "HUB",
                ClientID = "eHub",
                SchemaName = "http://www.wisetechglobal.com/Schemas/Configuration#Configuration",
                SchemaType = MessageSchemaType.Xml,
                MessageStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlString)).CompressAndEncode()
            };

            var mockPartyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
            mockPartyAccessor.Stub(x => x.IsXHSystem(message.ClientID)).Return(false);

            MessageHandlerFactory.GetPartyAccessor = () => mockPartyAccessor;
            var handler = MessageHandlerFactory.CreateMessageHandler("SenderID", message);
            Assert.IsInstanceOfType(handler, typeof(ConfigurationMessageHandler));
        }

        [TestMethod]
        public void TestConfigurationMessageHandler_UnexpectedError()
        {
            Func<int, Guid> formatGuid = i => new Guid(Enumerable.Repeat((byte)((i % 16) * 0x11), 16).ToArray());
            var guidQueue = new Queue<Guid>(Enumerable.Range(5, 10).Select(i => formatGuid(i)));
            ConfigurationMessageHandler.InternalNewGuid = () => guidQueue.Dequeue();

            var eHubClients = new TestDbSet<eHubClient>
            {
                new eHubClient {CC_PK = formatGuid(1), CC_ID = "eHub"},
                new eHubClient {CC_PK = formatGuid(2), CC_ID = "AAABBBCCC"}
            };
            var eHubInboxMessages = new TestDbSet<eHubInboxMessage>();
            var eHubOutboxMessages = new TestDbSet<eHubOutboxMessage>();
            var eHubMessageTypes = new TestDbSet<eHubMessageType>() {
                new eHubMessageType { DT_PK = formatGuid(2), DT_Code = "MessageStatusSuccess" },
                new eHubMessageType { DT_PK = formatGuid(3), DT_Code = "MessageStatusFailed" },
                new eHubMessageType { DT_PK = formatGuid(4), DT_Code = "http://www.wisetechglobal.com/Schemas/Configuration#Configuration" }};
            var eHubErrors = new TestDbSet<eHubError>();
            var stubContext = MockRepository.GenerateStub<eHubTransactionsContext>();
            stubContext.eHubClients = eHubClients;
            stubContext.eHubInboxMessages = eHubInboxMessages;
            stubContext.eHubOutboxMessages = eHubOutboxMessages;
            stubContext.eHubMessageTypes = eHubMessageTypes;
            stubContext.eHubErrors = eHubErrors;

            var stubConfigurationHandler = MockRepository.GenerateMock<IConfigurationHandler, IDbContextInjectable, IConfigurationValidation>();
            stubConfigurationHandler.Expect(x => x.AddOrUpdate(Arg<ConfigurationMessage>.Is.Anything)).Throw(new InvalidOperationException("Test"));

            var message = new eHubGatewayMessage
            {
                ClientID = "eHub",
                ApplicationCode = "UDM",
                SchemaName = "http://www.wisetechglobal.com/Schemas/Configuration#Configuration",
                SchemaType = MessageSchemaType.Xml,
                MessageTrackingID = formatGuid(4),
                MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("")).CompressAndEncode()
            };

            var handler = new ConfigurationMessageHandler(() => stubContext,
                configurationMessage => stubConfigurationHandler,
                new TraceLoggerFactoryAdapter().GetLogger(typeof(ConfigurationMessageHandlerTests).Name));

            try
            {
                handler.Handle("AAABBBCCC", formatGuid(4), message);
                Assert.Fail("Exception should be thrown.");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "Test");
            }
            ((IDbContextInjectable)stubConfigurationHandler).AssertWasCalled(x => x.SetDbContext(stubContext));
            stubContext.AssertWasCalled(x => x.SaveChanges());

            var expectedInboxMessages = new TestDbSet<eHubInboxMessage>
            {
                new eHubInboxMessage
                {
                    EI_PK = formatGuid(5),
                    EI_MessageTrackingID = formatGuid(4).ToString(),
                    EI_EnvelopeTrackingID = string.Empty,
                    EI_CC_Sender = formatGuid(2),
                    EI_CC_Recipient = formatGuid(1),
                    EI_MessageType = message.SchemaName,
                    EI_IsFlatFile = false,
                    EI_ApplicationCode = message.ApplicationCode,
                    EI_EmailSubjectOverride = string.Empty,
                    EI_FileNameOverride = string.Empty,
                    EI_Status = 255,
                    EI_Content = message.MessageStream.ReadToEnd()
                },
                new eHubInboxMessage
                {
                    EI_PK = formatGuid(9),
                    EI_MessageTrackingID = formatGuid(8).ToString(),
                    EI_EnvelopeTrackingID = string.Empty,
                    EI_CC_Sender = formatGuid(1),
                    EI_CC_Recipient = formatGuid(2),
                    EI_MessageType = message.SchemaName,
                    EI_IsFlatFile = false,
                    EI_ApplicationCode = message.ApplicationCode,
                    EI_EmailSubjectOverride = string.Empty,
                    EI_FileNameOverride = string.Empty,
                    EI_Status = 2,
                    EI_Content = "H4sIAAAAAAAEAHu/e7+Nc35eWmZ6aVFiSWZ+nkJFbk5esa1SRklJgZW+fnl5uV55ZnFqSWpyRnpOflJijl5yfq5+cHJGam5isT6KViUFfTsAxUdljVAAAAA="
                }
            };
            var compInboxConfig = new ComparisonConfig();
            compInboxConfig.MembersToIgnore.Add("EI_InsertUTC");
            compInboxConfig.MembersToIgnore.Add("EI_LastUpdateUTC");
			var compInbox = new CompareLogic(compInboxConfig);
            var diffInbox = compInbox.Compare(expectedInboxMessages, eHubInboxMessages);
            Assert.IsTrue(diffInbox.AreEqual, diffInbox.DifferencesString);

            var expectedOutboxMessages = new TestDbSet<eHubOutboxMessage>
            {
                new eHubOutboxMessage
                {
                    OI_PK = formatGuid(7),
                    OI_CC_Sender = formatGuid(1),
                    OI_CC_Recipient = formatGuid(2),
                    OI_MessageTrackingID = formatGuid(4).ToString(),
                    OI_Status = 0,
                    OI_DT_Target = formatGuid(3)
                },
                new eHubOutboxMessage
                {
                    OI_PK = formatGuid(10),
                    OI_CC_Sender = formatGuid(1),
                    OI_CC_Recipient = formatGuid(2),
                    OI_MessageTrackingID = formatGuid(8).ToString(),
                    OI_Status = 0,
                    OI_DT_Target = formatGuid(4)
                }
            };
            var compOutboxConfig = new ComparisonConfig();
            compOutboxConfig.MembersToIgnore.Add("OI_InsertUTC");
            compOutboxConfig.MembersToIgnore.Add("OI_Content");
            compOutboxConfig.MembersToIgnore.Add("eHubInboxMessage");
            var compOutbox = new CompareLogic(compOutboxConfig);
            var diffOutbox = compOutbox.Compare(expectedOutboxMessages, eHubOutboxMessages);
            Assert.IsTrue(diffOutbox.AreEqual, diffOutbox.DifferencesString);
            Assert.AreEqual(formatGuid(9), eHubOutboxMessages.Last().eHubInboxMessage.EI_PK);
        }

        [TestMethod]
        public void TestConfigurationMessageHandler_ValidationError()
        {
            Func<int, Guid> formatGuid = i => new Guid(Enumerable.Repeat((byte)((i % 16) * 0x11), 16).ToArray());
            var guidQueue = new Queue<Guid>(Enumerable.Range(5, 10).Select(i => formatGuid(i)));
            ConfigurationMessageHandler.InternalNewGuid = () => guidQueue.Dequeue();

            var eHubClients = new TestDbSet<eHubClient>
            {
                new eHubClient {CC_PK = formatGuid(1), CC_ID = "eHub"},
                new eHubClient {CC_PK = formatGuid(2), CC_ID = "AAABBBCCC"}
            };
            var eHubInboxMessages = new TestDbSet<eHubInboxMessage>();
            var eHubOutboxMessages = new TestDbSet<eHubOutboxMessage>();
            var eHubMessageTypes = new TestDbSet<eHubMessageType>() {
                new eHubMessageType { DT_PK = formatGuid(2), DT_Code = "MessageStatusSuccess" },
                new eHubMessageType { DT_PK = formatGuid(3), DT_Code = "MessageStatusFailed" },
                new eHubMessageType { DT_PK = formatGuid(4), DT_Code = "http://www.wisetechglobal.com/Schemas/Configuration#Configuration" }};
            var eHubErrors = new TestDbSet<eHubError>();
            var stubContext = MockRepository.GenerateStub<eHubTransactionsContext>();
            stubContext.eHubClients = eHubClients;
            stubContext.eHubInboxMessages = eHubInboxMessages;
            stubContext.eHubOutboxMessages = eHubOutboxMessages;
            stubContext.eHubMessageTypes = eHubMessageTypes;
            stubContext.eHubErrors = eHubErrors;

            var stubConfigurationHandler = MockRepository.GenerateMock<IConfigurationHandler, IDbContextInjectable, IConfigurationValidation>();
            stubConfigurationHandler.Expect(x => x.AddOrUpdate(Arg<ConfigurationMessage>.Is.Anything));

            ((IConfigurationValidation)stubConfigurationHandler).Expect(x => x.ValidateAndGenerateResponseIfFailed(Arg<ConfigurationMessage>.Is.Anything)).Return(Tuple.Create(new ConfigurationMessage(), new StringBuilder("Test")));

            var message = new eHubGatewayMessage
            {
                ClientID = "eHub",
                ApplicationCode = "UDM",
                SchemaName = "http://www.wisetechglobal.com/Schemas/Configuration#Configuration",
                SchemaType = MessageSchemaType.Xml,
                MessageTrackingID = formatGuid(4),
                MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("")).CompressAndEncode()
            };

            var handler = new ConfigurationMessageHandler(() => stubContext,
                configurationMessage => stubConfigurationHandler,
                new TraceLoggerFactoryAdapter().GetLogger(typeof(ConfigurationMessageHandlerTests).Name));

            handler.Handle("AAABBBCCC", formatGuid(4), message);

            ((IDbContextInjectable)stubConfigurationHandler).AssertWasCalled(x => x.SetDbContext(stubContext));
            stubContext.AssertWasCalled(x => x.SaveChanges());

            var expectedInboxMessages = new TestDbSet<eHubInboxMessage>
            {
                new eHubInboxMessage
                {
                    EI_PK = formatGuid(5),
                    EI_MessageTrackingID = formatGuid(4).ToString(),
                    EI_EnvelopeTrackingID = string.Empty,
                    EI_CC_Sender = formatGuid(2),
                    EI_CC_Recipient = formatGuid(1),
                    EI_MessageType = message.SchemaName,
                    EI_IsFlatFile = false,
                    EI_ApplicationCode = message.ApplicationCode,
                    EI_EmailSubjectOverride = string.Empty,
                    EI_FileNameOverride = string.Empty,
                    EI_Status = 255,
                    EI_Content = message.MessageStream.ReadToEnd()
                },
                new eHubInboxMessage
                {
                    EI_PK = formatGuid(9),
                    EI_MessageTrackingID = formatGuid(8).ToString(),
                    EI_EnvelopeTrackingID = string.Empty,
                    EI_CC_Sender = formatGuid(1),
                    EI_CC_Recipient = formatGuid(2),
                    EI_MessageType = message.SchemaName,
                    EI_IsFlatFile = false,
                    EI_ApplicationCode = message.ApplicationCode,
                    EI_EmailSubjectOverride = string.Empty,
                    EI_FileNameOverride = string.Empty,
                    EI_Status = 2,
                    EI_Content = "H4sIAAAAAAAEAHu/e7+Nc35eWmZ6aVFiSWZ+nkJYalExkLZVMtQzUFKoyM3JK7ZVyigpKbDS1y8vL9crzyxOLUlNzkjPyU9KzNFLzs/VD07OSM1NLNZHMUhJQd8OAEzuDxNeAAAA"
                }
            };
            var compInboxConfig = new ComparisonConfig();
            compInboxConfig.MembersToIgnore.Add("EI_InsertUTC");
			compInboxConfig.MembersToIgnore.Add("EI_LastUpdateUTC");
            var compInbox = new CompareLogic(compInboxConfig);
            var diffInbox = compInbox.Compare(expectedInboxMessages, eHubInboxMessages);
            Assert.IsTrue(diffInbox.AreEqual, diffInbox.DifferencesString);

            var expectedOutboxMessages = new TestDbSet<eHubOutboxMessage>
            {
                new eHubOutboxMessage
                {
                    OI_PK = formatGuid(7),
                    OI_CC_Sender = formatGuid(1),
                    OI_CC_Recipient = formatGuid(2),
                    OI_MessageTrackingID = formatGuid(4).ToString(),
                    OI_Status = 0,
                    OI_DT_Target = formatGuid(3)
                },
                new eHubOutboxMessage
                {
                    OI_PK = formatGuid(10),
                    OI_CC_Sender = formatGuid(1),
                    OI_CC_Recipient = formatGuid(2),
                    OI_MessageTrackingID = formatGuid(8).ToString(),
                    OI_Status = 0,
                    OI_DT_Target = formatGuid(4)
                }
            };
            var compOutboxConfig = new ComparisonConfig();
            compOutboxConfig.MembersToIgnore.Add("OI_InsertUTC");
            compOutboxConfig.MembersToIgnore.Add("OI_Content");
            compOutboxConfig.MembersToIgnore.Add("eHubInboxMessage");
            var compOutbox = new CompareLogic(compOutboxConfig);
            var diffOutbox = compOutbox.Compare(expectedOutboxMessages, eHubOutboxMessages);
            Assert.IsTrue(diffOutbox.AreEqual, diffOutbox.DifferencesString);
            Assert.AreEqual(formatGuid(9), eHubOutboxMessages.Last().eHubInboxMessage.EI_PK);
        }
    }
}
