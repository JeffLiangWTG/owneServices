using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Text;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using eServices.eHubDataAccess.Integration;
using eServices.eHubDataAccess.Sql;
using CargoWise.Billing.Kafka.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.Data.SqlClient;

namespace CargoWise.eHub.Gateway.Tests.InboxMessageHandler
{
	[TestClass]
	public class EHubRegistryUpdateHandlerTests
	{
		[TestMethod]
		public void TestEHubRegistryUpdateMessage_Success()
		{
			var xml = @"<eHubRegistryUpdate xmlns=""http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate""><EHINudgeURL>http://test.url/nudge</EHINudgeURL></eHubRegistryUpdate>";
			Guid envelopTrackingId = Guid.NewGuid();
			Guid messageTrackingId = Guid.NewGuid();
			var sendStreamRequest = new SendStreamRequest { SendStreamRequestTrackingID = envelopTrackingId };
			var messages = new List<eHubGatewayMessage>();
			var message = new eHubGatewayMessage
			{
				ApplicationCode = "HUB",
				ClientID = "EDIEDIDAT",
				MessageTrackingID = messageTrackingId,
				SchemaName = "http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate",
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)).CompressAndEncode(),
			};
			messages.Add(message);
			sendStreamRequest.Messages = messages.ToArray();

			var accessor = MockRepository.GeneratePartialMock<EHubClientSystemAccessor>();
			accessor.Expect(x => x.InsertOrUpdateClientSystemAndGenerateSuccessStatusMessage("EDIDAT", "http://test.url/nudge", messageTrackingId, "EDIEDIDAT")).Repeat.Once();
			var updateHandler = MockRepository.GeneratePartialMock<eHubRegistryUpdateHandler>();
			updateHandler.Expect(x => x.NewEHubClientSystemAccessor).Return(accessor).Repeat.Once();
			var serviceMock = MockRepository.GeneratePartialMock<eHubStreamedServiceTest>();
			serviceMock.Expect(x => x.GetHandler(Arg<eHubGatewayMessage>.Is.Anything)).Return(updateHandler).Repeat.Once();

			serviceMock.SendStream(sendStreamRequest);

			accessor.VerifyAllExpectations();
			updateHandler.VerifyAllExpectations();
			serviceMock.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestMissingEHINudgeURLNode_MessageExceptionDictionary_Fail()
		{
			var xml = @"<eHubRegistryUpdate xmlns=""http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate""></eHubRegistryUpdate>";
			Guid envelopTrackingId = Guid.NewGuid();
			Guid messageTrackingId = Guid.NewGuid();
			var sendStreamRequest = new SendStreamRequest { SendStreamRequestTrackingID = envelopTrackingId };
			var messages = new List<eHubGatewayMessage>();
			var message = new eHubGatewayMessage
			{
				ApplicationCode = "HUB",
				ClientID = "EDIEDIDAT",
				MessageTrackingID = messageTrackingId,
				SchemaName = "http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate",
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)).CompressAndEncode(),
			};
			messages.Add(message);
			sendStreamRequest.Messages = messages.ToArray();

			var accessor = MockRepository.GeneratePartialMock<EHubClientSystemAccessor>();
			var updateHandler = MockRepository.GeneratePartialMock<eHubRegistryUpdateHandler>();
			var serviceMock = MockRepository.GeneratePartialMock<eHubStreamedServiceTest>();
			serviceMock.Expect(x => x.GetHandler(Arg<eHubGatewayMessage>.Is.Anything)).Return(updateHandler).Repeat.Once();

			var kafkaClientMock = MockRepository.GenerateMock<BillingKafkaClient>("hosts", null);
			var kafkaClientMockLazy = new Lazy<BillingKafkaClient>(() => kafkaClientMock);
			kafkaClientMock.Expect(x => x.Dispose()).Repeat.Never();
			serviceMock.Expect(x => x.CreateBillingKafkaClient()).Return(kafkaClientMockLazy);
			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);

			try
			{
				serviceMock.SendStream(sendStreamRequest);
				Assert.Fail("An exception should have been thrown");
			}
			catch (FaultException<ApplicationFault> fex)
			{
				StringAssert.Contains(fex.Detail.MessageExceptionDictionary[message.MessageTrackingID], "Message does not contains EHI nudge url.");
			}

			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);
			kafkaClientMock.VerifyAllExpectations();
			serviceMock.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestNonRetryableSqlExceptions_Fail()
		{
			var accessor = MockRepository.GenerateMock<IEHubClientSystemAccessor>();
			var serviceMock = MockRepository.GeneratePartialMock<eHubStreamedServiceTest>();
			var kafkaClientMock = MockRepository.GenerateMock<BillingKafkaClient>("hosts", null);
			var kafkaClientMockLazy = new Lazy<BillingKafkaClient>(() => kafkaClientMock);
			kafkaClientMock.Expect(x => x.Dispose()).Repeat.Never();

			foreach (var errorCode in SqlExceptionHandler.sqlApplicationErrorCode)
			{
				var xml = @"<eHubRegistryUpdate xmlns=""http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate""><EHINudgeURL>aaaaa...999999characters...aaa</EHINudgeURL></eHubRegistryUpdate>";
				Guid envelopTrackingId = Guid.NewGuid();
				Guid messageTrackingId = Guid.NewGuid();
				var sendStreamRequest = new SendStreamRequest { SendStreamRequestTrackingID = envelopTrackingId };
				var messages = new eHubGatewayMessage[]
				{
					new eHubGatewayMessage
					{
						ApplicationCode = "HUB",
						ClientID = "EDIEDIDAT",
						MessageTrackingID = messageTrackingId,
						SchemaName = "http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate",
						MessageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)).CompressAndEncode(),
					}
				};
				sendStreamRequest.Messages = messages;

				var sqlException = SqlExceptionMock.CreateSqlException("Mock SqlException message", errorCode);
				accessor.Expect(x => x.InsertOrUpdateClientSystemAndGenerateSuccessStatusMessage("EDIDAT", "aaaaa...999999characters...aaa", messageTrackingId, "EDIEDIDAT"))
					.Throw(sqlException).Repeat.Once();
				var updateHandler = MockRepository.GeneratePartialMock<eHubRegistryUpdateHandler>();
				updateHandler.Expect(x => x.NewEHubClientSystemAccessor).Return(accessor).Repeat.Once();
				serviceMock.Expect(x => x.GetHandler(Arg<eHubGatewayMessage>.Is.Anything)).Return(updateHandler).Repeat.Once();
				serviceMock.Expect(x => x.CreateBillingKafkaClient()).Return(kafkaClientMockLazy);
				Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);
				try
				{
					serviceMock.SendStream(sendStreamRequest);
					Assert.Fail("An exception should have been thrown");
				}
				catch (FaultException<ApplicationFault> ex)
				{
					StringAssert.StartsWith(ex.Detail.ErrorMessage, "1 errors occured during processing send request:\r\nMock SqlException message\r\nIncorrect function\r\n");
				}
				Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);
			}

			accessor.VerifyAllExpectations();
			kafkaClientMock.VerifyAllExpectations();
			serviceMock.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestRetryableSqlException1001_ShouldCauseSystemUnderMaintainanceException_Fail()
		{
			var xml = @"<eHubRegistryUpdate xmlns=""http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate""><EHINudgeURL>http://test.url/nudge</EHINudgeURL></eHubRegistryUpdate>";
			Guid envelopTrackingId = Guid.NewGuid();
			Guid messageTrackingId = Guid.NewGuid();
			var sendStreamRequest = new SendStreamRequest { SendStreamRequestTrackingID = envelopTrackingId };
			var messages = new List<eHubGatewayMessage>();
			var message = new eHubGatewayMessage
			{
				ApplicationCode = "HUB",
				ClientID = "EDIEDIDAT",
				MessageTrackingID = messageTrackingId,
				SchemaName = "http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate",
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)).CompressAndEncode(),
			};
			messages.Add(message);
			sendStreamRequest.Messages = messages.ToArray();

			var accessor = MockRepository.GenerateMock<IEHubClientSystemAccessor>();
			var sqlException = SqlExceptionMock.CreateSqlException("Length or precision specification is invalid.", 1001);
			accessor.Expect(x => x.InsertOrUpdateClientSystemAndGenerateSuccessStatusMessage("EDIDAT", "http://test.url/nudge", messageTrackingId, "EDIEDIDAT")).
				Throw(sqlException).Repeat.Once();
			var updateHandler = MockRepository.GeneratePartialMock<eHubRegistryUpdateHandler>();
			updateHandler.Expect(x => x.NewEHubClientSystemAccessor).Return(accessor).Repeat.Once();
			var serviceMock = MockRepository.GeneratePartialMock<eHubStreamedServiceTest>();
			serviceMock.Expect(x => x.GetHandler(Arg<eHubGatewayMessage>.Is.Anything)).Return(updateHandler).Repeat.Once();

			var kafkaClientMock = MockRepository.GenerateMock<BillingKafkaClient>("hosts", null);
			var kafkaClientMockLazy = new Lazy<BillingKafkaClient>(() => kafkaClientMock);
			kafkaClientMock.Expect(x => x.Dispose()).Repeat.Never();
			serviceMock.Expect(x => x.CreateBillingKafkaClient()).Return(kafkaClientMockLazy);
			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);

			try
			{
				serviceMock.SendStream(sendStreamRequest);
				Assert.Fail("An exception should have been thrown");
			}
			catch (SystemException e)
			{
				StringAssert.StartsWith(e.Message, "eHub Gateway under maintenance. Messages will be resubmitted to eHub automatically on next service task run. Please ignore the following message: Server under maintenance and have not a valid ediEnterprise licence code.");
				Assert.AreEqual(typeof(SystemUnderMaintananceException), e.InnerException.GetType());
				Assert.AreEqual(sqlException, e.InnerException.InnerException);
			}

			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);
			accessor.VerifyAllExpectations();
			kafkaClientMock.VerifyAllExpectations();
			serviceMock.VerifyAllExpectations();
		}

		public class eHubStreamedServiceTest : eHubStreamedService
		{
			public override string GetCurrentClientId()
			{
				return "EDIEDIDAT";
			}

			public override bool IsIntegrationUserNamePasswordValidator()
			{
				return false;
			}
		}
	}
}
