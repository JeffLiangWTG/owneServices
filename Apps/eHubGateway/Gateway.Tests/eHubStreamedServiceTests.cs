using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using eServices.eHubDataAccess.Integration;
using CargoWise.eHub.Gateway.OpenAPIs.XHGateway;
using CargoWise.eHub.Gateway.Tests.InboxMessageHandler;
using CargoWise.Billing.Kafka.API;
using CargoWise.Billing.Client;
using Confluent.Kafka;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rhino.Mocks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using CargoWise.Billing.API;
using Common.Logging;
using APIBillingTransaction = CargoWise.Billing.API.BillingTransaction;
using MockRepository = Rhino.Mocks.MockRepository;

namespace CargoWise.eHub.Gateway.Tests
{
	[TestClass]
	public class eHubStreamedServiceTests
	{
		[TestInitialize]
		public void Initialize()
		{
			new BillingMessageWithBlacklistingHandlerTests.BillingMessageWithBlacklistingHandlerMock().BlacklistedBillingClients.Clear();
			BillingMessageHandler.BillingKafkaTopic = "billing-topic";
			BillingMessageHandler.BillingKafkaConfig = ServiceHelper.GetKafkaProducerConfig();
		}

		[TestMethod]
		public void eHubStreamedService_CheckClientIdOfSenderAndMessage_WarnLogging()
		{
			var service = new eHubStreamedServiceMockWithValidSenderID();
			var sendStreamRequest = new SendStreamRequest { SendStreamRequestTrackingID = Guid.NewGuid() };

			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <BillableCount>2</BillableCount>
  <ClientID>ABCDEFXYZ</ClientID>
  <ClientNumber>98765432100123456789</ClientNumber>
  <ClientStaffCode>ABC</ClientStaffCode>
  <PriceItemCode>DEF</PriceItemCode>
  <Reference1>REFERENCE 1</Reference1>
  <Reference2>REFERENCE 2</Reference2>
  <Reference3>REFERENCE 3</Reference3>
  <ReportingSource>XYZ</ReportingSource>
  <ServiceOccuredUTC>2014-10-02T08:50:30</ServiceOccuredUTC>
</BillingTransaction>";
			var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
			sendStreamRequest.Messages = new[] {
				new eHubGatewayMessage
				{
					ApplicationCode = "SCV",
					ClientID = "HYEDAUUAT",
					EmailSubject = "EmailSubject",
					FileName = "FileName",
					MessageTrackingID = Guid.NewGuid(),
					SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.1",
					SchemaType = MessageSchemaType.Xml,
					MessageStream = messageStream.CompressAndEncode()
				}
			};

			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);
				service.SendStream(sendStreamRequest);
				var serviceOccurredUtc = new DateTime(2014, 10, 2, 8, 50, 30).ToString("s");
				StringAssert.Contains(sw.ToString(), $"[WARN]  ClientMismatch - System ID 'ABCDEFXYZ' with IP <UNKNOWN> is not match with Auth ID 'CLIENT_ID'. Billing Transaction: Version: 0, Category: UNK, PriceItemCode: DEF, BillableCount: 2, ReportingSource: XYZ, ServiceOccuredUTC: {serviceOccurredUtc}, ClientID: ABCDEFXYZ, ClientNumber: 98765432100123456789, ClientStaffCode: ABC, Branch: , Reference1: REFERENCE 1, Reference2: REFERENCE 2, Reference3: REFERENCE 3, Reference4: , Reference5: , MessageTrackingID: ");
			}
		}

		[TestMethod]
		public void eHubStreamedService_CheckClientAuthorisation_Successfully()
		{
			var streamRequest = new SendStreamRequest();
			streamRequest.SendStreamRequestTrackingID = Guid.NewGuid();

			var recipientId = "HYEDAUUAT";
			var service = MockRepository.GeneratePartialMock<eHubStreamedServiceMockWithHandleAndClientAuthorisation>();
			var mockSecurityAccessor = MockRepository.GenerateMock<ISecurityAccessor>();
			service.Stub(_ => _.SecurityAccessor).Return(mockSecurityAccessor);
			mockSecurityAccessor.Expect(_ => _.CheckClientAuthorisation("CLIENT_ID", recipientId)).Return(true);

			service.Expect(_ => _.LogTransaction(Arg<string>.Is.Equal("OK"),
									 Arg<Func<string>>.Is.Anything,
									 Arg<string>.Is.Equal("SendStream"))).Repeat.Once();

			var mockKafkaClient = MockRepository.GeneratePartialMock<BillingKafkaClient>(BillingMessageHandler.BillingKafkaConfig, null);
			var kafkaClientMockLazy = new Lazy<BillingKafkaClient>(() => mockKafkaClient);
			mockKafkaClient.Expect(x => x.Dispose()).Repeat.Once();
			service.Expect(x => x.CreateBillingKafkaClient()).Return(kafkaClientMockLazy);
			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);

			var sendStreamRequest = new SendStreamRequest { SendStreamRequestTrackingID = Guid.NewGuid() };

			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <BillableCount>2</BillableCount>
  <ClientID>ABCDEFXYZ</ClientID>
  <ClientNumber>98765432100123456789</ClientNumber>
  <ClientStaffCode>ABC</ClientStaffCode>
  <PriceItemCode>DEF</PriceItemCode>
  <Reference1>REFERENCE 1</Reference1>
  <Reference2>REFERENCE 2</Reference2>
  <Reference3>REFERENCE 3</Reference3>
  <ReportingSource>XYZ</ReportingSource>
  <ServiceOccuredUTC>2014-10-02T08:50:30</ServiceOccuredUTC>
</BillingTransaction>";
			var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
			sendStreamRequest.Messages = new[] {
				new eHubGatewayMessage
				{
					ApplicationCode = "SCV",
					ClientID = "HYEDAUUAT",
					EmailSubject = "EmailSubject",
					FileName = "FileName",
					MessageTrackingID = Guid.NewGuid(),
					SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.1",
					SchemaType = MessageSchemaType.Xml,
					MessageStream = messageStream.CompressAndEncode()
				}
			};
			service.SendStream(sendStreamRequest);
			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, true);
			mockKafkaClient.VerifyAllExpectations();
			service.VerifyAllExpectations();
		}

		[TestMethod]
		public void eHubStreamedService_LicenceTypeIsNull_NotCheck()
		{
			System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			try
			{
				config.AppSettings.Settings.Add("ShouldForwardToProxyGateway", "true");
				config.Save(ConfigurationSaveMode.Modified);
				ConfigurationManager.RefreshSection("appSettings");

				var recipientId = "HYEDAUUAT";
				var service = MockRepository.GeneratePartialMock<eHubStreamedServiceMockWithHandleAndClientAuthorisation>();
				var mockEnterpriseExeDetailAccessor = MockRepository.GenerateMock<IEnterpriseExeDetailAccessor>();
				var mockSecurityAccessor = MockRepository.GenerateMock<ISecurityAccessor>();
				var mockTransformAccessor = MockRepository.GenerateMock<ITransformAccessor>();
				service.Stub(_ => _.EnterpriseExeDetailAccessor).Return(mockEnterpriseExeDetailAccessor);
				service.Stub(_ => _.SecurityAccessor).Return(mockSecurityAccessor);
				service.Stub(_ => _.TransformAccessor).Return(mockTransformAccessor);

				var clientId = service.GetCurrentClientId();
				mockEnterpriseExeDetailAccessor.Expect(_ => _.GetLicenceType(clientId)).Return(null);
				mockSecurityAccessor.Expect(_ => _.CheckClientAuthorisation("CLIENT_ID", recipientId)).Return(true);

				var mockKafkaClient = MockRepository.GeneratePartialMock<BillingKafkaClient>(BillingMessageHandler.BillingKafkaConfig, null);
				var kafkaClientMockLazy = new Lazy<BillingKafkaClient>(() => mockKafkaClient);
				mockKafkaClient.Expect(x => x.Dispose()).Repeat.Once();
				service.Expect(x => x.CreateBillingKafkaClient()).Return(kafkaClientMockLazy);

				service.Expect(_ => _.LogTransaction(Arg<string>.Is.Equal("OK"),
										 Arg<Func<string>>.Is.Anything,
										 Arg<string>.Is.Equal("SendStream"))).Repeat.Once();

				Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);

				var sendStreamRequest = new SendStreamRequest { SendStreamRequestTrackingID = Guid.NewGuid() };
				string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <BillableCount>2</BillableCount>
  <ClientID>ABCDEFXYZ</ClientID>
  <ClientNumber>98765432100123456789</ClientNumber>
  <ClientStaffCode>ABC</ClientStaffCode>
  <PriceItemCode>DEF</PriceItemCode>
  <Reference1>REFERENCE 1</Reference1>
  <Reference2>REFERENCE 2</Reference2>
  <Reference3>REFERENCE 3</Reference3>
  <ReportingSource>XYZ</ReportingSource>
  <ServiceOccuredUTC>2014-10-02T08:50:30</ServiceOccuredUTC>
</BillingTransaction>";
				var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
				sendStreamRequest.Messages = new[] {
				new eHubGatewayMessage
				{
					ApplicationCode = "SCV",
					ClientID = "HYEDAUUAT",
					EmailSubject = "EmailSubject",
					FileName = "FileName",
					MessageTrackingID = Guid.NewGuid(),
					SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.1",
					SchemaType = MessageSchemaType.Xml,
					MessageStream = messageStream.CompressAndEncode()
				}
			};
				service.SendStream(sendStreamRequest);

				mockTransformAccessor.AssertWasNotCalled(_ => _.GetRecipientCode("eHub", "eHub", "Forward To Proxy Gateway", "Forward To Proxy Gateway", "Should Forward", clientId, recipientId, null, null, null));
				Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, true);
				mockKafkaClient.VerifyAllExpectations();
				service.VerifyAllExpectations();
			}
			finally 
			{
				config.AppSettings.Settings.Remove("ShouldForwardToProxyGateway");
				config.Save(ConfigurationSaveMode.Modified);
				ConfigurationManager.RefreshSection("appSettings");
			}
		}

		[TestMethod]
		public void eHubStreamedService_CheckClientAuthorisation_ThrowException()
		{
			var streamRequest = new SendStreamRequest();
			streamRequest.SendStreamRequestTrackingID = Guid.NewGuid();

			var service = MockRepository.GeneratePartialMock<eHubStreamedServiceMockWithClientAuthorisation>();
			var mockSecurityAccessor = MockRepository.GenerateMock<ISecurityAccessor>();
			service.Stub(_ => _.SecurityAccessor).Return(mockSecurityAccessor);
			mockSecurityAccessor.Expect(_ => _.CheckClientAuthorisation("CLIENT_ID", "wrong recipient")).Return(false);
			var mockKafkaClient = MockRepository.GeneratePartialMock<BillingKafkaClient>(BillingMessageHandler.BillingKafkaConfig, null);
			var kafkaClientMockLazy = new Lazy<BillingKafkaClient>(() => mockKafkaClient);
			mockKafkaClient.Expect(x => x.Dispose()).Repeat.Never();
			service.Expect(x => x.CreateBillingKafkaClient()).Return(kafkaClientMockLazy);
			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);

			var sendStreamRequest = new SendStreamRequest { SendStreamRequestTrackingID = Guid.NewGuid() };

			sendStreamRequest.Messages = new[] {
				new eHubGatewayMessage
				{
					ApplicationCode = "SCV",
					ClientID = "HYEDAUUAT",
					EmailSubject = "EmailSubject",
					FileName = "FileName",
					MessageTrackingID = Guid.NewGuid(),
					SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.1",
					SchemaType = MessageSchemaType.Xml,
					MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("Message 1 stream")).CompressAndEncode()
				}
			};

			try
			{
				service.SendStream(sendStreamRequest);
			}
			catch (FaultException ex)
			{
				Assert.AreEqual("Client CLIENT_ID is not authorised for sending to HYEDAUUAT.", ex.Message);
			}

			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);
			mockKafkaClient.VerifyAllExpectations();
			service.VerifyAllExpectations();
		}

		[TestMethod]
		public void eHubStreamedService_SendStream_XHGatewayExceptionThrowsFaultException()
		{
			IReadOnlyDictionary<string, IEnumerable<string>> headers = new Dictionary<string, IEnumerable<string>>();
			var expectedErrorMessage = "Message is rejected by xT Server";

			var service = new eHubStreamedServiceMock
			{
				HandleMessageExceptionToThrow = new XHGatewayException(expectedErrorMessage, 406, "", headers, null)
			};

			Guid envelopTrackingId = Guid.NewGuid();
			var sendStreamRequest = new SendStreamRequest { SendStreamRequestTrackingID = envelopTrackingId };
			var messages = new List<eHubGatewayMessage>
				{
					new eHubGatewayMessage
					{
						ApplicationCode = "UDM",
						ClientID = "TestxT",
						EmailSubject = "EmailSubject",
						FileName = "FileName",
						MessageTrackingID = Guid.NewGuid(),
						SchemaName = "SchemaName",
						SchemaType = MessageSchemaType.Xml,
						MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("Message stream"))
					}
				};

			sendStreamRequest.Messages = messages.ToArray();

			try
			{
				service.SendStream(sendStreamRequest);
				Assert.Fail("An exception should have been thrown");
			}
			catch (FaultException<ApplicationFault> fex)
			{
				Assert.IsTrue(fex.Detail.ErrorMessage.Contains("1 errors occured during processing send request:"));
				Assert.IsTrue(fex.Detail.ErrorMessage.Contains(expectedErrorMessage));
				var exceptionDictionary = fex.Detail.MessageExceptionDictionary;
				Assert.AreEqual(exceptionDictionary.Count, 1);
				Assert.IsTrue(exceptionDictionary[messages[0].MessageTrackingID].Contains(expectedErrorMessage));
			}
		}

		[TestMethod]
		public void eHubStreamedService_SendStream_XHGatewayExceptionThrowsSystemMaintainanceException()
		{
			IReadOnlyDictionary<string, IEnumerable<string>> headers = new Dictionary<string, IEnumerable<string>>();

			var service = new eHubStreamedServiceMock
			{
				HandleMessageExceptionToThrow = new XHGatewayException("", 500, "", headers, null)
			};

			Guid envelopTrackingId = Guid.NewGuid();
			var sendStreamRequest = new SendStreamRequest { SendStreamRequestTrackingID = envelopTrackingId };
			var messages = new List<eHubGatewayMessage>
				{
					new eHubGatewayMessage
					{
						ApplicationCode = "UDM",
						ClientID = "TestxT",
						EmailSubject = "EmailSubject",
						FileName = "FileName",
						MessageTrackingID = Guid.NewGuid(),
						SchemaName = "SchemaName",
						SchemaType = MessageSchemaType.Xml,
						MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("Message stream"))
					}
				};

			sendStreamRequest.Messages = messages.ToArray();

			try
			{
				service.SendStream(sendStreamRequest);
				Assert.Fail("An exception should have been thrown");
			}
			catch (SystemUnderMaintananceException ex)
			{
				Assert.IsTrue(ex.Message.Contains(XHGatewayExceptionHandler.DefaultMessage));
			}
		}

		[TestMethod]
		public void eHubStreamedService_SendStream_MessageExceptionDictionary()
		{
			var streamRequest = new SendStreamRequest();
			streamRequest.SendStreamRequestTrackingID = Guid.NewGuid();

			var messages = new List<eHubGatewayMessage>();

			// Message 1 - Produce InvalidOperationException and cache ToString
			var message1 = new eHubGatewayMessage
			{
				ApplicationCode = "SCV",
				ClientID = "XXXXXXXXX",
				EmailSubject = "EmailSubject1",
				FileName = "FileName1",
				MessageTrackingID = Guid.NewGuid(),
				SchemaType = MessageSchemaType.Xml,
				SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.0",
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("Message 1 stream")).CompressAndEncode()
			};
			messages.Add(message1);

			streamRequest.Messages = messages.ToArray();

			var service = new eHubStreamedServiceMockWithHandle();

			try
			{
				service.SenderId = message1.ClientID;
				service.SendStream(streamRequest);
				Assert.Fail("An exception should have been thrown");
			}
			catch (FaultException<ApplicationFault> fex)
			{
				Assert.IsTrue(fex.Detail.MessageExceptionDictionary.ContainsKey(message1.MessageTrackingID));
			}

			// Message 2 - Produce InvalidOperationException and get cached ToString
			messages.Clear();

			var message2 = new eHubGatewayMessage
			{
				ApplicationCode = "SCV",
				ClientID = "XXXXXXXXX",
				EmailSubject = "EmailSubject1",
				FileName = "FileName1",
				MessageTrackingID = Guid.NewGuid(),
				SchemaType = MessageSchemaType.Xml,
				SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.0",
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("Message 2 stream")).CompressAndEncode()
			};
			messages.Add(message2);

			streamRequest.Messages = messages.ToArray();

			try
			{
				service.SenderId = message2.ClientID;
				service.SendStream(streamRequest);
				Assert.Fail("An exception should have been thrown");
			}
			catch (FaultException<ApplicationFault> fex)
			{
				Assert.IsTrue(fex.Detail.MessageExceptionDictionary.ContainsKey(message2.MessageTrackingID));
			}


			// Message 3 - Produce InvalidOperationException and cache ToString (different client)
			messages.Clear();

			var message3 = new eHubGatewayMessage
			{
				ApplicationCode = "SCV",
				ClientID = "YYYYYYYYY",
				EmailSubject = "EmailSubject1",
				FileName = "FileName1",
				MessageTrackingID = Guid.NewGuid(),
				SchemaType = MessageSchemaType.Xml,
				SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.0",
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("Message 2 stream")).CompressAndEncode()
			};
			messages.Add(message3);

			streamRequest.Messages = messages.ToArray();

			try
			{
				service.SenderId = message3.ClientID;
				service.SendStream(streamRequest);
				Assert.Fail("An exception should have been thrown");
			}
			catch (FaultException<ApplicationFault> fex)
			{
				Assert.IsTrue(fex.Detail.MessageExceptionDictionary.ContainsKey(message3.MessageTrackingID));
			}
		}

		[TestMethod]
		public void eHubStreamedService_SendStream_ExceptionToStringNoCache()
		{
			var streamRequest = new SendStreamRequest();
			streamRequest.SendStreamRequestTrackingID = Guid.NewGuid();

			var messages = new List<eHubGatewayMessage>();

			// Message 1 - Produce InvalidDataException and do NOT cache ToString
			var message1 = new eHubGatewayMessage
			{
				ApplicationCode = "SCV",
				ClientID = "ZZZZZZZZZ",
				EmailSubject = "EmailSubject1",
				FileName = "FileName1",
				MessageTrackingID = Guid.NewGuid(),
				SchemaType = MessageSchemaType.Xml,
				SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.0",
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("Message 1 stream"))
			};
			messages.Add(message1);

			streamRequest.Messages = messages.ToArray();

			var service = new eHubStreamedServiceMockWithHandle();

			try
			{
				service.SenderId = message1.ClientID;
				service.SendStream(streamRequest);
				Assert.Fail("An exception should have been thrown");
			}
			catch (FaultException<ApplicationFault> fex)
			{
				Assert.IsTrue(fex.Detail.MessageExceptionDictionary.ContainsKey(message1.MessageTrackingID), "Missing Message Exception");
				Assert.IsTrue(fex.Detail.MessageExceptionDictionary[message1.MessageTrackingID].Contains("Found invalid data while decoding"), "Wrong Exception Type: " + fex.Detail.MessageExceptionDictionary[message1.MessageTrackingID]);
			}
		}

		[TestMethod]
		public void TesteHubStreamService_TooManyUnprocessedMessage_ForThoseHandlersWhoCheck()
		{
			var streamRequest = new SendStreamRequest();
			streamRequest.SendStreamRequestTrackingID = Guid.NewGuid();

			var messages = new List<eHubGatewayMessage>();

			var message1 = new eHubGatewayMessage
			{
				ApplicationCode = "CIM",
				ClientID = "HYEDAUUAT",
				EmailSubject = "EmailSubject1",
				FileName = "FileName1",
				MessageTrackingID = Guid.NewGuid(),
				SchemaName = "SchemaName1",
				SchemaType = MessageSchemaType.Xml,
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("Message 1 stream"))
			};
			messages.Add(message1);

			streamRequest.Messages = messages.ToArray();

			var service = MockRepository.GeneratePartialMock<eHubStreamedServiceMockWithHandle>();
			service.SenderId = message1.ClientID;
			service.Expect(_ => _.LogTransaction(Arg<string>.Is.Equal("ER"),
												 Arg<Func<string>>.Is.Anything,
												 Arg<string>.Is.Equal("SendStream"))).Repeat.Once();
			var handler = MockRepository.GeneratePartialMock<AirMessageHandler>();
			handler.Stub(_ => _.CheckIfTooManyUnprocessedMessages(string.Empty, string.Empty)).IgnoreArguments().Throw(new SystemThrottleException("The server has refused new messages because it is still processing your previously sent messages. You do not need to take action, the service task will attempt to transfer them on the next run. Please ignore the following message: Too Many Requests not a valid ediEnterprise licence code.")).Repeat.Any();
			service.setHandler(message1.MessageTrackingID, handler);

			var mockKafkaClient = MockRepository.GeneratePartialMock<BillingKafkaClient>(BillingMessageHandler.BillingKafkaConfig, null);
			var kafkaClientMockLazy = new Lazy<BillingKafkaClient>(() => mockKafkaClient);
			mockKafkaClient.Expect(x => x.Dispose()).Repeat.Never();
			service.Expect(x => x.CreateBillingKafkaClient()).Return(kafkaClientMockLazy);
			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);

			try
			{
				service.SendStream(streamRequest);
				Assert.Fail("An exception should have been thrown");
			}
			catch (SystemException se)
			{
				Assert.AreEqual("The server has refused new messages because it is still processing your previously sent messages. You do not need to take action, the service task will attempt to transfer them on the next run. Please ignore the following message: Too Many Requests not a valid ediEnterprise licence code.", se.Message);
			}

			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);
			mockKafkaClient.VerifyAllExpectations();
			service.VerifyAllExpectations();
		}

		[TestMethod]
		public void TesteHubStreamService_TransformZACustomsClientID()
		{
			var outboxMock = MockRepository.GenerateMock<IOutboxAccessor>();

			var messages = new eHubGatewayMessage[4];

			var message1 = new eHubGatewayMessage
			{
				ApplicationCode = "ZAC",
				ClientID = "ZACustomsTest",
				EmailSubject = "",
				FileName = "FileName1",
				MessageTrackingID = Guid.NewGuid(),
				SchemaName = "SchemaName1",
				SchemaType = MessageSchemaType.FlatFile,
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("Message 1 stream"))
			};
			messages[0] = message1;

			var message2 = new eHubGatewayMessage
			{
				ApplicationCode = "UDM",
				ClientID = "ZACustomsDocTest",
				EmailSubject = "",
				FileName = "FileName2",
				MessageTrackingID = Guid.NewGuid(),
				SchemaName = "SchemaName2",
				SchemaType = MessageSchemaType.Xml,
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("Message 2 stream"))
			};
			messages[1] = message2;

			var message3 = new eHubGatewayMessage
			{
				ApplicationCode = "UDM",
				ClientID = "ZACustomsDoc",
				EmailSubject = "",
				FileName = "FileName3",
				MessageTrackingID = Guid.NewGuid(),
				SchemaName = "SchemaName3",
				SchemaType = MessageSchemaType.Xml,
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("Message 3 stream"))
			};
			messages[2] = message3;

			var message4 = new eHubGatewayMessage
			{
				ApplicationCode = "UDM",
				ClientID = "TSTTSTTST",
				EmailSubject = "",
				FileName = "FileName4",
				MessageTrackingID = Guid.NewGuid(),
				SchemaName = "SchemaName4",
				SchemaType = MessageSchemaType.Xml,
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("Message 4 stream"))
			};
			messages[3] = message4;

			outboxMock.Stub(_ => _.GetMessageBatch(string.Empty, string.Empty, 1, 1, 1)).IgnoreArguments().Return(messages);

			var retrieveStreamByCompanyListRequest = new RetrieveStreamByCompanyListRequest { CompanyIDs = new string[] { "TST001TST", "TST002TST" } };

			var service = MockRepository.GeneratePartialMock<eHubStreamedServiceMockWithHandle>();
			service.SenderId = "TSTTSTTST";

			service.Stub(_ => _.OutboxAccessor).Return(outboxMock);
			service.Expect(_ => _.LogTransaction(Arg<string>.Is.Equal("OK"),
												 Arg<Func<string>>.Is.Anything,
												 Arg<string>.Is.Equal("RetrieveStream"))).Repeat.Once();
			service.Expect(_ => _.LogTransaction(Arg<string>.Is.Equal("OK"),
												 Arg<Func<string>>.Is.Anything,
												 Arg<string>.Is.Equal("RetrieveStreamByCompanyList"))).Repeat.Once();

			var retrievedStream = service.RetrieveStream();
			var retrievedCompanyListStream = service.RetrieveStreamByCompanyList(retrieveStreamByCompanyListRequest);

			CollectionAssert.AreEqual(new string[] { "ZACustoms", "ZACustoms", "ZACustoms", "TSTTSTTST" },
									  retrievedStream.Messages.Select(msg => msg.ClientID).ToArray(),
									  "RetrievedStreamResponse");

			CollectionAssert.AreEqual(new string[] { "ZACustoms", "ZACustoms", "ZACustoms", "TSTTSTTST" },
									  retrievedCompanyListStream.CompanyToMessagesMap["TST001TST"].Select(msg => msg.ClientID).ToArray(),
									  "RetrievedStreamByCompanyListResponse[TST001TST]");

			CollectionAssert.AreEqual(new string[] { "ZACustoms", "ZACustoms", "ZACustoms", "TSTTSTTST" },
									  retrievedCompanyListStream.CompanyToMessagesMap["TST002TST"].Select(msg => msg.ClientID).ToArray(),
									  "RetrievedStreamByCompanyListResponse[TST002TST]");

			service.VerifyAllExpectations();
		}

		[TestMethod]
		public void eHubStreamedService_SendStream_ExceptionThrown_OptimizeDictionary()
		{
			var service = new eHubStreamedServiceMock();
			service.HandleMessageExceptionToThrow = new Exception("Some error.");

			Guid envelopTrackingId = Guid.NewGuid();
			Guid message1TrackingId = Guid.NewGuid();
			Guid message2TrackingId = Guid.NewGuid();
			var sendStreamRequest = new SendStreamRequest { SendStreamRequestTrackingID = envelopTrackingId };
			var messages = new List<eHubGatewayMessage>();

			var message1 = new eHubGatewayMessage
			{
				ApplicationCode = "USC",
				ClientID = "HYEDAUUAT",
				EmailSubject = "EmailSubject1",
				FileName = "FileName1",
				MessageTrackingID = message1TrackingId,
				SchemaName = "SchemaName1",
				SchemaType = MessageSchemaType.Xml,
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("Message 1 stream"))
			};


			var message2 = new eHubGatewayMessage
			{
				ApplicationCode = "USC",
				ClientID = "HYEDAUUAT",
				EmailSubject = "EmailSubject2",
				FileName = "FileName2",
				MessageTrackingID = message2TrackingId,
				SchemaName = "SchemaName2",
				SchemaType = MessageSchemaType.FlatFile,
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("Message 2 stream"))
			};

			messages.Add(message1);
			messages.Add(message2);
			sendStreamRequest.Messages = messages.ToArray();
			var exceptionMessageDictionary = new Dictionary<Guid, String>();

			try
			{
				service.SendStream(sendStreamRequest);
			}
			catch (FaultException<ApplicationFault> ex)
			{
				StringAssert.Contains(ex.Message, "System.Exception: Some error");
				exceptionMessageDictionary = ex.Detail.MessageExceptionDictionary;
			}
			Assert.AreEqual(2, exceptionMessageDictionary.Count);

			Assert.AreEqual(message1TrackingId, exceptionMessageDictionary.First(x => x.Value.Contains("Some error")).Key);
			Assert.AreEqual(message2TrackingId, exceptionMessageDictionary.First(x => x.Value.Equals("This Exception is the same as " + message1TrackingId)).Key);
		}

		[TestMethod]
		public void eHubStreamedService_SendStream_WithManyMessages_CombineExceptionsAndThrow()
		{
			var exceptionText = @"This is a reasonably lenghty error description to test large amount of exception data being returned via the service.
				Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor.
				Aenean massa.Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus.
				Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem.Nulla consequat massa quis enim.Donec pede justo, fringilla vel, aliquet nec, vulputate.
				Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo.
				Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt.
				Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur?
				Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla pariatur?";

			for (int i = 0; i < 10; i++)
			{
				exceptionText += exceptionText;
			}
			var service = new eHubStreamedServiceMock();
			service.HandleMessageExceptionToThrow = new Exception(exceptionText);

			var stringLimit = 307200;

			Guid envelopTrackingId = Guid.NewGuid();
			var sendStreamRequest = new SendStreamRequest { SendStreamRequestTrackingID = envelopTrackingId };
			var messages = new List<eHubGatewayMessage>();

			var message = new eHubGatewayMessage
			{
				ApplicationCode = "USC",
				ClientID = "HYEDAUUAT",
				EmailSubject = "EmailSubject",
				FileName = "FileName",
				MessageTrackingID = Guid.NewGuid(),
				SchemaName = "SchemaName",
				SchemaType = MessageSchemaType.Xml,
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("Message stream"))
			};
			messages.Add(message);

			sendStreamRequest.Messages = messages.ToArray();

			try
			{
				service.SendStream(sendStreamRequest);
				Assert.Fail("An exception should have been thrown");
			}
			catch (FaultException<ApplicationFault> fex)
			{
				Assert.IsTrue(fex.Detail.ErrorMessage.Length > stringLimit);
			}
		}

		[TestMethod]
		public void eHubStreamedService_SendStream_GivenInvalidClientID_ShouldThrowException_NoStackTrace()
		{
			var errorMessageArray = new string[]
			{
				"Recipient ID fsdfsrgt could not be found. Sender ID is HYEDAUUAT.",
				"Sender ID InvalidSenderID could not be found",
				"Client System ID InvalidSystemID could not be found"
			};

			foreach (var expectedErrorMessage in errorMessageArray)
			{
				var service = new eHubStreamedServiceMock
				{
					HandleMessageExceptionToThrow = SqlExceptionMock.CreateSqlException(expectedErrorMessage, 50000)
				};

				Guid envelopTrackingId = Guid.NewGuid();
				var sendStreamRequest = new SendStreamRequest { SendStreamRequestTrackingID = envelopTrackingId };
				var messages = new List<eHubGatewayMessage>
				{
					new eHubGatewayMessage
					{
						ApplicationCode = "USC",
						ClientID = "InvalidClientID",
						EmailSubject = "EmailSubject",
						FileName = "FileName",
						MessageTrackingID = Guid.NewGuid(),
						SchemaName = "SchemaName",
						SchemaType = MessageSchemaType.Xml,
						MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("Message stream"))
					}
				};

				sendStreamRequest.Messages = messages.ToArray();

				try
				{
					service.SendStream(sendStreamRequest);
					Assert.Fail("An exception should have been thrown");
				}
				catch (FaultException<ApplicationFault> fex)
				{
					Assert.IsTrue(fex.Detail.ErrorMessage.Contains($"1 errors occured during processing send request:\r\n{expectedErrorMessage}"));
					var exceptionDictionary = fex.Detail.MessageExceptionDictionary;
					Assert.AreEqual(exceptionDictionary.Count, 1);
					Assert.IsTrue(exceptionDictionary[messages[0].MessageTrackingID].Contains(expectedErrorMessage));
				}
			}
		}

		[TestMethod]
		public void eHubStreamedService_SendStream_BillingHandlerHasError_Kafka()
		{
			BillingMessageHandler.SendBillingToKafka = true;
			var billingContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns=""http://www.edi.com.au/EnterpriseService/#Billing_1.4"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <BillableCount>2</BillableCount>
  <Branch>KLM</Branch>
  <ClientID>ABCDEFXYZ</ClientID>
  <ClientNumber>98765432100123456789</ClientNumber>
  <ClientStaffCode>ABC</ClientStaffCode>
  <PriceItemCode>DEF</PriceItemCode>
  <Category>TST</Category>
  <Reference1>REFERENCE 1</Reference1>
  <Reference2>REFERENCE 2</Reference2>
  <Reference3>REFERENCE 3</Reference3>
  <Reference4>REFERENCE 4</Reference4>
  <Reference5>REFERENCE 5</Reference5>
  <ReportingSource>XYZ</ReportingSource>
  <ServiceOccuredUTC>{0}</ServiceOccuredUTC>
  <Version>4</Version>
  <MessageTrackingID>{1}</MessageTrackingID>
  <AdditionalRefs>{{""objetKey"":""value""}}</AdditionalRefs>
</BillingTransaction>";
			var streamRequest = new SendStreamRequest();
			streamRequest.SendStreamRequestTrackingID = Guid.NewGuid();
			var issuerManagerMock = new Mock<IssueManager>();
			issuerManagerMock.Setup(x => x.ReportToIssueManager(It.IsAny<string>(), It.IsAny<Exception>(),
				It.IsAny<ILog>(), It.IsAny<NameValueCollection>(), false));

			var call = 1;
			var producerMock = new Mock<IBillingTransactionProducer<string, APIBillingTransaction>>();
			producerMock.Setup(_ => _.Flush());
			producerMock.Setup(_ => _.Produce(It.Is<string>(topic => topic.Equals("billing-topic")), It.IsAny<Message<string, APIBillingTransaction>>(), It.IsAny<Action<DeliveryReport<string, APIBillingTransaction>>>()))
				.Callback<string, Message<string, APIBillingTransaction>, Action<DeliveryReport<string, APIBillingTransaction>>>(
					(topic, message, deliveryHandler) =>
					{
						var topicPartitionOffset = new TopicPartitionOffset(topic, new Partition(0), Offset.Unset);
						var report = new DeliveryReport<string, APIBillingTransaction>
						{
							Error = new Error(call == 2 ? ErrorCode.BrokerNotAvailable : ErrorCode.NoError),
							Message = new Message<string, APIBillingTransaction> { Key = message.Key, Value = message.Value },
							TopicPartitionOffset = topicPartitionOffset
						};
						deliveryHandler(report);
						call++;
					});
			var billingClientMock = new Mock<IBillingServiceClient>();
			billingClientMock.Setup(x => x.AddTransaction(It.IsAny<APIBillingTransaction>()))
				.Throws(new InvalidOperationException("Test Billing service exception"));

			var messages = new List<eHubGatewayMessage>();
			var handlersDictionary = new Dictionary<Guid, MessageHandler>();
			for (int i = 0; i < 3; i++)
			{
				using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(string.Format(billingContent, DateTime.UtcNow.ToString("s"), Guid.NewGuid()))))
				{
					var message = new eHubGatewayMessage
					{
						ApplicationCode = "SCV",
						ClientID = "ZZZZZZZZZ",
						EmailSubject = $"EmailSubject{i + 1}",
						FileName = $"FileName{i + 1}",
						MessageTrackingID = Guid.NewGuid(),
						SchemaType = MessageSchemaType.Xml,
						SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.4",
						MessageStream = messageStream.CompressAndEncode()
					};
					messages.Add(message);

					var billingHandlerMock = new Mock<BillingMessageHandlerV4>();
					billingHandlerMock.CallBase = true;
					var kafkaClientMock = new Mock<BillingKafkaClient>(BillingMessageHandler.BillingKafkaConfig, null);
					kafkaClientMock.CallBase = true;
					kafkaClientMock.Setup(x => x.CreateProducer(It.IsAny<Func<IBillingTransactionProducer<string, APIBillingTransaction>>>()))
						.Returns(new Lazy<IBillingTransactionProducer<string, APIBillingTransaction>>(() => producerMock.Object));
					billingHandlerMock.Setup(x => x.KafkaClient).Returns(kafkaClientMock.Object);
					billingHandlerMock.Setup(x => x.CreateBillingServiceClient()).Returns(billingClientMock.Object);

					handlersDictionary.Add(message.MessageTrackingID, billingHandlerMock.Object);
				}
			}

			streamRequest.Messages = messages.ToArray();

			var service = MockRepository.GeneratePartialMock<eHubStreamedServiceMockWithHandle>();
			service.setHandlers(handlersDictionary);
			eHubStreamedService.IssueManger = new Lazy<IssueManager>(() => issuerManagerMock.Object);

			try
			{
				service.SenderId = messages[0].ClientID;
				service.SendStream(streamRequest);
				Assert.Fail("An exception should have been thrown");
			}
			catch (FaultException<ApplicationFault> fex)
			{
				Assert.IsTrue(fex.Detail.MessageExceptionDictionary.ContainsKey(messages[1].MessageTrackingID), "Missing Message Exception");
				Assert.IsTrue(fex.Detail.MessageExceptionDictionary[messages[1].MessageTrackingID].Contains("Errors occurred when sending transactions to both kafka topic 'billing-topic' and service"), "Wrong Exception Type: " + fex.Detail.MessageExceptionDictionary[messages[1].MessageTrackingID]);
				Assert.IsTrue(fex.Detail.MessageExceptionDictionary[messages[1].MessageTrackingID].Contains("Broker: Broker not available"));
				Assert.IsTrue(fex.Detail.MessageExceptionDictionary[messages[1].MessageTrackingID].Contains("Test Billing service exception"));
			}

			issuerManagerMock.Verify(x =>
				x.ReportToIssueManager("Sending billing transaction to Kafka failed. Topic: billing-topic. Reason: Broker: Broker not available"
					, It.Is<Exception>(ex => ex.Message.StartsWith("Broker: Broker not available"))
					, It.IsAny<ILog>()
					, It.IsAny<NameValueCollection>(), false), Times.Exactly(1));
		}

		[TestMethod]
		public void eHubStreamedService_SendStream_KafkaBillingClientIsDisposed()
		{
			BillingMessageHandler.SendBillingToKafka = true;
			var billingContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns=""http://www.edi.com.au/EnterpriseService/#Billing_1.4"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <BillableCount>2</BillableCount>
  <Branch>KLM</Branch>
  <ClientID>ABCDEFXYZ</ClientID>
  <ClientNumber>98765432100123456789</ClientNumber>
  <ClientStaffCode>ABC</ClientStaffCode>
  <PriceItemCode>DEF</PriceItemCode>
  <Category>TST</Category>
  <Reference1>REFERENCE 1</Reference1>
  <Reference2>REFERENCE 2</Reference2>
  <Reference3>REFERENCE 3</Reference3>
  <Reference4>REFERENCE 4</Reference4>
  <Reference5>REFERENCE 5</Reference5>
  <ReportingSource>XYZ</ReportingSource>
  <ServiceOccuredUTC>{0}</ServiceOccuredUTC>
  <Version>4</Version>
  <MessageTrackingID>{1}</MessageTrackingID>
  <AdditionalRefs>{{""objetKey"":""value""}}</AdditionalRefs>
</BillingTransaction>";
			var streamRequest = new SendStreamRequest();
			streamRequest.SendStreamRequestTrackingID = Guid.NewGuid();

			var producerMock = new Mock<IBillingTransactionProducer<string, APIBillingTransaction>>();
			producerMock.Setup(_ => _.Flush());
			producerMock.Setup(_ => _.Produce(It.Is<string>(topic => topic.Equals("billing-topic")), It.IsAny<Message<string, APIBillingTransaction>>(), It.IsAny<Action<DeliveryReport<string, APIBillingTransaction>>>()))
				.Callback<string, Message<string, APIBillingTransaction>, Action<DeliveryReport<string, APIBillingTransaction>>>(
					(topic, message, deliveryHandler) =>
					{
						var topicPartitionOffset = new TopicPartitionOffset(topic, new Partition(0), Offset.Unset);
						var report = new DeliveryReport<string, APIBillingTransaction>
						{
							Error = new Error(ErrorCode.NoError),
							Message = new Message<string, APIBillingTransaction> { Key = message.Key, Value = message.Value },
							TopicPartitionOffset = topicPartitionOffset
						};
						deliveryHandler(report);
					});

			var messages = new List<eHubGatewayMessage>();
			var handlersDictionary = new Dictionary<Guid, MessageHandler>();
			var kafkaClientMock = new Mock<BillingKafkaClient>(BillingMessageHandler.BillingKafkaConfig, null) { CallBase = true };
			var kafkaClientMockLazy = new Lazy<BillingKafkaClient>(() => kafkaClientMock.Object);
			kafkaClientMock.Setup(x => x.Dispose()).Verifiable();
			kafkaClientMock.Setup(x => x.CreateProducer(It.IsAny<Func<IBillingTransactionProducer<string, APIBillingTransaction>>>()))
				.Returns(new Lazy<IBillingTransactionProducer<string, APIBillingTransaction>>(() => producerMock.Object)).Verifiable();
			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, false);
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(string.Format(billingContent, DateTime.UtcNow.ToString("s"), Guid.NewGuid()))))
			{
				var message = new eHubGatewayMessage
				{
					ApplicationCode = "SCV",
					ClientID = "ZZZZZZZZZ",
					EmailSubject = $"EmailSubject",
					FileName = $"FileName",
					MessageTrackingID = Guid.NewGuid(),
					SchemaType = MessageSchemaType.Xml,
					SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.4",
					MessageStream = messageStream.CompressAndEncode()
				};
				messages.Add(message);

				var billingHandlerMock = new Mock<BillingMessageHandlerV4>();
				billingHandlerMock.CallBase = true;
				billingHandlerMock.Setup(x => x.KafkaClient).Returns(kafkaClientMock.Object);
				handlersDictionary.Add(message.MessageTrackingID, billingHandlerMock.Object);
			}

			streamRequest.Messages = messages.ToArray();
			var service = new Mock<eHubStreamedServiceMockWithHandle>(){ CallBase = true};
			service.Setup(x => x.CreateBillingKafkaClient()).Returns(kafkaClientMockLazy).Verifiable();
			service.Object.setHandlers(handlersDictionary);

			service.Object.SenderId = messages[0].ClientID;
			service.Object.SendStream(streamRequest);

			Assert.AreEqual(kafkaClientMockLazy.IsValueCreated, true);
			service.Verify();
			kafkaClientMock.Verify();

		}

		[TestMethod]
		public void eHubStreamedService_RetrieveStream_ExceptionThrownLogTransaction()
		{
			var service = MockRepository.GeneratePartialMock<eHubStreamedServiceMockWithHandle>();
			service.Stub(_ => _.OutboxAccessor).Throw(new Exception("Test"));
			service.Expect(_ => _.LogTransaction(Arg<string>.Is.Equal("ER"),
												 Arg<Func<string>>.Is.Anything,
												 Arg<string>.Is.Equal("RetrieveStream"))).Repeat.Once();

			try
			{
				service.RetrieveStream();
			}
			catch(Exception ex)
			{
				Assert.AreEqual("Test", ex.Message);
			}

			service.VerifyAllExpectations();
		}

		[TestMethod]
		public void eHubStreamedService_RetrieveStream_TimeoutException()
		{
			var service = MockRepository.GeneratePartialMock<eHubStreamedServiceMock>();
			var exception = new TimeoutException("Test");
			service.Stub(_ => _.OutboxAccessor).Throw(new AggregateException(exception));
			service.Expect(_ => _.LogTransaction(Arg<string>.Is.Equal("ER"), Arg<Func<string>>.Is.Anything, Arg<string>.Is.Equal("RetrieveStream"))).Repeat.Once();

			var result = service.RetrieveStream();

			Assert.IsNotNull(result);
			Assert.IsNotNull(result.TrackingID);
			Assert.AreNotEqual(result.TrackingID, string.Empty);
			Assert.AreEqual(result.Messages.Length, 0);

			Assert.AreEqual(service.ErrorLogged.Count, 1);
			Assert.AreEqual(service.ExceptionThrown.Count, 1);
			Assert.AreEqual(service.ErrorLogged.FirstOrDefault(), "RetrieveStream has timed out for Recipient: TESTSENDER");
			Assert.AreEqual(service.ExceptionThrown.FirstOrDefault()?.GetType(), typeof(AggregateException));
			Assert.IsTrue((service.ExceptionThrown.FirstOrDefault() as AggregateException)?.InnerExceptions.Any(x => x is TimeoutException) ?? false);

			service.VerifyAllExpectations();
		}

		[TestMethod]
		public void eHubStreamedService_RetrieveStreamByCompanyListRequest_ExceptionThrownLogTransaction()
		{
			var service = MockRepository.GeneratePartialMock<eHubStreamedServiceMockWithHandle>();
			service.SenderId = "TSTTSTTST";

			service.Stub(_ => _.OutboxAccessor).Throw(new Exception("Test"));
			service.Expect(_ => _.LogTransaction(Arg<string>.Is.Equal("ER"),
												 Arg<Func<string>>.Is.Anything,
												 Arg<string>.Is.Equal("RetrieveStreamByCompanyList"))).Repeat.Once();

			try
			{
				service.RetrieveStreamByCompanyList(new RetrieveStreamByCompanyListRequest() { CompanyIDs = new string[] { "TSTTSTTST" } });
			}
			catch (Exception ex)
			{
				Assert.AreEqual("Test", ex.Message);
			}

			service.VerifyAllExpectations();
		}

		[TestMethod]
		public void eHubStreamedService_RetrieveStreamByCompanyListRequest_TimeoutException()
		{
			var service = MockRepository.GeneratePartialMock<eHubStreamedServiceMockWithValidSenderID>();
			var exception = new TimeoutException("Test");
			service.Stub(_ => _.OutboxAccessor).Throw(new AggregateException(exception));
			service.Expect(_ => _.LogTransaction(Arg<string>.Is.Equal("ER"),
												 Arg<Func<string>>.Is.Anything,
												 Arg<string>.Is.Equal("RetrieveStreamByCompanyList"))).Repeat.Once();

			var result = service.RetrieveStreamByCompanyList(new RetrieveStreamByCompanyListRequest() { CompanyIDs = new string[] { "CLIENT_ID" } });

			Assert.IsNotNull(result);
			Assert.IsNotNull(result.TrackingID);
			Assert.AreNotEqual(string.Empty, result.TrackingID);
			Assert.AreEqual(0, result.CompanyToMessagesMap.Count);

			Assert.AreEqual(1, service.ErrorLogged.Count);
			Assert.AreEqual(1, service.ExceptionThrown.Count);
			Assert.AreEqual("RetrieveStreamByCompanyList has timed out for Recipients: [CLIENT_ID]", service.ErrorLogged.FirstOrDefault());
			Assert.AreEqual(typeof(AggregateException), service.ExceptionThrown.FirstOrDefault()?.GetType());
			Assert.IsTrue((service.ExceptionThrown.FirstOrDefault() as AggregateException)?.InnerExceptions.Any(x => x is TimeoutException) ?? false);

			service.VerifyAllExpectations();
		}

		[TestMethod]
		public void eHubStreamedService_RetrieveStreamByCompanyListRequest_CheckForInvalidCompanyIDs()
		{
			var idsFromDifferentClients = new string[] { "TST001TST", "TST002TST", "HYEDAUUAT" };
			var idsOfInvalidLength = new string[] { "INVALID", "VALID1TST" };

			var service = MockRepository.GeneratePartialMock<eHubStreamedServiceMockWithHandle>();
			service.SenderId = "TSTTSTTST";

			service.Expect(_ => _.LogTransaction(Arg<string>.Is.Equal("ER"),
												 Arg<Func<string>>.Is.Anything,
												 Arg<string>.Is.Equal("RetrieveStreamByCompanyList"))).Repeat.Twice();
			try
			{
				service.RetrieveStreamByCompanyList(new RetrieveStreamByCompanyListRequest() { CompanyIDs = idsFromDifferentClients });
				Assert.Fail("Should have thrown an exception: ids from different clients.");
			}
			catch (FaultException ex)
			{
				Assert.AreEqual("One or more companies were not from the same client or were invalid.", ex.Message,
								"idsFromDifferentClients");
			}

			try
			{
				service.RetrieveStreamByCompanyList(new RetrieveStreamByCompanyListRequest() { CompanyIDs = idsOfInvalidLength });
				Assert.Fail("Should have thrown an exception: ids of invalid length.");
			}
			catch (FaultException ex)
			{
				Assert.AreEqual("One or more companies were not from the same client or were invalid.", ex.Message,
								"idsOfInvalidLength");
			}

			service.VerifyAllExpectations();
		}

		[TestMethod]
		public void eHubStreamedService_RetrieveStreamByCompanyListRequest()
		{
			var sampleGatewayMessageForCompany1 = new eHubGatewayMessage() { ClientID = "TEST1", MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("Message 1 stream")) };
			var sampleGatewayMessageForCompany2 = new eHubGatewayMessage() { ClientID = "TEST2", MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("Message 2 stream")) };
			var sampleRequest = new RetrieveStreamByCompanyListRequest() { CompanyIDs = new string[] { "TST001TST", "TST002TST" } };

			var service = MockRepository.GeneratePartialMock<eHubStreamedServiceMockWithHandle>();
			service.SenderId = "TSTTSTTST";
			var outboxAccessor = MockRepository.GenerateMock<IOutboxAccessor>();
			outboxAccessor.Stub(_ => _.GetMessageBatch(Arg<string>.Is.Equal("TST001TST"),
													   Arg<string>.Is.Anything,
													   Arg<int>.Is.Anything,
													   Arg<long>.Is.Anything,
													   Arg<int>.Is.Anything)).Return(new eHubGatewayMessage[] { sampleGatewayMessageForCompany1 });
			outboxAccessor.Stub(_ => _.GetMessageBatch(Arg<string>.Is.Equal("TST002TST"),
													   Arg<string>.Is.Anything,
													   Arg<int>.Is.Anything,
													   Arg<long>.Is.Anything,
													   Arg<int>.Is.Anything)).Return(new eHubGatewayMessage[] { sampleGatewayMessageForCompany2 });
			service.Stub(_ => _.OutboxAccessor).Return(outboxAccessor);

			var result = service.RetrieveStreamByCompanyList(sampleRequest);

			Assert.IsNotNull(result);
			Assert.IsNotNull(result.TrackingID);
			Assert.AreNotEqual(string.Empty, result.TrackingID);

			CollectionAssert.Contains(result.CompanyToMessagesMap.Keys, "TST001TST");
			CollectionAssert.Contains(result.CompanyToMessagesMap.Keys, "TST002TST");
			CollectionAssert.AreEqual(new eHubGatewayMessage[] { sampleGatewayMessageForCompany1 }, result.CompanyToMessagesMap["TST001TST"]);
			CollectionAssert.AreEqual(new eHubGatewayMessage[] { sampleGatewayMessageForCompany2 }, result.CompanyToMessagesMap["TST002TST"]);
		}

		[TestMethod]
		public void eHubStreamedService_FinaliseBatch_SuccessLogTransaction()
		{
			var requestId = string.Empty;
			var outboxMock = MockRepository.GenerateMock<IOutboxAccessor>();
			var service = MockRepository.GeneratePartialMock<eHubStreamedServiceMockWithHandle>();
			service.Stub(_ => _.OutboxAccessor).Return(outboxMock);
			service.Expect(_ => _.LogTransaction(Arg<string>.Is.Equal("OK"),
												 Arg<Func<string>>.Is.Anything,
												 Arg<string>.Is.Equal("FinaliseBatch"))).Repeat.Once();

			service.FinaliseBatch(requestId);

			service.VerifyAllExpectations();
		}

		[TestMethod]
		public void eHubStreamedService_FinaliseBatch_ExceptionThrownLogTransaction()
		{
			var requestId = string.Empty;
			var service = MockRepository.GeneratePartialMock<eHubStreamedServiceMockWithHandle>();
			service.Stub(_ => _.OutboxAccessor).Throw(new Exception("Test"));
			service.Stub(_ => _.GetCurrentClientId()).Return("HYETSTTST");
			service.Expect(_ => _.LogTransaction(Arg<string>.Is.Equal("ER"), 
												 Arg<Func<string>>.Is.Anything,
												 Arg<string>.Is.Equal("FinaliseBatch"))).Repeat.Once();

			try
			{
				service.FinaliseBatch(requestId);
			}
			catch(Exception ex)
			{
				Assert.AreEqual("Client: [HYETSTTST] encountered exception at FinaliseBatch", ex.Message);
				Assert.IsNotNull(ex.InnerException);
				Assert.AreEqual("Test", ex.InnerException.Message);
			}

			service.VerifyAllExpectations();
		}

		[TestMethod]
		public void eHubStreamedService_ProducerConfig()
		{
			var producerConfig = eHubStreamedService.CreateProducerConfig();
			Assert.AreEqual("kafka-hosts", producerConfig.BootstrapServers);
			Assert.AreEqual(50, producerConfig.LingerMs);
			Assert.AreEqual(true, producerConfig.EnableSslCertificateVerification);
			Assert.AreEqual(5000, producerConfig.MessageTimeoutMs);
			Assert.AreEqual(SecurityProtocol.SaslSsl, producerConfig.SecurityProtocol);
			Assert.AreEqual("testUsername", producerConfig.SaslUsername);
			Assert.AreEqual( "testpassword", producerConfig.SaslPassword);
			Assert.AreEqual(SaslMechanism.Plain, producerConfig.SaslMechanism);
			Assert.AreEqual("ROOT", producerConfig.SslCaCertificateStores);
		}
	}

	public class eHubStreamedServiceMock : eHubStreamedService
	{
		public eHubStreamedServiceMock()
		{
			ErrorLogged = new List<string>();
			ExceptionThrown = new List<Exception>();
		}

		public List<string> ErrorLogged { get; set; }
		public List<Exception> ExceptionThrown { get; set; }
		public Exception HandleMessageExceptionToThrow { get; set; }

		public override string GetCurrentClientId()
		{
			return "TESTSENDER";
		}

		public override bool IsIntegrationUserNamePasswordValidator()
		{
			return false;
		}

		protected override void LogError(string message, Exception exception)
		{
			ErrorLogged.Add(message);
			ExceptionThrown.Add(exception);
		}

		protected override void HandleMessage(MessageHandler handler, Guid envelopTrackingId, eHubGatewayMessage message, string senderId)
		{
			if (HandleMessageExceptionToThrow != null) throw HandleMessageExceptionToThrow;
			base.HandleMessage(handler, envelopTrackingId, message, senderId);
		}

		public override MessageHandler GetHandler(eHubGatewayMessage message)
		{
			if (message.ClientID == "TestxT") return new XHMessageHandlerMock();

			switch (message.ApplicationCode)
			{
				case "USC":
					return new USCustomsInboxMessageHandlerMock();
				case "NZC":
					return new NZCustomsMessageHandlerMock();
				case "SCV":
					switch (message.SchemaName)
					{
						case "http://www.edi.com.au/EnterpriseService/#Billing_1.1":
							return new BillingMessageHandlerV1();
						case "http://www.edi.com.au/EnterpriseService/#Billing_1.2":
							return new BillingMessageHandlerV2();
						case "http://www.edi.com.au/EnterpriseService/#Billing_1.3":
							return new BillingMessageHandlerV3();
						default:
							return new InvalidMessageHandler();
					}
				default:
					return null;
			}
		}
	}

	public class eHubStreamedServiceMockWithValidSenderID : eHubStreamedServiceMock
	{
		public override string GetCurrentClientId()
		{
			return "CLIENT_ID";
		}

		public override MessageHandler GetHandler(eHubGatewayMessage message)
		{
			var handler = MockRepository.GeneratePartialMock<BillingMessageHandlerV1>();
			handler.Stub(_ => _.SendBillingInfo(null, null)).IgnoreArguments();
			handler.Stub(_ => _.IsCW1Sender(message.ClientID, message)).Return(true);
			return handler;
		}
	}

	public class eHubStreamedServiceMockWithClientAuthorisation : eHubStreamedService
	{
		public override string GetCurrentClientId()
		{
			return "CLIENT_ID";
		}

		public override bool IsIntegrationUserNamePasswordValidator()
		{
			return true;
		}
	}

	class USCustomsInboxMessageHandlerMock : USCustomsInboxMessageHandler
	{
		public override void CheckIfTooManyUnprocessedMessages(string senderId, string recipientId)
		{
		}
	}

	class NZCustomsMessageHandlerMock : NZCustomsMessageHandler
	{
		public override void CheckIfTooManyUnprocessedMessages(string senderId, string recipientId)
		{
		}
	}

	class XHMessageHandlerMock : XHMessageHandler
	{
		public override void CheckIfTooManyUnprocessedMessages(string senderId, string recipientId)
		{
		}
	}

	public class eHubStreamedServiceMockWithHandle : eHubStreamedService
	{
		public string SenderId { get; set; }

		public eHubStreamedServiceMockWithHandle()
		{
		}

		public override string GetCurrentClientId()
		{
			return SenderId;
		}

		public override bool IsIntegrationUserNamePasswordValidator()
		{
			return false;
		}

		public override MessageHandler GetHandler(eHubGatewayMessage message)
		{
			if (handlersDictionary.Count == 0)
			{
				return base.GetHandler(message);
			}
			else
			{
				return handlersDictionary[message.MessageTrackingID];
			}
		}

		public void setHandler(Guid trackingId, MessageHandler handler)
		{
			handlersDictionary.Add(trackingId, handler);
		}

		public void setHandlers(Dictionary<Guid, MessageHandler> handlersDictionary)
		{
			this.handlersDictionary = handlersDictionary;
		}

		Dictionary<Guid, MessageHandler> handlersDictionary = new Dictionary<Guid, MessageHandler>();
	}

	public class eHubStreamedServiceMockWithHandleAndClientAuthorisation : eHubStreamedServiceMockWithHandle
	{
		public override string GetCurrentClientId()
		{
			return "CLIENT_ID";
		}

		public override bool IsIntegrationUserNamePasswordValidator()
		{
			return true;
		}

		public override MessageHandler GetHandler(eHubGatewayMessage message)
		{
			var handler = MockRepository.GeneratePartialMock<BillingMessageHandlerV1>();
			handler.Stub(_ => _.SendBillingInfo(null, null)).IgnoreArguments();
			handler.Stub(_ => _.IsCW1Sender(message.ClientID, message)).Return(true);
			return handler;
		}
	}
}

