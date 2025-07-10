using CargoWise.Types;
using Enterprise.Integration.Accounting;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface IExchangeRateProvider
	{
		IExchangeRateJobBilling GetExchangeRate(ZString currencyCode, ZGuid orgPk, ExchangeRateValidLedgerEnum legder, bool forceExchangeRateCreation = false, InvoiceCurrencyType invoiceCurrencyType = InvoiceCurrencyType.NotApplicable);
	}
}
