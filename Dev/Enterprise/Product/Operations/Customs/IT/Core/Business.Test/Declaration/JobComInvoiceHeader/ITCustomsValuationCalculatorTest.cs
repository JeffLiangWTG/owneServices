using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class ITCustomsValuationCalculatorTest : EuCustomsValuationCalculatorTest
{
	public void TestExtraEUFreightCharges()
	{
		AssertEquals("No charges", 0m, calculator.GetExtraEUFreightChargesAmount());

		SetUpCharges();
		AssertEquals("ExtraEUFreightCharges", 650m, calculator.GetExtraEUFreightChargesAmount());
	}

	public void TestEUFreightCharges()
	{
		AssertEquals("No charges", 0m, calculator.GetEUFreightChargesAmount());

		SetUpCharges();
		AssertEquals("EUFreightCharges", 2150m, calculator.GetEUFreightChargesAmount());
	}

	public void TestDomesticFreightCharges()
	{
		AssertEquals("No charges", 0m, calculator.GetDomesticFreightChargesAmount());

		SetUpCharges();
		AssertEquals("DomesticFreightCharges", 3950m, calculator.GetDomesticFreightChargesAmount());
	}

	protected override EU.Business.Declaration.JobDeclaration GetNewJobDeclaration() => Factory.New<JobDeclaration>();

	protected override void SetUp()
	{
		base.SetUp();
		SetUpExchangeRate();
		invoiceLine = Factory.New<JobComInvoiceLine>();
		calculator = new ITCustomsValuationCalculator(invoiceLine);
	}
	JobComInvoiceLine invoiceLine;
	ITCustomsValuationCalculator calculator;
	void SetUpExchangeRate()
	{
		var usd = RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCodes.UnitedStates);
		usd.ExchangeRates.DeleteAll();

		var thisMonthUsdRate = usd.ExchangeRates.AddNew();
		thisMonthUsdRate.RE_ExRateType = "CUS";
		thisMonthUsdRate.RE_StartDate = ZDateTime.MinSmallDateTimeValue;
		thisMonthUsdRate.RE_ExpiryDate = ZDateTime.MaxSmallDateTimeValue;
		thisMonthUsdRate.RE_SellRate = 0.5;
		Factory.Save();
	}

	void SetUpCharges()
	{
		//Extra EU Freight Charges
		invoiceLine.Charges.Add(CreateInvoiceLineCharge<InvoiceLineCharge>(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, 50m, CurrencyCodes.EuropeanUnion, true, true, true));
		invoiceLine.Charges.Add(CreateInvoiceLineCharge<InvoiceLineCharge>(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 100m, CurrencyCodes.UnitedStates, true, true, true));
		invoiceLine.Charges.Add(CreateInvoiceLineCharge<InvoiceLineCharge>(UCCCustomsChargeTypeList.Codes.Additions71Charge, 150m, CurrencyCodes.EuropeanUnion, true, true, true));

		invoiceLine.ApportionedCharges.Add(CreateInvoiceLineCharge<InvoiceLineApportionCharge>(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, 100m, CurrencyCodes.EuropeanUnion, true, true, true));
		invoiceLine.ApportionedCharges.Add(CreateInvoiceLineCharge<InvoiceLineApportionCharge>(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 150m, CurrencyCodes.UnitedStates, true, true, true));
		invoiceLine.ApportionedCharges.Add(CreateInvoiceLineCharge<InvoiceLineApportionCharge>(UCCCustomsChargeTypeList.Codes.Additions71Charge, 200m, CurrencyCodes.EuropeanUnion, true, true, true));

		//EU Freight Charges
		invoiceLine.Charges.Add(CreateInvoiceLineCharge<InvoiceLineCharge>(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, 250m, CurrencyCodes.EuropeanUnion, false, true, true));
		invoiceLine.Charges.Add(CreateInvoiceLineCharge<InvoiceLineCharge>(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 300m, CurrencyCodes.UnitedStates, false, true, true));
		invoiceLine.Charges.Add(CreateInvoiceLineCharge<InvoiceLineCharge>(UCCCustomsChargeTypeList.Codes.Additions71Charge, 350m, CurrencyCodes.EuropeanUnion, false, true, true));

		invoiceLine.ApportionedCharges.Add(CreateInvoiceLineCharge<InvoiceLineApportionCharge>(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, 400m, CurrencyCodes.EuropeanUnion, false, true, true));
		invoiceLine.ApportionedCharges.Add(CreateInvoiceLineCharge<InvoiceLineApportionCharge>(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 450m, CurrencyCodes.UnitedStates, false, true, true));
		invoiceLine.ApportionedCharges.Add(CreateInvoiceLineCharge<InvoiceLineApportionCharge>(UCCCustomsChargeTypeList.Codes.Additions71Charge, 500m, CurrencyCodes.EuropeanUnion, false, true, true));

		//Domestic Freight Charges
		invoiceLine.Charges.Add(CreateInvoiceLineCharge<InvoiceLineCharge>(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, 550m, CurrencyCodes.EuropeanUnion, false, false, true));
		invoiceLine.Charges.Add(CreateInvoiceLineCharge<InvoiceLineCharge>(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 600m, CurrencyCodes.UnitedStates, false, false, true));
		invoiceLine.Charges.Add(CreateInvoiceLineCharge<InvoiceLineCharge>(UCCCustomsChargeTypeList.Codes.Additions71Charge, 650m, CurrencyCodes.EuropeanUnion, false, false, true));

		invoiceLine.ApportionedCharges.Add(CreateInvoiceLineCharge<InvoiceLineApportionCharge>(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, 700m, CurrencyCodes.EuropeanUnion, false, false, true));
		invoiceLine.ApportionedCharges.Add(CreateInvoiceLineCharge<InvoiceLineApportionCharge>(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 750m, CurrencyCodes.UnitedStates, false, false, true));
		invoiceLine.ApportionedCharges.Add(CreateInvoiceLineCharge<InvoiceLineApportionCharge>(UCCCustomsChargeTypeList.Codes.Additions71Charge, 800m, CurrencyCodes.EuropeanUnion, false, false, true));
	}

	T CreateInvoiceLineCharge<T>(ZString type, ZDecimal amount, ZString currency, ZBool isDutiable, ZBool isStatisticalValueApplicable, ZBool isGstApplicable)
		where T : JobComInvCharge
	{
		var invoiceLineCharge = Factory.New<T>();
		invoiceLineCharge.J7_ChargeType = type;
		invoiceLineCharge.J7_IsDutiable = isDutiable;
		invoiceLineCharge.J7_IsStatisticalValueApplicable = isStatisticalValueApplicable;
		invoiceLineCharge.J7_IsGSTApplicable = isGstApplicable;
		invoiceLineCharge.J7_Amount = amount;
		invoiceLineCharge.J7_RX_NKCurrency = currency;
		return invoiceLineCharge;
	}
}
