using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMHeaderDeclarantTraderTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMHeaderDeclarantTrader(null));
			AssertNoExceptionThrown(() => new IMHeaderConsignor(new Mock<IDeclarantTrader>().Object));
		});
	}
}
