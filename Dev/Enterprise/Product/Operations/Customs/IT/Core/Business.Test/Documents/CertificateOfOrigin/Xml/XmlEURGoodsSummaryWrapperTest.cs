using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;
using Moq;
using NUnit.Framework;
using IXmlEURGoodsSummary = CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin.IEURGoodsSummary;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class XmlEURGoodsSummaryWrapperTest : TestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When goodsSummaryList is null", () => new XmlEURGoodsSummaryWrapper(goodsSummaryList: null));
	}

	public void TestInvoiceNumbers()
	{
		xmlEURGoodsSummaryMock1.Setup(x => x.InvoiceNumbers).Returns(new string[] { "INV1", "INV2", "INV3" });
		xmlEURGoodsSummaryMock2.Setup(x => x.InvoiceNumbers).Returns(new string[] { "INV4" });

		const string expectedInvoiceNumbers =
			"INV1\r\n" +
			"INV2\r\n" +
			"INV3\r\n" +
			"INV4";
		var xmlEURGoodsSummaryWrapper = GetNewXmlEURGoodsSummaryWrapper();
		AssertEquals("InvoiceNumbers", expectedInvoiceNumbers, xmlEURGoodsSummaryWrapper.InvoiceNumbers);
	}

	public void TestDescription()
	{
		xmlEURGoodsSummaryMock1.Setup(x => x.ItemNumber).Returns(1);
		xmlEURGoodsSummaryMock1.Setup(x => x.PackagesNumber).Returns(12);
		xmlEURGoodsSummaryMock1.Setup(x => x.PackageType).Returns("PK");
		xmlEURGoodsSummaryMock1.Setup(x => x.Description).Returns("DESCR 1");
		xmlEURGoodsSummaryMock2.Setup(x => x.ItemNumber).Returns(2);
		xmlEURGoodsSummaryMock2.Setup(x => x.PackagesNumber).Returns(4);
		xmlEURGoodsSummaryMock2.Setup(x => x.PackageType).Returns("BLK");
		xmlEURGoodsSummaryMock2.Setup(x => x.Description).Returns("DESCR 2");

		const string expectedDescription =
			"1; 12; PK; DESCR 1\r\n" +
			"2; 4; BLK; DESCR 2\r\n" +
			"-------------------------------------------------------------------------------------------------------";
		var xmlEURGoodsSummaryWrapper = GetNewXmlEURGoodsSummaryWrapper();
		AssertEquals("Description", expectedDescription, xmlEURGoodsSummaryWrapper.Description);
	}

	public void TestWeightAndVolume()
	{
		xmlEURGoodsSummaryMock1.Setup(x => x.GrossMass).Returns(100m);
		xmlEURGoodsSummaryMock2.Setup(x => x.GrossMass).Returns(99.32m);

		const string expectedWeightAndVolume =
				"100 KG\r\n" +
				"99.32 KG";
		var xmlEURGoodsSummaryWrapper = GetNewXmlEURGoodsSummaryWrapper();
		AssertEquals("WeightAndVolume", expectedWeightAndVolume, xmlEURGoodsSummaryWrapper.WeightAndVolume);
	}

	protected override void SetUp()
	{
		base.SetUp();
		xmlEURGoodsSummaryMock1 = new Mock<IXmlEURGoodsSummary>();
		xmlEURGoodsSummaryMock2 = new Mock<IXmlEURGoodsSummary>();
	}

	Mock<IXmlEURGoodsSummary> xmlEURGoodsSummaryMock1;
	Mock<IXmlEURGoodsSummary> xmlEURGoodsSummaryMock2;

	IEURGoodsSummary GetNewXmlEURGoodsSummaryWrapper() => new XmlEURGoodsSummaryWrapper(new List<IXmlEURGoodsSummary>() { xmlEURGoodsSummaryMock1.Object, xmlEURGoodsSummaryMock2.Object });
}
