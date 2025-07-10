using System;
using System.Linq;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETMessageTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETMessage(null));
			AssertNoExceptionThrown(() => new ETMessage(new Mock<IETMessageSendingObject>().Object));
		});
	}

	public void TestProgressiveNumber()
	{
		var etMessageSendingObjectMock = new Mock<IETMessageSendingObject>();
		etMessageSendingObjectMock.Setup(x => x.MessageHeader).Returns(new Mock<IETHeader>().Object);
		etMessageSendingObjectMock.Setup(x => x.MessageLines).Returns(new[] { new Mock<IETLine>().Object });

		etMessageSendingObjectMock.Setup(x => x.NBMessages).Returns(new[] { GetNbMessageSendingObject(), GetNbMessageSendingObject() });

		var etMessage = new ETMessage(etMessageSendingObjectMock.Object);

		CombineAssertions("Assert progressive numbers", () =>
		{
			AssertEquals("Main ET Message Progressive Number", 0, etMessage.MessageHeader.FixedPart.ProgressiveNumber);
			AssertEquals("NB Message 1 Progressive Number", 1, etMessage.NBMessages.ElementAt(0).Header.FixedPart.ProgressiveNumber);
			AssertEquals("NB Message 2 Progressive Number", 2, etMessage.NBMessages.ElementAt(1).Header.FixedPart.ProgressiveNumber);
		});

		INBMessageSendingObject GetNbMessageSendingObject()
		{
			var nbMessageSendingMock = new Mock<INBMessageSendingObject>();
			nbMessageSendingMock.Setup(x => x.AnnualProgressiveNumber).Returns("1000");
			nbMessageSendingMock.Setup(x => x.Header).Returns(new Mock<INBHeader>().Object);

			return nbMessageSendingMock.Object;
		}
	}
}
