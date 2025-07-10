using System;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class MalaysiaEInvoicingPivotStatusProvider : IEInvoicingPivotStatusProvider
	{
		public bool CanCreateNotEligibleForEInvoicingPivot(AccTransactionHeader transaction) => false;

		public string GetInitialPivotStatus(ITransactionHeader header)
		{
			var transactionHeader = header as AccTransactionHeader;
			var companyPk = transactionHeader.AH_GC.ToGuid();

			if ((transactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable && !AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.GetValueWithoutFallback(companyPk, Guid.Empty, Guid.Empty))
				|| (transactionHeader.AH_Ledger == LedgerTypes.AccountsPayable && !AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.GetValueWithoutFallback(companyPk, Guid.Empty, Guid.Empty)))
			{
				return EInvoicingPivotState.Discarded;
			}

			return string.Empty;
		}
	}
}
