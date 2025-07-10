using System.Diagnostics.CodeAnalysis;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class ColombiaEInvoicingEligibilityDecider : IEInvoicingEligibilityDecider
	{
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer logs should be in English")]
		string IEInvoicingEligibilityDecider.GetAdditionalTraceLog(IEInvoicingEligibilityLiteTransaction transaction)
		{
			return $"Compliance Sub Type ({transaction.ComplianceSubType}) is valid: {transaction.HasEligibleComplianceSubType()}";
		}

		bool IEInvoicingEligibilityDecider.IsTransactionEligible(IEInvoicingEligibilityLiteTransaction transaction)
			=> transaction.Ledger == LedgerTypes.AccountsReceivable
				&& (transaction.TransactionType == TransactionTypes.Invoice || transaction.TransactionType == TransactionTypes.CreditNote)
				&& transaction.HasEligibleComplianceSubType();
	}
}
