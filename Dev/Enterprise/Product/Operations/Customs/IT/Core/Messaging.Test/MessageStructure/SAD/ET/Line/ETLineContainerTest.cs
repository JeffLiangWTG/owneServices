using System;
using Enterprise.Customs.IT.Messaging.SAD;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETLineContainerTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentException>(() => new ETLineContainer(null));
			AssertExceptionThrown<ArgumentException>(() => new ETLineContainer(""));
			AssertNoExceptionThrown(() => new ETLineContainer("A"));
		});
	}
}
