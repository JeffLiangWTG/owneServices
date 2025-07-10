using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Accounting.Business.EInvoicing.ElectronicInvoicingEligibilityDecider;

namespace Enterprise.Accounting.Business.EInvoicing
{
	internal class ElectronicInvoicingEligibilityDeciderForTurkey
	{
		internal ElectronicInvoicingEligibilityDeciderForTurkey(ITransactionHeaderWrapper header, Func<bool> isEligibleComplianceSubType)
		{
			Header = header;
			IsEligibleComplianceSubType = isEligibleComplianceSubType;
		}

		readonly ITransactionHeaderWrapper Header;
		readonly Func<bool> IsEligibleComplianceSubType;

		#region Turkey Eligibility Rules

		internal bool IsTransactionHeaderEligible
			=> IsTransactionHeaderEligibleARAP || IsTransactionHeaderEligiblePA;

		bool IsTransactionHeaderEligibleARAP
			=> (IsEligibleARAP || IsEligibleReturnOfARAP)
			&& Header.HeaderVATInfo.CountryCode == Constants.CountryCodes.Turkey && !string.IsNullOrEmpty(Header.HeaderVATInfo.Number)
			&& (IsEligibleComplianceSubType() || IsEligibleReversal);

		bool IsEligibleARAP
			=> !Header.AH_TransactionReference.IsEmpty
			&& (Header.AH_Ledger == LedgerTypes.AccountsReceivable || Header.AH_Ledger == LedgerTypes.AccountsPayable)
			&& ((Header.AH_TransactionType == TransactionTypes.Invoice && !Header.IsReverseTransaction) || Header.AH_TransactionType == TransactionTypes.CreditNote);

		bool IsEligibleReturnOfARAP
			=> !Header.AH_TransactionReference.IsEmpty
			&& (Header.AH_Ledger == LedgerTypes.AccountsReceivable || Header.AH_Ledger == LedgerTypes.AccountsPayable)
			&& ((Header.AH_TransactionType == TransactionTypes.CreditNote && !Header.IsReverseTransaction) || Header.AH_TransactionType == TransactionTypes.Invoice);

		bool IsEligibleReversal
			=> (IsARReversal || IsAPReversal || IsAPReversalForReturnOfSales || IsARReversalForReturnOfPurchase)
			&& !Header.IsWritingOff;

		bool IsARReversal
			=> Header.AH_ComplianceSubType == TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN
			&& (Header.OriginalTransaction?.AH_ComplianceSubType ?? ZString.Empty) == TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR
			&& Header.HasSubmitPivotForOriginalTransaction;

		bool IsAPReversal => Header.AH_ComplianceSubType == TurkeyComplianceInfo.ComplianceSubTypeCodes.DAR || Header.AH_ComplianceSubType == TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN;

		bool IsARReversalForReturnOfPurchase => Header.AH_ComplianceSubType == TurkeyComplianceInfo.ComplianceSubTypeCodes.CCN;

		bool IsAPReversalForReturnOfSales
			=> Header.AH_ComplianceSubType == TurkeyComplianceInfo.ComplianceSubTypeCodes.DCN
			&& (Header.OriginalTransaction?.AH_ComplianceSubType ?? ZString.Empty) == TurkeyComplianceInfo.ComplianceSubTypeCodes.DAR
			&& Header.HasSubmitPivotForOriginalTransaction;

		bool IsTransactionHeaderEligiblePA
			=> !Header.AH_GovernmentAllocatedID.IsEmpty
			&& Header.AH_Ledger == LedgerTypes.TransactionsPendingAllocation
			&& Header.AH_TransactionType == TransactionTypes.InvoicePendingAllocation
			// TO DO: We should add compliance sub type conditions here after we decide about AP compliance sub types.
			;

		#endregion

	}
}
