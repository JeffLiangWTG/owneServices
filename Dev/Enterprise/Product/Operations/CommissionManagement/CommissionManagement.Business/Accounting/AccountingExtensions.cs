using System.Linq;
using CargoWise.Common;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.CommissionManagement.Business
{
	public static class AccountingExtensions
	{
		public static bool IsFullAmendingCreditNote(this TransactionHeaderWithLines transaction)
		{
			if (!transaction.IsAmendingCreditNote || !transaction.AH_TransactionBelongsToGroup.IsValid)
			{
				return false;
			}

			var invoiceTransaction = transaction.Factory.Load<InvoicingBase>(transaction.AH_TransactionBelongsToGroup);
			if (invoiceTransaction == null)
			{
				ErrorReporter.ReportOnce("IsFullAmendingCreditNote|EmptyInvoiceTransaction", $"Invoice not found. PK {transaction.AH_TransactionBelongsToGroup}");
			}
			if (invoiceTransaction == null || !invoiceTransaction.AH_TransactionType.Equals(TransactionTypes.Invoice))
			{
					return false;
			}

			var invoiceLineGroups = (from InvoicingLineBase l in invoiceTransaction.Lines
									 group l by new { l.AL_AC, l.AL_RX_NKTransactionCurrency, l.AL_JH, l.AL_OH }
															 into grp
									 select new
									 {
										 grp.Key.AL_AC,
										 grp.Key.AL_RX_NKTransactionCurrency,
										 grp.Key.AL_JH,
										 grp.Key.AL_OH,
										 LineAmountSubTotal = grp.Sum(a => a.AL_LineAmount)
									 }).ToList();

			var creditNoteLineGroups = (from InvoicingLineBase l in transaction.Lines
										group l by new { l.AL_AC, l.AL_RX_NKTransactionCurrency, l.AL_JH, l.AL_OH }
															 into grp
										select new
										{
											grp.Key.AL_AC,
											grp.Key.AL_RX_NKTransactionCurrency,
											grp.Key.AL_JH,
											grp.Key.AL_OH,
											LineAmountSubTotal = grp.Sum(a => -a.AL_LineAmount)
										}).ToList();

			if (invoiceLineGroups.Count != creditNoteLineGroups.Count)
			{
				return false;
			}

			var matches = from invoiceLineGroup in invoiceLineGroups
						  from creditLineGroup in creditNoteLineGroups
						  where invoiceLineGroup.AL_AC == creditLineGroup.AL_AC
							  && invoiceLineGroup.AL_RX_NKTransactionCurrency == creditLineGroup.AL_RX_NKTransactionCurrency
							  && invoiceLineGroup.AL_OH == creditLineGroup.AL_OH
							  && invoiceLineGroup.AL_JH == creditLineGroup.AL_JH
							  && invoiceLineGroup.LineAmountSubTotal == creditLineGroup.LineAmountSubTotal
						  select invoiceLineGroup;

			return matches.Count() == invoiceLineGroups.Count;
		}
	}
}
