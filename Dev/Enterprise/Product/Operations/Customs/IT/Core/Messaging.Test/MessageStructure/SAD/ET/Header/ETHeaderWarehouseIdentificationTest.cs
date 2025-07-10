using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETHeaderWarehouseIdentificationTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETHeaderWarehouseIdentification(null));
			AssertNoExceptionThrown(() => new ETHeaderWarehouseIdentification(new Mock<IWarehouseIdentification>().Object));
		});
	}
}
