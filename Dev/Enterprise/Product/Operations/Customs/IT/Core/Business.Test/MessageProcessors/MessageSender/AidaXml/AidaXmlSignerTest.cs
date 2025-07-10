using System;
using CargoWise.Application;
using CargoWise.Cryptoki.Common.ClientServerApi;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class AidaXmlSignerTest : TestCase
{
	public void TestTokenPinIsInvalidated_WhenSigningProcessFailsWithNonCriticalException()
	{
		var aidaXmlSigner = (IAidaXmlSigner)new AidaXmlSigner();
		var passwordMock = SetUpPasswordMock();

		var cryptoApiMock = new Mock<ICryptoApi>();
		cryptoApiMock
			.Setup(x =>
				x.SignXadesWithToken(
					It.IsAny<byte[]>(),
					It.IsAny<Chipset>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<DateTime>())
				)
			.Throws(new ExceptionForTest("Crypto Exception") { IsCriticalException = false });

		ObjectFactory.Substitute(cryptoApiMock.Object);
		AssertExceptionThrown<AidaXmlSignerException>("When SignXadesWithToken throws exception",
			"Crypto Exception",
			() => aidaXmlSigner.Sign(new byte[] { 0, 1, 2 }, passwordMock.Object, ZDateTime.Now.ToDateTime()));

		passwordMock.Verify(x => x.TokenPinStore.ResetPin(), Times.Once());
	}

	public void TestTokenPinIsNotInvalidated_WhenSigningProcessFailsWithCriticalException()
	{
		var aidaXmlSigner = (IAidaXmlSigner)new AidaXmlSigner();
		var passwordMock = SetUpPasswordMock();

		var cryptoApiMock = new Mock<ICryptoApi>();
		cryptoApiMock
			.Setup(x =>
				x.SignXadesWithToken(
					It.IsAny<byte[]>(),
					It.IsAny<Chipset>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<DateTime>())
				)
			.Throws(new ExceptionForTest() { IsCriticalException = true });

		ObjectFactory.Substitute(cryptoApiMock.Object);
		AssertExceptionThrown<Exception>(() => aidaXmlSigner.Sign(new byte[] { 0, 1, 2 }, passwordMock.Object, ZDateTime.Now.ToDateTime()));
		passwordMock.Verify(x => x.TokenPinStore.ResetPin(), Times.Never());
	}

	public void TestTokenPinIsNotInvalidated_WhenSigningProcessCompleteSuccessfully()
	{
		var aidaXmlSigner = (IAidaXmlSigner)new AidaXmlSigner();
		var passwordMock = SetUpPasswordMock();

		var cryptoApiMock = new Mock<ICryptoApi>();
		cryptoApiMock
			.Setup(x =>
				x.SignXadesWithToken(
					It.IsAny<byte[]>(),
					It.IsAny<Chipset>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<DateTime>())
				)
			.Returns(Array.Empty<byte>());

		ObjectFactory.Substitute(cryptoApiMock.Object);
		AssertNoExceptionThrown(() => aidaXmlSigner.Sign(new byte[] { 0, 1, 2 }, passwordMock.Object, ZDateTime.Now.ToDateTime()));
		passwordMock.Verify(x => x.TokenPinStore.ResetPin(), Times.Never());
	}

	public void TestSign_ThrowsExceptionIfXmlBytesIsNullOrEmpty()
	{
		var aidaXmlSigner = (IAidaXmlSigner)new AidaXmlSigner();

#if NET
		var expectedMessageForNull = "XML message is null. Cannot proceed with signing. (Parameter 'xmlBytes')";
		var expectedMessageForEmpty = "XML message is empty. Cannot proceed with signing. (Parameter 'xmlBytes')";
#else
		var expectedMessageForNull = "XML message is null. Cannot proceed with signing.\r\nParameter name: xmlBytes";
		var expectedMessageForEmpty = "XML message is empty. Cannot proceed with signing.\r\nParameter name: xmlBytes";
#endif

		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Attempt to sign null XML object", expectedMessageForNull, () => aidaXmlSigner.Sign(xmlBytes: null, new Mock<ICryptokiGlbExternalPassword>().Object, DateTime.Now));
			AssertExceptionThrown<ArgumentException>("Attempt to sign empty XML", expectedMessageForEmpty, () => aidaXmlSigner.Sign(xmlBytes: Array.Empty<byte>(), new Mock<ICryptokiGlbExternalPassword>().Object, DateTime.Now));
		});
	}

	public void TestSign_ThrowsExceptionIfCryptokiPasswordOrTokenPinStoreAreNull()
	{
		var aidaXmlSigner = (IAidaXmlSigner)new AidaXmlSigner();
		AssertExceptionThrown<ArgumentNullException>(() => aidaXmlSigner.Sign(new byte[] { 0, 1, 2 }, cryptokiCertificatePassword: null, DateTime.Now));

		var passwordMock = new Mock<ICryptokiGlbExternalPassword>();
		passwordMock.Setup(x => x.TokenPinStore).Returns(value: null);
		AssertExceptionThrown<ArgumentNullException>(() => aidaXmlSigner.Sign(new byte[] { 0, 1, 2 }, passwordMock.Object, DateTime.Now));
	}

	Mock<ICryptokiGlbExternalPassword> SetUpPasswordMock()
	{
		var passwordMock = new Mock<ICryptokiGlbExternalPassword>();
		passwordMock.Setup(x => x.GP_Name).Returns("BIT4ID");
		passwordMock.Setup(x => x.GP_CertificateSerialNumber).Returns("0123");
		passwordMock.Setup(x => x.TokenPinStore.GetPin()).Returns("XYZ");
		passwordMock.Setup(x => x.TokenPinStore.ResetPin());
		return passwordMock;
	}

	[Serializable]
	class ExceptionForTest : Exception, CargoWise.Common.ICriticalException
	{
		public ExceptionForTest()
		{
		}

		public ExceptionForTest(string message) : base(message)
		{
		}

		public ExceptionForTest(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected ExceptionForTest(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif

		public bool IsCriticalException { get; set; }
	}
}
