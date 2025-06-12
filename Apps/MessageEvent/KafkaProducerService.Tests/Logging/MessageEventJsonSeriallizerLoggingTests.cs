using System;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.eHub.MessageEvent.KafkaProducerService.KafkaProducer;
using CargoWise.eHub.MessageEvent.KafkaProducerService.Logging;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Serilog;

namespace CargoWise.eHub.MessageEvent.KafkaProducerService.Tests.Logging
{
	public class MessageEventJsonSeriallizerLoggingTests
	{
		private MessageEventJsonSerializer _messageSerializer;
		private StringWriter _writer;

		[SetUp]
		public void SetUp()
		{
			_writer = new StringWriter();
			Console.SetOut(_writer);

			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.Enrich.FromLogContext()
				.Enrich.WithMachineName()
				.Enrich.WithProcessId()
				.Enrich.WithThreadId()
				.Enrich.With<LogLevelEnricher>()
				.Enrich.With<ContextSourceReplaceEnricher>()
				.WriteTo.Console(
					formatter: new IndentationMessageTemplateTextFormatter()
				)
				.CreateBootstrapLogger();

			var loggerFactory = new LoggerFactory().AddSerilog();
			var _logger = loggerFactory.CreateLogger<MessageEventJsonSerializer>();

			_messageSerializer = new MessageEventJsonSerializer(_logger);
		}

		[Test]
		public void TestSerialize_ReturnValidBytes()
		{
			_writer.GetStringBuilder().Clear();
			using var input = new MemoryStream(Encoding.Unicode.GetBytes(@"<MessageEvent timestamp=""2024-04-21T02:30:11.8797424"">
    <Event>
        <Type>Received</Type>
        <TimeUTC>2022-08-29T07:21:32.337</TimeUTC>
    </Event>
    <Message>
        <SenderID>HYETSTTST</SenderID>
        <RecipientID>HYETSTREP</RecipientID>
        <TrackingID>025AC2F7-9869-4AD6-BE0D-F54FC05B8A5A</TrackingID>
    </Message>
    <eHub>
        <Event>
            <Source>SYDCO-MACHINE-1</Source>
        </Event>
        <Inbox>
            <Status>0</Status>
            <PK>5E3FC0D3-1637-4BE7-9B82-5E4DCE16D3E5</PK>
            <ApplicationCode>UDM</ApplicationCode>
            <TrackingID>6E6E4DB5-C6FC-479B-A74E-481BE1CDF6AF</TrackingID>
            <ColumnsUpdated>0xFF7F</ColumnsUpdated>
        </Inbox>
    </eHub>
</MessageEvent>
"));
			using var xmlReader = XmlReader.Create(input);
			var doc = new XmlDocument();
			doc.Load(xmlReader);

			var result = _messageSerializer.Serialize(doc, SerializationContext.Empty);

			Assert.That(TestLog.Message.SplitAndCheck(_writer.ToString()), Is.True);

		}
	}
}
