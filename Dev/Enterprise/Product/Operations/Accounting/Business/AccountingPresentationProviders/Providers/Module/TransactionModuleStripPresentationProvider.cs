using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;

namespace Enterprise.Accounting.Business.AccountingPresentationProviders
{
	public interface ITransactionModuleStripPresentationProvider
	{
		bool IsEInvoicingColumnsAvailable(ZString ledgerType);
	}

	public class TransactionModuleStripPresentationProvider : ITransactionModuleStripPresentationProvider
	{
		bool ITransactionModuleStripPresentationProvider.IsEInvoicingColumnsAvailable(ZString ledgerType)
		{
			return ObjectFactory.Get<IElectronicInvoicingAccountingObjectFactory>().GetEInvoicingConfigurationChecks().IsEInvoicingSupportedForCurrentCountry(ledgerType);
		}
	}
}
