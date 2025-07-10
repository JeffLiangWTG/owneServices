using System;
using System.Collections.Generic;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETHeaderTransitCustomsOfficeCollectionTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETHeaderTransitCustomsOfficeCollection(null));
			AssertNoExceptionThrown(() => new ETHeaderTransitCustomsOfficeCollection(new Mock<IEnumerable<IETHeaderTransitCustomsOffice>>().Object));
		});
	}
}
