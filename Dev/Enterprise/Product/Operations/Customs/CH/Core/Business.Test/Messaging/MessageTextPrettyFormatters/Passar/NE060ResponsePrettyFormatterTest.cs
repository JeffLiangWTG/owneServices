using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.NCTS.Business;
using Moq;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class NE060ResponsePrettyFormatterTest : TestCaseWithFactory
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		var responseDetailMock = new Mock<INE060ResponseDetail>();
		AssertExceptionThrown<ArgumentNullException>("Factory null", () => new NE060ResponsePrettyFormatter(null, responseDetailMock.Object));
		AssertExceptionThrown<ArgumentNullException>("ResponseDetail null", () => new NE060ResponsePrettyFormatter(Factory, null));
	});

	public void TestGetFormattedText() => CombineAssertions(() =>
	{
		var detailMock = new Mock<INE060ResponseDetail>();
		detailMock.Setup(x => x.GDRN).Returns("00CH12345678901234");
		detailMock.Setup(x => x.GDRNVersion).Returns("3");
		detailMock.Setup(x => x.CustomsOfficeOfDepartureReferenceNumber).Returns("IT044104");
		detailMock.Setup(x => x.SelectionNotificationStatus).Returns("FINAL");
		detailMock.Setup(x => x.SelectionNotificationDecision).Returns("CLEAR");

		var formattedText = new NE060ResponsePrettyFormatter(Factory, detailMock.Object).GetFormattedText();

		AssertContains("<h2>Decision to Control</h2>", formattedText);
		AssertContains("<p><b>Goods Declaration Reference Number:</b> 00CH12345678901234.3</p>", formattedText);
		AssertContains($"<p><b>{CustomsOfficeLabel}:</b> IT044104</p>", formattedText);
		AssertContains("<p><b>Selection Status:</b> FINAL</p>", formattedText);
		AssertContains("<p><b>Inspection Decision:</b> CLEAR</p>", formattedText);
	});

	public void TestCustomsOfficeOfDepartureNotProvided()
	{
		var detailMock = new Mock<INE060ResponseDetail>();
		detailMock.Setup(d => d.CustomsOfficeOfDepartureReferenceNumber).Returns((string)null);

		var formattedText = new NE060ResponsePrettyFormatter(Factory, detailMock.Object).GetFormattedText();

		AssertNotContains(CustomsOfficeLabel, formattedText);
	}

	const string CustomsOfficeLabel = "Customs Office of Departure";
}
