using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMMessageTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMMessage(null));
			AssertNoExceptionThrown(() => new IMMessage(new Mock<IIMMessageSendingObject>().Object));
		});
	}
}
