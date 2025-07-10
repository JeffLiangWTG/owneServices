using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework.Testing;
using Moq;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NT055ResponsePrettyFormatterTest : TestCaseWithFactory
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		var responseDetailMock = new Mock<INT055ResponseDetail>();
		AssertExceptionThrown<ArgumentNullException>("Factory null", () => new NT055ResponsePrettyFormatter(null, responseDetailMock.Object));
		AssertExceptionThrown<ArgumentNullException>("ResponseDetail null", () => new NT055ResponsePrettyFormatter(Factory, null));
	});

	public void TestGetFormattedText()
	{
		var testHelper = new RefDataTestHelper(Factory);
		testHelper.CreateCodeList(RefCusCodeList.PassarTypes.N0252).CreateCode("R1").WithDescription("reason 1");
		testHelper.CreateCodeList(RefCusCodeList.PassarTypes.N0252).CreateCode("R2").WithDescription("reason 2");
		Factory.Save();

		var guarantee1Mock = new Mock<INT055GuaranteeReference>();
		guarantee1Mock.Setup(x => x.GRN).Returns("GRN1");
		guarantee1Mock.Setup(x => x.InvalidGuaranteeReasonCode).Returns("R1");
		var guarantee2Mock = new Mock<INT055GuaranteeReference>();
		guarantee2Mock.Setup(x => x.GRN).Returns("GRN2");
		guarantee2Mock.Setup(x => x.InvalidGuaranteeReasonCode).Returns("R2");
		var guarantee3Mock = new Mock<INT055GuaranteeReference>();
		guarantee3Mock.Setup(x => x.GRN).Returns("GRN3");
		guarantee3Mock.Setup(x => x.InvalidGuaranteeReasonCode).Returns("R?");
		var detailMock = new Mock<INT055ResponseDetail>();
		detailMock.Setup(x => x.MRN).Returns("00CH12345678901234");
		detailMock.Setup(x => x.MRNVersion).Returns("3");
		detailMock.Setup(x => x.GuaranteeReferences).Returns(new[] { guarantee1Mock.Object, guarantee2Mock.Object, guarantee3Mock.Object });

		var formattedText = new NT055ResponsePrettyFormatter(Factory, detailMock.Object).GetFormattedText();

		AssertContains("<h2>Invalid Guarantee(s)</h2>", formattedText);
		AssertContains("<p><b>MRN:</b> 00CH12345678901234.3</p>", formattedText);
		AssertContains("<p><b>Guarantee Reference Number:</b> GRN1</p>", formattedText);
		AssertContains("<p><b>Reason:</b> R1 - reason 1</p>", formattedText);
		AssertContains("<p><b>Guarantee Reference Number:</b> GRN2</p>", formattedText);
		AssertContains("<p><b>Reason:</b> R2 - reason 2</p>", formattedText);
		AssertContains("<p><b>Guarantee Reference Number:</b> GRN3</p>", formattedText);
		AssertContains("<p><b>Reason:</b> R? - </p>", formattedText);
	}
}
