using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using CargoWise.Application;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ExceptionHandlerCryptokiCertificateProviderDecoratorTest : TestCase
{
	public void TestGetCertificateList_ThrowsFriendlyException()
	{
		CombineAssertions(() =>
		{
			var exceptionHandlerProvider = GetExceptionHandlerProvider("Token");

			_ = AssertExceptionThrown<CryptographicException>(
				"GetCertificateList when drivers for BIT4ID chipset are uninstalled",
				"The system cannot find the drivers for your Certificate (bit4xpki.dll). Please install the drivers following the instructions of the manufacturer.",
				() => exceptionHandlerProvider.GetCertificateList("BIT4ID")
			);

			_ = AssertExceptionThrown<CryptographicException>(
				"GetCertificateList when drivers for YUBICO chipset are uninstalled",
				"The system cannot find the drivers for your Certificate (libykcs11.dll). Please install the drivers following the instructions of the manufacturer.",
				() => exceptionHandlerProvider.GetCertificateList("YUBICO")
			);

			_ = AssertExceptionThrown<CryptographicException>(
				"GetCertificateList when drivers for Unknown chipset are uninstalled",
				"Unknown chipset: 'XXX'.",
				() => exceptionHandlerProvider.GetCertificateList("XXX")
			);

			using (SubstituteCryptokiCertificateProvider(
				getCertificateProvider: () => throw new IOException("Wrapped Exception: Cannot find PKCS#11 library.")))
			{
				exceptionHandlerProvider = GetExceptionHandlerProvider("Dummy");
				_ = AssertExceptionThrown<CryptographicException>(
					"GetCertificateList when exception is wrapped by another one",
					"The system cannot find the drivers for your Certificate (bit4xpki.dll). Please install the drivers following the instructions of the manufacturer.",
					() => exceptionHandlerProvider.GetCertificateList("BIT4ID")
				);
			}

			using (SubstituteCryptokiCertificateProvider(getCertificateProvider: () => throw new CryptographicException("Mock Exception")))
			{
				exceptionHandlerProvider = GetExceptionHandlerProvider("Dummy");
				_ = AssertExceptionThrown<CryptographicException>(
					"GetCertificateList when unexpected exception",
					"Mock Exception",
					() => exceptionHandlerProvider.GetCertificateList("BIT4ID")
				);
			}
		});
	}

	public void TestReadCertificate_ThrowsFriendlyException()
	{
		var exceptionHandlerProvider = GetExceptionHandlerProvider("Token");

		CombineAssertions(() =>
		{
			_ = AssertExceptionThrown<CryptographicException>(
				"ReadCertificate when drivers for BIT4ID chipset are uninstalled",
				"The system cannot find the drivers for your Certificate (bit4xpki.dll). Please install the drivers following the instructions of the manufacturer.",
				() => exceptionHandlerProvider.ReadCertificate("BIT4ID", Array.Empty<byte>())
			);

			_ = AssertExceptionThrown<CryptographicException>(
				"ReadCertificate when drivers for YUBICO chipset are uninstalled",
				"The system cannot find the drivers for your Certificate (libykcs11.dll). Please install the drivers following the instructions of the manufacturer.",
				() => exceptionHandlerProvider.ReadCertificate("YUBICO", Array.Empty<byte>())
			);

			_ = AssertExceptionThrown<CryptographicException>(
				"ReadCertificate when drivers for Unknown chipset are uninstalled",
				"Unknown chipset: 'XXX'.",
				() => exceptionHandlerProvider.ReadCertificate("XXX", Array.Empty<byte>())
			);

			using (SubstituteCryptokiCertificateProvider(
				readCertificateProvider: () => throw new IOException("Wrapped Exception: Cannot find PKCS#11 library.")))
			{
				exceptionHandlerProvider = GetExceptionHandlerProvider("Dummy");
				_ = AssertExceptionThrown<CryptographicException>(
					"ReadCertificate when exception is wrapped by another one",
					"The system cannot find the drivers for your Certificate (bit4xpki.dll). Please install the drivers following the instructions of the manufacturer.",
					() => exceptionHandlerProvider.ReadCertificate("BIT4ID", Array.Empty<byte>())
				);
			}

			using (SubstituteCryptokiCertificateProvider(readCertificateProvider: () => throw new CryptographicException("Mock Exception")))
			{
				exceptionHandlerProvider = GetExceptionHandlerProvider("Dummy");
				_ = AssertExceptionThrown<CryptographicException>(
					"ReadCertificate when unexpected exception",
					"Mock Exception",
					() => exceptionHandlerProvider.ReadCertificate("BIT4ID", Array.Empty<byte>())
				);
			}
		});
	}

	ICryptokiCertificateProvider GetExceptionHandlerProvider(string certificateSource)
	{
		var provider = CertificateHelper.GetNewCryptokiCertificateProvider(certificateSource);
		return new ExceptionHandlerCryptokiCertificateProviderDecorator(provider);
	}

	IDisposable SubstituteCryptokiCertificateProvider(
		Func<IReadOnlyList<CryptokiCertificate>> getCertificateProvider = null,
		Func<CryptokiCertificate> readCertificateProvider = null)
	{
		var cryptokiCertificateProviderMock = new Mock<ICryptokiCertificateProvider>();
		_ = cryptokiCertificateProviderMock.Setup(m => m.GetCertificateList(It.IsAny<string>())).Returns(getCertificateProvider);
		_ = cryptokiCertificateProviderMock.Setup(m => m.ReadCertificate(It.IsAny<string>(), It.IsAny<byte[]>())).Returns(readCertificateProvider);

		var objectHandleMock = new Mock<ObjectHandle>();
		_ = objectHandleMock.Setup(m => m.GetObject()).Returns(cryptokiCertificateProviderMock.Object);

		var cryptokiCertificateProvider = new KeyObjectHandleDictionaryObject
			{
				{ "Dummy", objectHandleMock.Object }
			};

		return ObjectFactory.Substitute(nameof(ICryptokiCertificateProvider), cryptokiCertificateProvider);
	}
}
