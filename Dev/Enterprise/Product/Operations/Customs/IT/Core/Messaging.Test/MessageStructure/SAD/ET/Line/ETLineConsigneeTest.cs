using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETLineConsigneeTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETLineConsignee(null));
			AssertNoExceptionThrown(() => new ETLineConsignee(new Mock<ITrader>().Object));
		});
	}
}
