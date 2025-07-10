using System;
using Enterprise.Customs.IT.Messaging.SAD;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETHeaderSealTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentException>(() => new ETHeaderSeal(null));
			AssertNoExceptionThrown(() => new ETHeaderSeal("A"));
		});
	}
}
