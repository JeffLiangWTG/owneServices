using System.Collections.Generic;
using System.Text;
using CargoWise.Common;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework
{
	public interface IWithholdingJournalCreatorForMultipleInvoices
	{
		(IReadOnlyCollection<WithholdingJournalsPerInvoice> journalCollectionPerInvoice, string errorMessage) Create(IReadOnlyCollection<InvoicingBase> invoices);
	}

	public class WithholdingJournalCreatorForMultipleInvoices : IWithholdingJournalCreatorForMultipleInvoices
	{
		public WithholdingJournalCreatorForMultipleInvoices(IWithholdingJournalCreatorForSingleInvoice withholdingJournalCreator)
		{
			WithholdingJournalCreator = Argument.NotNull(withholdingJournalCreator, nameof(withholdingJournalCreator));
		}

		public (IReadOnlyCollection<WithholdingJournalsPerInvoice> journalCollectionPerInvoice, string errorMessage) Create(IReadOnlyCollection<InvoicingBase> invoices)
		{
			var invoicesWithJournalsCollection = new List<WithholdingJournalsPerInvoice>();
			var errorMessages = new StringBuilder();

			foreach (var invoice in invoices)
			{
				var (journalsPerInvoice, errorMessage) = WithholdingJournalCreator.Create(invoice);

				if (!string.IsNullOrEmpty(errorMessage))
				{
					if (errorMessages.Length > 0)
					{
						errorMessages.AppendLine();
					}
					errorMessages.Append($"{invoice.AH_TransactionNum}: {errorMessage}");
				}
				else
				{
					invoicesWithJournalsCollection.Add(journalsPerInvoice);
				}
			}

			return (invoicesWithJournalsCollection.AsReadOnly(), errorMessages.ToString());
		}

		public IWithholdingJournalCreatorForSingleInvoice WithholdingJournalCreator { get; }
	}
}
