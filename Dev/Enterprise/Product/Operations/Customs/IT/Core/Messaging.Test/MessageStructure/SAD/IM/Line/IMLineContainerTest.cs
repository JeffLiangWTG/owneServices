using System;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMLineContainerTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentException>(() => new IMLineContainer(null));
			AssertExceptionThrown<ArgumentException>(() => new IMLineContainer(ZString.Empty));
			AssertNoExceptionThrown(() => new IMLineContainer("A"));
		});
	}
}
