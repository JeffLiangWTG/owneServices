using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NT045ResponsePrettyFormatterTest : TestCaseWithFactory
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		var responseDetailMock = new Mock<INT045ResponseDetail>();
		AssertExceptionThrown<ArgumentNullException>("Factory null", () => new NT045ResponsePrettyFormatter(null, responseDetailMock.Object));
		AssertExceptionThrown<ArgumentNullException>("ResponseDetail null", () => new NT045ResponsePrettyFormatter(Factory, null));
	});

	public void TestGetFormattedText()
	{
		var detailMock = new Mock<INT045ResponseDetail>();
		detailMock.Setup(x => x.MRN).Returns("00CH12345678901234");
		detailMock.Setup(x => x.MRNVersion).Returns("3");
		detailMock.Setup(x => x.WriteOffDate).Returns(new DateTime(2004, 02, 14));

		var formattedText = new NT045ResponsePrettyFormatter(Factory, detailMock.Object).GetFormattedText();

		CombineAssertions(() =>
		{
			AssertContains("<h2>Transit Movement successfully closed</h2>", formattedText);
			AssertContains("<p><b>MRN:</b> 00CH12345678901234.3</p>", formattedText);
			AssertContains("<p><b>Write-off date:</b> 14.02.2004</p>", formattedText);
		});
	}
}
