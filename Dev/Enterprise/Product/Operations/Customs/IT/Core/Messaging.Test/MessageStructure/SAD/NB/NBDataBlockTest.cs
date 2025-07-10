using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class NBDataBlockTest : TestCase
{
	public void TestConstructor()
	{
		var nbDataBlockList = new Mock<IEnumerable<IPreviousOperationInfo>>().Object;

		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new NBDataBlock(null, ZBool.False));
			AssertNoExceptionThrown(() => new NBDataBlock(nbDataBlockList, ZBool.False));
		});
	}
}
