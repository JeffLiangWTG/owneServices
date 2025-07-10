using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMHeaderLocationOfGoodsTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMHeaderLocationOfGoods(null));
			AssertNoExceptionThrown(() => new IMHeaderLocationOfGoods(new Mock<IIMHeaderLocationOfGoods>().Object));
		});
	}
}
