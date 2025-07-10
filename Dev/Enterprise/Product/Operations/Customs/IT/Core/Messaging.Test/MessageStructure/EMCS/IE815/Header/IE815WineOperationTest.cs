using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815WineOperationTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			var wineOperationMock = new Mock<IWineOperation>().Object;
			AssertExceptionThrown<ArgumentNullException>(() => new IE815WineOperation(null, 0));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => new IE815WineOperation(wineOperationMock, -1));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => new IE815WineOperation(wineOperationMock, 100));
			AssertNoExceptionThrown(() => new IE815WineOperation(wineOperationMock, 99));
		});
	}
}
