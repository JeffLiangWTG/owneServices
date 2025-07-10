using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMHeaderDeferredPaymentTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMHeaderDeferredPayment(null));
			AssertNoExceptionThrown(() => new IMHeaderDeferredPayment(new Mock<IDeferredPayment>().Object));
		});
	}
}
