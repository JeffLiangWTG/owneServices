using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETLineDutyTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETLineDuty(null));
			AssertNoExceptionThrown(() => new ETLineDuty(new Mock<IDutyTaxFee>().Object));
		});
	}
}
