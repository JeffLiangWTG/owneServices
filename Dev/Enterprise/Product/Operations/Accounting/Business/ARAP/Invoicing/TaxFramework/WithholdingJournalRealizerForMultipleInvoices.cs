using System.Collections.Generic;
using System.Text;
using CargoWise.Common;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework
{
	public interface IWithholdingJournalRealizerForMultipleInvoices
	{
		string Realize(IReadOnlyCollection<WithholdingJournalsPerInvoice> journalCollectionPerInvoice);
	}

	public class WithholdingJournalRealizerForMultipleInvoices : IWithholdingJournalRealizerForMultipleInvoices
	{
		public WithholdingJournalRealizerForMultipleInvoices(IWithholdingJournalRealizerForSingleInvoice withholdingJournalRealizer)
		{
			WithholdingJournalRealizer = Argument.NotNull(withholdingJournalRealizer, nameof(withholdingJournalRealizer));
		}

		public string Realize(IReadOnlyCollection<WithholdingJournalsPerInvoice> journalsForMultipleInvoices)
		{
			var errorMessages = new StringBuilder();

			foreach (var withholdingJournal in journalsForMultipleInvoices)
			{
				var errorMessage = WithholdingJournalRealizer.Realize(withholdingJournal);

				if (!string.IsNullOrEmpty(errorMessage))
				{
					if (errorMessages.Length > 0)
					{
						errorMessages.AppendLine();
					}
					errorMessages.Append($"{withholdingJournal.ParentInvoice.AH_TransactionNum}: {errorMessage}");
				}
			}

			return errorMessages.ToString();
		}

		public IWithholdingJournalRealizerForSingleInvoice WithholdingJournalRealizer { get; }
	}
}
