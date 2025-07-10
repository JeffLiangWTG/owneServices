using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class SpainEInvoicingEligibilityDecider : IEInvoicingEligibilityDecider
	{
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer logs should be in English")]
		string IEInvoicingEligibilityDecider.GetAdditionalTraceLog(IEInvoicingEligibilityLiteTransaction transaction)
		{
			return $"Branch Org Proxy has SII Registration (NIF Number): {transaction.BranchOrgProxy.HasAnyRegistrationCode(SpainOrgCusCodeInfo.OrgCusCodes.SII, CountryCodes.Spain)}";
		}

		bool IEInvoicingEligibilityDecider.IsTransactionEligible(IEInvoicingEligibilityLiteTransaction transaction)
			=> (transaction.Ledger == LedgerTypes.AccountsReceivable
				|| transaction.Ledger == LedgerTypes.AccountsPayable)
			&& (transaction.TransactionType == TransactionTypes.Invoice
				|| transaction.TransactionType == TransactionTypes.CreditNote
				|| transaction.TransactionType == TransactionTypes.AdjustmentNote)
			&& transaction.BranchOrgProxy.HasAnyRegistrationCode(SpainOrgCusCodeInfo.OrgCusCodes.SII, CountryCodes.Spain)
			&& transaction.Lines.Any()
			&& !transaction.Lines.All(l => l.LineAmount == 0m);
	}
}
