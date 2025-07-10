using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	public class PaidCashAdvanceRequestReverser : ICashAdvanceRequestProcessingByJournalVisitor
	{
		public PaidCashAdvanceRequestReverser(TransactionHeader cashAdvancePaymentOrReceiptTransaction)
		{
			CashAdvancePaymentOrReceiptTransaction = cashAdvancePaymentOrReceiptTransaction;
		}

		TransactionHeader CashAdvancePaymentOrReceiptTransaction { get; }

		public void Visit(ARJournal arJournal)
		{
			if (arJournal.AH_TransactionCategory == Core.Constants.TransactionCategory.Codes.CashAdvanceReceived)
			{
				VisitInternal(arJournal, Res.GetString("8b48b665-30c6-464e-b1fa-2f109909d520", "Receipt"));
			}
		}

		public void Visit(APJournal apJournal)
		{
			if (apJournal.AH_TransactionCategory == Core.Constants.TransactionCategory.Codes.CashAdvancePaid)
			{
				VisitInternal(apJournal, Res.GetString("9dbb63aa-d304-4459-b28d-673ac92cf634", "Payment"));
			}
		}

		void VisitInternal(Journal.Journal journal, string transactionTypeDescription)
		{
			if (journal != null &&
				journal.CashAdvanceRequestHeader != null &&
				(CashAdvancePaymentOrReceiptTransaction?.IsReversed ?? false) &&
				CashAdvancePaymentOrReceiptTransaction.ReverseTransaction is TransactionHeader cashAdvancePaymentOrReceiptReverseTransaction)
			{
				cashAdvancePaymentOrReceiptReverseTransaction.UnmatchDate = cashAdvancePaymentOrReceiptReverseTransaction.AH_PostDate;

				//Reversing CAR or CAP journal associated with the cash advance
				var reversingFactory = new ReversingFactory();
				var reverser = reversingFactory.NewReversing(journal);
				if (!reverser.CanReverseTransaction)
				{
					throw new CannotGenerateCashAdvanceJournalException(null, Res.GetString("e36b896f-8e9d-4295-a989-335e04a0b142", "JNL {0} {1} cannot be reversed for the following reason.\r\n{2}"
																	, journal.AH_TransactionCategory
																	, journal.AH_TransactionNum
																	, reverser.CantReverseErrorMessage));
				}

				reverser.Reverse();

				//Updating relevant cash advance
				if (journal.CashAdvanceRequestHeader.CAH_Status == CashAdvanceStatusCodes.RequestHeader.Paid)
				{
					journal.CashAdvanceRequestHeader.UndoPaidStatus(updatedViaMatchingJournal: true);
				}

				reverser.ReverseTransaction.ReversingCode = CashAdvancePaymentOrReceiptTransaction.ReverseTransaction.ReversingCode;
				reverser.ReverseTransaction.ReversingReason = CashAdvancePaymentOrReceiptTransaction.ReverseTransaction.ReversingReason;

				var safeSizedDesc = new ZString(Res.GetString("bf911de3-45c0-425a-8e5b-f0942d3e8ee0", "Reversal Related to [{0}]. {1} [{2}] reversed. {3}", journal.AH_TransactionNum, transactionTypeDescription, CashAdvancePaymentOrReceiptTransaction.AH_TransactionNum, reverser.ReverseTransaction.ReversingReason))
										.SubstringSafe(0, AccTransactionHeaderSchema.AH_Desc.MaxLength);
				var reversedJournal = reverser.ReverseTransaction as Journal.Journal;
				reversedJournal.AH_Desc = safeSizedDesc;
				reversedJournal.AH_InvoiceDate = cashAdvancePaymentOrReceiptReverseTransaction.AH_InvoiceDate;
				reversedJournal.AH_DueDate = cashAdvancePaymentOrReceiptReverseTransaction.AH_DueDate;
				reversedJournal.AH_PostDate = cashAdvancePaymentOrReceiptReverseTransaction.AH_PostDate;
			}
		}
	}
}
