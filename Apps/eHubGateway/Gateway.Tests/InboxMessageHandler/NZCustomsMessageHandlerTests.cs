using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Text;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using eServices.eHubDataAccess.Integration;
using eHub2Common = CargoWise.eHub2.Common;
using CargoWise.Billing.Kafka.API;

namespace CargoWise.eHub.Gateway.Tests.InboxMessageHandler
{
	[TestClass]
	public class NZCustomsMessageHandlerTests
	{
		[TestMethod]
		public void TestSendStreamToNZCustoms_ClientLicenceTypeIsTST_FailMessage()
		{
			Guid envelopTrackingId = Guid.NewGuid();
			Guid messageTrackingId = Guid.NewGuid();

			var message = new eHubGatewayMessage
			{
				ApplicationCode = "NZC",
				ClientID = "NZCustoms",
				EmailSubject = "EmailSubject1",
				FileName = "FileName1",
				MessageTrackingID = messageTrackingId,
				SchemaName = "SchemaName1",
				SchemaType = MessageSchemaType.Xml,
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("Message 1 stream"))
			};

			var messages = new List<eHubGatewayMessage>();
			messages.Add(message);

			var sendStreamRequest = new SendStreamRequest { SendStreamRequestTrackingID = envelopTrackingId };
			sendStreamRequest.Messages = messages.ToArray();

			var serviceMock = MockRepository.GeneratePartialMock<eHubStreamedService>();
			var senderId = "EDIEDIDAT";
			serviceMock.Expect(_ => _.GetCurrentClientId()).Return(senderId);
			serviceMock.Expect(_ => _.IsIntegrationUserNamePasswordValidator()).Return(false);
			serviceMock.Expect(_ => _.LogTransaction(Arg<string>.Is.Equal("OK"),
													 Arg<Func<string>>.Is.Anything,
												     Arg<string>.Is.Equal("SendStream"))).Repeat.Once();

			var kafkaClientMock = MockRepository.GenerateMock<BillingKafkaClient>("hosts", null);
			var kafkaClientMockLazy = new Lazy<BillingKafkaClient>(() => kafkaClientMock);
			kafkaClientMock.Expect(x => x.Dispose()).Repeat.Never();
			serviceMock.Expect(x => x.CreateBillingKafkaClient()).Return(kafkaClientMockLazy);
			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);

			var handlerMock = MockRepository.GeneratePartialMock<NZCustomsMessageHandler>();

			var enterpriseAccessor = MockRepository.GenerateStrictMock<IEnterpriseExeDetailAccessor>();
			enterpriseAccessor.Expect(x => x.GetLicenceType(senderId)).Return("TST").Repeat.Once();

			var inboxAccessor = MockRepository.GenerateStrictMock<IInboxAccessor>();
			inboxAccessor.Expect(x => x.InsertToInbox(Arg<string>.Is.Equal(senderId), Arg<Guid>.Is.Anything, Arg<Guid>.Is.Anything, Arg<MessageStatus>.Is.Equal(MessageStatus.Received), Arg<eHubGatewayMessage>.Is.Equal(message), Arg<Boolean>.Is.Equal(false))).Repeat.Once();

			var errorAccessor = MockRepository.GenerateStrictMock<IExceptionsAccessor>();
			errorAccessor.Expect(x => x.SubmitErrorAndUpdateStatus(Arg<Guid>.Is.Anything, Arg<string>.Is.Equal("GTW"), Arg<string>.Is.Equal("UKN"), Arg<string>.Is.Equal("Only production licensed CW1/Enterprise systems are permitted to send production messages to NZ Customs."), Arg<Guid>.Is.Anything, Arg<Guid>.Is.Anything, Arg<Guid>.Is.Anything, Arg<Guid>.Is.Anything, Arg<System.Data.SqlClient.SqlConnection>.Is.Anything)).Repeat.Once();

			handlerMock.Expect(x => x.NewInboxAccessor).Return(inboxAccessor).Repeat.Once();
			handlerMock.Expect(x => x.NewErrorAccessor).Return(errorAccessor).Repeat.Once();
			handlerMock.Expect(x => x.NewEnterpriseExeDetailAccessor).Return(enterpriseAccessor).Repeat.Once();
			handlerMock.Expect(x => x.CheckIfTooManyUnprocessedMessages(senderId, message.ClientID)).Do(new Action<string, string>(delegate { }));
			serviceMock.Expect(x => x.GetHandler(message)).Return(handlerMock).Repeat.Once();

			serviceMock.SendStream(sendStreamRequest);

			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);
			serviceMock.VerifyAllExpectations();
			enterpriseAccessor.VerifyAllExpectations();
			handlerMock.VerifyAllExpectations();
			kafkaClientMock.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestSendStreamToNZCustoms_ClientLicenceNotFound_FailMessage()
		{
			Guid envelopTrackingId = Guid.NewGuid();
			Guid messageTrackingId = Guid.NewGuid();

			var message = new eHubGatewayMessage
			{
				ApplicationCode = "NZC",
				ClientID = "NZCustoms",
				EmailSubject = "EmailSubject1",
				FileName = "FileName1",
				MessageTrackingID = messageTrackingId,
				SchemaName = "SchemaName1",
				SchemaType = MessageSchemaType.Xml,
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("Message 1 stream"))
			};

			var messages = new List<eHubGatewayMessage>();
			messages.Add(message);

			var sendStreamRequest = new SendStreamRequest { SendStreamRequestTrackingID = envelopTrackingId };
			sendStreamRequest.Messages = messages.ToArray();

			var serviceMock = MockRepository.GeneratePartialMock<eHubStreamedService>();
			var senderId = "EDIEDIDAT";
			serviceMock.Expect(_ => _.GetCurrentClientId()).Return(senderId);
			serviceMock.Expect(_ => _.IsIntegrationUserNamePasswordValidator()).Return(false);

			var kafkaClientMock = MockRepository.GenerateMock<BillingKafkaClient>("hosts", null);
			var kafkaClientMockLazy = new Lazy<BillingKafkaClient>(() => kafkaClientMock);
			kafkaClientMock.Expect(x => x.Dispose()).Repeat.Never();
			serviceMock.Expect(x => x.CreateBillingKafkaClient()).Return(kafkaClientMockLazy);
			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);

			var handlerMock = MockRepository.GeneratePartialMock<NZCustomsMessageHandler>();

			var enterpriseAccessor = MockRepository.GenerateStrictMock<IEnterpriseExeDetailAccessor>();
			enterpriseAccessor.Expect(x => x.GetLicenceType(senderId)).Return(string.Empty).Repeat.Once();

			var inboxAccessor = MockRepository.GenerateStrictMock<IInboxAccessor>();

			inboxAccessor.Expect(x => x.InsertToInbox(Arg<string>.Is.Equal(senderId), Arg<Guid>.Is.Anything, Arg<Guid>.Is.Anything, Arg<MessageStatus>.Is.Equal(MessageStatus.Received), Arg<eHubGatewayMessage>.Is.Equal(message), Arg<Boolean>.Is.Equal(false))).Repeat.Once();

			var errorAccessor = MockRepository.GenerateStrictMock<IExceptionsAccessor>();
			errorAccessor.Expect(x => x.SubmitErrorAndUpdateStatus(Arg<Guid>.Is.Anything, Arg<string>.Is.Equal("GTW"), Arg<string>.Is.Equal("UKN"), Arg<string>.Is.Equal("Only production licensed CW1/Enterprise systems are permitted to send production messages to NZ Customs."), Arg<Guid>.Is.Anything, Arg<Guid>.Is.Anything, Arg<Guid>.Is.Anything, Arg<Guid>.Is.Anything, Arg<System.Data.SqlClient.SqlConnection>.Is.Anything)).Repeat.Once();

			handlerMock.Expect(x => x.NewInboxAccessor).Return(inboxAccessor).Repeat.Once();
			handlerMock.Expect(x => x.NewErrorAccessor).Return(errorAccessor).Repeat.Once();
			handlerMock.Expect(x => x.NewEnterpriseExeDetailAccessor).Return(enterpriseAccessor).Repeat.Once();
			handlerMock.Expect(x => x.CheckIfTooManyUnprocessedMessages(senderId, message.ClientID)).Do(new Action<string, string>(delegate { }));
			serviceMock.Expect(x => x.GetHandler(message)).Return(handlerMock).Repeat.Once();

			serviceMock.SendStream(sendStreamRequest);
			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);
			serviceMock.VerifyAllExpectations();
			enterpriseAccessor.VerifyAllExpectations();
			handlerMock.VerifyAllExpectations();
			kafkaClientMock.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestSendStreamToNZCustoms_FailNZCustomsMessageDeliveryConfigured_FailMessage()
		{
			Guid envelopTrackingId = Guid.NewGuid();
			Guid messageTrackingId = Guid.NewGuid();

			var message = new eHubGatewayMessage
			{
				ApplicationCode = "NZC",
				ClientID = "NZCustoms",
				EmailSubject = "EmailSubject1",
				FileName = "FileName1",
				MessageTrackingID = messageTrackingId,
				SchemaName = "SchemaName1",
				SchemaType = MessageSchemaType.Xml,
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("Message 1 stream"))
			};

			var messages = new List<eHubGatewayMessage>();
			messages.Add(message);

			var sendStreamRequest = new SendStreamRequest { SendStreamRequestTrackingID = envelopTrackingId };
			sendStreamRequest.Messages = messages.ToArray();

			var serviceMock = MockRepository.GeneratePartialMock<eHubStreamedService>();
			var senderId = "EDIEDIDAT";
			serviceMock.Expect(_ => _.GetCurrentClientId()).Return(senderId);
			serviceMock.Expect(_ => _.IsIntegrationUserNamePasswordValidator()).Return(false);

			var kafkaClientMock = MockRepository.GenerateMock<BillingKafkaClient>("hosts", null);
			var kafkaClientMockLazy = new Lazy<BillingKafkaClient>(() => kafkaClientMock);
			kafkaClientMock.Expect(x => x.Dispose()).Repeat.Never();
			serviceMock.Expect(x => x.CreateBillingKafkaClient()).Return(kafkaClientMockLazy);

			var handlerMock = MockRepository.GeneratePartialMock<NZCustomsMessageHandler>();
			handlerMock.Expect(_ => _.GetConfigurationFailNZCustomsMessageDelivery()).Return(true);

			var inboxAccessor = MockRepository.GenerateStrictMock<IInboxAccessor>();
			inboxAccessor.Expect(x => x.InsertToInbox(Arg<string>.Is.Equal(senderId), Arg<Guid>.Is.Anything, Arg<Guid>.Is.Anything, Arg<MessageStatus>.Is.Equal(MessageStatus.Received), Arg<eHubGatewayMessage>.Is.Equal(message), Arg<Boolean>.Is.Equal(false))).Repeat.Once();

			var errorAccessor = MockRepository.GenerateStrictMock<IExceptionsAccessor>();
			errorAccessor.Expect(x => x.SubmitErrorAndUpdateStatus(Arg<Guid>.Is.Anything, Arg<string>.Is.Equal("GTW"), Arg<string>.Is.Equal("UKN"), Arg<string>.Is.Equal("Cannot be delivered as this system does not have a NZCustoms connection."), Arg<Guid>.Is.Anything, Arg<Guid>.Is.Anything, Arg<Guid>.Is.Anything, Arg<Guid>.Is.Anything, Arg<System.Data.SqlClient.SqlConnection>.Is.Anything)).Repeat.Once();

			handlerMock.Expect(x => x.NewInboxAccessor).Return(inboxAccessor).Repeat.Once();
			handlerMock.Expect(x => x.NewErrorAccessor).Return(errorAccessor).Repeat.Once();
			handlerMock.Expect(x => x.CheckIfTooManyUnprocessedMessages(senderId, message.ClientID)).Do(new Action<string, string>(delegate { }));
			serviceMock.Expect(x => x.GetHandler(message)).Return(handlerMock).Repeat.Once();
			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);

			serviceMock.SendStream(sendStreamRequest);

			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);
			serviceMock.VerifyAllExpectations();
			handlerMock.VerifyAllExpectations();
			kafkaClientMock.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestSendStreamToNZCustomsTest_Succeed()
		{
			Guid envelopTrackingId = Guid.NewGuid();
			Guid messageTrackingId = Guid.NewGuid();

			var message = new eHubGatewayMessage
			{
				ApplicationCode = "NZC",
				ClientID = "NZCustomsTest",
				EmailSubject = "EmailSubject1",
				FileName = "FileName1",
				MessageTrackingID = messageTrackingId,
				SchemaName = "SchemaName1",
				SchemaType = MessageSchemaType.Xml,
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("Message 1 stream")).CompressAndEncode()
			};

			var messages = new List<eHubGatewayMessage>();
			messages.Add(message);

			var sendStreamRequest = new SendStreamRequest { SendStreamRequestTrackingID = envelopTrackingId };
			sendStreamRequest.Messages = messages.ToArray();

			var serviceMock = MockRepository.GeneratePartialMock<eHubStreamedService>();
			var senderId = "EDIEDIDAT";
			serviceMock.Expect(_ => _.GetCurrentClientId()).Return(senderId);
			serviceMock.Expect(_ => _.IsIntegrationUserNamePasswordValidator()).Return(false);

			var kafkaClientMock = MockRepository.GenerateMock<BillingKafkaClient>("hosts", null);
			var kafkaClientMockLazy = new Lazy<BillingKafkaClient>(() => kafkaClientMock);
			kafkaClientMock.Expect(x => x.Dispose()).Repeat.Never();
			serviceMock.Expect(x => x.CreateBillingKafkaClient()).Return(kafkaClientMockLazy);
			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);

			var channelMock = MockRepository.GenerateMock<eHub2Common.IEHub2Reciever>();
			channelMock.Stub(_ => _.SendMessage(Arg<eHub2Common.eHub2GatewayMessage>.Is.Anything));

			var channelFactoryMock = MockRepository.GeneratePartialMock<ChannelFactory<eHub2Common.IEHub2Reciever>>();
			var endpointAddress = new EndpointAddress(new Uri("http://tempuri.org/"));
			channelFactoryMock.Endpoint.Address = endpointAddress;
			channelFactoryMock.Expect(_ => _.CreateChannel()).Return(channelMock);

			var handlerMock = MockRepository.GeneratePartialMock<NZCustomsTestMessageHandler>();
			handlerMock.Expect(x => x.NewChannelFactory()).Return(channelFactoryMock);
			handlerMock.Expect(x => x.CheckIfTooManyUnprocessedMessages(senderId, message.ClientID)).Do(new Action<string, string>(delegate { }));

			serviceMock.Expect(x => x.GetHandler(message)).Return(handlerMock).Repeat.Once();

			serviceMock.SendStream(sendStreamRequest);

			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);
			serviceMock.VerifyAllExpectations();
			handlerMock.VerifyAllExpectations();
			kafkaClientMock.VerifyAllExpectations();
		}
	}
}
