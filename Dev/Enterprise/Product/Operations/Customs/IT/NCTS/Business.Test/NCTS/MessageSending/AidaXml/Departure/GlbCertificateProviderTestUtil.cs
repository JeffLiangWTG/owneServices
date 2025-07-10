using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Moq;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class GlbCertificateProviderTestUtil
{
	internal static Mock<IGlbCertificateProvider> GetGlbCertificateProviderMock()
	{
		var mauCertificateMock = new Mock<IGlbMauExternalPassword>();
		mauCertificateMock.Setup(m => m.PK).Returns(ZGuid.NewZGuid);
		mauCertificateMock.Setup(m => m.DeclarantTaxNumber).Returns("ABCDE");

		var cryptokiCertificateMock = new Mock<ICryptokiGlbExternalPassword>();
		cryptokiCertificateMock.Setup(c => c.GP_Name).Returns("BIT4ID");
		cryptokiCertificateMock.Setup(c => c.GP_CertificateSerialNumber).Returns("SR123");
		cryptokiCertificateMock.Setup(c => c.TokenPinStore.GetPin()).Returns("PIN1");

		var glbCertificateProvider = new Mock<IGlbCertificateProvider>();
		glbCertificateProvider.Setup(c => c.GetCryptokiCertificate()).Returns(cryptokiCertificateMock.Object);
		glbCertificateProvider.Setup(c => c.GetMauCertificatePassword(It.IsAny<string>())).Returns(mauCertificateMock.Object);

		return glbCertificateProvider;
	}
}
