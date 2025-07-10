using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815DeliveryPlaceTraderTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IE815DeliveryPlaceTrader(null));
			AssertNoExceptionThrown(() => new IE815DeliveryPlaceTrader(new Mock<IDeliveryPlaceTrader>().Object));
		});
	}
}
