using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EVVGoodsItemDutyOrTaxWrapper))]
sealed class EVVGoodsItemDutyOrTaxWrapperTest : TestCaseWithFactory
{
	public void TestNew() => CombineAssertions(() =>
	{
		var dutyOrTaxMock = new Mock<IEvvGoodsItemDutyOrTax>();
		AssertExceptionThrown<ArgumentNullException>("Null message", () => EVVGoodsItemDutyOrTaxWrapper.New(null, Factory, SwissCustomsLanguageList.Codes.German));
		AssertExceptionThrown<ArgumentNullException>("Null factory", () => EVVGoodsItemDutyOrTaxWrapper.New(dutyOrTaxMock.Object, null, SwissCustomsLanguageList.Codes.German));
	});

	public void TestProperties() => CombineAssertions(() =>
	{
		var dutyOrTaxMock = new Mock<IEvvGoodsItemDutyOrTax>();
		dutyOrTaxMock.Setup(m => m.BasisForAssessment).Returns("basis");
		dutyOrTaxMock.Setup(m => m.AlcoholLevel).Returns(12.5m);
		dutyOrTaxMock.Setup(m => m.Rate).Returns("rate");
		dutyOrTaxMock.Setup(m => m.Amount).Returns(34.56m);
		dutyOrTaxMock.Setup(m => m.RoundedAmount).Returns(false);

		var wrapper = EVVGoodsItemDutyOrTaxWrapper.New(dutyOrTaxMock.Object, Factory, SwissCustomsLanguageList.Codes.German);

		AssertEquals("BasisForAssessment", "basis", wrapper.BasisForAssessment);
		AssertEquals("AlcoholLevel", 12.5m, wrapper.AlcoholLevel);
		AssertEquals("Rate", "rate", wrapper.Rate);
		AssertEquals("Amount", 34.56m, wrapper.Amount);
		AssertEquals("RefundAmount", 34.56m, wrapper.RefundAmount);
	});

	public void TestDescription() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateRateCodeList(Factory, RateTypes.AdditionalTaxes, new[] { "221" }, includeTranslations: true);
		RefCusCodeTestHelper.CreateRateCodeList(Factory, RateTypes.AdditionalFees, new[] { "321" }, includeTranslations: true);

		using var resCacheFR = Res.GetLanguageInstance(Factory.GetDocumentLanguage(SwissCustomsLanguageList.Codes.French)).UseMockData();
		resCacheFR.PutString("FAAC5187-EF80-4ABA-92F2-56962AA805CA", "DutiesFR");

		var dutyOrTaxMock = new Mock<IEvvGoodsItemDutyOrTax>();

		var language = "EN";
		AssertDescription("110", "Duties");
		AssertDescription("221", "ADT221");
		AssertDescription("321", "FEE321");
		AssertDescription("900", "900");

		language = SwissCustomsLanguageList.Codes.French;
		AssertDescription("110", "DutiesFR");
		AssertDescription("221", "ADT221FR");
		AssertDescription("321", "FEE321FR");
		AssertDescription("900", "900");

		void AssertDescription(string type, string expectedDescription)
		{
			dutyOrTaxMock.Setup(m => m.Type).Returns(type);
			var wrapper = EVVGoodsItemDutyOrTaxWrapper.New(dutyOrTaxMock.Object, Factory, Factory.GetDocumentLanguage(language));
			AssertEquals($"Type={type} Language={language}", expectedDescription, wrapper.Description);
		}
	});

	public void TestAlcoholLevel()
	{
		var dutyOrTaxMock = new Mock<IEvvGoodsItemDutyOrTax>();
		dutyOrTaxMock.Setup(m => m.AlcoholLevel).Returns((decimal?)null);
		var wrapper = EVVGoodsItemDutyOrTaxWrapper.New(dutyOrTaxMock.Object, Factory, SwissCustomsLanguageList.Codes.German);
		AssertEquals("when null", 0m, wrapper.AlcoholLevel);
	}
}
