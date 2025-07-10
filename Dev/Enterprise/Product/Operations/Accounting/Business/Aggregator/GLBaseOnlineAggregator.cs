using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Aggregator
{
	public enum BizOState { Unchanged, Added, Modified, Deleted }

	public abstract class GLBaseOnlineAggregator
	{
		public readonly int MaxLineSize = 10;

		protected GLBaseOnlineAggregator(GLJournal gL, GLJournal originalGL)
		{
			this.GL = gL;
			this.OriginalGL = originalGL;
		}

		public void Aggregate()
		{
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			GenerateUpdateCommandsWithBizO(dbCommandFactory, GL, OriginalGL);
			dbCommandFactory.ExecuteNonEmptyCommands();
		}

		public void AggregateReverse()
		{
			var gLList = SplitJournal(GL, MaxLineSize);
			foreach (GLJournal journal in gLList)
			{
				AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
				GenerateReverseCommandsWithBizO(dbCommandFactory, journal);
				dbCommandFactory.ExecuteNonEmptyCommands();
			}
		}

		public void AggregateNew()
		{
			var gLList = SplitJournal(GL, MaxLineSize);
			foreach (GLJournal journal in gLList)
			{
				AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
				GenerateNewCommandsWithBizO(dbCommandFactory, journal);
				dbCommandFactory.ExecuteNonEmptyCommands();
			}
		}

		#region Implementation

		readonly GLJournal GL;
		readonly GLJournal OriginalGL;

		protected virtual string GetQueryPerLine(int lineNo, int lineToReverse, GLJournalLine line, GLJournalLine originalLine, BizOState state)
		{
			string sQL = "";

			if (state != BizOState.Unchanged)
			{
				if (state != BizOState.Deleted && HasNaturalKeyChanged(line, originalLine))
				{
					sQL = " INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
						" VALUES (newid(), @LineAmount" + lineToReverse + ", @PostPeriod" + lineToReverse + ", @GLAccountPK" + lineToReverse + ", @BranchPK" + lineToReverse + ", @CompanyPK" + lineToReverse + ", @DepartmentPK" + lineToReverse + ", @TransactionCategory" + lineToReverse + ") " +
						" ; " +
						" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
						" VALUES (newid(), @NewLineAmount" + lineNo + ", @NewPostPeriod" + lineNo + ", @NewGLAccountPK" + lineNo + ", @NewBranchPK" + lineNo + ", @NewCompanyPK" + lineNo + ", @NewDepartmentPK" + lineNo + ", @NewTransactionCategory" + lineNo + ") " +
						" ; ";
				}
				else
				{
					sQL = " INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
						" VALUES (newid(), @LineAmount" + lineNo + ", @PostPeriod" + lineNo + ", @GLAccountPK" + lineNo + ", @BranchPK" + lineNo + ", @CompanyPK" + lineNo + ", @DepartmentPK" + lineNo + ", @TransactionCategory" + lineNo + ") " +
						" ; ";
				}
			}
			return sQL;
		}

		protected virtual void SetQueryParametersPerLine(AggregatorDbCommandFactory dbCommandFactory, int lineNo, Guid aL_AG, int aL_PostPeriod, Guid aL_GB, Guid aL_GC, Guid aL_GE, decimal aL_LineAmount, string aH_TransactionCategory)
		{
			SetQueryParametersPerLine(dbCommandFactory, lineNo, lineNo, aL_AG, aL_PostPeriod, aL_GB, aL_GC, aL_GE, aL_LineAmount, aH_TransactionCategory);
		}

		protected virtual void SetQueryParametersPerLine(AggregatorDbCommandFactory dbCommandFactory, int trueLineNo, int lineNo, Guid aL_AG, int aL_PostPeriod, Guid aL_GB, Guid aL_GC, Guid aL_GE, decimal aL_LineAmount, string aH_TransactionCategory)
		{
			DbCommand command = dbCommandFactory.GetCommandForLine(trueLineNo);
			command.AddParameterBasedOnDbColumn("@GLAccountPK" + lineNo, aL_AG, AccTransactionLinesSchema.AL_AG);
			command.AddParameterBasedOnDbColumn("@PostPeriod" + lineNo, aL_PostPeriod, AccTransactionLinesSchema.AL_PostPeriod);

			if (aL_GB != Guid.Empty)
			{
				command.AddParameter("@BranchPK" + lineNo, SqlDbType.UniqueIdentifier, aL_GB);
			}
			else
			{
				command.AddParameter("@BranchPK" + lineNo, SqlDbType.UniqueIdentifier, DBNull.Value);
			}

			if (aL_GC != Guid.Empty)
			{
				command.AddParameter("@CompanyPK" + lineNo, SqlDbType.UniqueIdentifier, aL_GC);
			}
			else
			{
				command.AddParameter("@CompanyPK" + lineNo, SqlDbType.UniqueIdentifier, DBNull.Value);
			}

			if (aL_GE != Guid.Empty)
			{
				command.AddParameter("@DepartmentPK" + lineNo, SqlDbType.UniqueIdentifier, aL_GE);
			}
			else
			{
				command.AddParameter("@DepartmentPK" + lineNo, SqlDbType.UniqueIdentifier, DBNull.Value);
			}

			command.AddParameter("@LineAmount" + lineNo, SqlDbType.Money, aL_LineAmount);
			command.AddParameterBasedOnDbColumn("@TransactionCategory" + lineNo, aH_TransactionCategory, AccTransactionHeaderSchema.AH_TransactionCategory);
		}

		protected virtual void SetQueryParametersPerLine(AggregatorDbCommandFactory dbCommandFactory, int lineNo, GLJournal journal)
		{
			DbCommand command = dbCommandFactory.GetCommandForLine(lineNo);
			command.AddParameter("@GLAccountPK" + lineNo, SqlDbType.UniqueIdentifier, journal.GLJournalLines[lineNo].AL_AG);
			command.AddParameter("@PostPeriod" + lineNo, SqlDbType.Int, journal.GLJournalLines[lineNo].AL_PostDatePeriod);

			if (journal.GLJournalLines[lineNo].AL_GB != Guid.Empty)
			{
				command.AddParameter("@BranchPK" + lineNo, SqlDbType.UniqueIdentifier, journal.GLJournalLines[lineNo].AL_GB);
			}
			else
			{
				command.AddParameter("@BranchPK" + lineNo, SqlDbType.UniqueIdentifier, DBNull.Value);
			}

			if (journal.GLJournalLines[lineNo].AL_GC != Guid.Empty)
			{
				command.AddParameter("@CompanyPK" + lineNo, SqlDbType.UniqueIdentifier, journal.GLJournalLines[lineNo].AL_GC);
			}
			else
			{
				command.AddParameter("@CompanyPK" + lineNo, SqlDbType.UniqueIdentifier, DBNull.Value);
			}

			if (journal.GLJournalLines[lineNo].AL_GE != Guid.Empty)
			{
				command.AddParameter("@DepartmentPK" + lineNo, SqlDbType.UniqueIdentifier, journal.GLJournalLines[lineNo].AL_GE);
			}
			else
			{
				command.AddParameter("@DepartmentPK" + lineNo, SqlDbType.UniqueIdentifier, DBNull.Value);
			}

			command.AddParameter("@LineAmount" + lineNo, SqlDbType.Money, journal.GLJournalLines[lineNo].AL_LocalExTaxAmount);
			command.AddParameter("@TransactionCategory" + lineNo, SqlDbType.VarChar, journal.AH_TransactionCategory);
		}

		protected virtual void SetQueryParametersPerNewLine(AggregatorDbCommandFactory dbCommandFactory, int lineNo, Guid aL_AG, int aL_PostPeriod, Guid aL_GB, Guid aL_GC, Guid aL_GE, decimal aL_LineAmount, string aH_TransactionCategory)
		{
			SetQueryParametersPerNewLine(dbCommandFactory, lineNo, lineNo, aL_AG, aL_PostPeriod, aL_GB, aL_GC, aL_GE, aL_LineAmount, aH_TransactionCategory);
		}

		protected virtual void SetQueryParametersPerNewLine(AggregatorDbCommandFactory dbCommandFactory, int trueLineNo, int lineNo, Guid aL_AG, int aL_PostPeriod, Guid aL_GB, Guid aL_GC, Guid aL_GE, decimal aL_LineAmount, string aH_TransactionCategory)
		{
			DbCommand command = dbCommandFactory.GetCommandForLine(trueLineNo);
			command.AddParameter("@NewGLAccountPK" + lineNo, SqlDbType.UniqueIdentifier, aL_AG);
			command.AddParameter("@NewPostPeriod" + lineNo, SqlDbType.Int, aL_PostPeriod);

			if (aL_GB != Guid.Empty)
			{
				command.AddParameter("@NewBranchPK" + lineNo, SqlDbType.UniqueIdentifier, aL_GB);
			}
			else
			{
				command.AddParameter("@NewBranchPK" + lineNo, SqlDbType.UniqueIdentifier, DBNull.Value);
			}

			if (aL_GC != Guid.Empty)
			{
				command.AddParameter("@NewCompanyPK" + lineNo, SqlDbType.UniqueIdentifier, aL_GC);
			}
			else
			{
				command.AddParameter("@NewCompanyPK" + lineNo, SqlDbType.UniqueIdentifier, DBNull.Value);
			}

			if (aL_GE != Guid.Empty)
			{
				command.AddParameter("@NewDepartmentPK" + lineNo, SqlDbType.UniqueIdentifier, aL_GE);
			}
			else
			{
				command.AddParameter("@NewDepartmentPK" + lineNo, SqlDbType.UniqueIdentifier, DBNull.Value);
			}

			command.AddParameter("@NewLineAmount" + lineNo, SqlDbType.Money, aL_LineAmount);
			command.AddParameterBasedOnDbColumn("@NewTransactionCategory" + lineNo, aH_TransactionCategory, AccTransactionHeaderSchema.AH_TransactionCategory);
		}

		protected BizOState GetLineState(GLJournal currentGL, GLJournalLine originalLineToCompare)
		{
			BizOState lineState = BizOState.Unchanged;

			if (!currentGL.GLJournalLines.Contains(originalLineToCompare.PK))
			{
				lineState = BizOState.Deleted;
			}
			else
			{
				GLJournalLine currentLine = currentGL.GLJournalLines.FindByPK(originalLineToCompare.PK) as GLJournalLine;

				if (!currentLine.IsInDatabase)
				{
					lineState = BizOState.Added;
				}
				else if (currentLine.HasChanges)
				{
					lineState = BizOState.Modified;
				}
				else
				{
					lineState = BizOState.Unchanged;
				}
			}
			return lineState;
		}

		protected bool HasNaturalKeyChanged(GLJournalLine line, GLJournalLine originalLine)
		{
			int reversePeriod = 0;
			int originalReversePeriod = 0;

			// to cater for possible null date. if to dates are null, two periods are considered to be equal.
			if (!line.AL_ReverseDate.IsEmpty)
			{
				reversePeriod = line.AL_ReverseDatePeriod;
			}

			if (!originalLine.AL_ReverseDate.IsEmpty)
			{
				originalReversePeriod = originalLine.AL_ReverseDatePeriod;
			}

			bool result = (
				line.AL_AG != originalLine.AL_AG ||
				line.AL_PostDatePeriod != originalLine.AL_PostDatePeriod ||
				line.AL_GB != originalLine.AL_GB ||
				line.AL_GE != originalLine.AL_GE ||
				reversePeriod != originalReversePeriod);

			return result;
		}

		protected int AddToPeriod(int period, int increment)
		{
			int newPeriod = period;

			for (int i = 0; i < increment; i++)
			{
				newPeriod = GL.PeriodCalculator.GetNextPeriod(newPeriod);
			}

			return newPeriod;
		}

		protected static List<GLJournal> SplitJournal(GLJournal journal, int size)
		{
			var splittedJournals = new List<GLJournal>();

			var i = 0;

			if (journal != null)
			{
				while (journal.GLJournalLines.Count > 0)
				{
					if ((i % size) == 0)
					{
						splittedJournals.Add((GLJournal)journal.Clone());
					}

					var splittedJournal = splittedJournals[splittedJournals.Count - 1];

					var lineToAdd = journal.Factory.Load(journal.GLJournalLines[0].GetType(), journal.GLJournalLines[0].PK);
					journal.GLJournalLines.Remove(lineToAdd);
					splittedJournal.GLJournalLines.Add(lineToAdd);

					i++;
				}
			}

			return splittedJournals;
		}

		#region Abstract Methods

		protected internal abstract void GenerateUpdateCommandsWithBizO(AggregatorDbCommandFactory dbCommandFactory, GLJournal gL, GLJournal originalGL);
		protected internal abstract void GenerateReverseCommandsWithBizO(AggregatorDbCommandFactory dbCommandFactory, GLJournal gL);
		protected internal abstract void GenerateNewCommandsWithBizO(AggregatorDbCommandFactory dbCommandFactory, GLJournal gL);

		#endregion

		#endregion
	}
}