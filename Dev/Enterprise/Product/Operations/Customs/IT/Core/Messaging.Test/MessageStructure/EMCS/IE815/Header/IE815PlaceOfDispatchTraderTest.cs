using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815PlaceOfDispatchTraderTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IE815PlaceOfDispatchTrader(null));
			AssertNoExceptionThrown(() => new IE815PlaceOfDispatchTrader(new Mock<IPlaceOfDispatchTrader>().Object));
		});
	}
}
