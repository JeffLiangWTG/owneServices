using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMHeaderTest : TestCase
{
	public void TestConstructor()
	{
		var imHeader = new Mock<IIMHeader>().Object;
		var sadMessageSendingObject = new Mock<ISadMessageSendingObject>().Object;

		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMMessageHeader(null, null));
			AssertExceptionThrown<ArgumentNullException>(() => new IMMessageHeader(imHeader, null));
			AssertExceptionThrown<ArgumentNullException>(() => new IMMessageHeader(null, sadMessageSendingObject));
			AssertNoExceptionThrown(() => new IMMessageHeader(imHeader, sadMessageSendingObject));
		});
	}
}
