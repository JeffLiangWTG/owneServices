using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingPresentationProviders
{
	public interface IInvoicePrintingControlPresentationProvider
	{
		bool IsTaxBranchColumnAvailable();
	}

	public class InvoicePrintingControlPresentationProvider : IInvoicePrintingControlPresentationProvider
	{
		public bool IsTaxBranchColumnAvailable()
		{
			return AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.Value;
		}
	}
}
