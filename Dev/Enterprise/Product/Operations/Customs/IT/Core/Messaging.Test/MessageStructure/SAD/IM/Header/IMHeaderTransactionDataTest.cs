using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMHeaderTransactionDataTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMHeaderTransactionData(null));
			AssertNoExceptionThrown(() => new IMHeaderTransactionData(new Mock<ITransactionData>().Object));
		});
	}
}
