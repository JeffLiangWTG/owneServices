using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework.Testing;
using Moq;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NT061ResponsePrettyFormatterTest : TestCaseWithFactory
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		var responseDetailMock = new Mock<INT061ResponseDetail>();
		AssertExceptionThrown<ArgumentNullException>("Factory null", () => new NT061ResponsePrettyFormatter(null, responseDetailMock.Object));
		AssertExceptionThrown<ArgumentNullException>("ResponseDetail null", () => new NT061ResponsePrettyFormatter(Factory, null));
	});

	public void TestGetFormattedText() => CombineAssertions(() =>
	{
		var testHelper = new RefDataTestHelper(Factory);
		testHelper.CreateCodeList(RefCusCodeListTypes.Codes.CustomsOffice).CreateCode("CH000001").WithDescription("Highway Border");
		Factory.Save();

		var responseDetailMock = new Mock<INT061ResponseDetail>();
		responseDetailMock.Setup(x => x.ArrivalOperationReferenceNumber).Returns("00CH12345678901234");
		responseDetailMock.Setup(x => x.CustomsOfficeOfDestinationActualReferenceNumber).Returns("CH000001");
		responseDetailMock.Setup(x => x.SelectionNotificationStatus).Returns("CLEAR");
		responseDetailMock.Setup(x => x.SelectionInspectionDecision).Returns("INTERVENTION");

		var formatter = new NT061ResponsePrettyFormatter(Factory, responseDetailMock.Object);
		var formattedText = formatter.GetFormattedText();

		AssertContains("<p><b>Customs Arrival Reference Number:</b> 00CH12345678901234</p>", formattedText);
		AssertContains($"<p><b>{CustomsOfficeLabel}:</b> CH000001 - Highway Border</p>", formattedText);
		AssertContains("<p><b>Selection Status:</b> CLEAR</p>", formattedText);
		AssertContains("<p><b>Inspection Decision:</b> INTERVENTION</p>", formattedText);
	});

	public void TestUnkownCustomsOffice()
	{
		var testHelper = new RefDataTestHelper(Factory);
		testHelper.CreateCodeList(RefCusCodeListTypes.Codes.CustomsOffice).CreateCode("CH000001").WithDescription("Highway Border");
		Factory.Save();

		var responseDetailMock = new Mock<INT061ResponseDetail>();
		responseDetailMock.Setup(x => x.CustomsOfficeOfDestinationActualReferenceNumber).Returns("CH999999");

		var formatter = new NT061ResponsePrettyFormatter(Factory, responseDetailMock.Object);
		var formattedText = formatter.GetFormattedText();

		AssertContains($"<p><b>{CustomsOfficeLabel}:</b> CH999999 - </p>", formattedText);
	}

	public void TestNoCustomsOffice()
	{
		var responseDetailMock = new Mock<INT061ResponseDetail>();
		responseDetailMock.Setup(x => x.CustomsOfficeOfDestinationActualReferenceNumber).Returns<string>(null);

		var formatter = new NT061ResponsePrettyFormatter(Factory, responseDetailMock.Object);
		var formattedText = formatter.GetFormattedText();

		AssertNotContains(CustomsOfficeLabel, formattedText);
	}

	const string CustomsOfficeLabel = "Customs Office of Destination";
}
