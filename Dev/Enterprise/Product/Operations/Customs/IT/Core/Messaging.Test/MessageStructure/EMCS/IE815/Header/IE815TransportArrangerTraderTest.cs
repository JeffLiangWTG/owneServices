using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815TransportArrangerTraderTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IE815TransportArrangerTrader(null));
			AssertNoExceptionThrown(() => new IE815TransportArrangerTrader(new Mock<ITransportTrader>().Object));
		});
	}
}
