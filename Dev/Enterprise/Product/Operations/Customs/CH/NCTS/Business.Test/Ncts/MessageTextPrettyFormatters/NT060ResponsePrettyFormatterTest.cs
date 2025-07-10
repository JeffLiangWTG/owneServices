using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business.Testing;
using Moq;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NT060ResponsePrettyFormatterTest : TestCaseWithFactory
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		var responseDetailMock = new Mock<INT060ResponseDetail>();
		AssertExceptionThrown<ArgumentNullException>("Factory null", () => new NT060ResponsePrettyFormatter(null, responseDetailMock.Object));
		AssertExceptionThrown<ArgumentNullException>("ResponseDetail null", () => new NT060ResponsePrettyFormatter(Factory, null));
	});

	public void TestGetFormattedText() => CombineAssertions(() =>
	{
		var testHelper = new RefDataTestHelper(Factory);
		testHelper.CreateCodeList(RefCusCodeListTypes.Codes.CustomsOffice).CreateCode("CH0001").WithDescription("Highway Border");
		Factory.Save();

		var detailMock = new Mock<INT060ResponseDetail>();
		detailMock.Setup(x => x.MRN).Returns("00CH12345678901234");
		detailMock.Setup(x => x.MRNVersion).Returns("3");
		detailMock.Setup(x => x.CustomsOfficeOfDepartureReferenceNumber).Returns("CH0001");
		detailMock.Setup(x => x.SelectionStatus).Returns(PassarMessagingConstants.SelectionStatusCodes.Preliminary);
		detailMock.Setup(x => x.SelectionInspectionDecision).Returns(PassarMessagingConstants.InspectionDecisionCodes.Intervention);

		var formattedText = new NT060ResponsePrettyFormatter(Factory, detailMock.Object).GetFormattedText();

		AssertContains("<h2>Intention to Control</h2>", formattedText);
		AssertContains("<p><b>MRN:</b> 00CH12345678901234.3</p>", formattedText);
		AssertContains($"<p><b>{CustomsOfficeLabel}:</b> CH0001 - Highway Border</p>", formattedText);
		AssertContains("<p><b>Selection Status:</b> PRELIMINARY</p>", formattedText);
		AssertContains("<p><b>Inspection Decision:</b> INTERVENTION</p>", formattedText);
	});

	public void TestCustomsOfficeOfDepartureNotProvided()
	{
		var detailMock = new Mock<INT060ResponseDetail>();
		detailMock.Setup(d => d.CustomsOfficeOfDepartureReferenceNumber).Returns((string)null);

		var formattedText = new NT060ResponsePrettyFormatter(Factory, detailMock.Object).GetFormattedText();

		AssertNotContains(CustomsOfficeLabel, formattedText);
	}

	public void TestCustomsOfficeOfDepartureUnkown()
	{
		var detailMock = new Mock<INT060ResponseDetail>();
		detailMock.Setup(d => d.CustomsOfficeOfDepartureReferenceNumber).Returns("CH9999");

		var formattedText = new NT060ResponsePrettyFormatter(Factory, detailMock.Object).GetFormattedText();

		AssertContains($"<p><b>{CustomsOfficeLabel}:</b> CH9999 - </p>", formattedText);
	}

	const string CustomsOfficeLabel = "Customs Office of Departure";
}
