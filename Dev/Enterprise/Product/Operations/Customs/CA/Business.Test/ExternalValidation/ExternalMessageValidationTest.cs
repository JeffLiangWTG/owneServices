using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ExternalMessageValidationTest : TestCaseWithFactory
	{
		public void TestExchangeWarningNotShownOnMonOrSun()
		{
			var currency = Factory.New<RefCurrency>();
			currency.RX_Code = "XXX";
			var rate = currency.ExchangeRates.AddNew();
			rate.RE_ExRateType = "CUS";
			rate.RE_StartDate = new ZDateTime(2012, 04, 21); // Saturday
			rate.RE_ExpiryDate = new ZDateTime(2012, 04, 21); // Saturday
			rate.RE_SellRate = 0.123m;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();

			foreach (var date in new[] { new ZDateTime(2012, 04, 22), new ZDateTime(2012, 04, 23) }) // Monday & Sunday
			{
				invoice.JZ_ValuationDateOverride = date;
				invoice.JZ_RX_NKInvoice_Currency = "XXX";
				Assert(!invoice.JZ_RX_NKInvoice_CurrencyInfo.HasWarnings());
			}
		}
	}
}
