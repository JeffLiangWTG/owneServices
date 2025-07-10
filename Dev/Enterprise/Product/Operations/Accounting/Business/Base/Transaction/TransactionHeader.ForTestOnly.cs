#if DEBUG

using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public partial class TransactionHeader
	{
		public TransactionHeader FReverseTransaction_ForTestOnly
		{
			get { return fReverseTransaction; }
			set { fReverseTransaction = value; }
		}

		public ZDecimal RoundAmountToCurrencyDecimals_ForTestOnly(ZDecimal value)
		{
			return RoundAmountToCurrencyDecimals(value);
		}

		public ZString Ledger_ForTestOnly => Ledger;

		public AccountingNumberFountainWrapper NumberFountainForTransactionNumber_ForTestOnly => NumberFountainForTransactionNumber;

		public decimal AH_OSOutstandingAmount_WithMultiplier_ForTestOnly => AH_OSOutstandingAmount * Multiplier;

		public int Multiplier_ForTestOnly => Multiplier;

		public TransactionHeader ReverseTransaction_ForTestOnly => fReverseTransaction;

		public ZString TransactionType_ForTestOnly => TransactionType;

		public AccTransactionHeaderReference TransactionHeaderReferenceIRR_ForTestOnly => TransactionHeaderReferenceIRR;

		public bool IsAddressApplicableForDocumentSending_ForTestOnly => IsAddressApplicableForDocumentSending;

		public bool InvertSigns_ForTestOnly => InvertSigns;

		public bool InvertSignsOfOriginalTransactionOnReversing_ForTestOnly => InvertSignsOfOriginalTransactionOnReversing;

		public Type TypeOfReverseTransaction_ForTestOnly => TypeOfReverseTransaction;

		public AccTransactionHeaderReference TransactionHeaderReferenceIRD_ForTestOnly => TransactionHeaderReferenceIRD;

		public bool IsInUnapprovedTransactionContext_ForTestOnly => IsInUnapprovedTransactionContext;

		public AccTransactionHeaderValidation GetNewValidation_ForTestOnly() => GetNewValidation();

		public bool InvoiceUnpaid_ForTestOnly => InvoiceUnpaid;

		public StmALog CreateLog_ForTestOnly => CreateLog;

		public ZDecimal DebitField_ForTestOnly
		{
			set
			{
				fDebit = value;
			}
		}

		public ZDecimal CreditField_ForTestOnly
		{
			set
			{
				fCredit = value;
			}
		}

		public bool AllowDelete_ForTestOnly => AllowDelete;

		public bool IsInInvoiceBatchContext_ForTestOnly => IsInInvoiceBatchContext;

		[BusinessObjectTestExclude]
		public IMatchingCollection ParentMatchingCollection_ForTestOnly => ParentMatchingCollection;

		public ZGuid PaymentApprovalPKCurrentlyBeingMatched_ForTestOnly => PaymentApprovalPKCurrentlyBeingMatched;

		[BusinessObjectTestExclude]
		public PaymentApprovalItemCollection PaymentApprovalItemsField_ForTestOnly
		{
			set
			{
				fPaymentApprovalItems = value;
			}
		}

		public ZDecimal OSOutstandingAmountMatchingField_ForTestOnly
		{
			set
			{
				fOSOutstandingAmountMatching = value;
			}
		}

		public ZDecimal OutstandingAmountMatchingField_ForTestOnly
		{
			set
			{
				fOutstandingAmountMatching = value;
			}
		}

		public List<string> WritableProperties_ForTestOnly => WritableProperties;

		public void SetObjectReadOnly_ForTestOnly() => SetObjectReadOnly();

		public List<string> GetWritableProperties_ForTestOnly() => GetWritableProperties();
	}
}

#endif
