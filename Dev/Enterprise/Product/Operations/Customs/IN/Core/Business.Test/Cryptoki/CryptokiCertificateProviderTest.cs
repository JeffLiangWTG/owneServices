using System.IO;
using CargoWise.Application;
using CargoWise.Cryptoki.Common.ClientServerApi;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(CryptokiCertificateProvider))]
sealed class CryptokiCertificateProviderTest : TestCaseWithFactory
{
	public void TestGetCertificateList()
	{
		var cryptoApiMock = new Mock<ICryptoApi>();
		cryptoApiMock
			.Setup(x => x.GetCertificatesFromTokenWithLibrary(It.IsAny<string>()))
			.Returns(new[] { ReadSampleCertificate() });

		ObjectFactory.Substitute(cryptoApiMock.Object);

		ICryptokiCertificateProvider certificateProvider = new CryptokiCertificateProvider();
		var cerificates = certificateProvider.GetCertificateList("Sample.dll");
		AssertEquals("Count", 1, cerificates.Count);
		AssertEquals("Serial number", "02B9572B9CAD7250A906B3", cerificates[0].SerialNumber);
	}

	CertificateInfo ReadSampleCertificate()
	{
		var thisType = GetType();
		var fullResourcePath = thisType.Namespace + ".Cryptoki.sample-certificate.crt";

		using (var stream = thisType.Assembly.GetManifestResourceStream(fullResourcePath))
		{
			var mem = new MemoryStream();
			stream.CopyTo(mem);
			return new CertificateInfo { Content = mem.ToArray() };
		}
	}
}
