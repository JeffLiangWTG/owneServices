using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMHeaderWarehouseIdentificationTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMHeaderWarehouseIdentification(null));
			AssertNoExceptionThrown(() => new IMHeaderWarehouseIdentification(new Mock<IWarehouseIdentification>().Object));
		});
	}
}
