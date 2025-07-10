using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingPresentationProviders
{
	public interface IJobInvoicePrintingControlPresentationProvider
	{
		bool IsEInvoicingColumnsAvailable(ZString ledgerType);

		bool IsTaxBranchColumnAvailable();
	}

	public class JobInvoicePrintingControlPresentationProvider : IJobInvoicePrintingControlPresentationProvider
	{
		public bool IsTaxBranchColumnAvailable()
		{
			return AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.Value;
		}

		bool IJobInvoicePrintingControlPresentationProvider.IsEInvoicingColumnsAvailable(ZString ledgerType)
		{
			return ObjectFactory.Get<IElectronicInvoicingAccountingObjectFactory>().GetEInvoicingConfigurationChecks().IsEInvoicingSupportedForCurrentCountry(ledgerType);
		}
	}
}
