using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815ADetailPartTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			var headerDetailPartMock = new Mock<IIE815MessageHeaderDetailPart>().Object;
			AssertExceptionThrown<ArgumentNullException>(() => new IE815ADetailPart(null, 0));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => new IE815ADetailPart(headerDetailPartMock, -1));
			AssertNoExceptionThrown(() => new IE815ADetailPart(headerDetailPartMock, 1));
		});
	}
}
