using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETHeaderAgreedLocationOfGoodsTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETHeaderAgreedLocationOfGoods(null));
			AssertNoExceptionThrown(() => new ETHeaderAgreedLocationOfGoods(new Mock<IETHeaderAgreedLocationOfGoods>().Object));
		});
	}
}
