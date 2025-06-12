using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using APIBillingTransaction = CargoWise.Billing.API.BillingTransaction;
using CargoWise.eHub.Common.Extensions;
using CargoWise.Billing.API;
using CargoWise.Billing.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using eServices.eHubDataAccess.Integration;
using CargoWise.eHub.Common;
using Common.Logging;
using Confluent.Kafka;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CargoWise.Billing.Kafka.API;

namespace CargoWise.eHub.Gateway.Tests.InboxMessageHandler
{
	[TestClass]
	public class BillingMessageHandlerTests
	{
		[TestInitialize]
		public void Initialize()
		{
			BillingMessageHandler.BillingKafkaTopic = "billing-topic";
			BillingMessageHandler.BillingKafkaConfig = ServiceHelper.GetKafkaProducerConfig();
			eHubStreamedService.IssueManger = new Lazy<IssueManager>();
		}

		[TestMethod]
		public void TestBillingTransactionMessageV1()
		{
			const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
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
			var billingMessageHandlerMock = new BillingMessageHandlerV1Mock();
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);
				billingMessageHandlerMock.Handle("ABC123DEF", Guid.NewGuid(), new Common.eHubGatewayMessage()
				{
					SchemaType = Common.MessageSchemaType.Xml,
					SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.1",
					MessageStream = messageStream.CompressAndEncode(),
				});
				Assert.IsTrue(sw.ToString().Contains("[WARN]  ClientMismatch - System ID 'ABCDEFXYZ' with IP <UNKNOWN> is not match with Auth ID 'ABC123DEF'. Billing Transaction: "));
				billingMessageHandlerMock.Handle("ABC666XYZ", Guid.NewGuid(), new Common.eHubGatewayMessage()
				{
					SchemaType = Common.MessageSchemaType.Xml,
					SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.1",
					MessageStream = messageStream.CompressAndEncode(),
				});
				Assert.IsTrue(!sw.ToString().Contains("[WARN]  ClientMismatch - System ID 'ABCDEFXYZ' with IP <UNKNOWN> is not match with Auth ID 'ABC666XYZ'. Billing Transaction: "));
			}

			Assert.AreEqual(billingMessageHandlerMock.LastSentTransaction.BillableCount, 2);
			Assert.AreEqual(billingMessageHandlerMock.LastSentTransaction.Category, "UNK");
			Assert.AreEqual(billingMessageHandlerMock.LastSentTransaction.ClientID, "ABCDEFXYZ");
			Assert.AreEqual(billingMessageHandlerMock.LastSentTransaction.ClientNumber, "98765432100123456789");
			Assert.AreEqual(billingMessageHandlerMock.LastSentTransaction.ClientStaffCode, "ABC");
			Assert.AreEqual(billingMessageHandlerMock.LastSentTransaction.PriceItemCode, "DEF");
			Assert.AreEqual(billingMessageHandlerMock.LastSentTransaction.Reference1, "REFERENCE 1");
			Assert.AreEqual(billingMessageHandlerMock.LastSentTransaction.Reference2, "REFERENCE 2");
			Assert.AreEqual(billingMessageHandlerMock.LastSentTransaction.Reference3, "REFERENCE 3");
			Assert.AreEqual(billingMessageHandlerMock.LastSentTransaction.Reference4, null);
			Assert.AreEqual(billingMessageHandlerMock.LastSentTransaction.ReportingSource, "XYZ");
			Assert.AreEqual(billingMessageHandlerMock.LastSentTransaction.ServiceOccuredUTC, new DateTime(2014, 10, 2, 8, 50, 30));
		}

		[TestMethod]
		public void TestBillingTransactionMessageV1_BillableCountNotANumber()
		{
			const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <BillableCount>NOT A NUMBER</BillableCount>
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
			var billingMessageHandlerMock = new BillingMessageHandlerV1Mock();
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				try
				{
					billingMessageHandlerMock.Handle("ABC123DEF", Guid.NewGuid(), new Common.eHubGatewayMessage
					{
						SchemaType = Common.MessageSchemaType.Xml,
						SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.1",
						MessageStream = messageStream.CompressAndEncode(),
					});
					Assert.Fail("BillingTransactionValidationException should have been thrown.");
				}
				catch (BillingTransactionValidationException e)
				{
					Assert.AreEqual(
						"Billing transaction validation failed:" + Environment.NewLine +
						"The 'BillableCount' element is invalid - The value 'NOT A NUMBER' is invalid according to its datatype 'Int' - The string 'NOT A NUMBER' is not a valid Int32 value.",
						e.Message);
					Assert.IsNull(billingMessageHandlerMock.LastSentTransaction);
				}
			}
		}

		[TestMethod]
		public void TestBillingTransactionMessageV1_ServiceOccuredUTCNotADateTime()
		{
			const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
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
  <ServiceOccuredUTC>NOT A DATETIME</ServiceOccuredUTC>
</BillingTransaction>";
			var billingMessageHandlerMock = new BillingMessageHandlerV1Mock();
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				try
				{
					billingMessageHandlerMock.Handle("ABC123DEF", Guid.NewGuid(), new Common.eHubGatewayMessage
					{
						SchemaType = Common.MessageSchemaType.Xml,
						SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.1",
						MessageStream = messageStream.CompressAndEncode(),
					});
					Assert.Fail("BillingTransactionValidationException should have been thrown.");
				}
				catch (BillingTransactionValidationException e)
				{
					Assert.AreEqual(
						"Billing transaction validation failed:" + Environment.NewLine +
						"The 'ServiceOccuredUTC' element is invalid - The value 'NOT A DATETIME' is invalid according to its datatype 'DateTime' - The string 'NOT A DATETIME' is not a valid DateTime value.",
						e.Message);
					Assert.IsNull(billingMessageHandlerMock.LastSentTransaction);
				}
			}
		}

		[TestMethod]
		public void TestBillingTransactionMessageV1_MissingRequiredData()
		{
			const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <BillableCount>2</BillableCount>
  <ClientNumber>98765432100123456789</ClientNumber>
  <ClientStaffCode>ABC</ClientStaffCode>
  <PriceItemCode>DEF</PriceItemCode>
  <Reference1>REFERENCE 1</Reference1>
  <Reference2>REFERENCE 2</Reference2>
  <Reference3>REFERENCE 3</Reference3>
  <ReportingSource>XYZ</ReportingSource>
  <ServiceOccuredUTC>2014-10-02T08:50:30</ServiceOccuredUTC>
</BillingTransaction>";
			var billingMessageHandlerMock = new BillingMessageHandlerV1Mock();
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				try
				{
					billingMessageHandlerMock.Handle("ABC123DEF", Guid.NewGuid(), new Common.eHubGatewayMessage()
					{
						SchemaType = Common.MessageSchemaType.Xml,
						SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.1",
						MessageStream = messageStream.CompressAndEncode(),
					});
					Assert.Fail("BillingTransactionValidationException should have been thrown.");
				}
				catch (BillingTransactionValidationException e)
				{
					Assert.AreEqual(
						"Billing transaction validation failed:" + Environment.NewLine +
						"The element 'BillingTransaction' has incomplete content. List of possible elements expected: 'ClientID'.",
						e.Message);
					Assert.IsNull(billingMessageHandlerMock.LastSentTransaction);
				}
			}
		}

		[TestMethod]
		public void TestBillingTransactionMessageV1_WebServiceValidationException_Kafka()
		{
			const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <BillableCount>2</BillableCount>
  <ClientID>ABCDEFXYZ</ClientID>
  <ClientNumber>98765432100123456789</ClientNumber>
  <ClientStaffCode>ABC</ClientStaffCode>
  <PriceItemCode>DE$</PriceItemCode>
  <Reference1>REFERENCE 1</Reference1>
  <Reference2>REFERENCE 2</Reference2>
  <Reference3>REFERENCE 3</Reference3>
  <ReportingSource>XYZ</ReportingSource>
  <ServiceOccuredUTC>2014-10-02T08:50:30</ServiceOccuredUTC>
</BillingTransaction>";
			var senderID = "ABC123DEF";
			var producerMock = new Mock<IBillingTransactionProducer<string, APIBillingTransaction>>();
			producerMock
				.Setup(x => x.Produce(It.Is<string>(s => s.Equals("billing-topic")), It.IsAny<Message<string, APIBillingTransaction>>(), It.IsAny<Action<DeliveryReport<string, APIBillingTransaction>>>()));
			BillingMessageHandler.SendBillingToKafka = true;
			var kafkClientMock = new Mock<BillingKafkaClient>(BillingMessageHandler.BillingKafkaConfig, null);
			kafkClientMock.Setup(x => x.CreateProducer(It.IsAny<Func<IBillingTransactionProducer<string, APIBillingTransaction>>>())).Returns(new Lazy<IBillingTransactionProducer<string, APIBillingTransaction>>(() => producerMock.Object));
			var billingMessageHandlerMock = new Mock<BillingMessageHandlerV1> { CallBase = true };
			billingMessageHandlerMock.Setup(x => x.NewEnterpriseExeDetailAccessor.GetExeDetail(senderID)).Returns(new EnterpriseExeDetail("ALP", DateTime.MaxValue, "TST"));
			billingMessageHandlerMock.Setup(x => x.KafkaClient).Returns(kafkClientMock.Object);
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				try
				{
					billingMessageHandlerMock.Object.Handle(senderID, Guid.NewGuid(), new Common.eHubGatewayMessage
					{
						SchemaType = Common.MessageSchemaType.Xml,
						SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.1",
						MessageStream = messageStream.CompressAndEncode(),
					});
					Assert.Fail("BillingTransactionValidationException should have been thrown.");
				}
				catch (BillingTransactionValidationException e)
				{
					Assert.AreEqual(
						"Billing transaction validation failed:" + Environment.NewLine +
						"Field PriceItemCode has invalid characters. It must contain only ASCII letters, digits, fullstop, hyphen, hash or underscore." + Environment.NewLine +
						"The field ServiceOccuredUTC is required and must be no more than 5 years in the past and no more than one month in the future.",
						e.Message);
				}
			}
		}

		[TestMethod]
		public void TestBillingTransactionMessageV1_WebServiceValidationException_BillingServiceClient()
		{
			const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
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
			var senderID = "ABC123DEF";
			var billingServiceClientMock = new Mock<IBillingServiceClient>();
			billingServiceClientMock
				.Setup(x => x.AddTransaction(It.IsAny<CargoWise.Billing.API.BillingTransaction>()))
				.Throws(new ValidationException("Validation failed.", new[] { "Validation error 1", "Validation error 2" }));
			BillingMessageHandler.SendBillingToKafka = false;
			var billingMessageHandlerMock = new Mock<BillingMessageHandlerV1> { CallBase = true };
			billingMessageHandlerMock.Setup(x => x.NewEnterpriseExeDetailAccessor.GetExeDetail(senderID)).Returns(new EnterpriseExeDetail("ALP", DateTime.MaxValue, "TST"));
			billingMessageHandlerMock.Setup(x => x.CreateBillingServiceClient()).Returns(billingServiceClientMock.Object);
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				try
				{
					billingMessageHandlerMock.Object.Handle(senderID, Guid.NewGuid(), new Common.eHubGatewayMessage
					{
						SchemaType = Common.MessageSchemaType.Xml,
						SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.1",
						MessageStream = messageStream.CompressAndEncode(),
					});
					Assert.Fail("BillingTransactionValidationException should have been thrown.");
				}
				catch (BillingTransactionValidationException e)
				{
					Assert.AreEqual(
						"Billing transaction validation failed:" + Environment.NewLine +
						"Validation error 1" + Environment.NewLine +
						"Validation error 2",
						e.Message);
				}
			}
		}

		[TestMethod]
		public void TestApplyTransforms()
		{
			var transaction = new BillingTransaction();

			transaction.PriceItemCode = "EAO";
			transaction.Reference1 = Guid.NewGuid().ToString();
			new BillingMessageHandlerV1().ApplyTransforms(transaction, "http://www.edi.com.au/EnterpriseService/#Billing_1.0");
			Assert.AreEqual("EAD", transaction.Category);

			transaction.PriceItemCode = "EAO";
			transaction.Reference1 = Guid.NewGuid().ToString();
			transaction.Reference2 = "";
			transaction.Reference3 = Guid.NewGuid().ToString();
			new BillingMessageHandlerV1().ApplyTransforms(transaction, "http://www.edi.com.au/EnterpriseService/#Billing_1.0");
			Assert.AreEqual("STL", transaction.Category);

			transaction = new BillingTransaction();
			transaction.PriceItemCode = "IC1";
			new BillingMessageHandlerV1().ApplyTransforms(transaction, "http://www.edi.com.au/EnterpriseService/#Billing_1.0");
			Assert.AreEqual("EAD", transaction.Category);

			transaction = new BillingTransaction();
			transaction.PriceItemCode = "IU2";
			new BillingMessageHandlerV1().ApplyTransforms(transaction, "http://www.edi.com.au/EnterpriseService/#Billing_1.0");
			Assert.AreEqual("EAD", transaction.Category);

			transaction = new BillingTransaction();
			transaction.PriceItemCode = "CC3";
			new BillingMessageHandlerV1().ApplyTransforms(transaction, "http://www.edi.com.au/EnterpriseService/#Billing_1.0");
			Assert.AreEqual("ICN", transaction.Category);

			transaction.PriceItemCode = "CU4";
			new BillingMessageHandlerV1().ApplyTransforms(transaction, "http://www.edi.com.au/EnterpriseService/#Billing_1.1");
			Assert.AreEqual("ICN", transaction.Category);

			transaction = new BillingTransaction();
			transaction.PriceItemCode = "ACN";
			new BillingMessageHandlerV1().ApplyTransforms(transaction, "http://www.edi.com.au/EnterpriseService/#Billing_1.1");
			Assert.AreEqual("STL", transaction.Category);

			transaction = new BillingTransaction();
			transaction.PriceItemCode = "???";
			new BillingMessageHandlerV1().ApplyTransforms(transaction, "http://www.edi.com.au/EnterpriseService/#Billing_1.1");
			Assert.AreEqual("UNK", transaction.Category);

			transaction = new BillingTransaction();
			transaction.Category = "TST";
			new BillingMessageHandlerV2().ApplyTransforms(transaction, "http://www.edi.com.au/EnterpriseService/#Billing_1.2");
			Assert.AreEqual("TST", transaction.Category);
		}

		[TestMethod]
		public void TestSendBillingInfo_Kafka()
		{
			APIBillingTransaction apiTransaction = new APIBillingTransaction();
			var producerMock = new Mock<IBillingTransactionProducer<string, APIBillingTransaction>>();
			producerMock
				.Setup(x => x.Produce(It.Is<string>(s => s.Equals("billing-topic")), It.IsAny<Message<string, APIBillingTransaction>>(), It.IsAny<Action<DeliveryReport<string, APIBillingTransaction>>>()))
				.Callback<string, Message<string, APIBillingTransaction>, Action<DeliveryReport<string, APIBillingTransaction>>>((topic, message, deliveryHandler) =>
				{
					apiTransaction = message.Value;
				});
			BillingMessageHandler.SendBillingToKafka = true;
			var kafkClientMock = new Mock<BillingKafkaClient>(BillingMessageHandler.BillingKafkaConfig, null);
			kafkClientMock.Setup(x => x.CreateProducer(It.IsAny<Func<IBillingTransactionProducer<string, APIBillingTransaction>>>())).Returns(new Lazy<IBillingTransactionProducer<string, APIBillingTransaction>>(() => producerMock.Object));
			var handler = new Mock<BillingMessageHandlerV1> { CallBase = true };
			handler.Setup(x => x.KafkaClient).Returns(kafkClientMock.Object);

			var billingTransaction = new BillingTransaction()
			{
				BillableCount = 2,
				ClientID = "WTLEDINPN",
				ClientNumber = "98765432100123456789",
				ClientStaffCode = "TST",
				PriceItemCode = "TST",
				Reference1 = "TEST",
				Reference2 = "TEST",
				Reference3 = "TEST",
				Reference4 = "TEST",
				ReportingSource = "HUB",
				Category = "STL",
				ServiceOccuredUTC = DateTime.UtcNow
			};
			handler.Object.SendBillingInfo(Guid.NewGuid().ToString(), billingTransaction);
			producerMock.Verify(x => x.Produce(It.Is<string>(s => s.Equals("billing-topic")), It.IsAny<Message<string, APIBillingTransaction>>(), It.IsAny<Action<DeliveryReport<string, APIBillingTransaction>>>()), Times.Once);
			ObjectComparer.AssertPropertiesAreEqual(billingTransaction, apiTransaction);
		}

		[TestMethod]
		public void TestSendBillingInfo_BillingServiceClient()
		{
			APIBillingTransaction apiTransaction = null;
			var billingServiceClientMock = new Mock<IBillingServiceClient>();
			billingServiceClientMock
				.Setup(x => x.AddTransaction(It.IsAny<CargoWise.Billing.API.BillingTransaction>()))
				.Callback<APIBillingTransaction>(t => apiTransaction = t);
			BillingMessageHandler.SendBillingToKafka = false;
			var handler = new Mock<BillingMessageHandlerV1> { CallBase = true };
			handler.Setup(x => x.CreateBillingServiceClient()).Returns(billingServiceClientMock.Object);

			var billingTransaction = ObjectGenerator.CreateWithAllPropertiesSet<BillingTransaction>();
			handler.Object.SendBillingInfo(Guid.NewGuid().ToString(), billingTransaction);
			Assert.IsNotNull(apiTransaction, "Method AddTransaction on IBillingServiceClient was called.");
			ObjectComparer.AssertPropertiesAreEqual(billingTransaction, apiTransaction);
		}

		[TestMethod]
		public void TestBillingTransactionMessageV2()
		{
			const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns=""http://www.edi.com.au/EnterpriseService/#Billing_1.2"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
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
  <ServiceOccuredUTC>2014-10-02T08:50:30</ServiceOccuredUTC>
  <Version>3</Version>
</BillingTransaction>";
			var billingMessageHandler = new BillingMessageHandlerV2Mock();
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				billingMessageHandler.Handle("ABC123DEF", Guid.NewGuid(), new Common.eHubGatewayMessage()
				{
					SchemaType = Common.MessageSchemaType.Xml,
					SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.2",
					MessageStream = messageStream.CompressAndEncode(),
				});
			}
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.BillableCount, 2);
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.Branch, "KLM");
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.Category, "TST");
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.ClientID, "ABCDEFXYZ");
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.ClientNumber, "98765432100123456789");
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.ClientStaffCode, "ABC");
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.PriceItemCode, "DEF");
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.Reference1, "REFERENCE 1");
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.Reference2, "REFERENCE 2");
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.Reference3, "REFERENCE 3");
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.Reference4, "REFERENCE 4");
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.Reference5, "REFERENCE 5");
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.ReportingSource, "XYZ");
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.ServiceOccuredUTC, new DateTime(2014, 10, 2, 8, 50, 30));
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.Version, 3);
		}

		[TestMethod]
		public void TestBillingTransactionMessageV2_MissingRequiredData()
		{
			const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns=""http://www.edi.com.au/EnterpriseService/#Billing_1.2"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <BillableCount>2</BillableCount>
  <ClientNumber>98765432100123456789</ClientNumber>
  <ClientStaffCode>ABC</ClientStaffCode>
  <PriceItemCode>DEF</PriceItemCode>
  <Reference1>REFERENCE 1</Reference1>
  <Reference2>REFERENCE 2</Reference2>
  <Reference3>REFERENCE 3</Reference3>
  <ReportingSource>XYZ</ReportingSource>
  <ServiceOccuredUTC>2014-10-02T08:50:30</ServiceOccuredUTC>
</BillingTransaction>";
			var billingMessageHandler = new BillingMessageHandlerV2Mock();
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				try
				{
					billingMessageHandler.Handle("ABC123DEF", Guid.NewGuid(), new Common.eHubGatewayMessage()
					{
						SchemaType = Common.MessageSchemaType.Xml,
						SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.2",
						MessageStream = messageStream.CompressAndEncode(),
					});
					Assert.Fail("BillingTransactionValidationException should have been thrown.");
				}
				catch (BillingTransactionValidationException e)
				{
					StringAssert.Contains(e.Message, "Billing transaction validation failed:");
					StringAssert.Contains(e.Message, "The element 'BillingTransaction' in namespace 'http://www.edi.com.au/EnterpriseService/#Billing_1.2' has incomplete content. List of possible elements expected: ");
					StringAssert.Contains(e.Message, "ClientID");
					StringAssert.Contains(e.Message, "Version");
					StringAssert.Contains(e.Message, "Category");
					Assert.IsNull(billingMessageHandler.LastSentTransaction);
				}
			}
		}

		[TestMethod]
		public void TestBillingTransactionMessageV3()
		{
			const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns=""http://www.edi.com.au/EnterpriseService/#Billing_1.3"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
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
  <ServiceOccuredUTC>2014-10-02T08:50:30</ServiceOccuredUTC>
  <Version>3</Version>
  <MessageTrackingID>55B2D0DA-8230-43BE-83BF-5C7D5766343E</MessageTrackingID>
</BillingTransaction>";
			var billingMessageHandler = new BillingMessageHandlerV3Mock();
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				billingMessageHandler.Handle("ABC123DEF", Guid.NewGuid(), new Common.eHubGatewayMessage()
				{
					SchemaType = Common.MessageSchemaType.Xml,
					SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.3",
					MessageStream = messageStream.CompressAndEncode(),
				});
			}
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.BillableCount, 2);
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.Branch, "KLM");
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.Category, "TST");
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.ClientID, "ABCDEFXYZ");
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.ClientNumber, "98765432100123456789");
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.ClientStaffCode, "ABC");
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.PriceItemCode, "DEF");
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.Reference1, "REFERENCE 1");
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.Reference2, "REFERENCE 2");
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.Reference3, "REFERENCE 3");
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.Reference4, "REFERENCE 4");
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.Reference5, "REFERENCE 5");
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.ReportingSource, "XYZ");
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.ServiceOccuredUTC, new DateTime(2014, 10, 2, 8, 50, 30));
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.Version, 3);
			Assert.AreEqual(billingMessageHandler.LastSentTransaction.MessageTrackingID, "55B2D0DA-8230-43BE-83BF-5C7D5766343E");
		}

		[TestMethod]
		public void TestBillingTransactionMessageV4_Kafka()
		{
			var testSetups = new[]
			{
				new BillingTestSetup("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA", "DEF"),
				new BillingTestSetup("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB", "DEF", kafkaErrorCode: ErrorCode.Local_MsgTimedOut), //Fallback
				new BillingTestSetup("CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC", "DEF"),
				new BillingTestSetup("DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD", "DEF", kafkaErrorCode: ErrorCode.BrokerNotAvailable), //Fallback
				new BillingTestSetup("EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE", "DEF", kafkaErrorCode: ErrorCode.Local_MsgTimedOut, billingServiceException: new Exception("Test Error")), //Fallback failed
			};

			var expectedLog = @"[DEBUG] CargoWise.eHub.Gateway.BillingMessageHandler - Processing billing message...
[DEBUG] CargoWise.eHub.Gateway.BillingMessageHandler - Sent billing to Kafka billing-topic [[0]] @0
[TRACE] CargoWise.eHub.Gateway.BillingMessageHandler - Message's Details: 'Version: 4, Category: TST, PriceItemCode: DEF, BillableCount: 2, ReportingSource: XYZ, ServiceOccuredUTC: 2023-12-01T00:00:00, ClientID: ABCDEFXYZ, ClientNumber: 98765432100123456789, ClientStaffCode: ABC, Branch: KLM, Reference1: REFERENCE 1, Reference2: REFERENCE 2, Reference3: REFERENCE 3, Reference4: REFERENCE 4, Reference5: REFERENCE 5, MessageTrackingID: AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA, AdditionalRefs: '
[DEBUG] CargoWise.eHub.Gateway.BillingMessageHandler - Processing billing message...
[ERROR] CargoWise.eHub.Gateway.BillingMessageHandler - [IssueManager] Sending billing transaction to Kafka failed. Topic: billing-topic. Reason: Local: Message timed out
[ERROR] CargoWise.eHub.Gateway.BillingMessageHandler - Did not log to Issue Manager due to missing app setting: 'IssueManagerUri'
[INFO]  CargoWise.eHub.Gateway.BillingMessageHandler - Fallback to direct service
[DEBUG] CargoWise.eHub.Gateway.BillingMessageHandler - Processing billing message... SUCCESS
[TRACE] CargoWise.eHub.Gateway.BillingMessageHandler - Message's Details: 'Version: 4, Category: TST, PriceItemCode: DEF, BillableCount: 2, ReportingSource: XYZ, ServiceOccuredUTC: 2023-12-01T00:00:00, ClientID: ABCDEFXYZ, ClientNumber: 98765432100123456789, ClientStaffCode: ABC, Branch: KLM, Reference1: REFERENCE 1, Reference2: REFERENCE 2, Reference3: REFERENCE 3, Reference4: REFERENCE 4, Reference5: REFERENCE 5, MessageTrackingID: BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB, AdditionalRefs: '
[DEBUG] CargoWise.eHub.Gateway.BillingMessageHandler - Processing billing message...
[DEBUG] CargoWise.eHub.Gateway.BillingMessageHandler - Sent billing to Kafka billing-topic [[0]] @1
[TRACE] CargoWise.eHub.Gateway.BillingMessageHandler - Message's Details: 'Version: 4, Category: TST, PriceItemCode: DEF, BillableCount: 2, ReportingSource: XYZ, ServiceOccuredUTC: 2023-12-01T00:00:00, ClientID: ABCDEFXYZ, ClientNumber: 98765432100123456789, ClientStaffCode: ABC, Branch: KLM, Reference1: REFERENCE 1, Reference2: REFERENCE 2, Reference3: REFERENCE 3, Reference4: REFERENCE 4, Reference5: REFERENCE 5, MessageTrackingID: CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC, AdditionalRefs: '
[DEBUG] CargoWise.eHub.Gateway.BillingMessageHandler - Processing billing message...
[ERROR] CargoWise.eHub.Gateway.BillingMessageHandler - [IssueManager] Sending billing transaction to Kafka failed. Topic: billing-topic. Reason: Broker: Broker not available
[ERROR] CargoWise.eHub.Gateway.BillingMessageHandler - Did not log to Issue Manager due to missing app setting: 'IssueManagerUri'
[INFO]  CargoWise.eHub.Gateway.BillingMessageHandler - Fallback to direct service
[DEBUG] CargoWise.eHub.Gateway.BillingMessageHandler - Processing billing message... SUCCESS
[TRACE] CargoWise.eHub.Gateway.BillingMessageHandler - Message's Details: 'Version: 4, Category: TST, PriceItemCode: DEF, BillableCount: 2, ReportingSource: XYZ, ServiceOccuredUTC: 2023-12-01T00:00:00, ClientID: ABCDEFXYZ, ClientNumber: 98765432100123456789, ClientStaffCode: ABC, Branch: KLM, Reference1: REFERENCE 1, Reference2: REFERENCE 2, Reference3: REFERENCE 3, Reference4: REFERENCE 4, Reference5: REFERENCE 5, MessageTrackingID: DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD, AdditionalRefs: '
[DEBUG] CargoWise.eHub.Gateway.BillingMessageHandler - Processing billing message...
[ERROR] CargoWise.eHub.Gateway.BillingMessageHandler - [IssueManager] Sending billing transaction to Kafka failed. Topic: billing-topic. Reason: Local: Message timed out
[DEBUG] CargoWise.eHub.Gateway.BillingMessageHandler - Rate limit exceeded, skipping.
[INFO]  CargoWise.eHub.Gateway.BillingMessageHandler - Fallback to direct service
[ERROR] CargoWise.eHub.Gateway.BillingMessageHandler - Sending transaction to billing service failed (fallback). System.Exception: Test Error
[TRACE] CargoWise.eHub.Gateway.BillingMessageHandler - Message's Details: 'Version: 4, Category: TST, PriceItemCode: DEF, BillableCount: 2, ReportingSource: XYZ, ServiceOccuredUTC: 2023-12-01T00:00:00, ClientID: ABCDEFXYZ, ClientNumber: 98765432100123456789, ClientStaffCode: ABC, Branch: KLM, Reference1: REFERENCE 1, Reference2: REFERENCE 2, Reference3: REFERENCE 3, Reference4: REFERENCE 4, Reference5: REFERENCE 5, MessageTrackingID: EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE, AdditionalRefs: '";

			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);
				var handlerMocks = TestSendBillingInfoV4(true, testSetups);
				var actualLog = string.Join(Environment.NewLine, sw.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Where(x => x.Contains("CargoWise.eHub.Gateway.BillingMessageHandler - ")).Select(x => x));
				Assert.AreEqual(expectedLog, actualLog);

				for (int i = 0; i < testSetups.Length; i++)
				{
					var setup = testSetups[i];
					if (setup.KafkaErrorCode == ErrorCode.NoError || (setup.KafkaErrorCode == ErrorCode.Local_MsgTimedOut && setup.BillingServiceException == null))
					{
						Assert.AreEqual(default, handlerMocks[i].Object.MessageException, "Handler should not have error");
					}
					
					if (setup.KafkaErrorCode != ErrorCode.NoError && setup.BillingServiceException != null)
					{
						Assert.IsTrue(handlerMocks[i].Object.MessageException.Value is AggregateException);
						Assert.AreEqual("EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE", handlerMocks[i].Object.MessageException.Key.ToString().ToUpper());
						Assert.AreEqual("Local: Message timed out", (handlerMocks[i].Object.MessageException.Value as AggregateException)?.InnerExceptions[0].Message);
						Assert.AreEqual("Test Error", (handlerMocks[i].Object.MessageException.Value as AggregateException)?.InnerExceptions[1].Message);
					}
				}
			}

			billingProducerMock.Verify(x => x.Produce(
				It.Is<string>(topic => topic.Equals("billing-topic")),
				It.IsAny<Message<string, APIBillingTransaction>>(),
				It.IsAny<Action<DeliveryReport<string, APIBillingTransaction>>>()), Times.Exactly(5));
			billingServiceClientMock.Verify(x => x.AddTransaction(It.IsAny<APIBillingTransaction>()), Times.Exactly(3));

			billingServiceClientMock.Verify(x => x.AddTransaction(It.Is<APIBillingTransaction>(transaction => transaction.MessageTrackingID == "AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA")), Times.Never);
			billingServiceClientMock.Verify(x => x.AddTransaction(It.Is<APIBillingTransaction>(transaction => transaction.MessageTrackingID == "CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC")), Times.Never);
			billingServiceClientMock.Verify(x => x.AddTransaction(It.Is<APIBillingTransaction>(transaction => transaction.MessageTrackingID == "DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD")), Times.Once);
			billingServiceClientMock.Verify(x => x.AddTransaction(It.Is<APIBillingTransaction>(transaction => transaction.MessageTrackingID == "BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB")), Times.Once);
			billingServiceClientMock.Verify(x => x.AddTransaction(It.Is<APIBillingTransaction>(transaction => transaction.MessageTrackingID == "EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE")), Times.Once);
			billingServiceClientMock.Verify(x => x.Dispose(), Times.Exactly(3));
		}

		[TestMethod]
		public void TestBillingTransactionMessageV4_SendToBilling_ServiceClient()
		{
			var testSetups = new[]
			{
				new BillingTestSetup("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA", "DEF"),
				new BillingTestSetup("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB", "DEF"),
				new BillingTestSetup("CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC", "DEF")
			};

			var handlerMocks = TestSendBillingInfoV4(false, testSetups);
			billingServiceClientMock.Verify(x => x.AddTransaction(It.IsAny<APIBillingTransaction>()), Times.Exactly(3));
			billingServiceClientMock.Verify(x => x.Dispose(), Times.Exactly(3));
			Assert.IsTrue(handlerMocks.All(mock => mock.Object.MessageException.Equals(default(KeyValuePair<Guid, Exception>))));
		}

		private List<Mock<BillingMessageHandlerV4Mock>> TestSendBillingInfoV4(bool sendBillingViaKafka, BillingTestSetup[] testSetups)
		{
			var senderID = "ABC123DEF";
			var handlers = new List<Mock<BillingMessageHandlerV4Mock>>();
			int currentIndex = 0;
			long offsetCount = 0;

			billingProducerMock = new Mock<IBillingTransactionProducer<string, APIBillingTransaction>>();
			billingServiceClientMock = new Mock<IBillingServiceClient>();
			billingServiceClientMock
				.Setup(x => x.AddTransaction(It.IsAny<APIBillingTransaction>())).Callback<APIBillingTransaction>(
					transaction =>
					{
						AssertAPIBillingTransaction(handlers[currentIndex].Object.LastSentTransaction, transaction);
						if (testSetups[currentIndex].BillingServiceException != null)
						{
							throw testSetups[currentIndex].BillingServiceException;
						}

						SentToBillingDBTrackingIDs.Add(transaction.MessageTrackingID);
					});
			billingProducerMock
				.Setup(x => x.Produce(It.Is<string>(s => s.Equals("billing-topic")), It.IsAny<Message<string, APIBillingTransaction>>(), It.IsAny<Action<DeliveryReport<string, APIBillingTransaction>>>()))
				.Callback<string, Message<string, APIBillingTransaction>, Action<DeliveryReport<string, APIBillingTransaction>>>(
					(topic, message, deliveryHandler) =>
					{
						var transaction = message.Value;
						SentToBillingDBTrackingIDs.Add(transaction.MessageTrackingID);
						AssertAPIBillingTransaction(handlers[currentIndex].Object.LastSentTransaction, transaction);
						var setup = testSetups.FirstOrDefault(entry => entry.MessageTrackingID.Equals(transaction.MessageTrackingID));
						var resultErrorCode = setup?.KafkaErrorCode ?? ErrorCode.NoError;
						var report = new DeliveryReport<string, APIBillingTransaction>
						{
							Error = new Error(setup?.KafkaErrorCode ?? ErrorCode.NoError),
							Message = new Message<string, APIBillingTransaction> { Key = setup?.MessageTrackingID, Value = transaction },
							Key = setup?.MessageTrackingID,
						};

						report.Topic = "billing-topic";
						if (resultErrorCode == ErrorCode.NoError)
						{
							report.TopicPartitionOffset = new TopicPartitionOffset(topic, new Partition(0), new Offset(offsetCount++));
						}

						deliveryHandler(report);
					});
			billingProducerMock.Setup(x => x.Flush());
			billingProducerMock.Setup(x => x.Dispose());
			billingServiceClientMock.Setup(x => x.Dispose());
			BillingMessageHandler.SendBillingToKafka = sendBillingViaKafka;
			var kafkClientMock = new Mock<BillingKafkaClient>(BillingMessageHandler.BillingKafkaConfig, null);
			kafkClientMock.Setup(x => x.CreateProducer(It.IsAny<Func<IBillingTransactionProducer<string, APIBillingTransaction>>>()))
				.Returns(new Lazy<IBillingTransactionProducer<string, APIBillingTransaction>>(() => billingProducerMock.Object));

			foreach (var item in testSetups)
			{
				Mock<BillingMessageHandlerV4Mock> billingMessageHandlerMock = new Mock<BillingMessageHandlerV4Mock> { CallBase = true };
				billingMessageHandlerMock.Setup(x => x.NewEnterpriseExeDetailAccessor.GetExeDetail(senderID)).Returns(new EnterpriseExeDetail("ALP", DateTime.MaxValue, "TST"));
				billingMessageHandlerMock.Setup(x => x.CreateBillingServiceClient()).Returns(billingServiceClientMock.Object);
				billingMessageHandlerMock.Setup(x => x.KafkaClient).Returns(kafkClientMock.Object);
				handlers.Add(billingMessageHandlerMock);
				using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(string.Format(billingMessageV4Xml, item.ServiceOccuredUTC.ToString("s"), item.MessageTrackingID, item.PriceItemCode))))
				{
					billingMessageHandlerMock.Object.Handle(senderID, Guid.NewGuid(), new Common.eHubGatewayMessage()
					{
						SchemaType = Common.MessageSchemaType.Xml,
						SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.4",
						MessageStream = messageStream.CompressAndEncode(),
					});
				}
				currentIndex++;
			}

			return handlers;
		}

		private void AssertAPIBillingTransaction(BillingTransaction transaction, APIBillingTransaction billingTransaction)
		{
			Assert.AreEqual(transaction.BillableCount, billingTransaction.BillableCount);
			Assert.AreEqual(transaction.Branch, billingTransaction.Branch);
			Assert.AreEqual(transaction.Category, billingTransaction.Category);
			Assert.AreEqual(transaction.ClientID, billingTransaction.ClientID);
			Assert.AreEqual(transaction.ClientNumber, billingTransaction.ClientNumber);
			Assert.AreEqual(transaction.ClientStaffCode, billingTransaction.ClientStaffCode);
			Assert.AreEqual(transaction.PriceItemCode, billingTransaction.PriceItemCode);
			Assert.AreEqual(transaction.Reference1, billingTransaction.Reference1);
			Assert.AreEqual(transaction.Reference2, billingTransaction.Reference2);
			Assert.AreEqual(transaction.Reference3, billingTransaction.Reference3);
			Assert.AreEqual(transaction.Reference4, billingTransaction.Reference4);
			Assert.AreEqual(transaction.Reference5, billingTransaction.Reference5);
			Assert.AreEqual(transaction.ReportingSource, billingTransaction.ReportingSource);
			Assert.AreEqual(transaction.ServiceOccuredUTC, billingTransaction.ServiceOccuredUTC);
			Assert.AreEqual(transaction.Version, billingTransaction.Version);
			Assert.AreEqual(transaction.MessageTrackingID, billingTransaction.MessageTrackingID);
			Assert.AreEqual(transaction.AdditionalRefs, billingTransaction.AdditionalRefs);
		}

		[TestMethod]
		public void TestBillingTransactionForNonCW1Senders()
		{
			const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
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
			var billingMessageHandlerMock = new BillingMessageHandlerV1Mock();
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);
				billingMessageHandlerMock.Handle("ABC", Guid.NewGuid(), new Common.eHubGatewayMessage()
				{
					SchemaType = Common.MessageSchemaType.Xml,
					SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.1",
					MessageStream = messageStream.CompressAndEncode(),
				});
				Assert.IsFalse(sw.ToString().Contains("[WARN]  ClientMismatch - System ID 'ABCDEFXYZ' with IP <UNKNOWN> is not match with Auth ID 'ABC'. Billing Transaction: "), "A warning was raised to identify a mismatch, which should not be applicable to a non-CW1 sender 'ABC'");
				billingMessageHandlerMock.Handle("ABC666XYZ123", Guid.NewGuid(), new Common.eHubGatewayMessage()
				{
					SchemaType = Common.MessageSchemaType.Xml,
					SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.1",
					MessageStream = messageStream.CompressAndEncode(),
				});
				Assert.IsFalse(sw.ToString().Contains("[WARN]  ClientMismatch - System ID 'ABCDEFXYZ' with IP <UNKNOWN> is not match with Auth ID 'ABC666XYZ123'. Billing Transaction: "), "A warning was raised to identify a mismatch, which should not be applicable to a non-CW1 sender 'ABC666XYZ123'");
			}

			Assert.AreEqual(billingMessageHandlerMock.LastSentTransaction.BillableCount, 2);
			Assert.AreEqual(billingMessageHandlerMock.LastSentTransaction.Category, "UNK");
			Assert.AreEqual(billingMessageHandlerMock.LastSentTransaction.ClientID, "ABCDEFXYZ");
			Assert.AreEqual(billingMessageHandlerMock.LastSentTransaction.ClientNumber, "98765432100123456789");
			Assert.AreEqual(billingMessageHandlerMock.LastSentTransaction.ClientStaffCode, "ABC");
			Assert.AreEqual(billingMessageHandlerMock.LastSentTransaction.PriceItemCode, "DEF");
			Assert.AreEqual(billingMessageHandlerMock.LastSentTransaction.Reference1, "REFERENCE 1");
			Assert.AreEqual(billingMessageHandlerMock.LastSentTransaction.Reference2, "REFERENCE 2");
			Assert.AreEqual(billingMessageHandlerMock.LastSentTransaction.Reference3, "REFERENCE 3");
			Assert.AreEqual(billingMessageHandlerMock.LastSentTransaction.Reference4, null);
			Assert.AreEqual(billingMessageHandlerMock.LastSentTransaction.ReportingSource, "XYZ");
			Assert.AreEqual(billingMessageHandlerMock.LastSentTransaction.ServiceOccuredUTC, new DateTime(2014, 10, 2, 8, 50, 30));
		}

		[TestCleanup]
		public void TestCleanup()
		{
		}

		List<string> SentToBillingDBTrackingIDs = new List<string>();
		List<string> SentToELKTrackingIDs = new List<string>();

		internal class BillingMessageHandlerV1Mock : BillingMessageHandlerV1
		{
			public BillingTransaction LastSentTransaction { get; private set; }

			internal override void SendBillingInfo(string trackingId, BillingTransaction transaction)
			{
				LastSentTransaction = transaction;
			}

			internal override bool IsCW1Sender(string senderID, eHubGatewayMessage message)
			{
				var result = true;
				if (!string.IsNullOrEmpty(senderID) && senderID.Length != 9)
				{
					result = false;
				}
				return result;
			}
		}

		internal class BillingMessageHandlerV2Mock : BillingMessageHandlerV2
		{
			public BillingTransaction LastSentTransaction { get; private set; }

			internal override void SendBillingInfo(string trackingId, BillingTransaction transaction)
			{
				LastSentTransaction = transaction;
			}

			internal override bool IsCW1Sender(string senderID, eHubGatewayMessage message)
			{
				var result = true;
				if (!string.IsNullOrEmpty(senderID) && senderID.Length != 9)
				{
					result = false;
				}
				return result;
			}
		}

		internal class BillingMessageHandlerV3Mock : BillingMessageHandlerV3
		{
			public BillingTransaction LastSentTransaction { get; private set; }

			internal override void SendBillingInfo(string trackingId, BillingTransaction transaction)
			{
				LastSentTransaction = transaction;
			}

			internal override bool IsCW1Sender(string senderID, eHubGatewayMessage message)
			{
				var result = true;
				if (!string.IsNullOrEmpty(senderID) && senderID.Length != 9)
				{
					result = false;
				}
				return result;
			}
		}

		public class BillingMessageHandlerV4Mock : BillingMessageHandlerV4
		{
			public BillingTransaction LastSentTransaction { get; private set; }
			public string LastSentTransactionJson { get; set; }

			internal override void SendBillingInfo(string trackingId, BillingTransaction transaction)
			{
				LastSentTransaction = transaction;
				base.SendBillingInfo(trackingId, transaction);
			}
		}

		class BillingTestSetup
		{
			public BillingTestSetup(string trackingId, string priceItemCode, DateTime? occurredUtc = null, ErrorCode kafkaErrorCode = ErrorCode.NoError, Exception billingServiceException = null)
			{
				KafkaErrorCode = kafkaErrorCode;
				MessageTrackingID = trackingId;
				PriceItemCode = priceItemCode;
				BillingServiceException = billingServiceException;
				ServiceOccuredUTC = occurredUtc ?? new DateTime(2023, 12, 1, 0, 0, 0, DateTimeKind.Utc);
			}

			public DateTime ServiceOccuredUTC { get; }
			public string MessageTrackingID { get; }
			public string PriceItemCode { get; }
			public ErrorCode KafkaErrorCode { get;}
			public Exception BillingServiceException { get; }
		}

		private Mock<IBillingServiceClient> billingServiceClientMock;
		private Mock<IBillingTransactionProducer<string, APIBillingTransaction>> billingProducerMock;
		private const string billingMessageV4Xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns=""http://www.edi.com.au/EnterpriseService/#Billing_1.4"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <BillableCount>2</BillableCount>
  <Branch>KLM</Branch>
  <ClientID>ABCDEFXYZ</ClientID>
  <ClientNumber>98765432100123456789</ClientNumber>
  <ClientStaffCode>ABC</ClientStaffCode>
  <PriceItemCode>{2}</PriceItemCode>
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
  <AdditionalRefs></AdditionalRefs>
</BillingTransaction>";
	}
}
