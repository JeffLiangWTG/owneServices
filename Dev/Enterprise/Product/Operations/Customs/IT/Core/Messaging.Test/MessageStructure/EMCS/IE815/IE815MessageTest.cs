using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815MessageTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IE815Message(null));
			AssertNoExceptionThrown(() => new IE815Message(new Mock<IIE815EMCSMessage>().Object));
		});
	}
}
