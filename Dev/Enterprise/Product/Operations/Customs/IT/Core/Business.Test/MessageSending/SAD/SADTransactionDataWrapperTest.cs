using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SADTransactionDataWrapperTest : TestCaseWithFactory
{
	public void TestCurrencyCode()
	{
		AssertEquals(ZString.Empty, transactionDataWrapper.CurrencyCode);
		cusEntryHeader.InvoiceAmountCurrency = "EUR";
		AssertEquals("EUR", transactionDataWrapper.CurrencyCode);
	}

	public void TestTotalAmountInvoiced()
	{
		cusEntryHeader.InvoiceAmount = 31321.21;
		AssertEquals(31321.21m, transactionDataWrapper.TotalAmountInvoiced);
		cusEntryHeader.InvoiceAmount = 0;
		AssertEquals(0m, transactionDataWrapper.TotalAmountInvoiced);
	}

	public void TestNatureOfTransactionCode()
	{
		AssertEquals(ZString.Empty, transactionDataWrapper.NatureOfTransactionCode);
		invoice.JZ_ValuationCode = "11";
		AssertEquals("11", transactionDataWrapper.NatureOfTransactionCode);
	}

	public void TestExchangeRate()
	{
		cusEntryHeader.InvoiceAmountCurrency = "XXX";
		AssertEquals(ZDecimal.Zero, transactionDataWrapper.ExchangeRate);
		invoice.JZ_InvoiceCurrExRate = 1.34m;
		AssertEquals(1.34m, transactionDataWrapper.ExchangeRate);
		invoice.JZ_InvoiceCurrExRate = 0;
		AssertEquals(0m, transactionDataWrapper.ExchangeRate);

		void AssertForCurrencyRecognizedByCustoms(ZString currencyCode)
		{
			cusEntryHeader.InvoiceAmountCurrency = currencyCode;
			invoice.JZ_InvoiceCurrExRate = 1.34m;
			AssertNull(transactionDataWrapper.ExchangeRate);
		}

		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.EuropeanUnion);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.UnitedStates);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.Japan);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.CzechRepublic);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.Denmark);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.UnitedKingdom);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.Hungary);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.Poland);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.Romania);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.Sweden);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.Switzerland);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.Norway);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.Croatia);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.Russia);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.Turkey);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.Australia);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.Brazil);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.Canada);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.China);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.HongKong);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.Israel);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.India);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.NewZealand);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.SouthAfrica);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.KoreaRepublicOf);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.Mexico);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.Malaysia);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.Philippines);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.Singapore);
		AssertForCurrencyRecognizedByCustoms(CurrencyCodes.Thailand);
	}

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new SADTransactionDataWrapper(null));
		AssertNoExceptionThrown(() => new SADTransactionDataWrapper(cusEntryHeader));
	}

	protected override void SetUp()
	{
		base.SetUp();

		var jobDeclaration = Factory.New<JobDeclaration>();
		cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		invoice = cusEntryHeader.RandomHeader;
		transactionDataWrapper = new SADTransactionDataWrapper(cusEntryHeader);
	}
	CusEntryHeader cusEntryHeader;
	JobComInvoiceHeader invoice;
	SADTransactionDataWrapper transactionDataWrapper;
}
