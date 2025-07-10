using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETHeaderMeansOfTransportCrossingBorderTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETHeaderMeansOfTransportCrossingBorder(null));
			AssertNoExceptionThrown(() => new ETHeaderMeansOfTransportCrossingBorder(new Mock<IETHeaderMeansOfTransportCrossingBorder>().Object));
		});
	}
}
