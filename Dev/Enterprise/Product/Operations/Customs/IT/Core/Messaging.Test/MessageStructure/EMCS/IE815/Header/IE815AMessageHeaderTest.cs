using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815AMessageHeaderTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			var messageMock = new Mock<IIE815EMCSMessage>();
			AssertExceptionThrown<ArgumentNullException>(() => new IE815AMessageHeader(null));
			AssertExceptionThrown<ArgumentNullException>(() => new IE815AMessageHeader(messageMock.Object));
			messageMock.Setup(m => m.Header).Returns(new Mock<IIE815EMCSMessageHeader>().Object);
			AssertNoExceptionThrown(() => new IE815AMessageHeader(messageMock.Object));
		});
	}
}
