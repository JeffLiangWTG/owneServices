using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	internal class CashAdvanceRequestInfoByPaymentOrReceipt
	{
		internal CashAdvanceRequestInfoByPaymentOrReceipt(ARReceipt receipt)
		{
			ReceiptOrPayment = receipt;
		}

		internal CashAdvanceRequestInfoByPaymentOrReceipt(APPayment payment)
		{
			ReceiptOrPayment = payment;
		}

		internal List<CashAdvanceRequestHeader> CashAdvanceRequestHeaders
		{
			get
			{
				var cahPKs = new List<ZGuid>();
				if (ReceiptOrPayment.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					cahPKs.AddRange(CashAdvanceJournals.OfType<ARJournal>()
									.Select(j => j.AH_CAH_CashAdvanceRequestHeader));
				}
				else if (ReceiptOrPayment.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					cahPKs.AddRange(CashAdvanceJournals.OfType<APJournal>()
									.Select(j => j.AH_CAH_CashAdvanceRequestHeader));
				}
				var cahs = ReceiptOrPayment.Factory.Load<CashAdvanceRequestHeader>(new ZQuery(AccCashAdvanceRequestHeaderSchema.PK, cahPKs.ToArray())).ToList();
				return cahs;
			}
		}

		internal List<Journal.Journal> CashAdvanceJournals
		{
			get
			{
				var journals = new List<Journal.Journal>();
				var matchGroupNumbers = GetMatchGroupNumbers();
				if (matchGroupNumbers.Any())
				{
					journals = GetJournals(ReceiptOrPayment.Factory, matchGroupNumbers, ReceiptOrPayment.AH_GC);
				}
				return journals;
			}
		}

		List<Journal.Journal> GetJournals(BusinessObjectFactory factory, ZString[] matchGroupNumbers, ZGuid companyPK)
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
			if (ReceiptOrPayment.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				journalFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, Core.Constants.TransactionCategory.Codes.CashAdvanceReceived);
				journals = factory.Load<ARJournal>(journalFilter).ToList<Journal.Journal>();
			}
			else if (ReceiptOrPayment.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				journalFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, Core.Constants.TransactionCategory.Codes.CashAdvancePaid);
				journals = factory.Load<APJournal>(journalFilter).ToList<Journal.Journal>();
			}
			return journals;
		}

		ZString[] GetMatchGroupNumbers()
		{
			if (ReceiptOrPayment != null && ReceiptOrPayment is IMatching matching && matching.IsMatched)
			{
				matching.Matchlinks.Load();
				return matching.Matchlinks.Cast<TransactionMatchLink>().Select(tm => tm.AP_MatchGroupNum).ToArray();
			}
			return Array.Empty<ZString>();
		}

		ReceiptPaymentBase ReceiptOrPayment { get; }
	}
}
