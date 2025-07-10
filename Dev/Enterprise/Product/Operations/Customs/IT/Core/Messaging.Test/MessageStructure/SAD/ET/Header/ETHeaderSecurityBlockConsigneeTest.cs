using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETHeaderSecurityBlockConsigneeTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETHeaderSecurityBlockConsignee(null));
			AssertNoExceptionThrown(() => new ETHeaderSecurityBlockConsignee(new Mock<ITrader>().Object));
		});
	}
}
