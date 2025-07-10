using System;
using System.Collections.Generic;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMLineCertificateCollectionTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMLineCertificateCollection(null));
			AssertNoExceptionThrown(() => new IMLineCertificateCollection(new Mock<IEnumerable<ICertificate>>().Object));
		});
	}
}
