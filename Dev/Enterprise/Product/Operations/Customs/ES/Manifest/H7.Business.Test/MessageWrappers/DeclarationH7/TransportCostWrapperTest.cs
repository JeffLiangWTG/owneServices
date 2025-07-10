using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing;

public class TransportCostWrapperTest : DataProviderTestCase<TransportCostWrapper>
{
	public void TestTransportCost_WhileInsuranceAndTransportAreUnderSameCurrencyCode()
	{
		bill.ABL_RX_NKInsuranceValueCurrency = Constants.CurrencyCodes.UnitedStates;
		bill.ABL_RX_NKTransportValueCurrency = Constants.CurrencyCodes.UnitedStates;
		bill.ABL_InsuranceValue = 10;
		bill.ABL_TransportValue = 100;

		CombineAssertions("When insurance and transport are both under the same currency code, the mapped value is simply their sum", () =>
		{
			AssertEquals((ZDecimal)110, wrapper.Amount);
			AssertEquals(Constants.CurrencyCodes.UnitedStates, wrapper.CurrencyCode);
		});
	}

	public void TestAmount_RoundsToTwoDecimalPlaces()
	{
		bill.ABL_InsuranceValue = 10.2834;
		bill.ABL_TransportValue = 100.9018;

		CombineAssertions("Amount is rounded to 2 decimal places", () =>
		{
			AssertEquals((ZDecimal)111.19, wrapper.Amount);
		});
	}

	public void TestTransportCost_WhileInsuranceAndTransportAreUnderDifferentCurrencyCode()
	{
		bill.ABL_RX_NKInsuranceValueCurrency = Constants.CurrencyCodes.UnitedStates;
		bill.ABL_RX_NKTransportValueCurrency = Constants.CurrencyCodes.Australia;
		bill.ABL_InsuranceValue = 10;
		bill.ABL_TransportValue = 100;

		var insuranceValue = bill.ABL_InsuranceValue;
		var currencyConverter = bill.Header.CurrencyConverter;
		var insuranceCurrency = new Currency(bill.ABL_RX_NKInsuranceValueCurrency);
		var transportCurrency = new Currency(bill.ABL_RX_NKTransportValueCurrency);
		var insuranceMoney = new Money(insuranceValue, insuranceCurrency);
		var convertedInsuranceMoney = currencyConverter.ConvertRounded(insuranceMoney, transportCurrency);
		insuranceValue = convertedInsuranceMoney.Amount;

		CombineAssertions("When insurance and transport are under different currency code, insurance value need to be converted to the transport currency and then sum them up", () =>
		{
			AssertEquals(insuranceValue + bill.ABL_TransportValue, wrapper.Amount);
			AssertEquals(Constants.CurrencyCodes.Australia, wrapper.CurrencyCode);
		});
	}

	public void TestCurrencyCode()
	{
		bill.ABL_RX_NKTransportValueCurrency = Constants.CurrencyCodes.Australia;
		AssertEquals(Constants.CurrencyCodes.Australia, wrapper.CurrencyCode);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var broker = Factory.NewWithValidTestData<GlbStaff>();
		var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		header.AMA_GS_NKCustomsAgent = broker.GS_Code;
		bill = header.Bills.AddNew();

		wrapper = new TransportCostWrapper(bill);
	}

	protected override TransportCostWrapper GetProvider()
	{
		return wrapper;
	}

	TransportCostWrapper wrapper;
	AsycudaBill bill;
}
