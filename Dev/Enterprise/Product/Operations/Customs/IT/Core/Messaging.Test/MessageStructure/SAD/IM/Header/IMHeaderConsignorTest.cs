using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMHeaderConsignorTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMHeaderConsignor(null));
			AssertNoExceptionThrown(() => new IMHeaderConsignor(new Mock<ITrader>().Object));
		});
	}
}
