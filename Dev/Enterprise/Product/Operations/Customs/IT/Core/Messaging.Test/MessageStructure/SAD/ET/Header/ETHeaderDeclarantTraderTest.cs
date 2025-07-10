using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETHeaderDeclarantTraderTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETHeaderDeclarantTrader(null));
			AssertNoExceptionThrown(() => new ETHeaderDeclarantTrader(new Mock<IDeclarantTrader>().Object));
		});
	}
}
