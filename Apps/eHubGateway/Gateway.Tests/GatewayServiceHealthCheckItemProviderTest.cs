using System;
using System.Threading.Tasks;
using CargoWise.eHub.Gateway.HealthCheckService;
using CargoWise.eServices.Monitoring.HealthCheck.API;
using APIBillingTransaction = CargoWise.Billing.API.BillingTransaction;
using Confluent.Kafka;
using Moq;
using NUnit.Framework;
using CargoWise.Billing.Kafka.API;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace CargoWise.eHub.Gateway.Tests
{
	public class GatewayServiceHealthCheckItemProviderTest
	{
		const string HealthCheckFallbackWarningMessage = "Direct service fallback successful. Original Kafka Error: ";

		[Test]
		public async Task TestGatewayServiceHealthCheckItemProvider()
		{
			BillingMessageHandler.SendBillingToKafka = false;
			var mockProvider = new Mock<GatewayServiceHealthCheckItemProvider> { CallBase = true };
			mockProvider.Setup<bool>(_ => _.StreamedWebService.Ping()).Returns(true);
			mockProvider.Setup<bool>(_ => _.IsAuthenticationEnabled()).Returns(true);
			mockProvider.Setup<bool>(_ => _.IsDatabaseConnected()).Returns(true);
			mockProvider.Setup(_ => _.IsBillingEnabled()).Returns(true);

			var provider = mockProvider.Object;
			Assert.AreEqual("GatewayWebService", provider.Name);

			var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.AreEqual(HealthCheckStatus.OK, checkItem.Status);
			Assert.AreEqual("Service is alive.", checkItem.Description);
		}

		[Test]
		public async Task TestGatewayServiceHealthCheckItemProvider_BillingKafka()
		{
			var mockProvider = new Mock<GatewayServiceHealthCheckItemProvider> { CallBase = true };
			mockProvider.Setup<bool>(_ => _.StreamedWebService.Ping()).Returns(true);
			mockProvider.Setup<bool>(_ => _.IsAuthenticationEnabled()).Returns(true);
			mockProvider.Setup<bool>(_ => _.IsDatabaseConnected()).Returns(true);

			var mockBillingProducer = new Mock<IBillingTransactionProducer<string, APIBillingTransaction>>();
			mockBillingProducer.Setup(_ => _.Produce(It.Is<string>(s => s.Equals("billing-topic")), It.IsAny<Message<string, APIBillingTransaction>>(), It.IsAny<Action<DeliveryReport<string, APIBillingTransaction>>>()))
				.Callback<string, Message<string, APIBillingTransaction>, Action<DeliveryReport<string, APIBillingTransaction>>>(
					(topic, message, handler) =>
					{
						DeliveryReport<string, APIBillingTransaction> report = new DeliveryReport<string, APIBillingTransaction>();
						report.Error = new Error(ErrorCode.NoError);
						mockProvider.Object.HandlerBillingDeliveryReport(report);
					});
			BillingMessageHandler.SendBillingToKafka = true;

			var kafkaClientMock = new Mock<BillingKafkaClient>(BillingMessageHandler.BillingKafkaConfig, null);
			kafkaClientMock.Setup(x => x.CreateProducer(It.IsAny<Func<IBillingTransactionProducer<string, APIBillingTransaction>>>()))
				.Returns(new Lazy<IBillingTransactionProducer<string, APIBillingTransaction>>(() => mockBillingProducer.Object));
			mockProvider.Setup(x => x.CreateKafkaClient()).Returns(kafkaClientMock.Object);

			var provider = mockProvider.Object;
			Assert.AreEqual("GatewayWebService", provider.Name);

			var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.AreEqual(HealthCheckStatus.OK, checkItem.Status);
			Assert.AreEqual("Service is alive.", checkItem.Description);
		}

		[Test]
		public async Task TestGatewayServiceHealthCheckItemProvider_Exception_BillingKafka()
		{
			var mockBillingProducer = new Mock<IBillingTransactionProducer<string, APIBillingTransaction>>();
			BillingMessageHandler.SendBillingToKafka = true;
			mockBillingProducer.Setup(_ => _.Produce(It.Is<string>(s => s.Equals("billing-topic")), It.IsAny<Message<string, APIBillingTransaction>>(), It.IsAny<Action<DeliveryReport<string, APIBillingTransaction>>>()))
				.Throws(new Exception("Billing Exception"));
			var kafkaClientMock = new Mock<BillingKafkaClient>(BillingMessageHandler.BillingKafkaConfig, null);
			kafkaClientMock.Setup(x => x.CreateProducer(It.IsAny<Func<IBillingTransactionProducer<string, APIBillingTransaction>>>()))
				.Returns(new Lazy<IBillingTransactionProducer<string, APIBillingTransaction>>(() => mockBillingProducer.Object));
			var mockProvider = new Mock<GatewayServiceHealthCheckItemProvider> { CallBase = true };
			mockProvider.Setup<bool>(_ => _.StreamedWebService.Ping()).Returns(true);
			mockProvider.Setup<bool>(_ => _.IsAuthenticationEnabled()).Returns(true);
			mockProvider.Setup<bool>(_ => _.IsDatabaseConnected()).Returns(true);
			mockProvider.Setup(_ => _.CreateKafkaClient()).Returns(kafkaClientMock.Object);
			mockProvider.Setup(_ => _.IsBillingEnabled()).Returns(true);

			var provider = mockProvider.Object;
			Assert.AreEqual("GatewayWebService", provider.Name);

			var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.AreEqual(HealthCheckStatus.Warning, checkItem.Status);
			Assert.AreEqual(HealthCheckFallbackWarningMessage + "Failed to send health check message to Kafka queues. Message: Billing Exception", checkItem.Description);
		}

		[Test]
		public async Task TestGatewayServiceHealthCheckItemProvider_ReportError_BillingKafka()
		{
			var mockProvider = new Mock<GatewayServiceHealthCheckItemProvider> { CallBase = true };
			mockProvider.Setup<bool>(_ => _.StreamedWebService.Ping()).Returns(true);
			mockProvider.Setup<bool>(_ => _.IsAuthenticationEnabled()).Returns(true);
			mockProvider.Setup<bool>(_ => _.IsDatabaseConnected()).Returns(true);
			mockProvider.Setup(_ => _.IsBillingEnabled()).Returns(true);

			var mockBillingProducer = new Mock<IBillingTransactionProducer<string, APIBillingTransaction>>();
			BillingMessageHandler.SendBillingToKafka = true;
			mockBillingProducer.Setup(_ => _.Produce(It.Is<string>(s => s.Equals("billing-topic")), It.IsAny<Message<string, APIBillingTransaction>>(), It.IsAny<Action<DeliveryReport<string, APIBillingTransaction>>>()))
				.Callback<string, Message<string, APIBillingTransaction>, Action <DeliveryReport<string, APIBillingTransaction>>>(
					(topic, message, handler) =>
					{
						DeliveryReport<string, APIBillingTransaction> report = new DeliveryReport<string, APIBillingTransaction>();
						report.Error = new Error(ErrorCode.Local_MsgTimedOut, "message timeout");
						mockProvider.Object.HandlerBillingDeliveryReport(report);
					});
			var kafkaClientMock = new Mock<BillingKafkaClient>(BillingMessageHandler.BillingKafkaConfig, null);
			kafkaClientMock.Setup(x => x.CreateProducer(It.IsAny<Func<IBillingTransactionProducer<string, APIBillingTransaction>>>()))
				.Returns(new Lazy<IBillingTransactionProducer<string, APIBillingTransaction>>(() => mockBillingProducer.Object));
			mockProvider.Setup(_ => _.CreateKafkaClient()).Returns(kafkaClientMock.Object);

			var provider = mockProvider.Object;
			Assert.AreEqual("GatewayWebService", provider.Name);

			var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.AreEqual(HealthCheckStatus.Warning, checkItem.Status);
			Assert.AreEqual(HealthCheckFallbackWarningMessage + "Failed to send health check message to Kafka queues. ErrorCode: Local_MsgTimedOut. Reason: message timeout", checkItem.Description);
		}

		[Test]
		public async Task TestGatewayServiceHealthCheckItemProviderThrowException()
		{
			BillingMessageHandler.SendBillingToKafka = true;
			var mockProvider = new Mock<GatewayServiceHealthCheckItemProvider> { CallBase = true };
			mockProvider.Setup<bool>(_ => _.StreamedWebService.Ping()).Returns(false);
			mockProvider.Setup<bool>(_ => _.IsAuthenticationEnabled()).Returns(true);
			mockProvider.Setup<bool>(_ => _.IsDatabaseConnected()).Returns(true);
			mockProvider.Setup(_ => _.IsBillingEnabled()).Returns(true);

			var provider = mockProvider.Object;
			Assert.AreEqual("GatewayWebService", provider.Name);

			var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			var exceptionMessage = "eHubStreamedWebService is not active. ";
			Assert.AreEqual(HealthCheckStatus.Error, checkItem.Status);
			Assert.AreEqual(exceptionMessage, checkItem.Description);
		}

		[Test]
		public async Task TestGatewayServiceHealthCheckItemProvider_FallbackError()
		{
			BillingMessageHandler.SendBillingToKafka = false;
			var mockProvider = new Mock<GatewayServiceHealthCheckItemProvider> { CallBase = false };
			mockProvider.Setup<bool>(_ => _.StreamedWebService.Ping()).Returns(true);
			mockProvider.Setup<bool>(_ => _.IsAuthenticationEnabled()).Returns(true);
			mockProvider.Setup<bool>(_ => _.IsDatabaseConnected()).Returns(true);
			mockProvider.Setup(_ => _.IsBillingEnabled()).Returns(false);

			var provider = mockProvider.Object;
			Assert.AreEqual("GatewayWebService", provider.Name);

			var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.AreEqual(HealthCheckStatus.Error, checkItem.Status);
			Assert.AreEqual("Billing web service is not active.", checkItem.Description);
		}

		[Test]
		public async Task TestGatewayServiceHealthCheckItemProvider_KafkaError()
		{
			BillingMessageHandler.SendBillingToKafka = true;
			var mockProvider = new Mock<GatewayServiceHealthCheckItemProvider> { CallBase = true };
			mockProvider.Setup<bool>(_ => _.StreamedWebService.Ping()).Returns(true);
			mockProvider.Setup<bool>(_ => _.IsAuthenticationEnabled()).Returns(true);
			mockProvider.Setup<bool>(_ => _.IsDatabaseConnected()).Returns(true);
			mockProvider.Setup(_ => _.IsBillingEnabled()).Returns(false);

			var mockBillingProducer = new Mock<IBillingTransactionProducer<string, APIBillingTransaction>>();
			BillingMessageHandler.SendBillingToKafka = true;
			mockBillingProducer.Setup(_ => _.Produce(It.Is<string>(s => s.Equals("billing-topic")), It.IsAny<Message<string, APIBillingTransaction>>(), It.IsAny<Action<DeliveryReport<string, APIBillingTransaction>>>()))
				.Callback<string, Message<string, APIBillingTransaction>, Action<DeliveryReport<string, APIBillingTransaction>>>(
					(topic, message, handler) =>
					{
						DeliveryReport<string, APIBillingTransaction> report = new DeliveryReport<string, APIBillingTransaction>();
						report.Error = new Error(ErrorCode.BrokerNotAvailable, "broker not available");
						mockProvider.Object.HandlerBillingDeliveryReport(report);
					});

			var kafkaClientMock = new Mock<BillingKafkaClient>(BillingMessageHandler.BillingKafkaConfig, null);
			kafkaClientMock.Setup(x => x.CreateProducer(It.IsAny<Func<IBillingTransactionProducer<string, APIBillingTransaction>>>()))
				.Returns(new Lazy<IBillingTransactionProducer<string, APIBillingTransaction>>(() => mockBillingProducer.Object));
			mockProvider.Setup(_ => _.CreateKafkaClient()).Returns(kafkaClientMock.Object);

			var provider = mockProvider.Object;
			Assert.AreEqual("GatewayWebService", provider.Name);

			var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.AreEqual(HealthCheckStatus.Error, checkItem.Status);
			Assert.AreEqual("Original Kafka Error: Failed to send health check message to Kafka queues. ErrorCode: BrokerNotAvailable. Reason: broker not available", checkItem.Description);
		}
	}
}
