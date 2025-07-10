using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class IsraelEInvoicingEligibilityDecider : IEInvoicingEligibilityDecider
	{
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer logs should be in English")]
		string IEInvoicingEligibilityDecider.GetAdditionalTraceLog(IEInvoicingEligibilityLiteTransaction transaction)
		{
			var linesWithTaxRate = transaction.Lines.Where(l => l.TaxRate > 0).ToArray();
			var traceLog = @$"Invoice Address Country: {transaction.InvoiceOrgAddressOverride.CountryCode}
OrgHeader Category: {transaction.OrgHeader.Category}
Transaction Lines count: {transaction.Lines.Count}
Transaction Lines with Tax Rate count: {linesWithTaxRate.Length}
Invoice Amount: {transaction.InvoiceAmount}";

			if (transaction.Ledger == LedgerTypes.AccountsReceivable)
			{
				traceLog += @$"
Transaction has some lines valid for eInvoicing: {linesWithTaxRate.Any(l => l.TaxType == AccTaxRate.Types.Rated || l.TaxType == AccTaxRate.Types.CapitalRated)}
Invoice Date restricted Amount: {IsraelInvoiceAmountBoundaryRetriever.GetAmount(transaction.InvoiceDate)}";
			}
			else if (transaction.Ledger == LedgerTypes.AccountsPayable)
			{
				traceLog += @$"
Transaction Type: {transaction.TransactionType}, Govt.ID: '{transaction.GovernmentAllocatedID}', VAT Amount is eligible: {IsVATAmountEligible(transaction)}";
			}
			return traceLog;
		}

		bool IEInvoicingEligibilityDecider.IsTransactionEligible(IEInvoicingEligibilityLiteTransaction transaction)
			=> (IsAccountReceivableEligible(transaction) || IsAccountPayableEligible(transaction))
				&& (!string.IsNullOrEmpty(transaction.InvoiceOrgAddressOverride.CountryCode)
					? transaction.InvoiceOrgAddressOverride.CountryCode == CountryCodes.Israel
					: transaction.OrgHeader.MainAddress.CountryCode == CountryCodes.Israel)
				&& transaction.OrgHeader.Category != OrgConstants.Category.NaturalPersonIndividual;

		bool IsAccountReceivableEligible(IEInvoicingEligibilityLiteTransaction transaction)
			=> transaction.Ledger == LedgerTypes.AccountsReceivable
			&& (transaction.TransactionType == TransactionTypes.Invoice
				|| transaction.TransactionType == TransactionTypes.CreditNote
				|| transaction.TransactionType == TransactionTypes.AdjustmentNote)
			&& transaction.Lines.Any(l => (l.TaxType == AccTaxRate.Types.Rated || l.TaxType == AccTaxRate.Types.CapitalRated) && l.TaxRate > 0)
			&& Math.Abs(transaction.InvoiceAmount) >= IsraelInvoiceAmountBoundaryRetriever.GetAmount(transaction.InvoiceDate);

		bool IsAccountPayableEligible(IEInvoicingEligibilityLiteTransaction transaction)
			=> transaction.Ledger == LedgerTypes.AccountsPayable
			&& (!transaction.GovernmentAllocatedID.IsEmpty
				|| ((transaction.TransactionType == TransactionTypes.Invoice
					|| (transaction.TransactionType == TransactionTypes.AdjustmentNote && (transaction.InvoiceAmount * -1) > 0))
				&& IsVATAmountEligible(transaction)));

		bool IsVATAmountEligible(IEInvoicingEligibilityLiteTransaction transaction)
		{
			var amountBoundary = IsraelInvoiceAmountBoundaryRetriever.GetAmount(transaction.InvoiceDate);
			var linesWithVat = transaction.Lines.Where(l => (l.TaxType == AccTaxRate.Types.Rated || l.TaxType == AccTaxRate.Types.CapitalRated) && l.TaxRate > 0);
			if (linesWithVat.Any())
			{
				var taxRate = linesWithVat.Select(l => l.TaxRate).FirstOrDefault();
				var totalGSTVATAmount = linesWithVat.Sum(l => l.GSTVATAmount * -1);
				var eligibilityAmount = amountBoundary * taxRate / 100;
				return totalGSTVATAmount >= eligibilityAmount;
			}
			return false;
		}
	}
}
