using System;
using System.Collections.Generic;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class NBMessageHeaderTest : TestCase
{
	public void TestConstructor()
	{
		var nBMessageHeader = new Mock<INBHeader>().Object;
		var nBDataBlockList = new Mock<IEnumerable<IPreviousOperationInfo>>().Object;
		var sadMessageSendingObject = new Mock<ISadMessageSendingObject>().Object;

		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new NBMessageHeader(null, nBDataBlockList, "1", 0, null));
			AssertExceptionThrown<ArgumentNullException>(() => new NBMessageHeader(nBMessageHeader, null, "1", 0, null));
			AssertExceptionThrown<ArgumentException>(() => new NBMessageHeader(nBMessageHeader, nBDataBlockList, "", 0, sadMessageSendingObject));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => new NBMessageHeader(nBMessageHeader, nBDataBlockList, "1", -1, sadMessageSendingObject));
			AssertNoExceptionThrown(() => new NBMessageHeader(nBMessageHeader, nBDataBlockList, "1", 0, sadMessageSendingObject));
		});
	}
}
