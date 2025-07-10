using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.China
{
	class ChinaEInvoicingEligibilityDecider : IEInvoicingEligibilityDecider
	{
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer logs should be in English")]
		string IEInvoicingEligibilityDecider.GetAdditionalTraceLog(IEInvoicingEligibilityLiteTransaction transaction)
		{
			var companyPK = transaction.CompanyPK.ToGuid();
			var branchPK = transaction.BranchPK.ToGuid();
			var excludedInvoiceLines = AccountingConfigurationRegistry.Instance.DoNotQueueInvoicesContainingSpecificChargesForTransmission
				.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty)
				.Cast<GenericChargeConfiguration>()
				.ToArray();

			return @$"Is Cancelled: {transaction.IsCancelled}
Invoice Amount: {transaction.InvoiceAmount}
Compliance Sub Type ({transaction.ComplianceSubType}) is valid: {transaction.HasEligibleComplianceSubType()}
ChinaEInvoicingCredentials Registry: {AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty)}

Number of Transaction Lines: {transaction.Lines.Count}
Registry Excluded Charges: {excludedInvoiceLines.Length} ({AccountingConfigurationRegistry.Instance.DoNotQueueInvoicesContainingSpecificChargesForTransmission.Caption})
Has Excluded Transaction Lines from Registry in Invoice: {transaction.Lines.Any(x => excludedInvoiceLines.Any(y => y.ChargePK == x.ChargePK))}";
		}

		bool IEInvoicingEligibilityDecider.IsTransactionEligible(IEInvoicingEligibilityLiteTransaction transaction)
			=> transaction.Ledger == LedgerTypes.AccountsReceivable
				&& transaction.TransactionType == TransactionTypes.Invoice
				&& !transaction.IsCancelled
				&& !transaction.InvoiceAmount.IsEmpty
				&& transaction.HasEligibleComplianceSubType()
				&& !string.IsNullOrEmpty(AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.GetFallBackValueAtAllLevels(transaction.CompanyPK.ToGuid(), transaction.BranchPK.ToGuid(), Guid.Empty))
				&& !transaction.Lines.Any(x => AccountingConfigurationRegistry.Instance.DoNotQueueInvoicesContainingSpecificChargesForTransmission.GetFallBackValueAtAllLevels(transaction.CompanyPK.ToGuid(), transaction.BranchPK.ToGuid(), Guid.Empty).Cast<GenericChargeConfiguration>().Any(y => y.ChargePK == x.ChargePK));
	}
}
