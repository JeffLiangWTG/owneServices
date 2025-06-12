using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub2.Common;
using CargoWise.Billing.Kafka.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using MessageSchemaType = CargoWise.eHub.Common.MessageSchemaType;

namespace CargoWise.eHub.Gateway.Tests.InboxMessageHandler
{
	[TestClass]
	public class AUCustomsMessageHandlerTests
	{
		[TestMethod]
		public void TestThrowingSystemUnderMaintananceException()
		{
			Guid envelopTrackingId = Guid.NewGuid();
			Guid messageTrackingId = Guid.NewGuid();
			var sendStreamRequest = new SendStreamRequest { SendStreamRequestTrackingID = envelopTrackingId };
			var messages = new List<eHubGatewayMessage>();

			var message = new eHubGatewayMessage
			{
				ApplicationCode = "CMR",
				ClientID = "EDIEDIDAT",
				EmailSubject = "EmailSubject1",
				FileName = "FileName1",
				MessageTrackingID = messageTrackingId,
				SchemaName = "SchemaName1",
				SchemaType = MessageSchemaType.Xml,
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("TWVzc2FnZSAxIHN0cmVhbQ==")).CompressAndEncode(),
			};

			messages.Add(message);
			sendStreamRequest.Messages = messages.ToArray();

			var serviceMock = MockRepository.GeneratePartialMock<eHubStreamedServiceMockWithHandle>();
			serviceMock.SenderId = message.ClientID;
			var handlerMock = MockRepository.GeneratePartialMock<AUCustomsMessageHandler>();

			var endpoint = new ServiceEndpoint(ContractDescription.GetContract(typeof(IEHub2Reciever)))
			{
				Address = new EndpointAddress(new Uri("net.tcp://localhost:11809/eHub2Gateway/eHub2Gateway.svc"))
			};

			var channelFactoryMock = MockRepository.GeneratePartialMock<ChannelFactory<IEHub2Reciever>>(endpoint);
			handlerMock.Expect(x => x.NewChannelFactory()).Return(channelFactoryMock).Repeat.Once();
			channelFactoryMock.Expect(x => x.CreateChannel()).Throw(new InvalidOperationException());
			handlerMock.Expect(x => x.CheckIfTooManyUnprocessedMessages(string.Empty, string.Empty)).IgnoreArguments().Do(new Action<string, string>(delegate { }));
			serviceMock.Expect(x => x.GetHandler(message)).Return(handlerMock).Repeat.Once();

			var kafkaClientMock = MockRepository.GenerateMock<BillingKafkaClient>("hosts", null);
			var kafkaClientMockLazy = new Lazy<BillingKafkaClient>(() => kafkaClientMock);
			kafkaClientMock.Expect(x => x.Dispose()).Repeat.Never();
			serviceMock.Expect(x => x.CreateBillingKafkaClient()).Return(kafkaClientMockLazy);
			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);

			try
			{
				serviceMock.SendStream(sendStreamRequest);
			}
			catch (Exception ex)
			{
				string expectedMessage = "eHub Gateway under maintenance. Messages will be resubmitted to eHub automatically on next service task run. Please ignore the following message: Server under maintenance and have not a valid ediEnterprise licence code.";
				Assert.AreEqual(ex.Message, expectedMessage);
			}
			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);
			serviceMock.VerifyAllExpectations();
			handlerMock.VerifyAllExpectations();
			channelFactoryMock.VerifyAllExpectations();
			kafkaClientMock.VerifyAllExpectations();
		}
	}
}
