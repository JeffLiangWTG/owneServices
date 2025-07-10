using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMHeaderCompanyRegisterTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMHeaderCompanyRegister(null));
			AssertNoExceptionThrown(() => new IMHeaderCompanyRegister(new Mock<IIMHeaderCompanyRegister>().Object));
		});
	}
}
