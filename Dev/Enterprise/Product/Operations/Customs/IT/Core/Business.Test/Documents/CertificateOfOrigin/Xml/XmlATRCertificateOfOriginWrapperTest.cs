using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;
using Moq;
using IATRCertificateOfOrigin = Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin.IATRCertificateOfOrigin;
using IXmlATRCertificateOfOrigin = CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin.IATRCertificateOfOrigin;
using IXmlATRGoodsSummary = CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin.IGoodsSummary;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class XmlATRCertificateOfOriginWrapperTest : XmlCertificateOfOriginWrapperTest<IXmlATRCertificateOfOrigin, XmlATRCertificateOfOriginWrapper>
{
	public void TestATRBoxItemBuilder()
	{
		xmlATRCertificateOfOriginMock.Setup(x => x.GoodsSummary).Returns(new List<IXmlATRGoodsSummary>());
		var atrCertificateOfOriginWrapper = (IATRCertificateOfOrigin)GetNewWrapper(Factory, xmlATRCertificateOfOriginMock.Object);
		AssertType<XmlATRBoxItemBuilder>("GoodsSummary builder Type", atrCertificateOfOriginWrapper.ATRBoxItemBuilder);
	}

	public void TestDeclaration()
	{
		AssertType<XmlATRCertificateDeclaration>(wrapper.Declaration);
	}

	public void TestTotalATRCertificateItem()
	{
		AssertEquals("TotalATRCertificateItem", null, wrapper.TotalATRCertificateItem);
	}

	public void TestGetShouldAddTotalCertificateItem()
	{
		AssertEquals("ShouldAddTotalCertificateItem  expected false for IT", false, wrapper.ShouldAddTotalCertificateItem);
	}

	public void TestARTNumberCaption()
	{
		AssertEquals("ART Number Caption expected A.TR.No", "A.TR.No", wrapper.ARTNumberCaption);
	}

	public void TestARTEuropeanUnionCaption()
	{
		AssertEquals("ART European Union Caption expected EUROPEAN UNION", "EUROPEAN UNION", wrapper.ARTEuropeanUnionCaption);
	}

	public void TestUrlWhenFilled()
	{
		xmlATRCertificateOfOriginMock.Setup(x => x.Url).Returns("https://www.argo.com");
		AssertEquals("Url", "https://www.argo.com", wrapper.Url);
	}

	protected override void SetUp()
	{
		base.SetUp();
		xmlATRCertificateOfOriginMock = new Mock<IXmlATRCertificateOfOrigin>();
		wrapper = GetNewWrapper(Factory, xmlATRCertificateOfOriginMock.Object);
	}
	IATRCertificateOfOrigin wrapper;
	Mock<IXmlATRCertificateOfOrigin> xmlATRCertificateOfOriginMock;

	protected override XmlATRCertificateOfOriginWrapper GetNewWrapper(BusinessObjectFactory factory, IXmlATRCertificateOfOrigin xmlCertificateOfOrigin) => new XmlATRCertificateOfOriginWrapper(factory, xmlCertificateOfOrigin);
}
