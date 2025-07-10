using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NT029ResponsePrettyFormatterTest : TestCaseWithFactory
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		var responseDetailMock = new Mock<INT029ResponseDetail>();
		AssertExceptionThrown<ArgumentNullException>("Factory null", () => new NT029ResponsePrettyFormatter(null, responseDetailMock.Object));
		AssertExceptionThrown<ArgumentNullException>("ResponseDetail null", () => new NT029ResponsePrettyFormatter(Factory, null));
	});

	public void TestGetFormattedText()
	{
		var detailMock = new Mock<INT029ResponseDetail>();
		detailMock.Setup(x => x.MRN).Returns("00CH12345678901234");
		detailMock.Setup(x => x.MRNVersion).Returns("3");

		var formattedText = new NT029ResponsePrettyFormatter(Factory, detailMock.Object).GetFormattedText();

		AssertContains("<h2>Transit Released</h2>", formattedText);
		AssertContains("<p><b>MRN:</b> 00CH12345678901234.3</p>", formattedText);
	}
}
