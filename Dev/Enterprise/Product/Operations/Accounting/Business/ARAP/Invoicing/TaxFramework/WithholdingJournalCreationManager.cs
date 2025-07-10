using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework
{
	public interface IWithholdingJournalCreationManager
	{
		IEnumerable<APJournal> CreateAPWithholdingJournalsIfApplicable(IMatching transaction, BusinessObjectFactory factory, ZDate matchDate);
		bool ShouldAPWithholdingJournalsBeCreated(BusinessObjectFactory factory);
		bool CheckIfAnyTransactionIsLinkedToWithholdingJournal(IEnumerable<IMatching> transactions);
		IEnumerable<APJournal> GetWithholdingJournalsToDelete(IMatching transaction);
		ZDecimal? CalculateRealizedWHT(ZGuid transactionPK);
		ZDecimal? CalculateNotionalWHT(ZGuid transactionPK);
		IEnumerable<IMatchTransactionDetails> GetAssociatedJournalDetails(IMatching transaction);
		void DeleteAllWithholdingJournals();
		void ResetCache();
	}

	public class WithholdingJournalCreationManager : IWithholdingJournalCreationManager
	{
		public WithholdingJournalCreationManager()
		{
			withholdingAPJournalCreator_constructorInitializedOnly = new WithholdingAPJournalCreator();
			WithholdingJournalsPerInvoice = new Dictionary<InvoicingBase, ITaxDetails>();
		}

		IEnumerable<APJournal> IWithholdingJournalCreationManager.CreateAPWithholdingJournalsIfApplicable(IMatching transaction, BusinessObjectFactory factory, ZDate matchDate)
		{
			var result = new List<APJournal>();
			if (transaction is InvoicingBase invoice && (invoice is APInvoice || invoice is APCreditNote))
			{
				var withholdingJournals = WithholdingAPJournalCreator.Create(factory, invoice, matchDate);
				if (withholdingJournals != null)
				{
					WithholdingJournalsPerInvoice.Add(invoice, withholdingJournals);
					result.AddRange(GetLinkedWithholdingJournals(invoice));
				}
			}

			return result;
		}

		bool IWithholdingJournalCreationManager.ShouldAPWithholdingJournalsBeCreated(BusinessObjectFactory factory) => !(factory.HasContext(BusinessContext.RemittanceFileImport) || factory.HasContext(BusinessContext.UniversalTransactionBatchImport));

		IEnumerable<APJournal> IWithholdingJournalCreationManager.GetWithholdingJournalsToDelete(IMatching transaction)
		{
			var journalsToDelete = new List<APJournal>();
			if (transaction is InvoicingBase invoice && WithholdingJournalsPerInvoice.ContainsKey(invoice))
			{
				journalsToDelete.AddRange(GetLinkedWithholdingJournals(invoice));
				if (journalsToDelete.Any())
				{
					WithholdingJournalsPerInvoice.Remove(invoice);
					journalsToDelete.ForEach(x => x.Delete());
				}
			}
			else if (transaction is APJournal apJournal)
			{
				var parentInvoice = GetParentTransactionIfJournalIsPresentInDictionary(apJournal);
				if (parentInvoice != null)
				{
					journalsToDelete.Add(apJournal);
					WithholdingJournalsPerInvoice[parentInvoice].JournalDetails.Remove(apJournal);
					apJournal.Delete();

					if (!WithholdingJournalsPerInvoice[parentInvoice].JournalDetails.Any())
					{
						WithholdingJournalsPerInvoice.Remove(parentInvoice);
					}
				}
			}

			return journalsToDelete;
		}

		bool IWithholdingJournalCreationManager.CheckIfAnyTransactionIsLinkedToWithholdingJournal(IEnumerable<IMatching> transactions)
		{
			foreach (var transaction in transactions)
			{
				if (transaction is InvoicingBase invoice && WithholdingJournalsPerInvoice.ContainsKey(invoice))
				{
					return true;
				}
				else if (transaction is APJournal apJournal)
				{
					var parentInvoice = GetParentTransactionIfJournalIsPresentInDictionary(apJournal);
					if (parentInvoice != null)
					{
						return true;
					}
				}
			}

			return false;
		}

		ZDecimal? IWithholdingJournalCreationManager.CalculateRealizedWHT(ZGuid transactionPK)
		{
			ZDecimal? result = default;
			var invoice = WithholdingJournalsPerInvoice.Keys.FirstOrDefault(i => i.PK == transactionPK);
			if (invoice != null)
			{
				var realizedWHTInMemory = GetRealizedWHTInMemory(invoice);
				var realizedWHT = WithholdingJournalsPerInvoice[invoice].RealizedWHT;
				result = realizedWHTInMemory + realizedWHT;
			}
			return result;
		}

		ZDecimal? IWithholdingJournalCreationManager.CalculateNotionalWHT(ZGuid transactionPK)
		{
			ZDecimal? result = default;
			var invoice = WithholdingJournalsPerInvoice.Keys.FirstOrDefault(i => i.PK == transactionPK);
			if (invoice != null)
			{
				var realizedWHTInMemory = GetRealizedWHTInMemory(invoice);
				var notionalWHT = WithholdingJournalsPerInvoice[invoice].NotionalWHT;
				result = notionalWHT - realizedWHTInMemory;
			}
			return result;
		}

		ZDecimal GetRealizedWHTInMemory(InvoicingBase invoice) => WithholdingJournalsPerInvoice[invoice].JournalDetails.Keys.Sum(j => j.AH_InvoiceAmount);

		IEnumerable<IMatchTransactionDetails> IWithholdingJournalCreationManager.GetAssociatedJournalDetails(IMatching transaction)
		{
			if (transaction is InvoicingBase invoice && WithholdingJournalsPerInvoice.ContainsKey(invoice))
			{
				return WithholdingJournalsPerInvoice[invoice].JournalDetails.Values;
			}

			return new List<IMatchTransactionDetails>() { };
		}

		void IWithholdingJournalCreationManager.DeleteAllWithholdingJournals()
		{
			var journalsToDelete = new List<APJournal>();
			foreach (var item in WithholdingJournalsPerInvoice)
			{
				journalsToDelete.AddRange(GetLinkedWithholdingJournals(item.Key));
			}

			journalsToDelete.ForEach(x => x.Delete());
			WithholdingJournalsPerInvoice.Clear();
		}

		void IWithholdingJournalCreationManager.ResetCache()
		{
			WithholdingJournalsPerInvoice.Clear();
		}

		IEnumerable<APJournal> GetLinkedWithholdingJournals(InvoicingBase invoice) => WithholdingJournalsPerInvoice[invoice].JournalDetails.Keys;

		InvoicingBase GetParentTransactionIfJournalIsPresentInDictionary(APJournal journal)
		{
			foreach (var item in WithholdingJournalsPerInvoice)
			{
				if (item.Value.JournalDetails.ContainsKey(journal))
				{
					return item.Key;
				}
			}

			return null;
		}

		IWithholdingAPJournalCreator WithholdingAPJournalCreator => withholdingAPJournalCreator_constructorInitializedOnly;
		IWithholdingAPJournalCreator withholdingAPJournalCreator_constructorInitializedOnly;

		Dictionary<InvoicingBase, ITaxDetails> WithholdingJournalsPerInvoice { get; }
#if DEBUG
		public void SubstituteWithholdingJournalCreationManager_ForTestOnly(IWithholdingAPJournalCreator replacement) => withholdingAPJournalCreator_constructorInitializedOnly = replacement;

		public IWithholdingAPJournalCreator WithholdingAPJournalCreator_ExposedForTestOnly => WithholdingAPJournalCreator;
#endif
	}
}
