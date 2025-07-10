using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;
using Moq;
using IXmlCertificateOfOrigin = CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin.ICertificateOfOrigin;
using IXmlCustomsEndorsement = CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin.ICustomsEndorsement;
using IXmlExporterDeclaration = CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin.IExporterDeclaration;
using IXmlTrader = CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin.ITrader;

namespace Enterprise.Customs.IT.Business.Testing;

abstract class XmlCertificateOfOriginWrapperTest<TXmlCertificateOfOriginSource, TXmlCertificateOfOriginDestination> : TestCaseWithFactory
	where TXmlCertificateOfOriginSource : class, IXmlCertificateOfOrigin
	where TXmlCertificateOfOriginDestination : XmlCertificateOfOriginWrapper
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When factory is null", () => GetNewWrapper(factory: null, xmlCertificateOfOriginMock.Object));
		AssertExceptionThrown<ArgumentNullException>("When certificateOfOrigin is null", () => GetNewWrapper(Factory, xmlCertificateOfOrigin: null));
	}

	public void TestDeclarationReference()
	{
		var certificateOfOriginWrapper = (ICertificateOfOrigin)GetNewWrapper(Factory, xmlCertificateOfOriginMock.Object);
		AssertEquals("DeclarationReference", "", certificateOfOriginWrapper.DeclarationReference);
	}

	public void TestRemarks()
	{
		xmlCertificateOfOriginMock.Setup(x => x.Remarks).Returns("This is it");
		var certificateOfOriginWrapper = (ICertificateOfOrigin)GetNewWrapper(Factory, xmlCertificateOfOriginMock.Object);
		AssertEquals("Remarks", "This is it", certificateOfOriginWrapper.Remarks);
	}

	public void TestReferenceDateFormat()
	{
		var certificateOfOriginWrapper = (ICertificateOfOrigin)GetNewWrapper(Factory, xmlCertificateOfOriginMock.Object);
		AssertEquals("ReferenceDateFormat", "dd/MM/yyyy", certificateOfOriginWrapper.ReferenceDateFormat);
	}

	public void TestSupplierAddress()
	{
		var certificateOfOriginWrapper = (ICertificateOfOrigin)GetNewWrapper(Factory, xmlCertificateOfOriginMock.Object);
		AssertEquals("SupplierAddress", "", certificateOfOriginWrapper.SupplierAddress);

		var xmlTraderMock = new Mock<IXmlTrader>();
		xmlTraderMock.Setup(x => x.Address).Returns("Address");
		xmlTraderMock.Setup(x => x.City).Returns("City");
		xmlTraderMock.Setup(x => x.CountryCode).Returns("CountryCode");
		xmlTraderMock.Setup(x => x.Name).Returns("Name");
		xmlTraderMock.Setup(x => x.ZipCode).Returns("ZIPCode");
		xmlCertificateOfOriginMock.Setup(x => x.Consignor).Returns(xmlTraderMock.Object);
		certificateOfOriginWrapper = GetNewWrapper(Factory, xmlCertificateOfOriginMock.Object);
		AssertEquals("SupplierAddress", "Name\r\nAddress\r\nCity\r\nZIPCode\r\nCountryCode", certificateOfOriginWrapper.SupplierAddress);
	}

	public void TestImporterAddress()
	{
		var certificateOfOriginWrapper = (ICertificateOfOrigin)GetNewWrapper(Factory, xmlCertificateOfOriginMock.Object);
		AssertEquals("ImporterAddress", "", certificateOfOriginWrapper.ImporterAddress);

		var xmlTraderMock = new Mock<IXmlTrader>();
		xmlTraderMock.Setup(x => x.Address).Returns("Address");
		xmlTraderMock.Setup(x => x.City).Returns("City");
		xmlTraderMock.Setup(x => x.CountryCode).Returns("CountryCode");
		xmlTraderMock.Setup(x => x.Name).Returns("Name");
		xmlTraderMock.Setup(x => x.ZipCode).Returns("ZIPCode");
		xmlCertificateOfOriginMock.Setup(x => x.Consignee).Returns(xmlTraderMock.Object);
		certificateOfOriginWrapper = GetNewWrapper(Factory, xmlCertificateOfOriginMock.Object);
		AssertEquals("ImporterAddress", "Name\r\nAddress\r\nCity\r\nZIPCode\r\nCountryCode", certificateOfOriginWrapper.ImporterAddress);
	}

	public void TestDestinationCountry()
	{
		var certificateOfOriginWrapper = (ICertificateOfOrigin)GetNewWrapper(Factory, xmlCertificateOfOriginMock.Object);
		AssertEquals("DestinationCountry", "", certificateOfOriginWrapper.DestinationCountry);

		xmlCertificateOfOriginMock.Setup(x => x.DestinationCountry).Returns("DE");
		certificateOfOriginWrapper = GetNewWrapper(Factory, xmlCertificateOfOriginMock.Object);
		AssertEquals("DestinationCountry", "Germany", certificateOfOriginWrapper.DestinationCountry);
	}

	public void TestTransportDetail()
	{
		var certificateOfOriginWrapper = (ICertificateOfOrigin)GetNewWrapper(Factory, xmlCertificateOfOriginMock.Object);
		AssertType<XmlTransportDetailWrapper>("TransportDetail Type", certificateOfOriginWrapper.TransportDetail);
	}

	public void TestCustomsEndorsement()
	{
		var certificateOfOriginWrapper = (ICertificateOfOrigin)GetNewWrapper(Factory, xmlCertificateOfOriginMock.Object);
		AssertType<EmptyCustomsEndorsementWrapper>("CustomsEndorsement Type", certificateOfOriginWrapper.CustomsEndorsement);

		xmlCertificateOfOriginMock.Setup(x => x.CustomsEndorsement).Returns(new Mock<IXmlCustomsEndorsement>().Object);
		certificateOfOriginWrapper = GetNewWrapper(Factory, xmlCertificateOfOriginMock.Object);
		AssertType<XmlCustomsEndorsementWrapper>("CustomsEndorsement Type", certificateOfOriginWrapper.CustomsEndorsement);
	}

	public void TestExporterDeclaration()
	{
		var certificateOfOriginWrapper = (ICertificateOfOrigin)GetNewWrapper(Factory, xmlCertificateOfOriginMock.Object);
		AssertType<EmptyExporterDeclarationWrapper>("ExporterDeclaration Type", certificateOfOriginWrapper.ExporterDeclaration);

		xmlCertificateOfOriginMock.Setup(x => x.ExporterDeclaration).Returns(new Mock<IXmlExporterDeclaration>().Object);
		certificateOfOriginWrapper = GetNewWrapper(Factory, xmlCertificateOfOriginMock.Object);
		AssertType<XmlExporterDeclarationWrapper>("ExporterDeclaration Type", certificateOfOriginWrapper.ExporterDeclaration);
	}

	public void TestUrl()
	{
		xmlCertificateOfOriginMock.Setup(x => x.Url).Returns("URL");
		var certificateOfOriginWrapper = (ICertificateOfOrigin)GetNewWrapper(Factory, xmlCertificateOfOriginMock.Object);
		AssertEquals("Url", "URL", certificateOfOriginWrapper.Url);
	}

	protected override void SetUp()
	{
		base.SetUp();
		xmlCertificateOfOriginMock = new Mock<TXmlCertificateOfOriginSource>();
	}

	Mock<TXmlCertificateOfOriginSource> xmlCertificateOfOriginMock;

	protected abstract TXmlCertificateOfOriginDestination GetNewWrapper(BusinessObjectFactory factory, TXmlCertificateOfOriginSource xmlCertificateOfOrigin);
}
