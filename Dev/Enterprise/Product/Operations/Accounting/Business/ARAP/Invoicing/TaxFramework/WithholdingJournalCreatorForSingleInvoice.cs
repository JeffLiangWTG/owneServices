using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework
{
	public interface IWithholdingJournalCreatorForSingleInvoice
	{
		(WithholdingJournalsPerInvoice journalsPerInvoice, string errorMessage) Create(InvoicingBase apTransaction);
	}

	public class WithholdingJournalCreatorForSingleInvoice : IWithholdingJournalCreatorForSingleInvoice
	{
		public WithholdingJournalCreatorForSingleInvoice(IWithholdingJournalCreationManager withholdingJournalCreationManager)
		{
			WithholdingJournalCreationManager = Argument.NotNull(withholdingJournalCreationManager, nameof(withholdingJournalCreationManager));
		}

		(WithholdingJournalsPerInvoice journalsPerInvoice, string errorMessage) IWithholdingJournalCreatorForSingleInvoice.Create(InvoicingBase apTransaction)
		{
			var emptyCollection = new WithholdingJournalsPerInvoice(apTransaction, Array.Empty<WithholdingJournalForDisplay>());

			if (apTransaction.AH_NotionalWHTTax == 0)
			{
				return (emptyCollection, NoNotionalWHTTaxToRealizeMessage);
			}

			var withholdingJournals = WithholdingJournalCreationManager.CreateAPWithholdingJournalsIfApplicable(apTransaction, apTransaction.Factory, ZDate.Today).ToList();
			if (!withholdingJournals.Any())
			{
				return (emptyCollection, NoNotionalWHTTaxToRealizeMessage);
			}

			var journals = withholdingJournals.Select(journal => new WithholdingJournalForDisplay(journal, apTransaction.AH_PostDate.Date)).ToList();

			var journalsPerInvoice = new WithholdingJournalsPerInvoice(apTransaction, journals);

			return (journalsPerInvoice, string.Empty);
		}

		static string NoNotionalWHTTaxToRealizeMessage => Res.GetString("571687a8-1e31-4a91-a08f-882e18ee2231", "No Journal can be created. There are no Notional Withholding Tax records to be realized.");

		public IWithholdingJournalCreationManager WithholdingJournalCreationManager { get; }
	}
}
