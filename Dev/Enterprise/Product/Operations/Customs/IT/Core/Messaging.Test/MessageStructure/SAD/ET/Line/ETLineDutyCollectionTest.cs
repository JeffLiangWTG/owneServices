using System;
using System.Collections.Generic;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETLineDutyCollectionTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETLineDutyCollection(null));
			AssertNoExceptionThrown(() => new ETLineDutyCollection(new Mock<IEnumerable<IDutyTaxFee>>().Object));
		});
	}
}
