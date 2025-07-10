using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.TaxFramework.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework
{
	public interface ITaxRealisationEnabler
	{
		string RealiseTaxIfApplicable(IMatchingCollection matchedTransactions, IWithholdingJournalCreationManager journalCreationManager, ZDate matchDate);
	}

	public class TaxRealisationEnabler : ITaxRealisationEnabler
	{
		string ITaxRealisationEnabler.RealiseTaxIfApplicable(IMatchingCollection matchedTransactions, IWithholdingJournalCreationManager journalCreationManager, ZDate matchDate)
		{
			var errorMessage = string.Empty;
			var taxProcessor = ObjectFactory.Get<ITaxProcessor>();
			foreach (IMatching transaction in matchedTransactions)
			{
				if (transaction is InvoicingBase invoicingBase)
				{
					var taxParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoicingBase);
					taxProcessor.ProcessTaxesOnMatching(taxParent, matchDate);
					if (journalCreationManager != null)
					{
						var matchTranasctionDetails = journalCreationManager.GetAssociatedJournalDetails(invoicingBase);
						if (matchTranasctionDetails.Any())
						{
							errorMessage = taxProcessor.ProcessPaymentRetentionTaxes(taxParent, matchTranasctionDetails);
						}
					}
				}
			}

			return errorMessage;
		}
	}
}
