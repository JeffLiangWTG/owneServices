using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Accounting.TaxFramework.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework
{
	public interface IWithholdingJournalRealizerForSingleInvoice
	{
		string Realize(WithholdingJournalsPerInvoice journalsPerInvoice);
	}

	public class WithholdingJournalRealizerForSingleInvoice : IWithholdingJournalRealizerForSingleInvoice
	{
		public WithholdingJournalRealizerForSingleInvoice(IWithholdingJournalCreationManager withholdingJournalCreationManager)
		{
			WithholdingJournalCreationManager = withholdingJournalCreationManager;
		}

		string IWithholdingJournalRealizerForSingleInvoice.Realize(WithholdingJournalsPerInvoice withholdingJournalsPerInvoice)
		{
			var errorMessage = string.Empty;
			InvoicingBase apTransaction = withholdingJournalsPerInvoice.ParentInvoice;
			var matchTransactionDetails = WithholdingJournalCreationManager.GetAssociatedJournalDetails(apTransaction).ToList();
			if (matchTransactionDetails.Any())
			{
				var matchTransctionDetailsDictionary = matchTransactionDetails.ToDictionary(x => x.PK);
				foreach (WithholdingJournalForDisplay journalForDisplay in withholdingJournalsPerInvoice.Journals)
				{
					matchTransctionDetailsDictionary[journalForDisplay.JournalPK].RealisationDate = journalForDisplay.PostDate;
				}
				errorMessage = ObjectFactory.Get<ITaxProcessor>().ProcessPaymentRetentionTaxes(TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(apTransaction), matchTransctionDetailsDictionary.Values);
			}
			else
			{
				errorMessage = Res.GetString("5e0991a7-7cc9-43a1-8d4b-c08b85dc13fa", "An unexpected error has occurred. Please try again.");
				ErrorReporter.ReportOnce("IWithholdingJournalProcessorForModule.Realize", "No MatchTranasctionDetails is found in the dictionary inside WithholdingJournalCreationManager for the AP tranaction.");
			}

			return errorMessage;
		}

		public IWithholdingJournalCreationManager WithholdingJournalCreationManager { get; }
	}
}
