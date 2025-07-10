using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;

namespace Enterprise.Accounting.Business.AccountingPresentationProviders
{
	public interface ITransactionFilterStripControlPresentationProvider
	{
		bool IsEInvoicingColumnsAvailable(ZString ledgerType);

		bool IsRelatedDisbursementTransactionFilterAvailable();
	}

	public class TransactionFilterStripControlPresentationProvider : ITransactionFilterStripControlPresentationProvider
	{
		bool ITransactionFilterStripControlPresentationProvider.IsEInvoicingColumnsAvailable(ZString ledgerType)
		{
			return ObjectFactory.Get<IElectronicInvoicingAccountingObjectFactory>().GetEInvoicingConfigurationChecks().IsEInvoicingSupportedForCurrentCountry(ledgerType);
		}

		bool ITransactionFilterStripControlPresentationProvider.IsRelatedDisbursementTransactionFilterAvailable()
		{
			return AccountingUtils.ShouldShowRelatedDisbursementTransactions();
		}
	}
}
