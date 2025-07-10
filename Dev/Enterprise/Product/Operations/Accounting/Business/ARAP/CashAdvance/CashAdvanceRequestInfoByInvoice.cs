using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	public class CashAdvanceRequestInfoByInvoice
	{
		public CashAdvanceRequestInfoByInvoice(Invoice invoice)
		{
			Invoice = invoice;
		}

		Invoice Invoice { get; }

		public List<BaseCharge> Charges => GetJobCharges();

		public List<ICashAdvanceRequirement> CashAdvanceRequirements => GetCashAdvanceRequirements();

		public List<CashAdvanceRequestHeader> CashAdvanceRequestHeaders => GetCashAdvanceRequests();

		List<ICashAdvanceRequirement> GetCashAdvanceRequirements()
		{
			var jobCharges = GetJobCharges();
			if (jobCharges?.Any() ?? false)
			{
				return Invoice.Lines.OfType<InvoicingLineBase>()
									.Where(l => l.RelatedCashAdvanceRequirement != null)
									.Select(l => l.RelatedCashAdvanceRequirement)
									.ToList();
			}
			return null;
		}

		List<CashAdvanceRequestHeader> GetCashAdvanceRequests()
		{
			var lines = GetCashAdvanceRequirements();
			if (lines?.Any() ?? false)
			{
				return lines.Where(l => l?.CashAdvanceRequest != null)
							.Select(l => l.CashAdvanceRequest)
							.Distinct()
							.ToList();
			}
			return null;
		}

		List<BaseCharge> GetJobCharges()
		{
			var charges = Invoice.Lines
								.OfType<InvoicingLineBase>()
								.Select(l => l.GetCashAdvanceRelatedCharge())
								.Where(c => c != null)
								.ToList();
			return charges;
		}

		internal List<ExchangeDifference> EXXTransactions
		{
			get
			{
				var exxTransactions = new List<ExchangeDifference>();
				var matchGroupNumbers = GetMatchGroupNumbers();
				if (matchGroupNumbers.Any())
				{
					exxTransactions = GetEXXTransactionsFromMatchGroup(Invoice.Factory, matchGroupNumbers, Invoice.AH_GC);
				}
				return exxTransactions;
			}
		}

		List<ExchangeDifference> GetEXXTransactionsFromMatchGroup(BusinessObjectFactory factory, ZString[] matchGroupNumbers, ZGuid companyPK)
		{
			var viewMatchLinkFilter = new ZQuery(ViewMatchGroupSchema.MG_GC, companyPK);
			viewMatchLinkFilter.AddToFilter(JoinCondition.And, ViewMatchGroupSchema.MG_MatchGroupNum, SQLComparisonOperator.Equal, matchGroupNumbers);
			viewMatchLinkFilter.AddToFilter(JoinCondition.And, ViewMatchGroupSchema.MG_TransactionType, SQLComparisonOperator.Equal, TransactionTypes.ExchangeDifference);
			var viewMatchLinks = new ViewMatchGroupCollection(factory, viewMatchLinkFilter);
			viewMatchLinks.Load();

			var transactionMatchLinkPKs = viewMatchLinks.Cast<ViewMatchGroup>().Select(m => m.PK);
			var transactionMatchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.PK, transactionMatchLinkPKs);
			var transactionMatchLinks = new AccTransactionMatchLinkCollection(factory, transactionMatchLinkFilter);
			transactionMatchLinks.Load();

			var exxTransactions = new List<ExchangeDifference>();
			var transactionPKs = transactionMatchLinks.Cast<AccTransactionMatchLink>().Select(m => m.AP_AH);
			var exxFilter = new ZQuery(AccTransactionHeaderSchema.PK, transactionPKs);

			if (Invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				exxTransactions = factory.Load<ARExchangeDifference>(exxFilter).ToList<ExchangeDifference>();
			}
			else if (Invoice.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				exxTransactions = factory.Load<APExchangeDifference>(exxFilter).ToList<ExchangeDifference>();
			}
			return exxTransactions;
		}

		internal List<Journal.Journal> InvoicedCashAdvanceJournals
		{
			get
			{
				var journals = new List<Journal.Journal>();
				var matchGroupNumbers = GetMatchGroupNumbers();
				if (matchGroupNumbers.Any())
				{
					journals = GetInvoicedJournalsFromMatchGroup(Invoice.Factory, matchGroupNumbers, Invoice.AH_GC);
				}
				return journals;
			}
		}

		List<Journal.Journal> GetInvoicedJournalsFromMatchGroup(BusinessObjectFactory factory, ZString[] matchGroupNumbers, ZGuid companyPK)
		{
			var viewMatchLinkFilter = new ZQuery(ViewMatchGroupSchema.MG_GC, companyPK);
			viewMatchLinkFilter.AddToFilter(JoinCondition.And, ViewMatchGroupSchema.MG_MatchGroupNum, SQLComparisonOperator.Equal, matchGroupNumbers);
			viewMatchLinkFilter.AddToFilter(JoinCondition.And, ViewMatchGroupSchema.MG_TransactionType, SQLComparisonOperator.Equal, TransactionTypes.Journal);
			var viewMatchLinks = new ViewMatchGroupCollection(factory, viewMatchLinkFilter);
			viewMatchLinks.Load();

			var transactionMatchLinkPKs = viewMatchLinks.Cast<ViewMatchGroup>().Select(m => m.PK);
			var transactionMatchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.PK, transactionMatchLinkPKs);
			var transactionMatchLinks = new AccTransactionMatchLinkCollection(factory, transactionMatchLinkFilter);
			transactionMatchLinks.Load();

			var journals = new List<Journal.Journal>();
			var transactionPKs = transactionMatchLinks.Cast<AccTransactionMatchLink>().Select(m => m.AP_AH);
			var journalFilter = new ZQuery(AccTransactionHeaderSchema.PK, transactionPKs);
			journalFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, Core.Constants.TransactionCategory.Codes.CashAdvanceInvoice);
			journalFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.Equal, null);
			if (Invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				journals = factory.Load<ARJournal>(journalFilter).ToList<Journal.Journal>();
			}
			else if (Invoice.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				journals = factory.Load<APJournal>(journalFilter).ToList<Journal.Journal>();
			}
			return journals;
		}

		ZString[] GetMatchGroupNumbers()
		{
			if (Invoice != null && Invoice is IMatching matching && matching.IsMatched)
			{
				matching.Matchlinks.Load();
				return matching.Matchlinks.Cast<TransactionMatchLink>().Select(tm => tm.AP_MatchGroupNum).ToArray();
			}
			return Array.Empty<ZString>();
		}
	}
}
