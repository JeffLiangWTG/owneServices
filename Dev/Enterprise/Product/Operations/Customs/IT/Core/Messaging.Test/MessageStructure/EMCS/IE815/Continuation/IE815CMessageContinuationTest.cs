using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815CMessageContinuationTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			var messageContinuationMock = new Mock<IIE815EMCSMessageContinuation>().Object;
			AssertExceptionThrown<ArgumentNullException>(() => new IE815CMessageContinuation(null, 0));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => new IE815CMessageContinuation(messageContinuationMock, -1));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => new IE815CMessageContinuation(messageContinuationMock, 100));
			AssertNoExceptionThrown(() => new IE815CMessageContinuation(messageContinuationMock, 1));
		});
	}
}
