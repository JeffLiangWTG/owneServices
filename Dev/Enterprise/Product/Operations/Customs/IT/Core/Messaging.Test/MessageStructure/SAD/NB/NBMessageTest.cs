using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class NBMessageTest : TestCase
{
	public void TestConstructor()
	{
		var nbMessageSendingObject = new Mock<INBMessageSendingObject>().Object;
		var sadMessageSendingObject = new Mock<ISadMessageSendingObject>().Object;

		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new NBMessage(null, null));
			AssertExceptionThrown<ArgumentNullException>(() => new NBMessage(nbMessageSendingObject, null));
			AssertExceptionThrown<ArgumentNullException>(() => new NBMessage(null, sadMessageSendingObject));
			AssertNoExceptionThrown(() => new NBMessage(nbMessageSendingObject, sadMessageSendingObject));

			AssertExceptionThrown<ArgumentNullException>(() => new NBMessage(null, 0, null));
			AssertExceptionThrown<ArgumentNullException>(() => new NBMessage(nbMessageSendingObject, 0, null));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => new NBMessage(nbMessageSendingObject, -1, sadMessageSendingObject));
			AssertNoExceptionThrown(() => new NBMessage(nbMessageSendingObject, 0, sadMessageSendingObject));
			AssertNoExceptionThrown(() => new NBMessage(nbMessageSendingObject, 1, sadMessageSendingObject));
		});
	}
}
