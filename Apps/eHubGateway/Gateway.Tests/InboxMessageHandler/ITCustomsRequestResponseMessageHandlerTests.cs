using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Text;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using eServices.eHubDataAccess.Integration;
using CargoWise.Billing.Kafka.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using CargoWise.eHub.Gateway.ITCustoms;

namespace CargoWise.eHub.Gateway.Tests.InboxMessageHandler
{
	[TestClass]
	public class ITCustomsRequestResponseMessageHandlerTests
	{
		[TestMethod]
		public void TestGetITCustomsJobStatusMessageHandler()
		{
			var message = new eHubGatewayMessage
			{
				ApplicationCode = "ITC",
				ClientID = "eHub",
				SchemaName = "http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse",
				SchemaType = MessageSchemaType.Xml
			};

			var handler = MessageHandlerFactory.CreateMessageHandler(message);
			Assert.IsInstanceOfType(handler, typeof(ITCustomsRequestResponseMessageHandler));
		}

		[TestMethod]
		public void TestTriggerJobStatus_Success()
		{
			var xml = @"<ITCustoms xmlns=""http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse"">
	<Files>
		<File>
			<Name>4UDG0927.R0A</Name>
		</File>
		<File>
			<Name>4UDG0927.R0B</Name>
		</File>
	</Files>
</ITCustoms>";
			var senderID = "SENDERTST";
			var recipientID = "eHub";
			var envelopTrackingId = Guid.NewGuid();
			var messageTrackingId = Guid.NewGuid();
			var message = new eHubGatewayMessage
			{
				ApplicationCode = "ITC",
				ClientID = recipientID,
				MessageTrackingID = messageTrackingId,
				SchemaName = "http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse",
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)).CompressAndEncode()
			};

			var jobStatusManager = MockRepository.GenerateMock<IJobStatusManager>();
			jobStatusManager.Expect(x => x.TriggerJobStatus(Arg<string>.Is.Equal("SENTST"), Arg<Files>.Matches(Is.Matching<Files>(y => y.File[0].Name == "4UDG0927.R0A" && y.File[1].Name == "4UDG0927.R0B")))).Repeat.Once();
			var handler = MockRepository.GeneratePartialMock<ITCustomsRequestResponseMessageHandler>();
			handler.Expect(x => x.JobStatusManager).Return(jobStatusManager);
			var outboxAccessor = MockRepository.GenerateMock<IOutboxAccessor>();
			handler.Stub(x => x.NewOutboxAccessor).Return(outboxAccessor);
			outboxAccessor.Expect(x => x.GenerateSuccessStatusMessage(messageTrackingId, senderID)).Repeat.Once();

			handler.Handle(senderID, envelopTrackingId, message);

			jobStatusManager.VerifyAllExpectations();
			handler.VerifyAllExpectations();
			outboxAccessor.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestTriggerJobStatus_MissingFilesInXML_Fail()
		{
			var xml = @"<ITCustoms xmlns=""http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse"">
	<NotFiles>
	</NotFiles>
</ITCustoms>";
			var recipientID = "eHub";
			var envelopTrackingId = Guid.NewGuid();
			var messageTrackingId = Guid.NewGuid();
			var sendStreamRequest = new SendStreamRequest { SendStreamRequestTrackingID = envelopTrackingId };
			var messages = new List<eHubGatewayMessage>();
			var message = new eHubGatewayMessage
			{
				ApplicationCode = "ITC",
				ClientID = recipientID,
				MessageTrackingID = messageTrackingId,
				SchemaName = "http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse",
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)).CompressAndEncode()
			};
			messages.Add(message);
			sendStreamRequest.Messages = messages.ToArray();
			
			var jobStatusManager = MockRepository.GenerateMock<IJobStatusManager>();

			var handler = MockRepository.GeneratePartialMock<ITCustomsRequestResponseMessageHandler>();
			handler.Expect(x => x.JobStatusManager).Repeat.Never();

			var serviceMock = MockRepository.GeneratePartialMock<EHubRegistryUpdateHandlerTests.eHubStreamedServiceTest>();
			serviceMock.Expect(x => x.GetHandler(Arg<eHubGatewayMessage>.Is.Anything)).Return(handler).Repeat.Once();

			var kafkaClientMock = MockRepository.GenerateMock<BillingKafkaClient>("hosts", null);
			var kafkaClientMockLazy = new Lazy<BillingKafkaClient>(() => kafkaClientMock);
			kafkaClientMock.Expect(x => x.Dispose()).Repeat.Never();
			serviceMock.Expect(x => x.CreateBillingKafkaClient()).Return(kafkaClientMockLazy);
			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);

			var outboxAccessor = MockRepository.GenerateMock<IOutboxAccessor>();
			handler.Expect(x => x.NewOutboxAccessor).Return(outboxAccessor).Repeat.Never();

			try
			{
				serviceMock.SendStream(sendStreamRequest);
				Assert.Fail("An exception should have been thrown");
			}
			catch (FaultException<ApplicationFault> fex)
			{
				StringAssert.Contains(fex.Detail.MessageExceptionDictionary[message.MessageTrackingID], "Message does not contains FileName.");
			}

			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);
			jobStatusManager.VerifyAllExpectations();
			handler.VerifyAllExpectations();
			serviceMock.VerifyAllExpectations();
			kafkaClientMock.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestTriggerJobStatus_InvalidXML_Fail()
		{
			var xml = @"<ITCustoms xmlns=""http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse"">
	<RequestFile>
		<AnotherNode/>
	</RequestFile>
</ITCustomsBrokens>";
			var recipientID = "eHub";
			var envelopTrackingId = Guid.NewGuid();
			var messageTrackingId = Guid.NewGuid();
			var sendStreamRequest = new SendStreamRequest { SendStreamRequestTrackingID = envelopTrackingId };
			var messages = new List<eHubGatewayMessage>();
			var message = new eHubGatewayMessage
			{
				ApplicationCode = "ITC",
				ClientID = recipientID,
				MessageTrackingID = messageTrackingId,
				SchemaName = "http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse",
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)).CompressAndEncode()
			};
			messages.Add(message);
			sendStreamRequest.Messages = messages.ToArray();

			var jobStatusManager = MockRepository.GenerateMock<IJobStatusManager>();
			var handler = MockRepository.GeneratePartialMock<ITCustomsRequestResponseMessageHandler>();
			handler.Expect(x => x.JobStatusManager).Repeat.Never();

			var serviceMock = MockRepository.GeneratePartialMock<EHubRegistryUpdateHandlerTests.eHubStreamedServiceTest>();
			serviceMock.Expect(x => x.GetHandler(Arg<eHubGatewayMessage>.Is.Anything)).Return(handler).Repeat.Once();

			var kafkaClientMock = MockRepository.GenerateMock<BillingKafkaClient>("hosts", null);
			var kafkaClientMockLazy = new Lazy<BillingKafkaClient>(() => kafkaClientMock);
			kafkaClientMock.Expect(x => x.Dispose()).Repeat.Never();
			serviceMock.Expect(x => x.CreateBillingKafkaClient()).Return(kafkaClientMockLazy);
			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);

			var outboxAccessor = MockRepository.GenerateMock<IOutboxAccessor>();
			handler.Expect(x => x.NewOutboxAccessor).Return(outboxAccessor).Repeat.Never();

			try
			{
				serviceMock.SendStream(sendStreamRequest);
				Assert.Fail("An exception should have been thrown");
			}
			catch (FaultException<ApplicationFault> fex)
			{
				StringAssert.Contains(fex.Detail.MessageExceptionDictionary[message.MessageTrackingID],
					"The 'ITCustoms' start tag on line 1 position 2 does not match the end tag of 'ITCustomsBrokens'. Line 5, position 3.");
			}

			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);
			jobStatusManager.VerifyAllExpectations();
			handler.VerifyAllExpectations();
			serviceMock.VerifyAllExpectations();
			kafkaClientMock.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestTriggerJobStatus_ConcurrencyIssue_Fail()
		{
			var xml = @"<ITCustoms xmlns=""http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse"">
	<Files>
		<File>
			<Name>4UDG0927.R0A</Name>
		</File>
		<File>
			<Name>4UDG0927.R0B</Name>
		</File>
	</Files>
</ITCustoms>";
			var recipientID = "eHub";
			var envelopTrackingId = Guid.NewGuid();
			var messageTrackingId = Guid.NewGuid();
			var sendStreamRequest = new SendStreamRequest { SendStreamRequestTrackingID = envelopTrackingId };
			var messages = new List<eHubGatewayMessage>();
			var message = new eHubGatewayMessage
			{
				ApplicationCode = "ITC",
				ClientID = recipientID,
				MessageTrackingID = messageTrackingId,
				SchemaName = "http//www.cargowise.com/ehub/Schemas/ITCustoms",
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)).CompressAndEncode()
			};
			messages.Add(message);
			sendStreamRequest.Messages = messages.ToArray();

			var jobStatusManager = MockRepository.GenerateMock<IJobStatusManager>();
			jobStatusManager.Expect(x => x.TriggerJobStatus("", null)).IgnoreArguments().Throw(new ApplicationException("Unable to trigger the retrieval of file responses. Failed filename(s): abc_xyz.")).Repeat.Once();
			var handler = MockRepository.GeneratePartialMock<ITCustomsRequestResponseMessageHandler>();
			handler.Expect(x => x.JobStatusManager).Return(jobStatusManager);

			var serviceMock = MockRepository.GeneratePartialMock<EHubRegistryUpdateHandlerTests.eHubStreamedServiceTest>();
			serviceMock.Expect(x => x.GetHandler(Arg<eHubGatewayMessage>.Is.Anything)).Return(handler).Repeat.Once();

			var kafkaClientMock = MockRepository.GenerateMock<BillingKafkaClient>("hosts", null);
			var kafkaClientMockLazy = new Lazy<BillingKafkaClient>(() => kafkaClientMock);
			kafkaClientMock.Expect(x => x.Dispose()).Repeat.Never();
			serviceMock.Expect(x => x.CreateBillingKafkaClient()).Return(kafkaClientMockLazy);
			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);

			var outboxAccessor = MockRepository.GenerateMock<IOutboxAccessor>();
			handler.Expect(x => x.NewOutboxAccessor).Return(outboxAccessor).Repeat.Never();

			try
			{
				serviceMock.SendStream(sendStreamRequest);
				Assert.Fail("An exception should have been thrown");
			}
			catch (FaultException<ApplicationFault> fex)
			{
				StringAssert.Contains(fex.Detail.MessageExceptionDictionary[message.MessageTrackingID],
					"Unable to trigger the retrieval of file responses. Failed filename(s): abc_xyz.");
			}

			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);
			jobStatusManager.VerifyAllExpectations();
			handler.VerifyAllExpectations();
			serviceMock.VerifyAllExpectations();
			kafkaClientMock.VerifyAllExpectations();
		}
	}
}
