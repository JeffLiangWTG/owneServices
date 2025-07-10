using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public abstract class VoucherDebitCreditLookUpTestCase : NonPersistentBusinessObjectTestCase
	{
		public void TestGetOSDebit()
		{
			AccTransactionHeader aHToTest = Factory.NewWithValidTestData<AccTransactionHeader>();
			aHToTest.AH_ExchangeRate = 1m;
			aHToTest.AH_OSTotal = 0m;
			aHToTest.AH_InvoiceAmount = 11m;
			aHToTest.AH_RX_NKTransactionCurrency = aHToTest.Company.GC_RX_NKLocalCurrency;
			VoucherDebitCreditLookUp voucherDebitCreditLookUpTest = new VoucherDebitCreditLookUp(aHToTest);
			AssertEquals(11m, voucherDebitCreditLookUpTest.GetOSDebit());
			aHToTest.AH_ExchangeRate = 6.5m;
			aHToTest.AH_OSTotal = 2m;
			aHToTest.AH_InvoiceAmount = 13m;
			aHToTest.AH_RX_NKTransactionCurrency = "USD";
			voucherDebitCreditLookUpTest = new VoucherDebitCreditLookUp(aHToTest);
			AssertEquals(2m, voucherDebitCreditLookUpTest.GetOSDebit());
			aHToTest.AH_ExchangeRate = 6.5m;
			aHToTest.AH_OSTotal = 0m;
			aHToTest.AH_InvoiceAmount = 13m;
			aHToTest.AH_RX_NKTransactionCurrency = "USD";
			voucherDebitCreditLookUpTest = new VoucherDebitCreditLookUp(aHToTest);
			AssertEquals(13m, voucherDebitCreditLookUpTest.GetOSDebit());
			AssertEquals(1m, voucherDebitCreditLookUpTest.GetExchangeRate());
			AssertEquals(GlbCompany.CurrentCompany.LocalCurrency, voucherDebitCreditLookUpTest.GetCurrencyCode());
		}

		public void TestGetOSCredit()
		{
			AccTransactionHeader aHToTest = Factory.NewWithValidTestData<AccTransactionHeader>();
			aHToTest.AH_ExchangeRate = 1m;
			aHToTest.AH_OSTotal = 0m;
			aHToTest.AH_InvoiceAmount = -11m;
			aHToTest.AH_RX_NKTransactionCurrency = aHToTest.Company.GC_RX_NKLocalCurrency;
			VoucherDebitCreditLookUp voucherDebitCreditLookUpTest = new VoucherDebitCreditLookUp(aHToTest);
			AssertEquals(11m, voucherDebitCreditLookUpTest.GetOSCredit());
			aHToTest.AH_ExchangeRate = 6.5m;
			aHToTest.AH_OSTotal = -2m;
			aHToTest.AH_InvoiceAmount = -13m;
			aHToTest.AH_RX_NKTransactionCurrency = "USD";
			voucherDebitCreditLookUpTest = new VoucherDebitCreditLookUp(aHToTest);
			AssertEquals(2m, voucherDebitCreditLookUpTest.GetOSCredit());
			aHToTest.AH_ExchangeRate = 6.5m;
			aHToTest.AH_OSTotal = 0m;
			aHToTest.AH_InvoiceAmount = -13m;
			aHToTest.AH_RX_NKTransactionCurrency = "USD";
			voucherDebitCreditLookUpTest = new VoucherDebitCreditLookUp(aHToTest);
			AssertEquals(13m, voucherDebitCreditLookUpTest.GetOSCredit());
			AssertEquals(1m, voucherDebitCreditLookUpTest.GetExchangeRate());
			AssertEquals(GlbCompany.CurrentCompany.LocalCurrency, voucherDebitCreditLookUpTest.GetCurrencyCode());
		}
	}
}