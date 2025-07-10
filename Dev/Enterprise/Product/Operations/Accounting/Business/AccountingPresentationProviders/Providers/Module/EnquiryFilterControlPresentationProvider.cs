using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingPresentationProviders
{
	public interface IEnquiryFilterControlPresentationProvider
	{
		bool IsTaxBranchColumnAvailable();
	}

	public class EnquiryFilterControlPresentationProvider : IEnquiryFilterControlPresentationProvider
	{
		public bool IsTaxBranchColumnAvailable()
		{
			return AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.Value;
		}
	}
}
