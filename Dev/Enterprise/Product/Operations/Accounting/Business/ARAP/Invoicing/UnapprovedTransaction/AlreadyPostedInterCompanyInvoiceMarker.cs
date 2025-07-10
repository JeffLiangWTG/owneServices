using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class AlreadyPostedInterCompanyInvoiceMarker
	{
		readonly IEnumerable<TransactionHeader> invoiceToMarkAsAlreadyPosted;

		public AlreadyPostedInterCompanyInvoiceMarker(IEnumerable<TransactionHeader> invoiceToMarkAsAlreadyPosted)
		{
			Argument.NotNull(invoiceToMarkAsAlreadyPosted, "invoiceToMarkAsAlreadyPosted");
			this.invoiceToMarkAsAlreadyPosted = invoiceToMarkAsAlreadyPosted;
		}

		public ZString GetErrorMessageForIncorrectSelection()
		{
			var invoicesOtherThanInvoiceOrCreditNote = from TransactionHeader invoice in invoiceToMarkAsAlreadyPosted
													   where (invoice.AH_TransactionType != TransactionTypes.Invoice && invoice.AH_TransactionType != TransactionTypes.CreditNote)
													   select invoice;
			if (invoicesOtherThanInvoiceOrCreditNote.Any())
			{
				return Res.GetString("34c0c1a6-1c59-4489-93c0-3bc9044c219e", @"You can only flag intercompany transactions as already posted. 
The following transaction(s) cannot be updated:

{0}

Please re-select the required transaction(s) to be updated as already posted.", GetTransactionNumbers(invoicesOtherThanInvoiceOrCreditNote));
			}
			else
			{
				return ZString.Empty;
			}
		}

		public ZString GetMessageForConfirmationPrompt()
		{
			return Res.GetString("e20f68d7-c6f9-481b-a7a1-b961779e9e0c", @"Do you wish to flag the following intercompany transactions as already posted?

{0}", GetTransactionNumbers(invoiceToMarkAsAlreadyPosted));
		}

		public void MarkAsAlreadyPosted()
		{
			invoiceToMarkAsAlreadyPosted.ToList().ForEach(x => x.AH_PostedInternal = true);
		}

		string GetTransactionNumbers(IEnumerable<TransactionHeader> invoices)
		{
			if (invoices != null)
			{
				var transactionNumbers = (from TransactionHeader invoice in invoices
																	orderby invoice.AH_TransactionNum
										  select invoice.AH_TransactionNum).Take(10);

				return string.Join(System.Environment.NewLine, transactionNumbers.ToArray());
			}

			return string.Empty;
		}
	}
}
