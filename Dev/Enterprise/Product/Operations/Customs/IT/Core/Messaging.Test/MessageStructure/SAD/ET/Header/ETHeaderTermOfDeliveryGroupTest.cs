using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETHeaderTermOfDeliveryGroupTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETHeaderTermOfDeliveryGroup(null));
			AssertNoExceptionThrown(() => new ETHeaderTermOfDeliveryGroup(new Mock<ITermOfDeliveryGroup>().Object));
		});
	}
}
