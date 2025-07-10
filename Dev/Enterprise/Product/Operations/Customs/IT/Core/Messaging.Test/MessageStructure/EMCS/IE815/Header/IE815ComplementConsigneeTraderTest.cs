using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815ConsigneeTraderTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IE815ComplementConsigneeTrader(null));
			AssertNoExceptionThrown(() => new IE815ComplementConsigneeTrader(new Mock<IComplementConsigneeTrader>().Object));
		});
	}
}
