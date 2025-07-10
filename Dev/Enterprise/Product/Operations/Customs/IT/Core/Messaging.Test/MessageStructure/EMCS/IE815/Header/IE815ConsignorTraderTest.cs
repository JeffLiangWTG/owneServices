using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815ConsignorTraderTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IE815ConsignorTrader(null));
			AssertNoExceptionThrown(() => new IE815ConsignorTrader(new Mock<IConsignorTrader>().Object));
		});
	}
}
