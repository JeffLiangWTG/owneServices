using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IReversalStatusCodeConfiguration
	{
		ReadOnlyCodeDescriptionPairList GetReversalStatusCodeLookup();

		ZString GetReversalStatusCodeReferenceType();

		ZBool GetIsReversalStatusCodeAllowed(ZString ledger);
	}

	public class GlobalReversalStatusCodeConfiguration : IReversalStatusCodeConfiguration
	{
		public ZBool GetIsReversalStatusCodeAllowed(ZString ledger) => ledger == LedgerTypes.AccountsReceivable
			&& AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value
			&& AccountingMasterFilesRegistry.Instance.EInvoicingReversalCodes.Value.Count > 0;

		public ReadOnlyCodeDescriptionPairList GetReversalStatusCodeLookup() => AccountingMasterFilesRegistry.Instance.EInvoicingReversalCodes.Value;

		public ZString GetReversalStatusCodeReferenceType() => AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.EINV_REVERSAL_CODE;
	}
}
