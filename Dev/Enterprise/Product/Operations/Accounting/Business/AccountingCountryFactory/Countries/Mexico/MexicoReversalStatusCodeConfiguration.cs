using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Mexico
{
	internal class MexicoReversalStatusCodeConfiguration : IReversalStatusCodeConfiguration
	{
		ZBool IReversalStatusCodeConfiguration.GetIsReversalStatusCodeAllowed(ZString ledger) => ledger == LedgerTypes.AccountsReceivable && AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value;

		ReadOnlyCodeDescriptionPairList IReversalStatusCodeConfiguration.GetReversalStatusCodeLookup() => AccountingConfigurationRegistry.Instance.MexicoEInvoicingReversalStatusCode.Value;

		ZString IReversalStatusCodeConfiguration.GetReversalStatusCodeReferenceType() => AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.MXR;
	}
}
