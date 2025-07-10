using System;
using System.Collections.Generic;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETLineCertificateCollectionTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETLineCertificateCollection(null));
			AssertNoExceptionThrown(() => new ETLineCertificateCollection(new Mock<IEnumerable<ICertificate>>().Object));
		});
	}
}
