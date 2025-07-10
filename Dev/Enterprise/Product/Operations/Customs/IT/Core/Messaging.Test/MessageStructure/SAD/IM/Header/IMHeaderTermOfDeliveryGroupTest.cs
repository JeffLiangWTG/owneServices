using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMHeaderTermOfDeliveryGroupTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMHeaderTermOfDeliveryGroup(null));
			AssertNoExceptionThrown(() => new IMHeaderTermOfDeliveryGroup(new Mock<ITermOfDeliveryGroup>().Object));
		});
	}
}
