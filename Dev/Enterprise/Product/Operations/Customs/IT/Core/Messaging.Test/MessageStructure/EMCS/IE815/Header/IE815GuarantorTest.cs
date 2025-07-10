using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815GuarantorTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			var guarantorTraderMock = new Mock<IGuarantorTrader>().Object;
			AssertExceptionThrown<ArgumentNullException>(() => new IE815GuarantorTrader(null, 0));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => new IE815GuarantorTrader(guarantorTraderMock, -1));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => new IE815GuarantorTrader(guarantorTraderMock, 3));
			AssertNoExceptionThrown(() => new IE815GuarantorTrader(guarantorTraderMock, 1));
		});
	}
}
