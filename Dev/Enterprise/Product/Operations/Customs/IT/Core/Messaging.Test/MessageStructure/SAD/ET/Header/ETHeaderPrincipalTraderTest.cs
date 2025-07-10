using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETHeaderPrincipalTraderTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETHeaderPrincipalTrader(null));
			AssertNoExceptionThrown(() => new ETHeaderPrincipalTrader(new Mock<IETHeaderPrincipalTrader>().Object));
		});
	}
}
