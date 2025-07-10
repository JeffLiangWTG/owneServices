using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETHeaderGuaranteeTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETHeaderGuarantee(null));
			AssertNoExceptionThrown(() => new ETHeaderGuarantee(new Mock<IETHeaderGuarantee>().Object));
		});
	}
}
