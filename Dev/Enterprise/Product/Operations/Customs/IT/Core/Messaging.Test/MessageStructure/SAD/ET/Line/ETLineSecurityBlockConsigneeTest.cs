using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETLineSecurityBlockConsigneeTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETLineSecurityBlockConsignee(null));
			AssertNoExceptionThrown(() => new ETLineSecurityBlockConsignee(new Mock<ITrader>().Object));
		});
	}
}
