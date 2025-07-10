using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class PolandEInvoicingEligibilityDecider : IEInvoicingEligibilityDecider
	{
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer logs should be in English")]
		string IEInvoicingEligibilityDecider.GetAdditionalTraceLog(IEInvoicingEligibilityLiteTransaction transaction)
		{
			return @$"Transaction is Disbursement Invoice: {transaction.IsDisbursementInvoice()} (Transaction Category: {transaction.TransactionCategory})
OrgHeader Category: {transaction.OrgHeader.Category}
Branch OrgProxy has PTU Code: {transaction.BranchOrgProxy.HasAnyRegistrationCode(OrgCusCode.PolandCodeTypes.PTU, CountryCodes.Poland)}
Transaction Lines count: {transaction.Lines.Count}
Transaction has some lines valid for eInvoicing: {!transaction.Lines.All(l => l.TaxType == AccTaxRate.Types.ExcludedFromTheTaxBase || l.TaxType == AccTaxRate.Types.NotReportable)}";
		}

		bool IEInvoicingEligibilityDecider.IsTransactionEligible(IEInvoicingEligibilityLiteTransaction transaction)
			=> transaction.Ledger == LedgerTypes.AccountsReceivable
			&& (transaction.TransactionType == TransactionTypes.Invoice || transaction.TransactionType == TransactionTypes.CreditNote || transaction.TransactionType == TransactionTypes.AdjustmentNote)
			&& !transaction.IsDisbursementInvoice()
			&& transaction.OrgHeader.Category != OrgConstants.Category.Government
			&& transaction.BranchOrgProxy.HasAnyRegistrationCode(OrgCusCode.PolandCodeTypes.PTU, CountryCodes.Poland)
			&& transaction.Lines.Any()
			&& !transaction.Lines.All(l => l.TaxType == AccTaxRate.Types.ExcludedFromTheTaxBase || l.TaxType == AccTaxRate.Types.NotReportable);
	}
}
