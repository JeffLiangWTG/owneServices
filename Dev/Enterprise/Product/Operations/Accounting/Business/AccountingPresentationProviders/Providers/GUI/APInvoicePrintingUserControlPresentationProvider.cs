using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingPresentationProviders
{
	public interface IAPInvoicePrintingUserControlPresentationProvider
	{
		bool IsTaxBranchColumnAvailable();
	}

	public class APInvoicePrintingUserControlPresentationProvider : IAPInvoicePrintingUserControlPresentationProvider
	{
		public bool IsTaxBranchColumnAvailable()
		{
			return AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.Value;
		}
	}
}
