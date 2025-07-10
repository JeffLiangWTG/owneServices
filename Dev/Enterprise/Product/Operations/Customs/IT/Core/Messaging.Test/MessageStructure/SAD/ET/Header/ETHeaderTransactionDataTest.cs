using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETHeaderTransactionDataTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETHeaderTransactionData(null));
			AssertNoExceptionThrown(() => new ETHeaderTransactionData(new Mock<ITransactionData>().Object));
		});
	}
}
