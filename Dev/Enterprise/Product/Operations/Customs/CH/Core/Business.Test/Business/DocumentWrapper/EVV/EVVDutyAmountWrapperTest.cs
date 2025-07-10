using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EVVDutyAmountWrapper))]
public class EVVDutyAmountWrapperTest : TestCaseWithFactory
{
	public void TestDescription() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateRateCodeList(Factory, RateTypes.AdditionalTaxes, new[] { "100" }, includeTranslations: true);
		RefCusCodeTestHelper.CreateRateCodeList(Factory, RateTypes.AdditionalFees, new[] { "200" }, includeTranslations: true);

		var french = Factory.GetDocumentLanguage(SwissCustomsLanguageList.Codes.French);
		using var resCacheFR = Res.GetLanguageInstance(french).UseMockData();
		resCacheFR.PutString("FAAC5187-EF80-4ABA-92F2-56962AA805CA", "DutiesFR");

		AssertDescription(AdditionalTaxesTypes.Duty, "Duties");
		AssertDescription(AdditionalTaxesTypes.Duty, "DutiesFR", french);
		AssertDescription("100", "ADT100");
		AssertDescription("100", "ADT100FR", french);
		AssertDescription("200", "FEE200");
		AssertDescription("200", "FEE200FR", french);
		AssertDescription("888", "888");

		void AssertDescription(string code, ZString expectedDescription, string documentLanguage = SwissCustomsLanguageList.Codes.German)
		{
			var dutyAmountMock = new Mock<IEvvDutyAmount>();
			dutyAmountMock.Setup(m => m.Type).Returns(code);
			var wrapper = EVVDutyAmountWrapper.New(dutyAmountMock.Object, Factory, documentLanguage);
			AssertEquals($"code={code} documentLanguage={documentLanguage}", expectedDescription, wrapper.Description);
		}
	});

	public void TestAmount()
	{
		var dutyAmountMock = new Mock<IEvvDutyAmount>();
		dutyAmountMock.Setup(m => m.Amount).Returns(123.45m);
		var wrapper = EVVDutyAmountWrapper.New(dutyAmountMock.Object, Factory, SwissCustomsLanguageList.Codes.German);
		AssertEquals(123.45m, wrapper.Amount);
	}
}

