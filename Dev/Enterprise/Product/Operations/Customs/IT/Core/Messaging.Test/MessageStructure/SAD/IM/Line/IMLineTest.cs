using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMLineTest : TestCase
{
	public void TestConstructor()
	{
		var imLine = new Mock<IIMLine>().Object;
		var imHeader = new Mock<IIMHeader>().Object;
		var sadMessageSendingObject = new Mock<ISadMessageSendingObject>().Object;

		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMMessageLine(null, null, null));
			AssertExceptionThrown<ArgumentNullException>(() => new IMMessageLine(imLine, null, null));
			AssertExceptionThrown<ArgumentNullException>(() => new IMMessageLine(imLine, imHeader, null));
			AssertNoExceptionThrown(() => new IMMessageLine(imLine, imHeader, sadMessageSendingObject));
		});
	}
}
