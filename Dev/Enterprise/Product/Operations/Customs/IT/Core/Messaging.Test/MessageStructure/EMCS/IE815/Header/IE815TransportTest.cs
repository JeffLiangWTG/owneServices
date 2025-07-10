using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815TransportTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IE815TransportDetailContainer(null));
			AssertNoExceptionThrown(() => new IE815TransportDetailContainer(new Mock<ITransportDetailContainer>().Object));
		});
	}
}
