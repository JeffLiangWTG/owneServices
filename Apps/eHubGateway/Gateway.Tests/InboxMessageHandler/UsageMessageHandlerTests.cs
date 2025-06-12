using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Billing.API;
using CargoWise.Billing.Kafka.API;
using CargoWise.Billing.Client;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using Confluent.Kafka;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CargoWise.eHub.Gateway.Tests.InboxMessageHandler
{
	[TestClass]
	public class UsageMessageHandlerTests
	{

		[TestMethod]
		public void TestUsageTransactionMessageV1()
		{
			var handler = new Mock<UsageMessageHandlerV1>(){CallBase = true};
			handler.Setup(_ => _.CreateBillingServiceClient()).Returns(mockBillingServiceClient.Object);
			TestHandler(handler.Object, xmlUsageTransactionV1, schemaNameV1, false);
			Assert.AreEqual(LastSentTransaction.UsageCount, 2);
			Assert.AreEqual(LastSentTransaction.ServiceOccuredUTC, new DateTime(2014, 10, 2, 8, 50, 30));
			Assert.AreEqual(LastSentTransaction.AdditionalRefs, "{{\"objectKey\":\"value\"}}");
		}

		[TestMethod]
		public void TestUsageTransactionMessageV2()
		{
			var handler = new Mock<UsageMessageHandlerV2>() { CallBase = true };
			handler.Setup(_ => _.CreateBillingServiceClient()).Returns(mockBillingServiceClient.Object);
			TestHandler(handler.Object, xmlUsageTransactionV2, schemaNameV2, false);
			Assert.AreEqual(LastSentTransaction.UsageCount, 2);
			Assert.AreEqual(LastSentTransaction.ServiceOccuredUTC, new DateTime(2014, 10, 2, 8, 50, 30));
			Assert.AreEqual(LastSentTransaction.AdditionalRefs, "{{\"objectKey\":\"value\"}}");
		}

		[TestMethod]
		public void TestUsageTransactionMessageV2_1()
		{
			var handler = new Mock<UsageMessageHandlerV2_1>() { CallBase = true };
			handler.Setup(_ => _.CreateBillingServiceClient()).Returns(mockBillingServiceClient.Object);
			TestHandler(handler.Object, xmlUsageTransactionV2_1, schemaNameV2_1, false);
			Assert.AreEqual(LastSentTransaction.UsageCount, 7);
			Assert.AreEqual(LastSentTransaction.ServiceOccuredUTC, new DateTime(2014, 10, 2, 8, 50, 30));
			Assert.AreEqual(LastSentTransaction.AdditionalRefs, "{{\"objectKey21\":\"value21\"}}");
			Assert.AreEqual(LastSentTransaction.EnterpriseCode, "EDI");
			Assert.AreEqual(LastSentTransaction.ServerCode, "PRD");
			Assert.AreEqual(LastSentTransaction.Environment, "PRD");
			Assert.AreEqual(LastSentTransaction.CompanyCode, "CPC");
			Assert.AreEqual(LastSentTransaction.CompanyName, "WiseTech Global (China) Information Technology Limited");
			Assert.AreEqual(LastSentTransaction.BranchCode, "BRN");
			Assert.AreEqual(LastSentTransaction.UsageCode, "USS");
		}

		[TestMethod]
		public void TestUsageTransactionMessageV1_Fallback_Succeed()
		{
			var handler = new Mock<UsageMessageHandlerV1>() { CallBase = true };
			handler.Setup(_ => _.CreateBillingServiceClient()).Returns(mockBillingServiceClient.Object);
			TestHandler(handler.Object, xmlUsageTransactionV1, schemaNameV1, true);
			Assert.AreEqual(LastSentTransaction.UsageCount, 2);
			Assert.AreEqual(LastSentTransaction.ServiceOccuredUTC, new DateTime(2014, 10, 2, 8, 50, 30));
			Assert.AreEqual(LastSentTransaction.AdditionalRefs, "{{\"objectKey\":\"value\"}}");
		}

		[TestMethod]
		public void TestUsageTransactionMessageV2_Fallback_Succeed()
		{
			var handler = new Mock<UsageMessageHandlerV2>() { CallBase = true };
			handler.Setup(_ => _.CreateBillingServiceClient()).Returns(mockBillingServiceClient.Object);
			TestHandler(handler.Object, xmlUsageTransactionV2, schemaNameV2, true);
			Assert.AreEqual(LastSentTransaction.UsageCount, 2);
			Assert.AreEqual(LastSentTransaction.ServiceOccuredUTC, new DateTime(2014, 10, 2, 8, 50, 30));
			Assert.AreEqual(LastSentTransaction.AdditionalRefs, "{{\"objectKey\":\"value\"}}");
		}

		[TestMethod]
		public void TestUsageTransactionMessageV2_1_Fallback_Succeed()
		{
			var handler = new Mock<UsageMessageHandlerV2_1>() { CallBase = true };
			handler.Setup(_ => _.CreateBillingServiceClient()).Returns(mockBillingServiceClient.Object);
			TestHandler(handler.Object, xmlUsageTransactionV2_1, schemaNameV2_1, true);
			Assert.AreEqual(LastSentTransaction.UsageCount, 7);
			Assert.AreEqual(LastSentTransaction.ServiceOccuredUTC, new DateTime(2014, 10, 2, 8, 50, 30));
			Assert.AreEqual(LastSentTransaction.AdditionalRefs, "{{\"objectKey21\":\"value21\"}}");
			Assert.AreEqual(LastSentTransaction.EnterpriseCode, "EDI");
			Assert.AreEqual(LastSentTransaction.ServerCode, "PRD");
			Assert.AreEqual(LastSentTransaction.Environment, "PRD");
			Assert.AreEqual(LastSentTransaction.CompanyCode, "CPC");
			Assert.AreEqual(LastSentTransaction.CompanyName, "WiseTech Global (China) Information Technology Limited");
			Assert.AreEqual(LastSentTransaction.BranchCode, "BRN");
			Assert.AreEqual(LastSentTransaction.UsageCode, "USS");
		}

		[TestMethod]
		public void TestUsageTransactionMessageV1_Fallback_Failed()
		{
			var handler = new Mock<UsageMessageHandlerV1>() { CallBase = true };
			handler.Setup(_ => _.CreateBillingServiceClient()).Returns(mockBillingServiceClient.Object);
			TestHandler(handler.Object, xmlUsageTransactionV1, schemaNameV1, true, false);
			Assert.IsNull(LastSentTransaction);
			Assert.IsTrue(handler.Object.MessageException.Value is AggregateException);
			Assert.AreEqual("Broker: Broker not available", (handler.Object.MessageException.Value as AggregateException)?.InnerExceptions[0].Message);
			Assert.AreEqual("Incorrect endpoint", (handler.Object.MessageException.Value as AggregateException)?.InnerExceptions[1].Message);
		}

		[TestMethod]
		public void TestUsageTransactionMessageV2_Fallback_Failed()
		{
			var handler = new Mock<UsageMessageHandlerV2>() { CallBase = true };
			handler.Setup(_ => _.CreateBillingServiceClient()).Returns(mockBillingServiceClient.Object);
			TestHandler(handler.Object, xmlUsageTransactionV2, schemaNameV2, true, false);
			Assert.IsNull(LastSentTransaction);
			Assert.AreEqual("Broker: Broker not available", (handler.Object.MessageException.Value as AggregateException)?.InnerExceptions[0].Message);
			Assert.AreEqual("Incorrect endpoint", (handler.Object.MessageException.Value as AggregateException)?.InnerExceptions[1].Message);
		}

		[TestMethod]
		public void TestUsageTransactionMessageV2_1_Fallback_Failed()
		{
			var handler = new Mock<UsageMessageHandlerV2_1>() { CallBase = true };
			handler.Setup(_ => _.CreateBillingServiceClient()).Returns(mockBillingServiceClient.Object);
			TestHandler(handler.Object, xmlUsageTransactionV2_1, schemaNameV2_1, true, false);
			Assert.IsNull(LastSentTransaction);
			Assert.AreEqual("Broker: Broker not available", (handler.Object.MessageException.Value as AggregateException)?.InnerExceptions[0].Message);
			Assert.AreEqual("Incorrect endpoint", (handler.Object.MessageException.Value as AggregateException)?.InnerExceptions[1].Message);
		}

		[TestMethod]
		public void TestUsageTransactionMessageV21_FailedValidation()
		{
			const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UsageTransaction xmlns=""http://www.edi.com.au/EnterpriseService/#Usage_2.1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <UsageCount>7</UsageCount>
  <ServiceOccuredUTC>2014-10-02T08:50:30</ServiceOccuredUTC>
  <AdditionalRefs>{""ServerCode"":""TST""}</AdditionalRefs>
  <EnterpriseCode>EDI</EnterpriseCode>
  <ServerCode>PRD</ServerCode>
  <Environment>PRD</Environment>
  <CompanyCode>CPC</CompanyCode>
  <CompanyName>WiseTech Global (China) Information Technology Limited</CompanyName>
  <BranchCode>BRN</BranchCode>
  <UsageCode>USS</UsageCode>
</UsageTransaction>";
			HandleFailedValidationTestUsageTransaction(new UsageMessageHandlerV2_1(), xml, schemaNameV2_1, "ServerCode");
		}

		[TestMethod]
		public void TestUsageTransactionMessageV2_FailedValidation()
		{
			const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UsageTransaction xmlns=""http://www.edi.com.au/EnterpriseService/#Usage_2.0"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <UsageCount>2</UsageCount>
  <ServiceOccuredUTC>2014-10-02T08:50:30</ServiceOccuredUTC>
  <AdditionalRefs>{""UsageCount"":""value""}</AdditionalRefs>
</UsageTransaction>";
			HandleFailedValidationTestUsageTransaction(new UsageMessageHandlerV2(), xml, schemaNameV2, "UsageCount");
		}

		[TestMethod]
		public void TestUsageTransactionMessageV1_FailedValidation()
		{
			const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UsageTransaction xmlns=""http://www.edi.com.au/EnterpriseService/#Usage_1.1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <Count>2</Count>
  <ServiceOccuredUTC>2014-10-02T08:50:30</ServiceOccuredUTC>
  <AdditionalRefs>{""UsageCount"":""value""}</AdditionalRefs>
</UsageTransaction>";
			HandleFailedValidationTestUsageTransaction(new UsageMessageHandlerV1(), xml, schemaNameV1, "UsageCount");
		}

		void HandleFailedValidationTestUsageTransaction<T>(UsageMessageHandler<T> handler, string xml, string schemaName, string clashingPropertyName) where T : class
		{
			handler.KafkaClient = new BillingKafkaClient("abc.test.zone");
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			using (var sw = new StringWriter())
			{
				try
				{
					handler.Handle("ABC666XYZ", Guid.NewGuid(), new eHubGatewayMessage()
					{
						SchemaType = MessageSchemaType.Xml,
						SchemaName = schemaName,
						MessageStream = messageStream.CompressAndEncode(),
					});
					Assert.Fail($"{handler.GetType().Name} should throw BillingTransactionValidationException");
				}
				catch (BillingTransactionValidationException e)
				{
					Assert.AreEqual("Billing transaction validation failed:\r\n", e.Message);
					Assert.IsTrue(e.ToString().Contains($"CargoWise.Billing.API.ValidationException: Transaction validation failed. ---> System.ArgumentException: Can not add or merge property {clashingPropertyName}. A property with the same name and a different value already exists."));
				}
			}
		}

		void TestHandler<T>(UsageMessageHandler<T> handler, string xml, string schemaName, bool fallback, bool fallbackSucceed = true) where T : class
		{
			if (fallback)
			{
				mockBillingServiceClient.Setup(_ => _.AddUsageTransaction(It.IsAny<UsageTransaction>()))
					.Callback<UsageTransaction>(
						transaction =>
						{
							if (fallbackSucceed)
							{
								LastSentTransaction = transaction;
							}
							else
							{
								throw new Exception("Incorrect endpoint");
							}
						});
				mockBillingKafkaClient.Setup(x => x.SendUsageInfoToELK(It.IsAny<string>(), It.IsAny<string>(),
						It.IsAny<UsageTransaction>(), It.IsAny<Action<DeliveryReport<string, ELKTransaction>>>()))
					.Callback<string, string, UsageTransaction, Action<DeliveryReport<string, ELKTransaction>>>((topic, id, transaction, report) =>
					{
						var error = new Error(ErrorCode.BrokerNotAvailable);
						var deliveryReport = new DeliveryReport<string, ELKTransaction>
						{
							Topic = "elk-topic",
							Error = error,
							Message = new Message<string, ELKTransaction>()
							{
								Key = Guid.NewGuid().ToString(),
								Value = transaction
							}
						};

						report(deliveryReport);
					});
			}
			else
			{
				mockBillingKafkaClient.Setup(x => x.SendUsageInfoToELK(It.IsAny<string>(), It.IsAny<string>(),
						It.IsAny<UsageTransaction>(), It.IsAny<Action<DeliveryReport<string, ELKTransaction>>>()))
					.Callback<string, string, UsageTransaction, Action<DeliveryReport<string, ELKTransaction>>>((topic, id, transaction, report) =>
					{
						LastSentTransaction = transaction;
					});
			}
			
			handler.KafkaClient = mockBillingKafkaClient.Object;

			string outputLog;
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);
				handler.Handle("ABC666XYZ", Guid.NewGuid(), new eHubGatewayMessage()
				{
					SchemaType = MessageSchemaType.Xml,
					SchemaName = schemaName,
					MessageStream = messageStream.CompressAndEncode(),
				});
				outputLog = sw.ToString();
			}


			if (!fallback || fallbackSucceed)
			{
				Assert.AreEqual(default, handler.MessageException);
			}

			if (fallback)
			{
				var expectedLog = firstReport ? "[ERROR] UsageMessageHandler - Did not log to Issue Manager due to missing app setting: 'IssueManagerUri'" : "[DEBUG] UsageMessageHandler - Rate limit exceeded, skipping.";
				firstReport = false;
				Assert.IsTrue(outputLog.Contains(expectedLog), $"Output log '{outputLog}' does not contains '{expectedLog}'");
				mockBillingServiceClient.Verify(_ => _.AddUsageTransaction(It.IsAny<UsageTransaction>()), Times.Once);
			}
			else
			{
				mockBillingServiceClient.Verify(_ => _.AddUsageTransaction(It.IsAny<UsageTransaction>()), Times.Never);
			}
		}

		[TestInitialize]
		public void Setup()
		{
			mockBillingKafkaClient = new Mock<IBillingKafkaClient>();
			mockBillingServiceClient = new Mock<IBillingServiceClient>();
		}

		Mock<IBillingKafkaClient> mockBillingKafkaClient;
		Mock<IBillingServiceClient> mockBillingServiceClient;
		public UsageTransaction LastSentTransaction { get; set; }

		const string xmlUsageTransactionV1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UsageTransaction xmlns=""http://www.edi.com.au/EnterpriseService/#Usage_1.1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <Count>2</Count>
  <ServiceOccuredUTC>2014-10-02T08:50:30</ServiceOccuredUTC>
  <AdditionalRefs>{{""objectKey"":""value""}}</AdditionalRefs>
</UsageTransaction>";

		const string xmlUsageTransactionV2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UsageTransaction xmlns=""http://www.edi.com.au/EnterpriseService/#Usage_2.0"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <UsageCount>2</UsageCount>
  <ServiceOccuredUTC>2014-10-02T08:50:30</ServiceOccuredUTC>
  <AdditionalRefs>{{""objectKey"":""value""}}</AdditionalRefs>
</UsageTransaction>";

		const string xmlUsageTransactionV2_1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UsageTransaction xmlns=""http://www.edi.com.au/EnterpriseService/#Usage_2.1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <UsageCount>7</UsageCount>
  <ServiceOccuredUTC>2014-10-02T08:50:30</ServiceOccuredUTC>
  <AdditionalRefs>{{""objectKey21"":""value21""}}</AdditionalRefs>
  <EnterpriseCode>EDI</EnterpriseCode>
  <ServerCode>PRD</ServerCode>
  <Environment>PRD</Environment>
  <CompanyCode>CPC</CompanyCode>
  <CompanyName>WiseTech Global (China) Information Technology Limited</CompanyName>
  <BranchCode>BRN</BranchCode>
  <UsageCode>USS</UsageCode>
</UsageTransaction>";

		const string schemaNameV1 = "http://www.edi.com.au/EnterpriseService/#Usage_1.1";
		const string schemaNameV2 = "http://www.edi.com.au/EnterpriseService/#Usage_2.0";
		const string schemaNameV2_1 = "http://www.edi.com.au/EnterpriseService/#Usage_2.1";
		static bool firstReport = true;
	}
}
