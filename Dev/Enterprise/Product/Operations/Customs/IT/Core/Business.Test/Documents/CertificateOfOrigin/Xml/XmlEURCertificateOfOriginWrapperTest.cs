using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;
using Moq;
using IXmlEURCertificateOfOrigin = CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin.IEURCertificateOfOrigin;
using IXmlEURGoodsSummary = CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin.IEURGoodsSummary;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class XmlEURCertificateOfOriginWrapperTest : XmlCertificateOfOriginWrapperTest<IXmlEURCertificateOfOrigin, XmlEURCertificateOfOriginWrapper>
{
	public void TestGoodsSummary()
	{
		xmlEURCertificateOfOriginMock.Setup(x => x.GoodsSummary).Returns(new List<IXmlEURGoodsSummary>());
		var eurCertificateOfOriginWrapper = (IEURCertificateOfOrigin)GetNewWrapper(Factory, xmlEURCertificateOfOriginMock.Object);
		AssertType<XmlEURGoodsSummaryWrapper>("GoodsSummary Type", eurCertificateOfOriginWrapper.GoodsSummary);
	}

	public void TestOriginCountry()
	{
		xmlEURCertificateOfOriginMock.Setup(x => x.OriginCountry).Returns("AU");
		var eurCertificateOfOriginWrapper = (IEURCertificateOfOrigin)GetNewWrapper(Factory, xmlEURCertificateOfOriginMock.Object);
		AssertEquals("OriginCountry", "Australia", eurCertificateOfOriginWrapper.OriginCountry);
	}

	public void TestOriginGroup()
	{
		xmlEURCertificateOfOriginMock.Setup(x => x.OriginGroup).Returns(new string[] { "" });
		var eurCertificateOfOriginWrapper = (IEURCertificateOfOrigin)GetNewWrapper(Factory, xmlEURCertificateOfOriginMock.Object);
		AssertEquals("OriginGroup", "", eurCertificateOfOriginWrapper.OriginGroup);

		xmlEURCertificateOfOriginMock.Setup(x => x.OriginGroup).Returns((IEnumerable<string>)null);
		eurCertificateOfOriginWrapper = GetNewWrapper(Factory, xmlEURCertificateOfOriginMock.Object);
		AssertEquals("OriginGroup", "", eurCertificateOfOriginWrapper.OriginGroup);

		xmlEURCertificateOfOriginMock.Setup(x => x.OriginGroup).Returns(new string[] { "UE" });
		eurCertificateOfOriginWrapper = GetNewWrapper(Factory, xmlEURCertificateOfOriginMock.Object);
		AssertEquals("OriginGroup", "Europe", eurCertificateOfOriginWrapper.OriginGroup);

		xmlEURCertificateOfOriginMock.Setup(x => x.OriginGroup).Returns(new string[] { "US" });
		eurCertificateOfOriginWrapper = GetNewWrapper(Factory, xmlEURCertificateOfOriginMock.Object);
		AssertEquals("OriginGroup", "United States", eurCertificateOfOriginWrapper.OriginGroup);

		xmlEURCertificateOfOriginMock.Setup(x => x.OriginGroup).Returns(new string[] { "UE", "US", "IT", "DE" });
		eurCertificateOfOriginWrapper = GetNewWrapper(Factory, xmlEURCertificateOfOriginMock.Object);
		AssertEquals("OriginGroup", "Europe, United States, Italy, Germany", eurCertificateOfOriginWrapper.OriginGroup);
	}

	public void TestDestinationGroup()
	{
		xmlEURCertificateOfOriginMock.Setup(x => x.DestinationGroup).Returns(new string[] { "" });
		var eurCertificateOfOriginWrapper = (IEURCertificateOfOrigin)GetNewWrapper(Factory, xmlEURCertificateOfOriginMock.Object);
		AssertEquals("DestinationGroup", "", eurCertificateOfOriginWrapper.DestinationGroup);

		xmlEURCertificateOfOriginMock.Setup(x => x.DestinationGroup).Returns((IEnumerable<string>)null);
		eurCertificateOfOriginWrapper = GetNewWrapper(Factory, xmlEURCertificateOfOriginMock.Object);
		AssertEquals("DestinationGroup", "", eurCertificateOfOriginWrapper.DestinationGroup);

		xmlEURCertificateOfOriginMock.Setup(x => x.DestinationGroup).Returns(new string[] { "UE" });
		eurCertificateOfOriginWrapper = GetNewWrapper(Factory, xmlEURCertificateOfOriginMock.Object);
		AssertEquals("DestinationGroup", "Europe", eurCertificateOfOriginWrapper.DestinationGroup);

		xmlEURCertificateOfOriginMock.Setup(x => x.DestinationGroup).Returns(new string[] { "US" });
		eurCertificateOfOriginWrapper = GetNewWrapper(Factory, xmlEURCertificateOfOriginMock.Object);
		AssertEquals("DestinationGroup", "United States", eurCertificateOfOriginWrapper.DestinationGroup);

		xmlEURCertificateOfOriginMock.Setup(x => x.DestinationGroup).Returns(new string[] { "UE", "US", "AT", "BE" });
		eurCertificateOfOriginWrapper = GetNewWrapper(Factory, xmlEURCertificateOfOriginMock.Object);
		AssertEquals("DestinationGroup", "Europe, United States, Austria, Belgium", eurCertificateOfOriginWrapper.DestinationGroup);
	}

	protected override void SetUp()
	{
		base.SetUp();
		xmlEURCertificateOfOriginMock = new Mock<IXmlEURCertificateOfOrigin>();
	}

	Mock<IXmlEURCertificateOfOrigin> xmlEURCertificateOfOriginMock;

	protected override XmlEURCertificateOfOriginWrapper GetNewWrapper(BusinessObjectFactory factory, IXmlEURCertificateOfOrigin xmlCertificateOfOrigin) => new XmlEURCertificateOfOriginWrapper(factory, xmlCertificateOfOrigin);
}
