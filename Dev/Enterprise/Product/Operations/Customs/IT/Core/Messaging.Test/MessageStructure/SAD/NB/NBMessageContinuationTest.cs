using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class NBMessageContinuationTest : TestCase
{
	public void TestConstructor()
	{
		var nBDataBlockList = new Mock<IEnumerable<IPreviousOperationInfo>>().Object;
		var sadMessageSendingObject = new Mock<ISadMessageSendingObject>().Object;

		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new NBMessageContinuation(null, "1", 0, ZBool.False, null));
			AssertExceptionThrown<ArgumentException>(() => new NBMessageContinuation(nBDataBlockList, "", 0, ZBool.False, sadMessageSendingObject));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => new NBMessageContinuation(nBDataBlockList, "1", -1, ZBool.False, sadMessageSendingObject));
			AssertNoExceptionThrown(() => new NBMessageContinuation(nBDataBlockList, "1", 0, ZBool.False, sadMessageSendingObject));
		});
	}
}
