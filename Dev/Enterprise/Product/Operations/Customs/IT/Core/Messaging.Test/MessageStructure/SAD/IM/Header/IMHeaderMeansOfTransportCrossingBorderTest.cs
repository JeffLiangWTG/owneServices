using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMHeaderMeansOfTransportCrossingBorderTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMHeaderMeansOfTransportCrossingBorder(null));
			AssertNoExceptionThrown(() => new IMHeaderMeansOfTransportCrossingBorder(new Mock<IMeansOfTransport>().Object));
		});
	}
}
