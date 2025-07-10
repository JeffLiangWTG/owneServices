using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public interface IEInvoicingConfigurationChecks
	{
		bool IsEInvoicingSupportedForCurrentCountry(ZString ledgerType);
	}

	class EInvoicingConfigurationChecks : IEInvoicingConfigurationChecks
	{
		bool IEInvoicingConfigurationChecks.IsEInvoicingSupportedForCurrentCountry(ZString ledgerType)
		{
			return GlbCompany.CurrentCompany.SupportsElectronicInvoicing
				&& ((ledgerType == LedgerTypes.AccountsReceivable && AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value)
				|| (ledgerType == LedgerTypes.AccountsPayable && AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.Value));
		}
	}
}
