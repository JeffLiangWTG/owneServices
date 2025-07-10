using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815FixedPartTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IE815FixedPart(null));
			AssertNoExceptionThrown(() => new IE815FixedPart(new Mock<IIE815Attributes>().Object));
		});
	}
}
