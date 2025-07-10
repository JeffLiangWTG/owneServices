using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework.Testing;
using Moq;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NT140ResponsePrettyFormatterTest : TestCaseWithFactory
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		var responseDetailMock = new Mock<INT140ResponseDetail>();
		AssertExceptionThrown<ArgumentNullException>("Factory null", () => new NT140ResponsePrettyFormatter(null, responseDetailMock.Object));
		AssertExceptionThrown<ArgumentNullException>("ResponseDetail null", () => new NT140ResponsePrettyFormatter(Factory, null));
	});

	public void TestGetFormattedText() => CombineAssertions(() =>
	{
		var testHelper = new RefDataTestHelper(Factory);
		testHelper.CreateCodeList(RefCusCodeListTypes.Codes.CustomsOffice).CreateCode("CH001471").WithDescription("Highway Border");
		Factory.Save();

		var responseDetailMock = new Mock<INT140ResponseDetail>();
		responseDetailMock.Setup(x => x.MRN).Returns("21CH16360164625756");
		responseDetailMock.Setup(x => x.MRNVersion).Returns("1");
		responseDetailMock.Setup(x => x.RequestOnNonArrivedMovementDate).Returns(new DateTime(2021, 11, 19));
		responseDetailMock.Setup(x => x.LimitForResponseDate).Returns(new DateTime(2021, 11, 30));
		responseDetailMock.Setup(x => x.CustomsOfficeOfDepartureReferenceNumber).Returns("CH001471");
		responseDetailMock.Setup(x => x.CustomsOfficeOfEnquiryAtDepartureReferenceNumber).Returns("CH001471");

		var formatter = new NT140ResponsePrettyFormatter(Factory, responseDetailMock.Object);
		var formattedText = formatter.GetFormattedText();

		AssertContains("<p><b>MRN:</b> 21CH16360164625756.1</p>", formattedText);
		AssertContains("<p><b>Enquiry Date:</b> 19 - 11 - 2021</p>", formattedText);
		AssertContains("<p><b>Limit for Response Date:</b> 30 - 11 - 2021</p>", formattedText);
		AssertContains("<p><b>Customs Office of Departure:</b> CH001471 - Highway Border</p>", formattedText);
		AssertContains("<p><b>Customs Office of Enquiry at Departure:</b> CH001471 - Highway Border</p>", formattedText);
	});
}
