using System;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business
{
	public enum NumberFountainType
	{
		None,
		ARInvoice,
		ARCreditNote,
		ARAdjustmentNote,
		ARJournal,
		ARTransfer,
		ARExchangeDifference,
		AROverpayment,
		ARDiscount,
		SelfBillingInvoice,
		APInvoice,
		APInvoiceInternalReference,
		APCreditNoteInternalReference,
		APCreditNote,
		APAdjustmentNoteInternalReference,
		APAdjustmentNote,
		APJournal,
		APTransfer,
		APExchangeDifference,
		APOverpayment,
		APDiscount,
		Contra,
		Payment,
		Receipt,
		DirectPayment,
		DirectReceipt,
		OpeningPayment,
		OpeningReceipt,
		CashBookExchangeDifference,
		CashBookTransfer,
		CFXJournal,
		GLJournal,
		JRJournal,
		APInvoiceApproval,
		CreditControlApproval,
		PaymentBatch,
		EPaymentQuoteInternalRef,
		APPaymentApprovalReference,
		ARPaymentApprovalReference,
		EPaymentDealInternalReference,
		EPaymentBeneficiaryRequestInternalRef
	}

	/// <summary>
	/// Summary description for AccountingNumberFountainWrapperFactory.
	/// </summary>
	public partial class AccountingNumberFountainWrapperFactory
	{
		#region Implementation

		protected
#if DEBUG
			internal /*Todo-Review*/
#endif
		AccountingNumberFountainWrapperFactory()
		{
		}

		public static AccountingNumberFountainWrapperFactory Instance
		{
			get
			{
				return Env.CurrentUserContext.GetInstance(() => new AccountingNumberFountainWrapperFactory());
			}
		}

		#endregion

		#region E-Payment Deal Internal Reference

		public AccountingNumberFountainWrapper EPaymentDealInternalReference
		{
			get
			{
				if (ePaymentDealInternalReference == null)
				{
					ePaymentDealInternalReference = new AccountingNumberFountainWrapper(Env.NumberFountains.EPaymentDealInternalReference, NumberFountainType.EPaymentDealInternalReference);
				}
				return ePaymentDealInternalReference;
			}
		}
		AccountingNumberFountainWrapper ePaymentDealInternalReference;

		#endregion

		#region AP Payment Approval Reference

		public AccountingNumberFountainWrapper APPaymentApprovalReference
		{
			get
			{
				if (apPaymentApprovalReference == null)
				{
					apPaymentApprovalReference = new AccountingNumberFountainWrapper(Env.NumberFountains.APPaymentApprovalReference, NumberFountainType.APPaymentApprovalReference);
				}
				return apPaymentApprovalReference;
			}
		}
		AccountingNumberFountainWrapper apPaymentApprovalReference;

		#endregion

		#region AR Payment Approval Reference

		public AccountingNumberFountainWrapper ARPaymentApprovalReference
		{
			get
			{
				if (arPaymentApprovalReference == null)
				{
					arPaymentApprovalReference = new AccountingNumberFountainWrapper(Env.NumberFountains.ARPaymentApprovalReference, NumberFountainType.ARPaymentApprovalReference);
				}
				return arPaymentApprovalReference;
			}
		}
		AccountingNumberFountainWrapper arPaymentApprovalReference;

		#endregion

		#region DDR Sequences

		AccountingNumberFountainNonVoucher fDDRBatchNo;
		public AccountingNumberFountainNonVoucher DDRBatchNo
		{
			get
			{
				if (fDDRBatchNo == null)
				{
					fDDRBatchNo = new AccountingNumberFountainNonVoucher(Env.NumberFountains.DDRBatchNo);
				}
				return fDDRBatchNo;
			}
		}

		#endregion

		#region JCJournal

		AccountingNumberFountainWrapper fJCJournal;
		public AccountingNumberFountainWrapper JCJournal
		{
			get
			{
				if (fJCJournal == null)
				{
					fJCJournal = new AccountingNumberFountainWrapper(Env.NumberFountains.JCJournalNo, NumberFountainType.CFXJournal);
				}
				return fJCJournal;
			}
		}

		#endregion

		#region APInvoiceApproval

		AccountingNumberFountainWrapper fAPInvoiceApproval;
		public AccountingNumberFountainWrapper APInvoiceApproval
		{
			get
			{
				if (fAPInvoiceApproval == null)
				{
					fAPInvoiceApproval = new AccountingNumberFountainWrapper(Env.NumberFountains.APInvoiceApproval, NumberFountainType.APInvoiceApproval);
				}
				return fAPInvoiceApproval;
			}
		}

		#endregion

		#region APInvoiceApproval

		AccountingNumberFountainWrapper creditControlApproval;
		public AccountingNumberFountainWrapper CreditControlApproval
		{
			get
			{
				if (creditControlApproval == null)
				{
					creditControlApproval = new AccountingNumberFountainWrapper(Env.NumberFountains.CreditControlApproval, NumberFountainType.CreditControlApproval);
				}
				return creditControlApproval;
			}
		}

		#endregion

		#region JRJournal

		public AccountingNumberFountainWrapper JRJournal
		{
			get
			{
				if (JRJournal_cachedValue == null)
				{
					JRJournal_cachedValue = new AccountingNumberFountainWrapper(Env.NumberFountains.JRJournalNo, NumberFountainType.JRJournal);
				}
				return JRJournal_cachedValue;
			}
		}
		AccountingNumberFountainWrapper JRJournal_cachedValue;

		#endregion

		#region Payment Batch

		public AccountingNumberFountainWrapper PaymentBatch
		{
			get
			{
				if (PaymentBatch_cachedValue == null)
				{
					PaymentBatch_cachedValue = new AccountingNumberFountainWrapper(Env.NumberFountains.PaymentBatchNo, NumberFountainType.PaymentBatch);
				}
				return PaymentBatch_cachedValue;
			}
		}
		AccountingNumberFountainWrapper PaymentBatch_cachedValue;

		#endregion

		#region AR Sequences

		AccountingNumberFountainWrapper fARInvoiceNo;
		public AccountingNumberFountainWrapper ARInvoiceNo
		{
			get
			{
				if (fARInvoiceNo == null)
				{
					if (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Inner.GetCurrentValueToUse(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty) == ValueToUse.DefaultValue)
					{
						fARInvoiceNo = new AccountingNumberFountainWrapper(Env.NumberFountains.ARInvoiceNo, NumberFountainType.ARInvoice, AccountingConfigurationRegistry.Instance.ARInvoiceNumberLengthConfiguration.Value);
					}
					else
					{
						fARInvoiceNo = new AccountingNumberFountainWrapper(Env.NumberFountains.ARInvoiceNo, NumberFountainType.ARInvoice);
					}
				}
				return fARInvoiceNo;
			}
		}

		AccountingNumberFountainWrapper fARCreditNote;
		public AccountingNumberFountainWrapper ARCreditNoteNo
		{
			get
			{
				if (fARCreditNote == null)
				{
					fARCreditNote = new AccountingNumberFountainWrapper(Env.NumberFountains.ARCreditNoteNo, NumberFountainType.ARCreditNote, AccountingConfigurationRegistry.Instance.ARInvoiceNumberLengthConfiguration.Value);
				}
				return fARCreditNote;
			}
		}

		AccountingNumberFountainWrapper fARAdjustmentNoteNo;
		public AccountingNumberFountainWrapper ARAdjustmentNoteNo
		{
			get
			{
				if (fARAdjustmentNoteNo == null)
				{
					fARAdjustmentNoteNo = new AccountingNumberFountainWrapper(Env.NumberFountains.ARAdjustmentNoteNo, NumberFountainType.ARAdjustmentNote, AccountingConfigurationRegistry.Instance.ARInvoiceNumberLengthConfiguration.Value);
				}
				return fARAdjustmentNoteNo;
			}
		}

		AccountingNumberFountainWrapper fARJournalNo;
		public AccountingNumberFountainWrapper ARJournalNo
		{
			get
			{
				if (fARJournalNo == null)
				{
					fARJournalNo = new AccountingNumberFountainWrapper(Env.NumberFountains.ARJournalNo, NumberFountainType.ARJournal);
				}
				return fARJournalNo;
			}
		}

		#endregion

		#region AP Sequences

		AccountingNumberFountainWrapper fAPInvoiceNo;
		public AccountingNumberFountainWrapper APInvoiceNo
		{
			get
			{
				if (fAPInvoiceNo == null)
				{
					fAPInvoiceNo = new AccountingNumberFountainWrapper(Env.NumberFountains.APInvoiceNo, NumberFountainType.APInvoice);
				}
				return fAPInvoiceNo;
			}
		}
		AccountingNumberFountainWrapper fAPInvoiceInternalRef;
		public AccountingNumberFountainWrapper APInvoiceInternalRef
		{
			get
			{
				if (fAPInvoiceInternalRef == null)
				{
					fAPInvoiceInternalRef = new AccountingNumberFountainWrapper(Env.NumberFountains.APInvoiceInternalRef, NumberFountainType.APInvoiceInternalReference);
				}
				return fAPInvoiceInternalRef;
			}
		}

		AccountingNumberFountainWrapper fAPCreditNoteInternalRef;
		public AccountingNumberFountainWrapper APCreditNoteInternalRef
		{
			get
			{
				if (fAPCreditNoteInternalRef == null)
				{
					fAPCreditNoteInternalRef = new AccountingNumberFountainWrapper(Env.NumberFountains.APCreditNoteInternalRef, NumberFountainType.APCreditNoteInternalReference);
				}
				return fAPCreditNoteInternalRef;
			}
		}

		AccountingNumberFountainWrapper fAPAdjustmentNoteInternalRef;
		public AccountingNumberFountainWrapper APAdjustmentNoteInternalRef
		{
			get
			{
				if (fAPAdjustmentNoteInternalRef == null)
				{
					fAPAdjustmentNoteInternalRef = new AccountingNumberFountainWrapper(Env.NumberFountains.APAdjustmentNoteInternalRef, NumberFountainType.APAdjustmentNoteInternalReference);
				}
				return fAPAdjustmentNoteInternalRef;
			}
		}

		AccountingNumberFountainWrapper fAPJournalNo;
		public AccountingNumberFountainWrapper APJournalNo
		{
			get
			{
				if (fAPJournalNo == null)
				{
					fAPJournalNo = new AccountingNumberFountainWrapper(Env.NumberFountains.APJournalNo, NumberFountainType.APJournal);
				}
				return fAPJournalNo;
			}
		}

		AccountingNumberFountainWrapper fSelfBillingInvoiceNo;
		public AccountingNumberFountainWrapper SelfBillingInvoiceNo
		{
			get
			{
				if (fSelfBillingInvoiceNo == null)
				{
					fSelfBillingInvoiceNo = new AccountingNumberFountainWrapper(Env.NumberFountains.SelfBillingInvoiceNo, NumberFountainType.SelfBillingInvoice);
				}
				return fSelfBillingInvoiceNo;
			}
		}

		#endregion

		#region Contra Sequences

		AccountingNumberFountainWrapper fContraNo;
		public AccountingNumberFountainWrapper ContraNo
		{
			get
			{
				if (fContraNo == null)
				{
					fContraNo = new AccountingNumberFountainWrapper(Env.NumberFountains.ContraNo, NumberFountainType.Contra);
				}
				return fContraNo;
			}
		}

		#endregion

		#region Receipt No Sequences

		AccountingNumberFountainWrapper fDirectReceiptNo;
		public AccountingNumberFountainWrapper DirectReceiptNo
		{
			get
			{
				if (fDirectReceiptNo == null)
				{
					fDirectReceiptNo = new AccountingNumberFountainWrapper(Env.NumberFountains.DirectReceiptNo, NumberFountainType.DirectReceipt);
				}
				return fDirectReceiptNo;
			}
		}

		AccountingNumberFountainWrapper fOpeningReceiptNo;
		public AccountingNumberFountainWrapper OpeningReceiptNo
		{
			get
			{
				if (fOpeningReceiptNo == null)
				{
					fOpeningReceiptNo = new AccountingNumberFountainWrapper(Env.NumberFountains.OpeningReceiptNo, NumberFountainType.OpeningReceipt);
				}
				return fOpeningReceiptNo;
			}
		}

		#endregion

		#region Payment No Sequences

		AccountingNumberFountainWrapper fDirectPaymentNo;
		public AccountingNumberFountainWrapper DirectPaymentNo
		{
			get
			{
				if (fDirectPaymentNo == null)
				{
					fDirectPaymentNo = new AccountingNumberFountainWrapper(Env.NumberFountains.DirectPaymentNo, NumberFountainType.DirectPayment);
				}
				return fDirectPaymentNo;
			}
		}

		AccountingNumberFountainWrapper fOpeningPaymentNo;
		public AccountingNumberFountainWrapper OpeningPaymentNo
		{
			get
			{
				if (fOpeningPaymentNo == null)
				{
					fOpeningPaymentNo = new AccountingNumberFountainWrapper(Env.NumberFountains.OpeningPaymentNo, NumberFountainType.OpeningPayment);
				}
				return fOpeningPaymentNo;
			}
		}

		#endregion

		#region Transfer No Sequences

		AccountingNumberFountainWrapper fARTransferNo;
		public AccountingNumberFountainWrapper ARTransferNo
		{
			get
			{
				if (fARTransferNo == null)
				{
					fARTransferNo = new AccountingNumberFountainWrapper(Env.NumberFountains.ARTransferNo, NumberFountainType.ARTransfer);
				}
				return fARTransferNo;
			}
		}

		AccountingNumberFountainWrapper fAPTransferNo;
		public AccountingNumberFountainWrapper APTransferNo
		{
			get
			{
				if (fAPTransferNo == null)
				{
					fAPTransferNo = new AccountingNumberFountainWrapper(Env.NumberFountains.APTransferNo, NumberFountainType.APTransfer);
				}
				return fAPTransferNo;
			}
		}

		AccountingNumberFountainWrapper fTransferNo;
		public AccountingNumberFountainWrapper TransferNo
		{
			get
			{
				if (fTransferNo == null)
				{
					fTransferNo = new AccountingNumberFountainWrapper(Env.NumberFountains.TransferNo, NumberFountainType.CashBookTransfer);
				}
				return fTransferNo;
			}
		}

		#endregion

		#region Batch Receipt Sequences

		AccountingNumberFountainNonVoucher fBatchReceiptNo;
		public AccountingNumberFountainNonVoucher BatchReceiptNo
		{
			get
			{
				if (fBatchReceiptNo == null)
				{
					fBatchReceiptNo = new AccountingNumberFountainNonVoucher(Env.NumberFountains.BatchReceiptNo);
				}
				return fBatchReceiptNo;
			}
		}

		#endregion

		#region Match Sequences

		AccountingNumberFountainNonVoucher fPaymentMatchNo;
		public AccountingNumberFountainNonVoucher PaymentMatchNo
		{
			get
			{
				if (fPaymentMatchNo == null)
				{
					fPaymentMatchNo = new AccountingNumberFountainNonVoucher(Env.NumberFountains.PaymentMatchNo);
				}
				return fPaymentMatchNo;
			}
		}

		#endregion

		#region Overpayments Sequences

		AccountingNumberFountainWrapper fAROverpaymentsNo;
		public AccountingNumberFountainWrapper AROverpaymentsNo
		{
			get
			{
				if (fAROverpaymentsNo == null)
				{
					fAROverpaymentsNo = new AccountingNumberFountainWrapper(Env.NumberFountains.AROverpaymentsNo, NumberFountainType.AROverpayment);
				}
				return fAROverpaymentsNo;
			}
		}

		AccountingNumberFountainWrapper fAPOverpaymentsNo;
		public AccountingNumberFountainWrapper APOverpaymentsNo
		{
			get
			{
				if (fAPOverpaymentsNo == null)
				{
					fAPOverpaymentsNo = new AccountingNumberFountainWrapper(Env.NumberFountains.APOverpaymentsNo, NumberFountainType.APOverpayment);
				}
				return fAPOverpaymentsNo;
			}
		}

		#endregion

		#region Exchange Difference Sequences

		AccountingNumberFountainWrapper fARExchangeDifferenceNo;
		public AccountingNumberFountainWrapper ARExchangeDifferenceNo
		{
			get
			{
				if (fARExchangeDifferenceNo == null)
				{
					fARExchangeDifferenceNo = new AccountingNumberFountainWrapper(Env.NumberFountains.ARExchangeDifferenceNo, NumberFountainType.ARExchangeDifference);
				}
				return fARExchangeDifferenceNo;
			}
		}

		AccountingNumberFountainWrapper fAPExchangeDifferenceNo;
		public AccountingNumberFountainWrapper APExchangeDifferenceNo
		{
			get
			{
				if (fAPExchangeDifferenceNo == null)
				{
					fAPExchangeDifferenceNo = new AccountingNumberFountainWrapper(Env.NumberFountains.APExchangeDifferenceNo, NumberFountainType.APExchangeDifference);
				}
				return fAPExchangeDifferenceNo;
			}
		}

		AccountingNumberFountainWrapper fExchangeDifferenceNo;
		public AccountingNumberFountainWrapper ExchangeDifferenceNo
		{
			get
			{
				if (fExchangeDifferenceNo == null)
				{
					fExchangeDifferenceNo = new AccountingNumberFountainWrapper(Env.NumberFountains.ExchangeDifferenceNo, NumberFountainType.CashBookExchangeDifference);
				}
				return fExchangeDifferenceNo;
			}
		}

		#endregion

		#region Discount No Sequences

		AccountingNumberFountainWrapper fARDiscountNo;
		public AccountingNumberFountainWrapper ARDiscountNo
		{
			get
			{
				if (fARDiscountNo == null)
				{
					fARDiscountNo = new AccountingNumberFountainWrapper(Env.NumberFountains.ARDiscountNo, NumberFountainType.ARDiscount);
				}
				return fARDiscountNo;
			}
		}

		AccountingNumberFountainWrapper fAPDiscountNo;
		public AccountingNumberFountainWrapper APDiscountNo
		{
			get
			{
				if (fAPDiscountNo == null)
				{
					fAPDiscountNo = new AccountingNumberFountainWrapper(Env.NumberFountains.APDiscountNo, NumberFountainType.APDiscount);
				}
				return fAPDiscountNo;
			}
		}

		#endregion

		#region Journal Sequences

		AccountingNumberFountainWrapper fGLJournal;
		public AccountingNumberFountainWrapper GLJournal
		{
			get
			{
				if (fGLJournal == null)
				{
					fGLJournal = new AccountingNumberFountainWrapper(Env.NumberFountains.GLJournal, NumberFountainType.GLJournal);
				}
				return fGLJournal;
			}
		}

		AccountingNumberFountainWrapper fWIPAccrualsJournal;
		public AccountingNumberFountainWrapper WIPAccrualsJournal
		{
			get
			{
				if (fWIPAccrualsJournal == null)
				{
					fWIPAccrualsJournal = new AccountingNumberFountainWrapper(Env.NumberFountains.WIPAccrualsJournal);
				}
				return fWIPAccrualsJournal;
			}
		}

		#endregion

		#region Payment Sequences

		AccountingNumberFountainWrapper fPayment;
		public AccountingNumberFountainWrapper Payment
		{
			get
			{
				if (fPayment == null)
				{
					fPayment = new AccountingNumberFountainWrapper(Env.NumberFountains.Payment, NumberFountainType.Payment);
				}
				return fPayment;
			}
		}

		#endregion

		#region Receipt Sequences

		AccountingNumberFountainWrapper fReceipt;
		public AccountingNumberFountainWrapper Receipt
		{
			get
			{
				if (fReceipt == null)
				{
					fReceipt = new AccountingNumberFountainWrapper(Env.NumberFountains.Receipt, NumberFountainType.Receipt);
				}
				return fReceipt;
			}
		}

		#endregion

		#region Transaction Export Sequences

		public AccountingNumberFountainNonVoucher TransactionExportBatchNo
		{
			get
			{
				if (fTransactionExportBatchNo == null)
				{
					fTransactionExportBatchNo = new AccountingNumberFountainNonVoucher(Env.NumberFountains.FinancialTransactionsExportBatchNo);
				}
				return fTransactionExportBatchNo;
			}
		}

		AccountingNumberFountainNonVoucher fTransactionExportBatchNo;

		#endregion

		#region Invoice Batch Sequence

		AccountingNumberFountainNonVoucher fInvoiceBatchNo;
		public AccountingNumberFountainNonVoucher InvoiceBatchNo
		{
			get
			{
				if (fInvoiceBatchNo == null)
				{
					fInvoiceBatchNo = new AccountingNumberFountainNonVoucher(Env.NumberFountains.InvoiceBatchNo);
				}
				return fInvoiceBatchNo;
			}
		}

		#endregion

		#region GenExportBatchSequence Number

		AccountingNumberFountainNonVoucher fGenExportBatchSequenceBatchNo;
		public AccountingNumberFountainNonVoucher GenExportBatchSequenceBatchNo
		{
			get
			{
				if (fGenExportBatchSequenceBatchNo == null)
				{
					fGenExportBatchSequenceBatchNo = new AccountingNumberFountainNonVoucher(Env.NumberFountains.GenExportBatchSequenceBatchNo);
				}
				return fGenExportBatchSequenceBatchNo;
			}
		}

		#endregion

		#region DisbursementJobCloseBatchNumber

		public INumberFountainProxy GetDisbursementJobCloseBatchNumber(Guid companyPK)
			=> Env.NumberFountains.GetAccountingNumberGeneratorFountain("19A420E0-872B-4190-8878-BC4C9F418364|DSB", companyPK);

		#endregion

		#region GenExportBatchSequence Number

		AccountingNumberFountainNonVoucher fAccCollectionBatchNo;
		public AccountingNumberFountainNonVoucher AccCollectionBatchNo
		{
			get
			{
				if (fAccCollectionBatchNo == null)
				{
					fAccCollectionBatchNo = new AccountingNumberFountainNonVoucher(Env.NumberFountains.AccCollectionBatchNo);
				}
				return fAccCollectionBatchNo;
			}
		}

		#endregion

		#region PositivePayFileExportBatchNumber

		AccountingNumberFountainNonVoucher positivePayFileExportBatchNumber;
		public AccountingNumberFountainNonVoucher PositivePayFileExportBatchNumber
		{
			get
			{
				if (positivePayFileExportBatchNumber == null)
				{
					positivePayFileExportBatchNumber = new AccountingNumberFountainNonVoucher(Env.NumberFountains.PositivePayFileExportBatchNumber);
				}
				return positivePayFileExportBatchNumber;
			}
		}

		#endregion

		#region AccEInvoicingBatchNumber

		AccountingNumberFountainNonVoucher accEInvoicingBatchNumber;
		public AccountingNumberFountainNonVoucher AccEInvoicingBatchNumber
		{
			get
			{
				if (accEInvoicingBatchNumber == null)
				{
					accEInvoicingBatchNumber = new AccountingNumberFountainNonVoucher(Env.NumberFountains.AccEInvoicingBatchNo);
				}
				return accEInvoicingBatchNumber;
			}
		}

		#endregion

		#region EPaymentQuoteInternalRef

		AccountingNumberFountainWrapper ePaymentQuoteInternalRef;
		public AccountingNumberFountainWrapper EPaymentQuoteInternalRef
		{
			get
			{
				if (ePaymentQuoteInternalRef == null)
				{
					ePaymentQuoteInternalRef = new AccountingNumberFountainWrapper(Env.NumberFountains.EPaymentQuoteInternalRef, NumberFountainType.EPaymentQuoteInternalRef);
				}
				return ePaymentQuoteInternalRef;
			}
		}

		#endregion

		#region EPaymentBeneficiaryRequestInternalRef

		AccountingNumberFountainWrapper ePaymentBeneficiaryRequestInternalRef;
		public AccountingNumberFountainWrapper EPaymentBeneficiaryRequestInternalRef
		{
			get
			{
				if (ePaymentBeneficiaryRequestInternalRef == null)
				{
					ePaymentBeneficiaryRequestInternalRef = new AccountingNumberFountainWrapper(Env.NumberFountains.EPaymentBeneficiaryRequestInternalRef, NumberFountainType.EPaymentBeneficiaryRequestInternalRef);
				}
				return ePaymentBeneficiaryRequestInternalRef;
			}
		}

		#endregion

		#region EPaymentQuoteInternalRef

		AccountingNumberFountainWrapper arCashAdvanceRequestReference;
		public AccountingNumberFountainWrapper ARCashAdvanceRequestReference
		{
			get
			{
				if (arCashAdvanceRequestReference == null)
				{
					arCashAdvanceRequestReference = new AccountingNumberFountainNonVoucher(Env.NumberFountains.ARCashAdvanceRequestReference);
				}
				return arCashAdvanceRequestReference;
			}
		}

		#endregion

		#region JobChargePostingQueueGroupId

		AccountingNumberFountainNonVoucher jobChargePostingQueueGroupId;
		public AccountingNumberFountainNonVoucher JobChargePostingQueueGroupId
		{
			get
			{
				if (jobChargePostingQueueGroupId == null)
				{
					jobChargePostingQueueGroupId = new AccountingNumberFountainNonVoucher(Env.NumberFountains.JobChargePostingQueueGroupId);
				}
				return jobChargePostingQueueGroupId;
			}
		}

		#endregion

		#region AccBillingHeaderInternalRef

		AccountingNumberFountainWrapper accBillingHeaderInternalRef;
		public AccountingNumberFountainWrapper AccBillingHeaderInternalRef
		{
			get
			{
				if (accBillingHeaderInternalRef == null)
				{
					accBillingHeaderInternalRef = new AccountingNumberFountainNonVoucher(Env.NumberFountains.AccBillingHeaderInternalReference);
				}
				return accBillingHeaderInternalRef;
			}
		}

		#endregion
	}
}
