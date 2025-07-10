using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMHeaderConsigneeTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMHeaderConsignee(null));
			AssertNoExceptionThrown(() => new IMHeaderConsignee(new Mock<ITrader>().Object));
		});
	}
}
