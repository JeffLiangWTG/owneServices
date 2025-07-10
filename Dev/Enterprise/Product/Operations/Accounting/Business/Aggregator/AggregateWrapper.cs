using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Aggregator
{
	public class AggregateWrapper : SaveInTransactionActionWithMainConnection
	{
		public event EventHandler AllFactoriesSavedSuccessfully;

		public AggregateWrapper(GLJournal journal, GLJournal originalJournal)
		{
			this.Journal = journal;
			this.OriginalJournal = originalJournal;
		}

		protected override void OnAllTransactionsCommitted(IChangedTableNames changedTableNames)
		{
			RaiseAllFactoriesSavedSuccessfully();
		}

		protected override IChangedTableNames SaveInTransaction()
		{
			Aggregate();
			return new ChangedTableNames(new[] { AccGLAggregateSchema.Constants.TableName });
		}

		protected void RaiseAllFactoriesSavedSuccessfully()
		{
			AllFactoriesSavedSuccessfully?.Invoke(this, EventArgs.Empty);
		}

		#region Implementation

		protected GLJournal Journal;
		protected GLJournal OriginalJournal;

		protected GLBaseOnlineAggregator Aggregator;
		protected GLBaseOnlineAggregator OriginalAggregator;

		protected bool fLastSaveSucceeded;

		protected bool JournalTypeChanged
		{
			get { return OriginalJournal != null && (Journal.AH_TransactionType != OriginalJournal.AH_TransactionType); }
		}

		public void AggregateNew()
		{
			switch (Journal.AH_TransactionType)
			{
				case TransactionTypes.GLStandardJournal:
				case TransactionTypes.GLNoteJournal:
					Aggregator = new GLGeneralOnlineAggregator(Journal, Journal);
					break;
				case TransactionTypes.GLReversingJournal:
					Aggregator = new GLReverseOnlineAggregator(Journal, Journal);
					break;
				case TransactionTypes.GLAutoJournal:
					Aggregator = new GLAutoOnlineAggregator(Journal, Journal);
					break;
				default:
					Aggregator = null;
					break;
			}

			Aggregator?.AggregateNew();
		}

		protected void Aggregate()
		{
			if (JournalTypeChanged)
			{
				switch (OriginalJournal.AH_TransactionType)
				{
					case TransactionTypes.GLStandardJournal:
					case TransactionTypes.GLNoteJournal:
						OriginalAggregator = new GLGeneralOnlineAggregator(OriginalJournal, OriginalJournal);
						break;
					case TransactionTypes.GLReversingJournal:
						OriginalAggregator = new GLReverseOnlineAggregator(OriginalJournal, OriginalJournal);
						break;
					case TransactionTypes.GLAutoJournal:
						OriginalAggregator = new GLAutoOnlineAggregator(OriginalJournal, OriginalJournal);
						break;
					default:
						OriginalAggregator = null;
						break;
				}

				switch (Journal.AH_TransactionType)
				{
					case TransactionTypes.GLStandardJournal:
					case TransactionTypes.GLNoteJournal:
						Aggregator = new GLGeneralOnlineAggregator(Journal, Journal);
						break;
					case TransactionTypes.GLReversingJournal:
						Aggregator = new GLReverseOnlineAggregator(Journal, Journal);
						break;
					case TransactionTypes.GLAutoJournal:
						Aggregator = new GLAutoOnlineAggregator(Journal, Journal);
						break;
					default:
						Aggregator = null;
						break;
				}

				OriginalAggregator?.AggregateReverse();
				Aggregator?.AggregateNew();
			}
			else
			{
				switch (Journal.AH_TransactionType)
				{
					case TransactionTypes.GLStandardJournal:
					case TransactionTypes.GLNoteJournal:
						Aggregator = new GLGeneralOnlineAggregator(Journal, OriginalJournal);
						break;
					case TransactionTypes.GLReversingJournal:
						Aggregator = new GLReverseOnlineAggregator(Journal, OriginalJournal);
						break;
					case TransactionTypes.GLAutoJournal:
						Aggregator = new GLAutoOnlineAggregator(Journal, OriginalJournal);
						break;
					default:
						Aggregator = null;
						break;
				}

				Aggregator?.Aggregate();
			}
		}

		#endregion
	}
}
