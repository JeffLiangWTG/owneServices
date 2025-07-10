using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETHeaderSecurityBlockConsignorTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETHeaderSecurityBlockConsignor(null));
			AssertNoExceptionThrown(() => new ETHeaderSecurityBlockConsignor(new Mock<ITrader>().Object));
		});
	}
}
