using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815TransportModeTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IE815TransportMode(null));
			AssertNoExceptionThrown(() => new IE815TransportMode(new Mock<ITransportMode>().Object));
		});
	}
}
