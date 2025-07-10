using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETLineSecurityBlockConsignorTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETLineSecurityBlockConsignor(null));
			AssertNoExceptionThrown(() => new ETLineSecurityBlockConsignor(new Mock<ITrader>().Object));
		});
	}
}
