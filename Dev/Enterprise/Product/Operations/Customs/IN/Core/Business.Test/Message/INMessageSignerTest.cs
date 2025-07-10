using System;
using CargoWise.Application;
using CargoWise.Cryptoki.Common.ClientServerApi;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(INMessageSigner))]
sealed class INMessageSignerTest : TestCaseWithFactory
{
	public void TestSign_NullMessage()
	{
		AssertExceptionThrown<ArgumentNullException>(() => signer.Sign(null));
	}

	public void TestSign_EmptyMessage()
	{
		AssertExceptionThrown<ArgumentException>(() => signer.Sign(string.Empty));
	}

	public void TestSign_NullCertificate()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new INMessageSigner(null));
	}

	public void TestSign_NullPinStore()
	{
		var certificateMock = new Mock<ICryptokiDetails>();
		AssertExceptionThrown<ArgumentNullException>(() => new INMessageSigner(certificateMock.Object));
	}

	public void TestSign_ValidMessageAndCertificateDetails()
	{
		using (Globals.TemporaryOverrideForIsTest(false))
		{
			var cryptoApiMock = new Mock<ICryptoApi>();
			cryptoApiMock
				.Setup(x =>
					x.SignIcegateMessageWithToken(
						It.IsAny<byte[]>(),
						It.IsAny<string>(),
						It.IsAny<string>(),
						It.IsAny<string>()));

			ObjectFactory.Substitute(cryptoApiMock.Object);

			AssertNoExceptionThrown(() => signer.Sign("Test"));

			cryptoApiMock
				.Verify(x =>
					x.SignIcegateMessageWithToken(
						It.IsAny<byte[]>(),
						It.IsAny<string>(),
						It.IsAny<string>(),
						It.IsAny<string>()), Times.Once());
		}
	}

	public void TestSign_ResetTokenPinOnSignException()
	{
		using (Globals.TemporaryOverrideForIsTest(false))
		{
			var cryptoApiMock = new Mock<ICryptoApi>();
			cryptoApiMock
				.Setup(x =>
					x.SignIcegateMessageWithToken(
						It.IsAny<byte[]>(),
						It.IsAny<string>(),
						It.IsAny<string>(),
						It.IsAny<string>()))
				.Throws(new Exception());

			ObjectFactory.Substitute(cryptoApiMock.Object);

			AssertExceptionThrown<Exception>(() => signer.Sign("Test"));
			tokenPinStoreMock.Verify(x => x.ResetPin());
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		tokenPinStoreMock = new Mock<ITokenPinStore>();
		tokenPinStoreMock.Setup(x => x.GetPin());

		var certificateMock = new Mock<ICryptokiDetails>();
		certificateMock.Setup(x => x.CertificateSerialNumber).Returns("123");
		certificateMock.Setup(x => x.LibraryName).Returns("sample.dll");
		certificateMock.Setup(x => x.TokenPinStore).Returns(tokenPinStoreMock.Object);
		signer = new INMessageSigner(certificateMock.Object);
	}

	ITextSigner signer;
	Mock<ITokenPinStore> tokenPinStoreMock;
}
