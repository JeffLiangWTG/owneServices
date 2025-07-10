using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815ComplementConsigneeTraderTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IE815ConsigneeTrader(null));
			AssertNoExceptionThrown(() => new IE815ConsigneeTrader(new Mock<IConsigneeTrader>().Object));
		});
	}
}
