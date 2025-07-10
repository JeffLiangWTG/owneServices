using System;
using System.Collections.Generic;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMLineDutyCollectionTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMLineDutyCollection(null));
			AssertNoExceptionThrown(() => new IMLineDutyCollection(new Mock<IEnumerable<IDutyTaxFee>>().Object));
		});
	}
}
