using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;
using Moq;
using NUnit.Framework;
using IXmlATRGoodsSummary = CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin.IGoodsSummary;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class XmlATRBoxItemBuilderTest : TestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When goodsSummaryList is null", () => new XmlATRBoxItemBuilder(goodsSummaryList: null));
	}

	public void TestItemsInfoBox9()
	{
		xmlATRGoodsSummaryMock1.Setup(x => x.ItemNumber).Returns(1);
		xmlATRGoodsSummaryMock2.Setup(x => x.ItemNumber).Returns(2);

		const string expectedItemNumbers =
			"1\r\n" +
			"2";

		var xmlATRGoodsSummaryBuilder = GetNewXmlATRGoodsSummaryBuilder();
		AssertEquals("Description", expectedItemNumbers, xmlATRGoodsSummaryBuilder.ItemsInfoBox9);
	}

	public void TestMarksNumberBox10()
	{
		xmlATRGoodsSummaryMock1.Setup(x => x.PackagesNumber).Returns(12);
		xmlATRGoodsSummaryMock1.Setup(x => x.PackageType).Returns("PK");
		xmlATRGoodsSummaryMock1.Setup(x => x.Description).Returns("DESCR 1");
		xmlATRGoodsSummaryMock2.Setup(x => x.PackagesNumber).Returns(4);
		xmlATRGoodsSummaryMock2.Setup(x => x.PackageType).Returns("BLK");
		xmlATRGoodsSummaryMock2.Setup(x => x.Description).Returns("DESCR 2");

		const string expectedDescription =
			"12; PK; DESCR 1\r\n" +
			"4; BLK; DESCR 2";

		var xmlATRGoodsSummaryBuilder = GetNewXmlATRGoodsSummaryBuilder();
		AssertEquals("Description", expectedDescription, xmlATRGoodsSummaryBuilder.MarksNumberBox10);
	}

	public void TestGrossWeightBox11()
	{
		xmlATRGoodsSummaryMock1.Setup(x => x.GrossMass).Returns(100m);
		xmlATRGoodsSummaryMock2.Setup(x => x.GrossMass).Returns(99.32m);

		const string expectedWeight =
				"100 KG\r\n" +
				"99.32 KG";

		var xmlATRGoodsSummaryBuilder = GetNewXmlATRGoodsSummaryBuilder();
		AssertEquals("Weight", expectedWeight, xmlATRGoodsSummaryBuilder.GrossWeightBox11);
	}

	protected override void SetUp()
	{
		base.SetUp();
		xmlATRGoodsSummaryMock1 = new Mock<IXmlATRGoodsSummary>();
		xmlATRGoodsSummaryMock2 = new Mock<IXmlATRGoodsSummary>();
	}

	Mock<IXmlATRGoodsSummary> xmlATRGoodsSummaryMock1;
	Mock<IXmlATRGoodsSummary> xmlATRGoodsSummaryMock2;

	IATRBoxItems GetNewXmlATRGoodsSummaryBuilder()
	{
		var builder = new XmlATRBoxItemBuilder(new List<IXmlATRGoodsSummary>() { xmlATRGoodsSummaryMock1.Object, xmlATRGoodsSummaryMock2.Object });
		((IATRBoxItems)builder).Build();
		return builder;
	}
}
