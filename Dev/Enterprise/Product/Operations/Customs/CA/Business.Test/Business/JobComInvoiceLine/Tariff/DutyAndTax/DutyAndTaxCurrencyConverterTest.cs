using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class DutyAndTaxCurrencyConverterTest : TestCaseWithFactory
	{
		public void TestDateOfValuation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2018, 08, 30);
			invoice.JZ_InvoiceDate = new ZDateTime(2018, 08, 31);
			ICurrencyConverterDataProviderWithFixedExRates currencyConverter = new DutyAndTaxCurrencyConverter(invoice);
			AssertEquals(new ZDateTime(2018, 08, 31), currencyConverter.DateOfValuation);
			invoice.JZ_InvoiceDate = ZDateTime.Empty;
			AssertEquals(new ZDateTime(2018, 08, 30), currencyConverter.DateOfValuation);
		}
	}
}
