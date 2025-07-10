using System;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;
using Moq;
using NUnit.Framework;
using IXmlCertificateOfOrigin = CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin.ICertificateOfOrigin;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class XmlTransportDetailWrapperTest : TestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When certificateOfOrigin is null", () => new XmlTransportDetailWrapper(certificateOfOrigin: null));
	}

	public void TestVoyage()
	{
		xmlCertificateOfOriginMock.Setup(x => x.TransportDetails).Returns("TRADET");
		var xmlTransportDetailWrapper = GetNewXmlTransportDetailWrapper();
		AssertEquals("Voyage", "TRADET", xmlTransportDetailWrapper.Voyage);
	}

	protected override void SetUp()
	{
		base.SetUp();
		xmlCertificateOfOriginMock = new Mock<IXmlCertificateOfOrigin>();
	}

	Mock<IXmlCertificateOfOrigin> xmlCertificateOfOriginMock;

	ITransportDetail GetNewXmlTransportDetailWrapper() => new XmlTransportDetailWrapper(xmlCertificateOfOriginMock.Object);
}
