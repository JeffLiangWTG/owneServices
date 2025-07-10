using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMLineCertificateTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMLineCertificate(null));
			AssertNoExceptionThrown(() => new IMLineCertificate(new Mock<ICertificate>().Object));
		});
	}
}
