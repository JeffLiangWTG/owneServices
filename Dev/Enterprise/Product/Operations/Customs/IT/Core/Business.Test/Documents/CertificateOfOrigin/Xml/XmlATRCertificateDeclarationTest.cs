using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;
using Moq;
using IXMLATRCertificateOfOrigin = CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin.IATRCertificateOfOrigin;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class XmlATRCertificateDeclarationTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When certificateOfOrigin is null", () => new XmlATRCertificateDeclaration(certificateOfOrigin: null, Factory));
		AssertExceptionThrown<ArgumentNullException>("When factory is null", () => new XmlATRCertificateDeclaration(xmlATRCertificateDeclarationMock.Object, factory: null));
	}

	public void TestGoodsOrigin()
	{
		xmlATRCertificateDeclarationMock.Setup(x => x.ExportCountry).Returns("UE");

		var wrapper = GetNewCertificateDeclaration();
		AssertEquals("GoodsOrigin", "UE", wrapper.GoodsOrigin);
	}

	public void TestGoodsDestination()
	{
		xmlATRCertificateDeclarationMock.Setup(x => x.DestinationCountry).Returns("Turchia");

		var wrapper = GetNewCertificateDeclaration();
		AssertEquals("GoodsDestination", "Turchia", wrapper.GoodsDestination);
	}

	public void TestEmptyTraders()
	{
		var wrapper = GetNewCertificateDeclaration();
		AssertEquals("FullFormattedExporterAddress", ZString.Empty, wrapper.FullFormattedExporterAddress);
		AssertEquals("FullFormattedImporterDocumentaryAddress", ZString.Empty, wrapper.FullFormattedImporterDocumentaryAddress);
	}

	public void TestFullFormattedExporterAddress()
	{
		xmlATRCertificateDeclarationMock.Setup(x => x.Consignor.Name).Returns("ACME Ltd.");
		xmlATRCertificateDeclarationMock.Setup(x => x.Consignor.Address).Returns("34, Long Valley RD");
		xmlATRCertificateDeclarationMock.Setup(x => x.Consignor.ZipCode).Returns("20126");
		xmlATRCertificateDeclarationMock.Setup(x => x.Consignor.City).Returns("Los Angeles");
		xmlATRCertificateDeclarationMock.Setup(x => x.Consignor.CountryCode).Returns("IT");

		const string expectedAddress =
			"ACME Ltd.\r\n" +
			"34, Long Valley RD\r\n" +
			"20126 Los Angeles\r\n" +
			"ITALY";

		var wrapper = GetNewCertificateDeclaration();
		AssertEquals("FullFormattedExporterAddress", expectedAddress, wrapper.FullFormattedExporterAddress);
	}

	public void TestFullFormattedImporterDocumentaryAddress()
	{
		xmlATRCertificateDeclarationMock.Setup(x => x.Consignee.Name).Returns("ACME Ltd.");
		xmlATRCertificateDeclarationMock.Setup(x => x.Consignee.Address).Returns("34, Long Valley RD");
		xmlATRCertificateDeclarationMock.Setup(x => x.Consignee.ZipCode).Returns("20126");
		xmlATRCertificateDeclarationMock.Setup(x => x.Consignee.City).Returns("Los Angeles");
		xmlATRCertificateDeclarationMock.Setup(x => x.Consignee.CountryCode).Returns("TR");

		const string expectedAddress =
			"ACME Ltd.\r\n" +
			"34, Long Valley RD\r\n" +
			"20126 Los Angeles\r\n" +
			"TURKEY";

		var wrapper = GetNewCertificateDeclaration();
		AssertEquals("FullFormattedImporterDocumentaryAddress", expectedAddress, wrapper.FullFormattedImporterDocumentaryAddress);
	}

	protected override void SetUp()
	{
		base.SetUp();
		xmlATRCertificateDeclarationMock = new Mock<IXMLATRCertificateOfOrigin>();
	}

	Mock<IXMLATRCertificateOfOrigin> xmlATRCertificateDeclarationMock;

	IATRCertificateDeclaration GetNewCertificateDeclaration()
	{
		return new XmlATRCertificateDeclaration(xmlATRCertificateDeclarationMock.Object, Factory);
	}
}
