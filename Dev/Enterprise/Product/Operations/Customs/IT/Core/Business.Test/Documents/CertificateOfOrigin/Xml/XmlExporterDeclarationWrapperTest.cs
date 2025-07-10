using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;
using Moq;
using NUnit.Framework;
using IXmlExporterDeclaration = CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin.IExporterDeclaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class XmlExporterDeclarationWrapperTest : TestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When exporterDeclaration is null", () => new XmlExporterDeclarationWrapper(exporterDeclaration: null));
	}

	public void TestPlace()
	{
		xmlExporterDeclarationMock.Setup(x => x.ReferencePlace).Returns("PLC");
		var wrapper = GetNewXMLExporterDeclarationWrapper();
		AssertEquals("Place", "PLC", wrapper.Place);
	}

	public void TestReferenceDate()
	{
		xmlExporterDeclarationMock.Setup(x => x.ReferenceDate).Returns(new DateTime(2022, 01, 01));
		var wrapper = GetNewXMLExporterDeclarationWrapper();
		AssertEquals("ReferenceDate", new ZDate(2022, 01, 01), wrapper.ReferenceDate);
	}

	public void TestExporterDetails()
	{
		var wrapper = GetNewXMLExporterDeclarationWrapper();
		AssertEquals("ExporterDetails", "", wrapper.ExporterDetails);
	}

	protected override void SetUp()
	{
		base.SetUp();
		xmlExporterDeclarationMock = new Mock<IXmlExporterDeclaration>();
	}

	Mock<IXmlExporterDeclaration> xmlExporterDeclarationMock;

	IExporterDeclaration GetNewXMLExporterDeclarationWrapper() => new XmlExporterDeclarationWrapper(xmlExporterDeclarationMock.Object);
}
