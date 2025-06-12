using System;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.eHub.MessageEvent.KafkaProducerService.KafkaProducer;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CargoWise.eHub.MessageEvent.KafkaProducerService.Tests.KafkaProducer
{
	public class MessageEventJsonSerializerTests
	{
		private Mock<ILogger<MessageEventJsonSerializer>> _loggerMock;
		private MessageEventJsonSerializer _messageSerializer;

		[SetUp]
		public void SetUp()
		{
			_loggerMock = new Mock<ILogger<MessageEventJsonSerializer>>();
			_loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
			_messageSerializer = new MessageEventJsonSerializer(_loggerMock.Object);
		}

		[Test]
		public void TestSerialize_ReturnEmpty_WhenMessageIsNull()
		{
			var result = _messageSerializer.Serialize(null, SerializationContext.Empty);
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Length, Is.EqualTo(0));
		}

		[Test]
		public void TestSerialize_ReturnValidBytes()
		{
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
            <Login/>
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

			var expected = "{\"@timestamp\":\"2024-04-21T02:30:11.8797424\",\"MessageEvent\":{\"Event\":{\"Type\":\"Received\",\"TimeUTC\":\"2022-08-29T07:21:32.337\"}," +
				"\"Message\":{\"SenderID\":\"HYETSTTST\",\"RecipientID\":\"HYETSTREP\",\"TrackingID\":\"025AC2F7-9869-4AD6-BE0D-F54FC05B8A5A\"}," +
				"\"eHub\":{\"Event\":{\"Source\":\"SYDCO-MACHINE-1\",\"Login\":\"\"}," +
				"\"Inbox\":{\"Status\":\"0\",\"PK\":\"5E3FC0D3-1637-4BE7-9B82-5E4DCE16D3E5\",\"ApplicationCode\":\"UDM\",\"TrackingID\":\"6E6E4DB5-C6FC-479B-A74E-481BE1CDF6AF\",\"ColumnsUpdated\":\"0xFF7F\"}}}}";
			Assert.That(result, Is.Not.Null);
			var resultDecoded = Encoding.Default.GetString(result);
			Assert.That(resultDecoded, Is.EqualTo(expected));
			_loggerMock.Verify(x => x.Log(
					LogLevel.Trace,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Serialized value:")),
					It.IsAny<Exception>(),
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once);
		}
	}
}
