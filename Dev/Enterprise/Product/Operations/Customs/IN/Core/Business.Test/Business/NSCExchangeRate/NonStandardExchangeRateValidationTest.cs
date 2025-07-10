using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(NonStandardExchangeRateValidation))]
sealed class NonStandardExchangeRateValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_Description()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(ExchangeRate.CSI_DescriptionInfo);
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		ValidationTestHelper.AssertErrorFieldIsNotMandatory(ExchangeRate.CSI_DescriptionInfo);
	}

	public void TestCheckCSI_EffectiveDate()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(ExchangeRate.CSI_EffectiveDateInfo);
	}

	public void TestCheckCSI_ReferenceNumber()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(ExchangeRate.CSI_ReferenceNumberInfo);
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		ValidationTestHelper.AssertErrorFieldIsNotMandatory(ExchangeRate.CSI_ReferenceNumberInfo);
	}

	public void TestCheckCSI_DateOfIssue()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(ExchangeRate.CSI_DateOfIssueInfo);
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		ValidationTestHelper.AssertErrorFieldIsNotMandatory(ExchangeRate.CSI_DateOfIssueInfo);
	}

	public void TestCheckCSI_Quantity()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(ExchangeRate.CSI_QuantityInfo);
	}

	public void TestCheckCSI_RX_NKCurrency()
	{
		Declaration.NonStandardExchangeRates.AddNew().CSI_RX_NKCurrency = "XYZ";

		ExchangeRate.CSI_RX_NKCurrency = "XYZ";
		AssertHasError(ExchangeRate.CSI_RX_NKCurrencyInfo, "Currency XYZ is already present");
		ExchangeRate.CSI_RX_NKCurrency = "ABC";
		AssertNoError(ExchangeRate.CSI_RX_NKCurrencyInfo, "Currency XYZ is already present");
	}

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;

	NonStandardExchangeRate ExchangeRate => exchangeRate ??= Declaration.NonStandardExchangeRates.AddNew();
	NonStandardExchangeRate exchangeRate;
}
