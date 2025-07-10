using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Unmatching;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public partial class PayablesAndReceivablesReversing : ReversingBase
	{
		public PayablesAndReceivablesReversing(IPayablesAndReceivables payablesAndReceivablesTransaction)
			: base(payablesAndReceivablesTransaction)
		{
		}

		protected override bool CanTransactionBeReversed()
		{
			return base.CanTransactionBeReversed() &&
						(!IsTransactionMatched || CanTransactionBeUnmatched) &&
						(!IsTAPorTNFTransaction() || FindJournalWithOppositeAmount() == null) &&
						!IsCashAdvanceOverpaymentJournal;
		}

		protected bool IsCashAdvanceOverpaymentJournal
		{
			get
			{
				var journal = OriginalTransaction as Journal;
				if (journal != null)
				{
					return journal.AH_TransactionCategory == Core.Constants.TransactionCategory.Codes.CashAdvanceInvoice && journal.AH_TransactionBelongsToGroup.IsValid;
				}
				return false;
			}
		}

		protected virtual bool IsTransactionMatched
		{
			get { return OriginalPayablesAndReceivables.IsMatched; }
		}

		protected virtual bool CanTransactionBeUnmatched
		{
			get { return OriginalPayablesAndReceivables is IUnmatchOnReversing; }
		}

		protected override ZString GenerateCantReverseErrorMessage()
		{
			var result = ZString.Empty;
			if (!OriginalPayablesAndReceivables.IsReversed && OriginalPayablesAndReceivables.IsMatched && !CanTransactionBeUnmatched)
			{
				result = MatchedAndCantReverseErrorMessage;
			}
			else if (IsTAPorTNFTransaction())
			{
				result = ReversingOfTNFTAPNotAllowedMessage(FindJournalWithOppositeAmount());
			}
			else if (IsCashAdvanceOverpaymentJournal)
			{
				result = JournalAssociatedWithInvoiceCantReverseErrorMessage;
			}
			else
			{
				result = base.GenerateCantReverseErrorMessage();
			}
			return result;
		}

		protected override IEnumerable<UnmatchingRow> GetAllRelatedUnmatchingRows()
			=> UnmatchingRowCollection.Cast<UnmatchingRow>();

		protected IPayablesAndReceivables OriginalPayablesAndReceivables
		{
			get { return (IPayablesAndReceivables)OriginalTransaction; }
		}

		protected IPayablesAndReceivables ReversePayablesAndReceivables
		{
			get { return (IPayablesAndReceivables)ReverseTransaction; }
		}

		protected override void DoReverseTransaction()
		{
			base.DoReverseTransaction();
			if (OriginalPayablesAndReceivables != null && ReversePayablesAndReceivables != null)
			{
				FullyPayBothTransactions();
				GetMatchLinksFromTransactions();
			}
		}

		protected override void GenerateReverseTransactions()
		{
			base.GenerateReverseTransactions();

			var originalPayablesAndReceivablesCasted = OriginalPayablesAndReceivables as IUnmatchOnReversing;
			var reversePayablesAndReceivablesCasted = ReversePayablesAndReceivables as IUnmatchOnReversing;
			if (originalPayablesAndReceivablesCasted != null && reversePayablesAndReceivablesCasted != null)
			{
				reversePayablesAndReceivablesCasted.UnmatchingData.InitializaReverseTransactionData(originalPayablesAndReceivablesCasted.UnmatchingData);
			}
		}

		protected override void UnmatchRelatedMatchingSessions()
		{
			base.UnmatchRelatedMatchingSessions();

			if (OriginalPayablesAndReceivables is IUnmatchOnReversing transactionForUnmatching)
			{
				bool allowBackPosting = UnmatchingRowCollection.Cast<UnmatchingRow>().All(x => x.AllowBackPosting);
				transactionForUnmatching.UnmatchingData.CalculateMaxMatchDate();
				transactionForUnmatching.UnmatchingData.AllowBackPosting = allowBackPosting;

				foreach (UnmatchingRow unmatchingRow in UnmatchingRowCollection)
				{
					unmatchingRow.UnmatchAnyGroup();
				}
			}
		}

		UnmatchingRowCollection UnmatchingRowCollection
		{
			get
			{
				if (unmatchingRowCollection == null)
				{
					unmatchingRowCollection = new UnmatchingRowCollection(OriginalTransaction.Factory);
					if (OriginalPayablesAndReceivables is IUnmatchOnReversing transactionForUnmatching)
					{
						foreach (var matchLink in transactionForUnmatching.UnmatchingData.MatchLinksToUnmatch)
						{
							var unmatchingRow = new UnmatchingRow(OriginalTransaction.Factory);
							unmatchingRow.Initialize(matchLink);
							unmatchingRowCollection.Add(unmatchingRow);
						}
					}
				}
				return unmatchingRowCollection;
			}
		}
		UnmatchingRowCollection unmatchingRowCollection;

		protected virtual void GetMatchLinksFromTransactions()
		{
			OriginalPayablesAndReceivables.GenerateMatchLinks();
			ReversePayablesAndReceivables.GenerateMatchLinks();

			OriginalTransactionMatchLinks = new TransactionMatchLinkCollection(OriginalPayablesAndReceivables.Factory);
			OriginalTransactionMatchLinks.AddRange(OriginalPayablesAndReceivables.CurrentMatchGroup);
			OriginalPayablesAndReceivables.CurrentMatchGroup.RemoveAll();
			ReverseTransactionMatchLinks = new TransactionMatchLinkCollection(ReversePayablesAndReceivables.Factory);
			ReverseTransactionMatchLinks.AddRange(ReversePayablesAndReceivables.CurrentMatchGroup);
			ReversePayablesAndReceivables.CurrentMatchGroup.RemoveAll();
		}

		protected override void HookFactorySaveToSetNumberFountainAndDateFields()
		{
			base.HookFactorySaveToSetNumberFountainAndDateFields();
			OriginalPayablesAndReceivables.Factory.Saving += new BusinessObjectFactory.SavingEventHandler(SetMatchGroupNumberAndMatchDateOnSaving);
		}

		protected virtual void FullyPayBothTransactions()
		{
			OriginalPayablesAndReceivables.FullyPay(((ITransaction)ReversePayablesAndReceivables).PostDate);
			ReversePayablesAndReceivables.FullyPay(((ITransaction)ReversePayablesAndReceivables).PostDate);
		}

		protected virtual void SetMatchGroupNumberAndMatchDateOnSaving(BusinessObjectFactory factory)
		{
			if (ReversePayablesAndReceivables != null)
			{
				FullyPayBothTransactions();

				if (OriginalTransactionMatchLinks != null && ReverseTransactionMatchLinks != null &&
					(OriginalTransactionMatchLinks.Count > 0 || ReverseTransactionMatchLinks.Count > 0))
				{
					ZString matchGroupNumber = GetMatchGroupNumber();
					ZDateTime matchDate = ((ITransaction)ReversePayablesAndReceivables).PostDate;

					TransactionMatchLinkGroup matchGroup = new TransactionMatchLinkGroup(OriginalTransactionMatchLinks);
					matchGroup.AddRange(ReverseTransactionMatchLinks);
					matchGroup.SetMatchGroupNumberAndMatchDate(matchGroupNumber, matchDate);
				}

				var transactionForUnmatching = ReversePayablesAndReceivables as IUnmatchOnReversing;
				if (transactionForUnmatching != null)
				{
					foreach (UnmatchingRow unmatchingRow in UnmatchingRowCollection)
					{
						unmatchingRow.UnmatchDate = transactionForUnmatching.UnmatchDate;
						unmatchingRow.UpdateUnmatchDateForAlreadyUnmatchedTransactions();
					}
				}
			}

			factory.Saving -= new BusinessObjectFactory.SavingEventHandler(SetMatchGroupNumberAndMatchDateOnSaving);
		}

		protected virtual ZString GetMatchGroupNumber()
		{
			return TransactionMatchLink.MatchGroupNumberFountain.GetNextFormatted(OriginalPayablesAndReceivables.Factory);
		}

		Journal FindJournalWithOppositeAmount()
		{
			var journal = OriginalTransaction as Journal;
			if (journal != null)
			{
				var journals = MatchingBase.FindJournalsWithOppositeAmount(journal);

				if (journals.Any())
				{
					// Pick ideally an unmatched journal, but if not available then a matched one
					var journalPKs = journals.Select(j => j.PK).ToList();
					var matchLinks = journal.Factory.Load<AccTransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, journalPKs));
					var matchLinkHeaderPKs = new HashSet<ZGuid>(matchLinks.Select(m => m.AP_AH));
					var unmatchedJournal = journals.FirstOrDefault(j => !matchLinkHeaderPKs.Contains(j.PK));
					return unmatchedJournal ?? journals.First();
				}
			}

			return null;
		}

		string ReversingOfTNFTAPNotAllowedMessage(Journal oppositeJournal)
		{
			var commonMessage = Res.GetString("27291941-4b6c-48da-bbee-2aad15e594d3", @"This TAP/TNF Journal cannot be reversed.
TAP/TNF journals are created in a pair. Reversing one part will result in an imbalance in the Clearing GL Account.");

			if (oppositeJournal == null)
			{
				return commonMessage;
			}

			if (IsPartOfMultipleReversing)
			{
				return Res.GetString("b0b90ba9-36f8-437f-b104-6c6e82d736e3", @"{0}

This pair consists of this journal {1} and the corresponding journal {2}.

Instead of reversing, you can do this instead: Ensure that both journals of the pair are unmatched from other transactions. 
Then match the pair of journals together.",
				commonMessage,
				((AccTransactionHeader)OriginalTransaction).AH_TransactionNum,
				oppositeJournal.AH_TransactionNum);
			}
			else
			{
				if (oppositeJournal.LatestMatchLink != null)
				{
					return Res.GetString("7266e8de-68b7-4b8e-96e2-e209cc6faa11", @"{0}

The corresponding journal {1} is currently matched in match group number {2}.
Please review the match group before deciding on the next course of action.

If the Journals are accidentally created, un-match match group number {2} and match off the pair of TAP/TNF journals in a new match session.
If the Journals are overpayment of which a refund is claimed, you can considered matching it off against Receipt/Payment transaction.
If the Journals are overpayment of which no refund is claimed, you can considered matching it off against Overpayment transaction.",
					commonMessage,
					oppositeJournal.AH_TransactionNum,
					oppositeJournal.LatestMatchLink.AP_MatchGroupNum);
				}
				else
				{
					return Res.GetString("78388418-5435-4823-90f3-eae8d4ff3aca", @"{0}

The corresponding journal {1} is currently not matched.
If the journals are accidentally created, the pair can be matched off.",
					commonMessage,
					oppositeJournal.AH_TransactionNum);
				}
			}
		}

		bool IsTAPorTNFTransaction()
		{
			return OriginalPayablesAndReceivables.TransactionCategory == Constants.TransactionCategory.Codes.TransactionNotFound ||
				OriginalPayablesAndReceivables.TransactionCategory == Constants.TransactionCategory.Codes.TransactionAlreadyPaid;
		}

		protected TransactionMatchLinkCollection OriginalTransactionMatchLinks;
		protected TransactionMatchLinkCollection ReverseTransactionMatchLinks;
	}
}

#region Test

// See Journal.cs for some of the tests for this class: i.e. TestTransactionWithIReversing_Standard and TestTransactionWithIReversing_TransactionAlreadyPaid

#endregion 
