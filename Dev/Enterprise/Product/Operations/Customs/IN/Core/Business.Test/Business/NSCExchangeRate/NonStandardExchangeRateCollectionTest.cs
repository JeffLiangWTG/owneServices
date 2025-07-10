using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(NonStandardExchangeRateCollection))]
sealed class NonStandardExchangeRateCollectionTest : CusSupportingInfoCollectionTest<NonStandardExchangeRate>
{
	public void TestSyncWithInvoiceCurrencies()
	{
		var standardCurrencyCode = RefDataSetupTestHelper.SetupStandardCurrencyCodes(Factory);
		var nonStandardCurrencyCode1 = "MNT";
		var nonStandardCurrencyCode2 = "MDD";

		var declaration = Factory.New<JobDeclaration>();
		var invoice1 = declaration.Invoices.AddNew();
		var invoice2 = declaration.Invoices.AddNew();
		invoice1.JZ_RX_NKInvoice_Currency = nonStandardCurrencyCode1;

		var collection = declaration.NonStandardExchangeRates;
		AssertContainsExactElementsInAnyOrder([nonStandardCurrencyCode1], collection.Select(x => x.CSI_RX_NKCurrency));
		invoice2.JZ_RX_NKInvoice_Currency = nonStandardCurrencyCode1;
		AssertContainsExactElementsInAnyOrder([nonStandardCurrencyCode1], collection.Select(x => x.CSI_RX_NKCurrency));
		invoice2.JZ_RX_NKInvoice_Currency = nonStandardCurrencyCode2;
		AssertContainsExactElementsInAnyOrder([nonStandardCurrencyCode1, nonStandardCurrencyCode2], collection.Select(x => x.CSI_RX_NKCurrency));
		invoice2.JZ_RX_NKInvoice_Currency = standardCurrencyCode;
		AssertContainsExactElementsInAnyOrder([nonStandardCurrencyCode1], collection.Select(x => x.CSI_RX_NKCurrency));

		var invoice3 = Factory.New<JobComInvoiceHeader>();
		invoice3.JZ_RX_NKInvoice_Currency = nonStandardCurrencyCode2;
		invoice3.JZ_JE = declaration.PK;
		AssertContainsExactElementsInAnyOrder([nonStandardCurrencyCode1, nonStandardCurrencyCode2], collection.Select(x => x.CSI_RX_NKCurrency));

		invoice1.Delete();
		AssertContainsExactElementsInAnyOrder([nonStandardCurrencyCode2], collection.Select(x => x.CSI_RX_NKCurrency));
		invoice3.JZ_JE = ZGuid.Empty;
		AssertContainsExactElementsInAnyOrder([], collection.Select(x => x.CSI_RX_NKCurrency));
	}

	public  void TestGetByCurrencyCode()
	{
		var declaration = Factory.New<JobDeclaration>();
		var collection = declaration.NonStandardExchangeRates;
		var exchangeRate1 = collection.AddNew();
		exchangeRate1.CSI_RX_NKCurrency = "MNT";
		var exchangeRate2 = collection.AddNew();
		exchangeRate2.CSI_RX_NKCurrency = "MDD";
		AssertEquals(null, collection.GetByCurrencyCode("XXX"));
		AssertEquals(exchangeRate1, collection.GetByCurrencyCode("MNT"));
		AssertEquals(exchangeRate2, collection.GetByCurrencyCode("MDD"));
	}

	public void TestAllowNew()
	{
		var collection = GetCusSupportingInfoCollection();
		AssertEquals(false, collection.AllowNew);
	}

	public void TestAllowRemove()
	{
		var collection = GetCusSupportingInfoCollection();
		AssertEquals(false, collection.AllowRemove);
	}

	protected override CusSupportingInfoCollection<NonStandardExchangeRate> GetCusSupportingInfoCollection()
	{
		return new NonStandardExchangeRateCollection(Factory.New<JobDeclaration>());
	}
}
