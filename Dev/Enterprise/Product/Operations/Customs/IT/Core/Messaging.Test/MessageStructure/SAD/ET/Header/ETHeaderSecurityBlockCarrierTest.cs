using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETHeaderSecurityBlockCarrierTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETHeaderSecurityBlockCarrier(null));
			AssertNoExceptionThrown(() => new ETHeaderSecurityBlockCarrier(new Mock<ITrader>().Object));
		});
	}
}
