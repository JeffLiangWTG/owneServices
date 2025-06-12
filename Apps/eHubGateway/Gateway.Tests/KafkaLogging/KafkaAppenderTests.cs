using Confluent.Kafka;
using log4net;
using log4net.Core;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;


namespace CargoWise.eHub.Gateway.Tests.KafkaLogging
{
	class KafkaAppenderTests
	{

		[Test]
		public void KafkaAppender_ShouldContainKafkaOptionsSettings()
		{
			var resource = TestHelper.GetEmbeddedResource("KafkaLogging.TestFiles.CargoWise.eHub.Gateway.KafkaAppender.log4net.config");
			log4net.Config.XmlConfigurator.Configure(resource);
			var appenders = LogManager.GetRepository().GetAppenders();
			var kafkaAppender = appenders[0] as KafkaAppender;

			ClassicAssert.IsTrue(LogManager.GetRepository().Configured);
			Assert.That(appenders.Length, Is.EqualTo(1));
			Assert.That(appenders[0].ToString(), Is.EqualTo("CargoWise.eHub.Gateway.KafkaAppender"));

			Assert.That(kafkaAppender.kafkaOptions.Brokers, Is.EqualTo("106-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044,107-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044"));
			Assert.That(kafkaAppender.kafkaOptions.Topic, Is.EqualTo("topic-au2-test-ehubgateway-logs-test"));
			Assert.That(kafkaAppender.kafkaOptions.Acks, Is.EqualTo(Acks.None));
			Assert.That(kafkaAppender.kafkaOptions.SecurityProtocol, Is.EqualTo(SecurityProtocol.SaslSsl));
			Assert.That(kafkaAppender.kafkaOptions.SaslUsername, Is.EqualTo("saslusername"));
			Assert.That(kafkaAppender.kafkaOptions.SaslMechanism, Is.EqualTo(SaslMechanism.Plain));
		}

		[Test]
		public void GetMessage_ShouldReturnMessage_WhenLoggingEventIsAdded()
		{
			var loggingData = new LoggingEventData();
			loggingData.Domain = "Domain";
			loggingData.LoggerName = "loggerName";
			loggingData.Level = Level.Debug;
			loggingData.Message = "This is a test message.";

			var loggingEvent = new LoggingEvent(loggingData);

			var message = KafkaAppender.GetMessage(loggingEvent);

			ClassicAssert.IsNull(message.Key);
			Assert.That(message.Value.ApplicationName, Is.EqualTo("Domain"));
			Assert.That(message.Value.LoggerName, Is.EqualTo("loggerName"));
			Assert.That(message.Value.Level, Is.EqualTo("DEBUG"));
			Assert.That(message.Value.Message, Is.EqualTo("This is a test message."));
			ClassicAssert.IsNull(message.Value.ExceptionID, "The exceptionID should be null.");
		}

		[Test]
		public void GetMessage_ShouldReturnExceptionID_WhenExceptionIDIsAdded()
		{
			var loggingData = new LoggingEventData();
			loggingData.Domain = "Domain";
			loggingData.LoggerName = "loggerName";
			loggingData.Level = Level.Debug;
			loggingData.Message = "This is a test message.";

			var loggingEvent = new LoggingEvent(loggingData);
			loggingEvent.Properties["ExceptionID"] = "30c140aa-de72-4ffd-b01e-a500f6273e53";

			var message = KafkaAppender.GetMessage(loggingEvent);

			ClassicAssert.IsNull(message.Key);
			Assert.That(message.Value.ApplicationName, Is.EqualTo("Domain"));
			Assert.That(message.Value.LoggerName, Is.EqualTo("loggerName"));
			Assert.That(message.Value.Level, Is.EqualTo("DEBUG"));
			Assert.That(message.Value.Message, Is.EqualTo("This is a test message."));
			Assert.That(message.Value.ExceptionID, Is.EqualTo("30c140aa-de72-4ffd-b01e-a500f6273e53"));
		}

		[Test]
		public void GetProducerConfig_ShouldReturnProducerConfig_WhenKafkaOptionsAreAdded()
		{
			var resource = TestHelper.GetEmbeddedResource("KafkaLogging.TestFiles.CargoWise.eHub.Gateway.KafkaAppender.log4net.config");
			log4net.Config.XmlConfigurator.Configure(resource);
			var appenders = LogManager.GetRepository().GetAppenders();
			var kafkaAppender = appenders[0] as KafkaAppender;
			;

			ClassicAssert.IsTrue(LogManager.GetRepository().Configured);
			Assert.That(appenders.Length, Is.EqualTo(1));
			Assert.That(appenders[0].ToString(), Is.EqualTo("CargoWise.eHub.Gateway.KafkaAppender"));

			var producerConfig = KafkaAppender.GetProducerConfig(kafkaAppender.kafkaOptions);

			Assert.That(producerConfig.BootstrapServers, Is.EqualTo("106-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044,107-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044"));
			Assert.That(producerConfig.Acks, Is.EqualTo(Acks.None));
			Assert.That(producerConfig.SecurityProtocol, Is.EqualTo(SecurityProtocol.SaslSsl));
			Assert.That(producerConfig.SaslUsername, Is.EqualTo("saslusername"));
			Assert.That(producerConfig.SaslPassword, Is.EqualTo("testsaslpassword"));
			Assert.That(producerConfig.SaslMechanism, Is.EqualTo(SaslMechanism.Plain));
			ClassicAssert.IsTrue(producerConfig.EnableSslCertificateVerification);
		}

		[Test]
		public void KafkaAppender_ShouldSendToKafka_WhenMessageLogged()
		{
			var resource = TestHelper.GetEmbeddedResource("KafkaLogging.TestFiles.CargoWise.eHub.Gateway.KafkaAppender.log4net.config");
			log4net.Config.XmlConfigurator.Configure(resource);

			var mockProducer = new Mock<IProducer<Null, LogEvent>>();

			var kafkaAppender = LogManager.GetRepository().GetAppenders()[0] as KafkaAppender;
			kafkaAppender.producer = mockProducer.Object;

			var logger = LogManager.GetLogger("testLogger");
			logger.Info("Logging information message");

			mockProducer.Verify(x => x.Produce(It.IsAny<string>(), It.IsAny<Message<Null, LogEvent>>(), null), Times.Once);
		}
	}
}
