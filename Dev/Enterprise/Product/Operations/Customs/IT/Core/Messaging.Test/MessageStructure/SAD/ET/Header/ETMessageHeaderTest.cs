using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETMessageHeaderTest : TestCase
{
	public void TestConstructor()
	{
		var etHeader = new Mock<IETHeader>().Object;
		var sadMessageSendingObject = new Mock<ISadMessageSendingObject>().Object;

		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETMessageHeader(null, null));
			AssertExceptionThrown<ArgumentNullException>(() => new ETMessageHeader(etHeader, null));
			AssertExceptionThrown<ArgumentNullException>(() => new ETMessageHeader(null, sadMessageSendingObject));
			AssertNoExceptionThrown(() => new ETMessageHeader(etHeader, sadMessageSendingObject));
		});
	}
}
