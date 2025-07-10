using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETHeaderTransitCustomsOfficeTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETHeaderTransitCustomsOffice(null));
			AssertNoExceptionThrown(() => new ETHeaderTransitCustomsOffice(new Mock<IETHeaderTransitCustomsOffice>().Object));
		});
	}
}
