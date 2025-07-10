using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.IN;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(NonStandardExchangeRate))]
sealed class NonStandardExchangeRateTest : CusSupportingInfoTest<NonStandardExchangeRate>
{
	public void TestCSI_RX_NKCurrency()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(NonStandardExchangeRate.CSI_RX_NKCurrencyInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Currency", resData.Caption);
			Assert(NonStandardExchangeRate.CSI_RX_NKCurrencyInfo.ReadOnly);
		});
	}

	public void TestCSI_Description()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(NonStandardExchangeRate.CSI_DescriptionInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Bank Name", resData.Caption);
			AssertEquals("Max Length", NonStandardExchangeRate.Schema.CSI_DescriptionMaxLength, NonStandardExchangeRate.CSI_DescriptionInfo.MaxLength);
		});
	}

	public void TestCSI_EffectiveDate()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(NonStandardExchangeRate.CSI_EffectiveDateInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Effective Date", resData.Caption);
		});
	}

	public void TestCSI_ReferenceNumber()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(NonStandardExchangeRate.CSI_ReferenceNumberInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Certificate Number", resData.Caption);
			AssertEquals("MaxLength", NonStandardExchangeRate.Schema.CSI_ReferenceNumberMaxLength, NonStandardExchangeRate.CSI_ReferenceNumberInfo.MaxLength);
		});
	}

	public void TestCSI_DateOfIssue()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(NonStandardExchangeRate.CSI_DateOfIssueInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Certificate Date", resData.Caption);
		});
	}

	public void TestCSI_Quantity()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(NonStandardExchangeRate.CSI_QuantityInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Unit in \u20b9", resData.Caption);
			AssertEquals("DecimalPlaces", 2, NonStandardExchangeRate.CSI_QuantityInfo.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);
		});

		NonStandardExchangeRate.CSI_RX_NKCurrency = "MNT";
		NonStandardExchangeRate.CSI_Value = 12;

		NonStandardExchangeRate.CSI_Quantity = 0;
		AssertEquals(0m, NonStandardExchangeRate.CSI_Value);
	}

	public void TestCSI_Value()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(NonStandardExchangeRate.CSI_ValueInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Exchange Rate", resData.Caption);
			AssertEquals("DecimalPlaces", 4, NonStandardExchangeRate.CSI_ValueInfo.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);
		});
	}

	public void TestCSI_Value_SetExchangeRate()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice1 = declaration.Invoices.AddNew();
		invoice1.JZ_RX_NKInvoice_Currency = "MNT";
		var invoice2 = declaration.Invoices.AddNew();
		invoice2.JZ_RX_NKInvoice_Currency = "MDD";

		var exchangeRate = declaration.NonStandardExchangeRates.GetByCurrencyCode("MNT");
		exchangeRate.CSI_Value = 1.2m;
		AssertEquals(1.2m, invoice1.JZ_InvoiceCurrExRate);
		AssertEquals(0m, invoice2.JZ_InvoiceCurrExRate);
	}

	public void TestRefreshExchangeRatePrintInfo()
	{
		NonStandardExchangeRate.CSI_RX_NKCurrency = "MNT";

		NonStandardExchangeRate.CSI_Quantity = 0;
		NonStandardExchangeRate.CSI_Value = 0;
		AssertEquals(ZString.Empty, NonStandardExchangeRate.CSI_AdditionalDescription);

		NonStandardExchangeRate.CSI_Quantity = 2;
		AssertEquals(ZString.Empty, NonStandardExchangeRate.CSI_AdditionalDescription);

		NonStandardExchangeRate.CSI_Value = 3.6;
		AssertEquals("2 MNT = 7.2 INR", NonStandardExchangeRate.CSI_AdditionalDescription);
	}

	public void TestCSI_AdditionalDescription()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(NonStandardExchangeRate.CSI_AdditionalDescriptionInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Ex. Rate Print Info.", resData.Caption);
			AssertEquals("MaxLength", 50, NonStandardExchangeRate.CSI_AdditionalDescriptionInfo.MaxLength);
		});
	}

	public void TestDefaultValues()
	{
		CombineAssertions(() =>
		{
			AssertEquals("CSI_UnitOfQuantity", Core.Constants.CurrencyCodes.India, NonStandardExchangeRate.CSI_UnitOfQuantity);
			AssertEquals("CSI_RN_NKCountryCode", Core.Constants.CountryCodes.India, NonStandardExchangeRate.CSI_RN_NKCountryCode);
			AssertEquals("CSI_Type", CusSupportingInfoTypeList.Codes.NonStandardCurrency, NonStandardExchangeRate.CSI_Type);
			AssertEquals("CSI_Quantity", 1m, NonStandardExchangeRate.CSI_Quantity);
		});
	}

	protected override IEnumerable<NonStandardExchangeRate> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		yield return GetNewBusinessObject(factory);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		return GetNewBusinessObject(factory);
	}

	protected override BusinessObject GetBusinessObjectForFetchForLoad()
	{
		return GetNewBusinessObject();
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return GetNewBusinessObject(Factory);
	}

	NonStandardExchangeRate GetNewBusinessObject(BusinessObjectFactory factory)
	{
		return factory.NewWithValidTestData<JobDeclaration>().NonStandardExchangeRates.AddNew();
	}

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;

	NonStandardExchangeRate NonStandardExchangeRate => nonStandardExchangeRate ??= Declaration.NonStandardExchangeRates.AddNew();
	NonStandardExchangeRate nonStandardExchangeRate;
}
