using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.eHub.MessageEvent.KafkaProducerService.Options;
using CargoWise.eHub.MessageEvent.KafkaProducerService.ServiceBroker;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace CargoWise.eHub.MessageEvent.KafkaProducerService.Tests.HostedService
{
	public class KafkaProducerServiceTests
	{
		private Mock<IOptions<KafkaOptions>> _kafkaOptionsMock;
		private Mock<IOptions<ServiceBrokerOptions>> _serviceBrokerOptionsMock;
		private Mock<ILogger<KafkaProducerService.HostedService.KafkaProducerService>> _loggerMock;
		private IList<XmlDocument> _messages;
		private Mock<IProducer<string, XmlDocument>> _producerMock;
		private Mock<IMessageEventServiceBroker> _serviceBrokerMock;
		private IServiceProvider _serviceProvider;
		private ServiceCollection _services;
		private KafkaProducerService.HostedService.KafkaProducerService _kafkaProducerService;
		private const string Topic = "TestTopic";
		private CancellationTokenSource _cancellationTokenSource;

		[SetUp]
		public void SetUp()
		{
			_services = new ServiceCollection();
			_producerMock = new Mock<IProducer<string, XmlDocument>>();
			_serviceBrokerMock = new Mock<IMessageEventServiceBroker>();
			_kafkaOptionsMock = new Mock<IOptions<KafkaOptions>>();
			_serviceBrokerOptionsMock = new Mock<IOptions<ServiceBrokerOptions>>();
			_loggerMock = new Mock<ILogger<KafkaProducerService.HostedService.KafkaProducerService>>();
			_cancellationTokenSource = new CancellationTokenSource();

			_services.AddSingleton(_producerMock.Object);
			_services.AddSingleton(_serviceBrokerMock.Object);
			_services.AddSingleton(_kafkaOptionsMock.Object);
			_services.AddSingleton(_serviceBrokerOptionsMock.Object);
			_serviceProvider = _services.BuildServiceProvider();

			const string content = @"
<MessageEvent>
	<Message>
		<TrackingID>TrackingID_Test</TrackingID>
	</Message>
	<eHub>
		<Inbox>
			<PK>A6B31702-D106-41D3-9915-2F8298255D06</PK>
		</Inbox>
	</eHub>
</MessageEvent>
";
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(content);
			_messages = new List<XmlDocument>
			{
				xmlDocument
			};
			_serviceBrokerMock.Setup(x => x.GetNextBatchMessagesAsync()).Returns(_messages.ToAsyncEnumerable());
			_loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
			_kafkaOptionsMock.SetupGet(x => x.Value).Returns(new KafkaOptions { Topic = Topic, TransactionTimeoutSecond = 100 });
			_serviceBrokerOptionsMock.SetupGet(x => x.Value).Returns(new ServiceBrokerOptions{ BatchSize = 1000, TimeoutSecond = 30, RetryIntervalSeconds = 5 });

			_kafkaProducerService = new KafkaProducerService.HostedService.KafkaProducerService(_serviceProvider, _loggerMock.Object);
		}

		[Test]
		public async Task TestExecuteAsync_DoWorkAsyncCalled()
		{
			var partialMock = new Mock<KafkaProducerService.HostedService.KafkaProducerService>(_serviceProvider, _loggerMock.Object) { CallBase = true };
			partialMock.Setup(x => x.DoWorkCoreAsync(It.IsAny<IProducer<string, XmlDocument>>())).Callback(() => _cancellationTokenSource.Cancel());

			await partialMock.Object.StartAsync(_cancellationTokenSource.Token);

			partialMock.Verify(x => x.DoWorkCoreAsync(It.IsAny<IProducer<string, XmlDocument>>()), Times.AtLeastOnce);
			_producerMock.Verify(x => x.Dispose(), Times.Once);
			_loggerMock.Verify(GetExpression("KafkaProducerService starting"), Times.Once);
		}

		[Test]
		public async Task TestExecuteAsync_DisposeExceptionLogged()
		{
			var exception = new Exception();

			var partialMock = new Mock<KafkaProducerService.HostedService.KafkaProducerService>(_serviceProvider, _loggerMock.Object) { CallBase = true };
			partialMock.Setup(x => x.DoWorkCoreAsync(It.IsAny<IProducer<string, XmlDocument>>())).Callback(() => _cancellationTokenSource.Cancel()).Returns(Task.FromResult(0));
			partialMock.Setup(x => x.LogExceptionAndWait(It.IsAny<Exception>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			_producerMock.Setup(x => x.Dispose()).Throws(exception);

			await partialMock.Object.StartAsync(_cancellationTokenSource.Token);

			partialMock.Verify(x => x.LogExceptionAndWait(exception, It.IsAny<CancellationToken>()), Times.Once);
		}

		[Test]
		public async Task TestExecuteAsync_ExceptionHandled()
		{
			var exception = new Exception();

			var partialMock = new Mock<KafkaProducerService.HostedService.KafkaProducerService>(_serviceProvider, _loggerMock.Object) { CallBase = true };
			partialMock.Setup(x => x.DoWorkCoreAsync(It.IsAny<IProducer<string, XmlDocument>>())).Callback(() => _cancellationTokenSource.Cancel()).Throws(exception);
			partialMock.Setup(x => x.LogExceptionAndWait(It.IsAny<Exception>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

			await partialMock.Object.StartAsync(_cancellationTokenSource.Token);

			partialMock.Verify(x => x.DoWorkCoreAsync(It.IsAny<IProducer<string, XmlDocument>>()), Times.Once);
			partialMock.Verify(x => x.LogExceptionAndWait(It.IsAny<Exception>(), It.IsAny<CancellationToken>()), Times.Once);
		}

		[Test]
		public async Task TestExecuteAsync_KafkaExceptionHandled_RenewProducer()
		{
			var exception = new KafkaException(new Error(ErrorCode.BrokerNotAvailable, "Reason", true));
			var loopCount = 0;
			var endLoopNum = 2;

			var partialMock = new Mock<KafkaProducerService.HostedService.KafkaProducerService>(_serviceProvider, _loggerMock.Object) { CallBase = true };
			partialMock.Setup(x => x.DoWorkCoreAsync(It.IsAny<IProducer<string, XmlDocument>>())).Callback(() =>
			{
				loopCount++;
				if (loopCount >= endLoopNum)
				{
					_cancellationTokenSource.Cancel();
				}
			}).Throws(exception);
			partialMock.Setup(x => x.LogExceptionAndWait(It.IsAny<Exception>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

			await partialMock.Object.StartAsync(_cancellationTokenSource.Token);

			_loggerMock.Verify(GetExpression("Caught Kafka exception: BrokerNotAvailable-Reason", LogLevel.Error), Times.Exactly(endLoopNum));
			partialMock.Verify(x => x.DoWorkCoreAsync(It.IsAny<IProducer<string, XmlDocument>>()), Times.Exactly(endLoopNum));
			partialMock.Verify(x => x.LogExceptionAndWait(It.IsAny<Exception>(), It.IsAny<CancellationToken>()), Times.Exactly(endLoopNum));
			_producerMock.Verify(x => x.Dispose(), Times.Exactly(endLoopNum));
		}

		[Test]
		public async Task TestDoWorkAsync_AllServiceCalled()
		{
			await _kafkaProducerService.DoWorkCoreAsync(_producerMock.Object);

			_serviceBrokerMock.Verify(x => x.BeginTransaction(), Times.Once);
			_serviceBrokerMock.Verify(x => x.GetNextBatchMessagesAsync(), Times.Once);
			_serviceBrokerMock.Verify(x => x.Commit(), Times.Once);
			_serviceBrokerMock.Verify(x => x.Dispose(), Times.Once);

			_producerMock.Verify(x => x.BeginTransaction(), Times.Once);
			_producerMock.Verify(x => x.Produce(Topic, It.IsAny<Message<string, XmlDocument>>(), null), Times.AtLeastOnce);
			_producerMock.Verify(x => x.CommitTransaction(), Times.Once);

			_serviceBrokerMock.Verify(x => x.Rollback(), Times.Never);

			_loggerMock.Verify(GetExpression("DoWorkCoreAsync started"), Times.Once);
			_loggerMock.Verify(GetExpression("DoWorkCoreAsync finished"), Times.Once);

			_loggerMock.Verify(GetExpression("BeginTransaction for producer started", LogLevel.Trace), Times.Once);
			_loggerMock.Verify(GetExpression("BeginTransaction for producer finished", LogLevel.Trace), Times.Once);
			_loggerMock.Verify(GetExpression("BeginTransaction for service broker started", LogLevel.Trace), Times.Once);
			_loggerMock.Verify(GetExpression("BeginTransaction for service broker finished", LogLevel.Trace), Times.Once);
			_loggerMock.Verify(GetExpression("Queried InboxPK: A6B31702-D106-41D3-9915-2F8298255D06", LogLevel.Trace), Times.Once);
			_loggerMock.Verify(GetExpression("CommitTransaction for producer started", LogLevel.Trace), Times.Once);
			_loggerMock.Verify(GetExpression("CommitTransaction for producer finished", LogLevel.Trace), Times.Once);
			_loggerMock.Verify(GetExpression("Commit for service broker started", LogLevel.Trace), Times.Once);
			_loggerMock.Verify(GetExpression("Commit for service broker finished", LogLevel.Trace), Times.Once);
		}

		[Test]
		public void TestDoWorkAsync_CalledRollback_ProduceException()
		{
			var exception = new ProduceException<string, XmlDocument>(new Error(ErrorCode.BrokerNotAvailable, "Reason"), new DeliveryResult<string, XmlDocument>());

			_producerMock
				.Setup(x => x.Produce(Topic, It.IsAny<Message<string, XmlDocument>>(), null
				))
				.Throws(exception);

			Assert.That(async () => await _kafkaProducerService.DoWorkCoreAsync(_producerMock.Object), Throws.InstanceOf<ProduceException<string, XmlDocument>>());

			_serviceBrokerMock.Verify(x => x.Rollback(), Times.Once);
			_producerMock.Verify(x => x.AbortTransaction(), Times.Once);

			_loggerMock.Verify(GetExpression("Rollback for service broker started", LogLevel.Trace), Times.Once);
			_loggerMock.Verify(GetExpression("Rollback for service broker finished", LogLevel.Trace), Times.Once);
			_loggerMock.Verify(GetExpression("AbortTransaction for producer started", LogLevel.Trace), Times.Once);
			_loggerMock.Verify(GetExpression("AbortTransaction for producer finished", LogLevel.Trace), Times.Once);
		}

		[Test]
		public void TestDoWorkAsync_CalledRollback_Exception()
		{
			var exception = new Exception();

			_serviceBrokerMock.Setup(x => x.GetNextBatchMessagesAsync()).Throws(exception);

			Assert.That(async () => await _kafkaProducerService.DoWorkCoreAsync(_producerMock.Object), Throws.InstanceOf<Exception>());

			_serviceBrokerMock.Verify(x => x.Rollback(), Times.Once);

			_loggerMock.Verify(GetExpression("Rollback for service broker started", LogLevel.Trace), Times.Once);
			_loggerMock.Verify(GetExpression("Rollback for service broker finished", LogLevel.Trace), Times.Once);
			_loggerMock.Verify(GetExpression("AbortTransaction for producer started", LogLevel.Trace), Times.Once);
			_loggerMock.Verify(GetExpression("AbortTransaction for producer finished", LogLevel.Trace), Times.Once);
		}

		[Test]
		public void TestDoWorkAsync_AbortTransaction_ExceptionLogged()
		{
			var serviceBrokerException = new Exception();
			var producerException = new Exception();

			_serviceBrokerMock.Setup(x => x.GetNextBatchMessagesAsync()).Throws(new Exception());
			_serviceBrokerMock.Setup(x => x.Rollback()).Throws(serviceBrokerException);
			_producerMock.Setup(x => x.AbortTransaction()).Throws(producerException);

			Assert.That(async () => await _kafkaProducerService.DoWorkCoreAsync(_producerMock.Object), Throws.InstanceOf<Exception>());

			_loggerMock.Verify(GetExpression("serviceBroker transaction rollback failed", LogLevel.Error, serviceBrokerException), Times.Once);
			_loggerMock.Verify(GetExpression("producer transaction rollback failed", LogLevel.Error, producerException), Times.Once);
		}

		private static Expression<Action<ILogger<KafkaProducerService.HostedService.KafkaProducerService>>> GetExpression(string message, LogLevel level = LogLevel.Information, Exception exception = null)
		{
			return x => x.Log(
				level,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((o, t) => o.ToString() == message),
				exception,
				It.IsAny<Func<It.IsAnyType, Exception, string>>());
		}
	}
}
