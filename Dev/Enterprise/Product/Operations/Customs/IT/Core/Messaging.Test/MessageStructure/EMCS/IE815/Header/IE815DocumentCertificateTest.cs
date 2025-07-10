using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815DocumentCertificateTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			var documentCertificateMock = new Mock<IDocumentCertificate>().Object;
			AssertExceptionThrown<ArgumentNullException>(() => new IE815DocumentCertificate(null, 0));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => new IE815DocumentCertificate(documentCertificateMock, -1));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => new IE815DocumentCertificate(documentCertificateMock, 100));
			AssertNoExceptionThrown(() => new IE815DocumentCertificate(documentCertificateMock, 1));
		});
	}
}
