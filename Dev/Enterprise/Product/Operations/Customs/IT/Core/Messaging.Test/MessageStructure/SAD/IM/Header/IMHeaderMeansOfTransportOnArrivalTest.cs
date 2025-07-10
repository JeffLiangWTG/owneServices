using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;
namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMHeaderMeansOfTransportOnArrivalTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMHeaderMeansOfTransportOnArrival(null));
			AssertNoExceptionThrown(() => new IMHeaderMeansOfTransportOnArrival(new Mock<IMeansOfTransport>().Object));
		});
	}
}
