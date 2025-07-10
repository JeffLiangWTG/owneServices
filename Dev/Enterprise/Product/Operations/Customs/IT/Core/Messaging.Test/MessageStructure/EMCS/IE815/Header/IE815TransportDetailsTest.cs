using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815TransportDetailsTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			var transportDetailMock = new Mock<ITransportDetails>().Object;
			AssertExceptionThrown<ArgumentNullException>(() => new IE815TransportDetails(null, 1));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => new IE815TransportDetails(transportDetailMock, 0));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => new IE815TransportDetails(transportDetailMock, 100));
			AssertNoExceptionThrown(() => new IE815TransportDetails(transportDetailMock, 1));
		});
	}
}
