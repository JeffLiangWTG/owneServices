using System;
using System.Data;
using System.Text;
using CargoWise.Data;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Aggregator
{
	public class GLAutoOnlineAggregator : GLBaseOnlineAggregator
	{
		public GLAutoOnlineAggregator(GLJournal gL, GLJournal originalGL)
			: base(gL, originalGL)
		{
		}

		#region Implementation

		protected internal override void GenerateReverseCommandsWithBizO(AggregatorDbCommandFactory dbCommandFactory, GLJournal gL)
		{
			ConstructSQLStatements(dbCommandFactory, gL);
			SetSQLParams(dbCommandFactory, gL, -1.0m);
		}

		protected internal override void GenerateNewCommandsWithBizO(AggregatorDbCommandFactory dbCommandFactory, GLJournal gL)
		{
			ConstructSQLStatements(dbCommandFactory, gL);
			SetSQLParams(dbCommandFactory, gL, 1.0m);
		}

		protected void ConstructSQLStatements(AggregatorDbCommandFactory dbCommandFactory, GLJournal gL)
		{
			for (int lineNo = 0; lineNo < gL.GLJournalLines.Count; lineNo++)
			{
				dbCommandFactory.AppendQueryLine(lineNo, GetQueryPerLine(lineNo, gL.GLJournalLines[lineNo], gL.GLJournalLines[lineNo], BizOState.Added, gL.PeriodCalculator));
			}
		}

		protected void SetSQLParams(AggregatorDbCommandFactory dbCommandFactory, GLJournal gL, decimal amountMultiplier)
		{
			for (int lineNo = 0; lineNo < gL.GLJournalLines.Count; lineNo++)
			{
				SetQueryParametersPerLine(dbCommandFactory, lineNo, gL.GLJournalLines[lineNo].AL_AG.ToGuid(),
					gL.GLJournalLines[lineNo].AL_PostDatePeriod,
					gL.GLJournalLines[lineNo].AL_ReverseDatePeriod,
					gL.GLJournalLines[lineNo].AL_GB.ToGuid(),
					gL.GLJournalLines[lineNo].AL_GC.ToGuid(),
					gL.GLJournalLines[lineNo].AL_GE.ToGuid(),
					gL.GLJournalLines[lineNo].AL_LocalExTaxAmount * amountMultiplier,
					gL.AH_TransactionCategory,
					gL.PeriodCalculator);
			}
		}

		protected void ConstructSQLStatements(AggregatorDbCommandFactory dbCommandFactory, int maxLineNo, GLJournal gL, GLJournal originalGL)
		{
			BizOState lineState = BizOState.Unchanged;

			int newLineNo = 0;
			int lineNo = 0;

			//for (int LineNo = 0; LineNo < MaxLineNo; LineNo++)
			while (lineNo < maxLineNo)
			{
				if (lineNo < originalGL.GLJournalLines.Count)
				{
					lineState = GetLineState(gL, originalGL.GLJournalLines[lineNo]);

					//Cater for Boundary Condition
					//					if (NewLineNo == GL.GLJournalLines.Count && GL.GLJournalLines.Count > 0)
					//					{
					//						NewLineNo--;
					//					}

					if (lineState != BizOState.Deleted)
					{
						dbCommandFactory.AppendQueryLine(lineNo, GetQueryPerLine(lineNo,
							gL.GLJournalLines[newLineNo], originalGL.GLJournalLines[lineNo], lineState, gL.PeriodCalculator));
						newLineNo++;
					}
					else
					{
						dbCommandFactory.AppendQueryLine(lineNo, GetQueryPerLine(lineNo,
							originalGL.GLJournalLines[lineNo], originalGL.GLJournalLines[lineNo], lineState, originalGL.PeriodCalculator));
						maxLineNo++;
					}
				}
				else
				{
					if (newLineNo < gL.GLJournalLines.Count)
					{
						dbCommandFactory.AppendQueryLine(lineNo, GetQueryPerLine(lineNo,
							gL.GLJournalLines[newLineNo], gL.GLJournalLines[newLineNo], BizOState.Added, gL.PeriodCalculator));
						newLineNo++;
					}
				}

				lineNo++;
			}
		}

		protected void SetSQLParams(AggregatorDbCommandFactory dbCommandFactory, int maxLineNo, GLJournal gL, GLJournal originalGL)
		{
			#region Temp Var Declaration
			decimal originalLineAmount = 0M;
			Guid originalBranch = Guid.Empty;
			Guid originalCompany = Guid.Empty;
			Guid originalDept = Guid.Empty;
			Guid originalAccount = Guid.Empty;
			int originalReversePeriod = 0;
			int originalPostPeriod = 0;
			string originalCategory = "";

			decimal lineAmount = 0M;
			Guid branch = Guid.Empty;
			Guid company = Guid.Empty;
			Guid dept = Guid.Empty;
			Guid account = Guid.Empty;
			int postPeriod = 0;
			int reversePeriod = 0;
			string category = "";

			int newLineNo = 0;
			int lineNo = 0;
			BizOState lineState = BizOState.Unchanged;
			#endregion

			newLineNo = 0;

			//for (int LineNo = 0; LineNo < MaxLineNo; LineNo++)
			while (lineNo < maxLineNo)
			{
				if (lineNo < originalGL.GLJournalLines.Count)
				{
					lineState = GetLineState(gL, originalGL.GLJournalLines[lineNo]);

					//Cater for Boundary Condition
					//						if (NewLineNo == GL.GLJournalLines.Count)
					//						{
					//							NewLineNo--;
					//						}

					#region Temp Var Setup
					originalLineAmount = originalGL.GLJournalLines[lineNo].AL_LocalExTaxAmount;
					originalBranch = originalGL.GLJournalLines[lineNo].AL_GB.ToGuid();
					originalCompany = originalGL.GLJournalLines[lineNo].AL_GC.ToGuid();
					originalDept = originalGL.GLJournalLines[lineNo].AL_GE.ToGuid();
					originalAccount = originalGL.GLJournalLines[lineNo].AL_AG.ToGuid();
					originalPostPeriod = originalGL.GLJournalLines[lineNo].AL_PostDatePeriod;
					originalReversePeriod = originalGL.GLJournalLines[lineNo].AL_ReverseDatePeriod;
					originalCategory = originalGL.AH_TransactionCategory;

					if (lineState != BizOState.Deleted)
					{
						lineAmount = gL.GLJournalLines[newLineNo].AL_LocalExTaxAmount;
						branch = gL.GLJournalLines[newLineNo].AL_GB.ToGuid();
						company = gL.GLJournalLines[newLineNo].AL_GC.ToGuid();
						dept = gL.GLJournalLines[newLineNo].AL_GE.ToGuid();
						account = gL.GLJournalLines[newLineNo].AL_AG.ToGuid();
						postPeriod = gL.GLJournalLines[newLineNo].AL_PostDatePeriod;
						reversePeriod = gL.GLJournalLines[newLineNo].AL_ReverseDatePeriod;
						category = gL.AH_TransactionCategory;
					}

					#endregion

					switch (lineState)
					{
						case BizOState.Added:
							SetQueryParametersPerLine(dbCommandFactory, lineNo, account, postPeriod, reversePeriod, branch, company, dept, lineAmount, category, gL.PeriodCalculator);
							break;
						case BizOState.Deleted:
							SetQueryParametersPerLine(dbCommandFactory, lineNo, originalAccount, originalPostPeriod, originalReversePeriod, originalBranch, originalCompany, originalDept, -originalLineAmount, originalCategory, originalGL.PeriodCalculator);
							break;
						case BizOState.Modified:
							{
								if (HasNaturalKeyChanged(gL.GLJournalLines[newLineNo], originalGL.GLJournalLines[lineNo]))
								{
									SetQueryParametersPerOriginalLine(dbCommandFactory, lineNo, originalAccount, originalPostPeriod, originalReversePeriod, originalBranch, originalCompany, originalDept, -originalLineAmount, originalCategory, originalGL.PeriodCalculator);
									SetQueryParametersPerNewLine(dbCommandFactory, lineNo, account, postPeriod, reversePeriod, branch, company, dept, lineAmount, category, gL.PeriodCalculator);
								}
								else
								{
									SetQueryParametersPerLine(dbCommandFactory, lineNo, account, postPeriod, reversePeriod, branch, company, dept, lineAmount - originalLineAmount, category, gL.PeriodCalculator);
								}
							}
							break;
						default:
							break;
					}

					if (lineState != BizOState.Deleted)
					{
						newLineNo++;
					}
					else
					{
						maxLineNo++;
					}
				}
				else
				{
					if (newLineNo < gL.GLJournalLines.Count)
					{
						#region Temp Var Setup
						lineAmount = gL.GLJournalLines[newLineNo].AL_LocalExTaxAmount;
						branch = gL.GLJournalLines[newLineNo].AL_GB.ToGuid();
						company = gL.GLJournalLines[newLineNo].AL_GC.ToGuid();
						dept = gL.GLJournalLines[newLineNo].AL_GE.ToGuid();
						account = gL.GLJournalLines[newLineNo].AL_AG.ToGuid();
						postPeriod = gL.GLJournalLines[newLineNo].AL_PostDatePeriod;
						reversePeriod = gL.GLJournalLines[newLineNo].AL_ReverseDatePeriod;
						category = gL.AH_TransactionCategory;
						#endregion

						SetQueryParametersPerLine(dbCommandFactory, lineNo, account, postPeriod, reversePeriod, branch, company, dept, lineAmount, category, gL.PeriodCalculator);
						newLineNo++;
					}
				}

				lineNo++;
			}
		}

		protected internal override void GenerateUpdateCommandsWithBizO(AggregatorDbCommandFactory dbCommandFactory, GLJournal gL, GLJournal originalGL)
		{
			int maxLineNo = gL.GLJournalLines.Count < originalGL.GLJournalLines.Count ? originalGL.GLJournalLines.Count : gL.GLJournalLines.Count;

			ConstructSQLStatements(dbCommandFactory, maxLineNo, gL, originalGL);
			SetSQLParams(dbCommandFactory, maxLineNo, gL, originalGL);
		}

		protected internal int ConcatenateLineNoAndPeriod(int lineNo, int period)
		{
			return lineNo * 1000000 + period;
		}

		protected string GetQueryPerLine(int lineNo, GLJournalLine line, GLJournalLine originalLine, BizOState state, AccountingPeriodCalculator periodCalc)
		{
			int startPeriod = 0;
			int reversePeriod = 0;
			int originalStartPeriod = 0;
			int originalReversePeriod = 0;
			if (state == BizOState.Deleted)
			{
				startPeriod = originalLine.AL_PostDatePeriod;
				reversePeriod = originalLine.AL_ReverseDatePeriod;
			}
			else if (state == BizOState.Modified)
			{
				startPeriod = line.AL_PostDatePeriod;
				originalStartPeriod = originalLine.AL_PostDatePeriod;
				reversePeriod = line.AL_ReverseDatePeriod;
				originalReversePeriod = originalLine.AL_ReverseDatePeriod;
			}
			else if (state == BizOState.Added)
			{
				startPeriod = line.AL_PostDatePeriod;
				reversePeriod = line.AL_ReverseDatePeriod;
			}

			int originalNoOfPeriods = periodCalc.GetPeriodCount(originalStartPeriod, originalReversePeriod);
			int noOfPeriods = periodCalc.GetPeriodCount(startPeriod, reversePeriod);

			StringBuilder queryBuilder = new StringBuilder();

			int originalPeriod = originalStartPeriod;
			int period = startPeriod;

			if (HasNaturalKeyChanged(line, originalLine) && state != BizOState.Deleted)
			{
				for (int i = 0; i < originalNoOfPeriods; i++)
				{
					queryBuilder.Append(GetOriginalQueryPerLine(ConcatenateLineNoAndPeriod(lineNo, originalPeriod)));
					originalPeriod = periodCalc.GetNextPeriod(originalPeriod);
				}

				for (int i = 0; i < noOfPeriods; i++)
				{
					queryBuilder.Append(GetNewQueryPerLine(ConcatenateLineNoAndPeriod(lineNo, period)));
					period = periodCalc.GetNextPeriod(period);
				}
			}
			else
			{
				for (int i = 0; i < noOfPeriods; i++)
				{
					queryBuilder.Append(base.GetQueryPerLine(
						ConcatenateLineNoAndPeriod(lineNo, period),
						ConcatenateLineNoAndPeriod(lineNo, period),
						line, originalLine, state
						));
					period = periodCalc.GetNextPeriod(period);
				}
			}
			return queryBuilder.ToString();
		}

		protected virtual string GetOriginalQueryPerLine(int lineNo)
		{
			string sQL = "";

			sQL = " INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @OriginalLineAmount" + lineNo + ", @OriginalPostPeriod" + lineNo + ", @OriginalGLAccountPK" + lineNo + ", @OriginalBranchPK" + lineNo + ", @OriginalCompanyPK" + lineNo + ", @OriginalDepartmentPK" + lineNo + ", @OriginalTransactionCategory" + lineNo + ") " +
				" ; ";

			return sQL;
		}

		protected virtual string GetNewQueryPerLine(int lineNo)
		{
			string sQL = "";

			sQL = " INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @NewLineAmount" + lineNo + ", @NewPostPeriod" + lineNo + ", @NewGLAccountPK" + lineNo + ", @NewBranchPK" + lineNo + ", @NewCompanyPK" + lineNo + ", @NewDepartmentPK" + lineNo + ", @NewTransactionCategory" + lineNo + ") " +
				" ; ";

			return sQL;
		}

		protected void SetQueryParametersPerLine(AggregatorDbCommandFactory dbCommandFactory, int lineNo, Guid aL_AG, int aL_PostPeriod, int aL_ReversePeriod, Guid aL_GB, Guid aL_GC, Guid aL_GE, decimal aL_LineAmount, string aH_TransactionCategory, AccountingPeriodCalculator periodCalc)
		{
			int noOfPeriods = periodCalc.GetPeriodCount(aL_PostPeriod, aL_ReversePeriod);

			for (int i = 0; i < noOfPeriods; i++)
			{
				base.SetQueryParametersPerLine(dbCommandFactory, lineNo, ConcatenateLineNoAndPeriod(lineNo, AddToPeriod(aL_PostPeriod, i)), aL_AG, AddToPeriod(aL_PostPeriod, i), aL_GB, aL_GC, aL_GE, aL_LineAmount, aH_TransactionCategory);
			}
		}

		protected void SetQueryParametersPerNewLine(AggregatorDbCommandFactory dbCommandFactory, int lineNo, Guid aL_AG, int aL_PostPeriod, int aL_ReversePeriod, Guid aL_GB, Guid aL_GC, Guid aL_GE, decimal aL_LineAmount, string aH_TransactionCategory, AccountingPeriodCalculator periodCalc)
		{
			int noOfPeriods = periodCalc.GetPeriodCount(aL_PostPeriod, aL_ReversePeriod);

			for (int i = 0; i < noOfPeriods; i++)
			{
				base.SetQueryParametersPerNewLine(dbCommandFactory, lineNo, ConcatenateLineNoAndPeriod(lineNo, AddToPeriod(aL_PostPeriod, i)), aL_AG, AddToPeriod(aL_PostPeriod, i), aL_GB, aL_GC, aL_GE, aL_LineAmount, aH_TransactionCategory);
			}
		}

		protected void SetQueryParametersPerOriginalLine(AggregatorDbCommandFactory dbCommandFactory, int lineNo, Guid aL_AG, int aL_PostPeriod, int aL_ReversePeriod, Guid aL_GB, Guid aL_GC, Guid aL_GE, decimal aL_LineAmount, string aH_TransactionCategory, AccountingPeriodCalculator periodCalc)
		{
			int noOfPeriods = periodCalc.GetPeriodCount(aL_PostPeriod, aL_ReversePeriod);
			int newLineNo = 0;
			DbCommand command = dbCommandFactory.GetCommandForLine(lineNo);

			for (int i = 0; i < noOfPeriods; i++)
			{
				newLineNo = ConcatenateLineNoAndPeriod(lineNo, AddToPeriod(aL_PostPeriod, i));
				command.AddParameter("@OriginalGLAccountPK" + newLineNo, SqlDbType.UniqueIdentifier, aL_AG);
				command.AddParameter("@OriginalPostPeriod" + newLineNo, SqlDbType.Int, AddToPeriod(aL_PostPeriod, i));
				command.AddParameter("@OriginalBranchPK" + newLineNo, SqlDbType.UniqueIdentifier, aL_GB);
				command.AddParameter("@OriginalCompanyPK" + newLineNo, SqlDbType.UniqueIdentifier, aL_GC);
				command.AddParameter("@OriginalDepartmentPK" + newLineNo, SqlDbType.UniqueIdentifier, aL_GE);
				command.AddParameter("@OriginalLineAmount" + newLineNo, SqlDbType.Money, aL_LineAmount);
				command.AddParameterBasedOnDbColumn("@OriginalTransactionCategory" + newLineNo, aH_TransactionCategory, AccTransactionHeaderSchema.AH_TransactionCategory);
			}
		}

		#endregion
	}
}