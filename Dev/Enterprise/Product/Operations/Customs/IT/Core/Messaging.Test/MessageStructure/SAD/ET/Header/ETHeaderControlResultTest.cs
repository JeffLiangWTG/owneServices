using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETHeaderControlResultTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETHeaderControlResult(null));
			AssertNoExceptionThrown(() => new ETHeaderControlResult(new Mock<IETHeaderControlResult>().Object));
		});
	}
}
