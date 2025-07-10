using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETHeaderMeansOfTransportAtDepartureTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETHeaderMeansOfTransportAtDeparture(null));
			AssertNoExceptionThrown(() => new ETHeaderMeansOfTransportAtDeparture(new Mock<IMeansOfTransport>().Object));
		});
	}
}
