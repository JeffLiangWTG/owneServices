using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815WineProductTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IE815WineProduct(null));
			AssertNoExceptionThrown(() => new IE815WineProduct(new Mock<IWineProduct>().Object));
		});
	}
}
