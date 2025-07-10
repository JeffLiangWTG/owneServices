using System;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;

namespace Enterprise.Accounting.Business.Aggregator
{
	public class GLGeneralOnlineAggregator : GLBaseOnlineAggregator
	{
		public GLGeneralOnlineAggregator(GLJournal gL, GLJournal originalGL)
			: base(gL, originalGL)
		{
		}

		#region Implementation

		protected void ConstructSQLStatements(AggregatorDbCommandFactory dbCommandFactory, GLJournal gL)
		{
			for (int lineNo = 0; lineNo < gL.GLJournalLines.Count; lineNo++)
			{
				dbCommandFactory.AppendQueryLine(lineNo, GetQueryPerLine(lineNo, lineNo,
					gL.GLJournalLines[lineNo], gL.GLJournalLines[lineNo], BizOState.Added));
			}
		}

		protected void ConstructSQLStatements(AggregatorDbCommandFactory dbCommandFactory, int maxLineNo, GLJournal gL, GLJournal originalGL)
		{
			BizOState lineState = BizOState.Unchanged;

			int newLineNo = 0;
			int lineNo = 0;

			while (lineNo < maxLineNo)
			{
				if (lineNo < originalGL.GLJournalLines.Count)
				{
					lineState = GetLineState(gL, originalGL.GLJournalLines[lineNo]);

					if (lineState != BizOState.Deleted)
					{
						dbCommandFactory.AppendQueryLine(lineNo, GetQueryPerLine(lineNo, lineNo,
							gL.GLJournalLines[newLineNo], originalGL.GLJournalLines[lineNo], lineState));
						newLineNo++;
					}
					else
					{
						dbCommandFactory.AppendQueryLine(lineNo, GetQueryPerLine(lineNo, lineNo,
							originalGL.GLJournalLines[lineNo], originalGL.GLJournalLines[lineNo], lineState));
						maxLineNo++;
					}
				}
				else
				{
					if (newLineNo < gL.GLJournalLines.Count)
					{
						dbCommandFactory.AppendQueryLine(lineNo, GetQueryPerLine(lineNo, lineNo,
							gL.GLJournalLines[newLineNo], gL.GLJournalLines[newLineNo], BizOState.Added));
						newLineNo++;
					}
				}

				lineNo++;
			}
		}

		protected void SetSQLParams(AggregatorDbCommandFactory dbCommandFactory, GLJournal gL, decimal amountMultiplier)
		{
			for (int lineNo = 0; lineNo < gL.GLJournalLines.Count; lineNo++)
			{
				SetQueryParametersPerLine(dbCommandFactory, lineNo, gL.GLJournalLines[lineNo].AL_AG.ToGuid(),
					gL.GLJournalLines[lineNo].AL_PostDatePeriod,
					gL.GLJournalLines[lineNo].AL_GB.ToGuid(),
					gL.GLJournalLines[lineNo].AL_GC.ToGuid(),
					gL.GLJournalLines[lineNo].AL_GE.ToGuid(),
					gL.GLJournalLines[lineNo].AL_LocalExTaxAmount * amountMultiplier,
					gL.AH_TransactionCategory);
			}
		}

		protected void SetSQLParams(AggregatorDbCommandFactory dbCommandFactory, int maxLineNo, GLJournal gL, GLJournal originalGL)
		{
			#region Local Var
			BizOState lineState = BizOState.Unchanged;
			int newLineNo = 0;
			int lineNo = 0;

			decimal originalLineAmount = 0M;
			Guid originalBranch = Guid.Empty;
			Guid originalCompany = Guid.Empty;
			Guid originalDept = Guid.Empty;
			Guid originalAccount = Guid.Empty;
			int originalPostPeriod = 0;
			string originalCategory = "";

			decimal lineAmount = 0M;
			Guid branch = Guid.Empty;
			Guid company = Guid.Empty;
			Guid dept = Guid.Empty;
			Guid account = Guid.Empty;
			int postPeriod = 0;
			string category = "";
			#endregion
			newLineNo = 0;

			while (lineNo < maxLineNo)
			{
				if (lineNo < originalGL.GLJournalLines.Count)
				{
					lineState = GetLineState(gL, originalGL.GLJournalLines[lineNo]);

					#region Temp Var Setup
					originalLineAmount = originalGL.GLJournalLines[lineNo].AL_LocalExTaxAmount;
					originalBranch = originalGL.GLJournalLines[lineNo].AL_GB.ToGuid();
					originalCompany = originalGL.GLJournalLines[lineNo].AL_GC.ToGuid();
					originalDept = originalGL.GLJournalLines[lineNo].AL_GE.ToGuid();
					originalAccount = originalGL.GLJournalLines[lineNo].AL_AG.ToGuid();
					originalPostPeriod = originalGL.GLJournalLines[lineNo].AL_PostDatePeriod;
					originalCategory = originalGL.AH_TransactionCategory;

					if (lineState != BizOState.Deleted)
					{
						lineAmount = gL.GLJournalLines[newLineNo].AL_LocalExTaxAmount;
						branch = gL.GLJournalLines[newLineNo].AL_GB.ToGuid();
						company = gL.GLJournalLines[newLineNo].AL_GC.ToGuid();
						dept = gL.GLJournalLines[newLineNo].AL_GE.ToGuid();
						account = gL.GLJournalLines[newLineNo].AL_AG.ToGuid();
						postPeriod = gL.GLJournalLines[newLineNo].AL_PostDatePeriod;
						category = gL.AH_TransactionCategory;
					}

					#endregion

					if (lineState == BizOState.Added)
					{
						SetQueryParametersPerLine(dbCommandFactory, lineNo, account, postPeriod, branch, company, dept, lineAmount, category);
					}
					else if (lineState == BizOState.Deleted)
					{
						SetQueryParametersPerLine(dbCommandFactory, lineNo, originalAccount, originalPostPeriod, originalBranch, originalCompany, originalDept, -originalLineAmount, originalCategory);
					}
					else if (lineState == BizOState.Modified)
					{
						if (HasNaturalKeyChanged(gL.GLJournalLines[newLineNo], originalGL.GLJournalLines[lineNo]))
						{
							SetQueryParametersPerLine(dbCommandFactory, lineNo, originalAccount, originalPostPeriod, originalBranch, originalCompany, originalDept, -originalLineAmount, originalCategory);
							SetQueryParametersPerNewLine(dbCommandFactory, lineNo, account, postPeriod, branch, company, dept, lineAmount, category);
						}
						else
						{
							SetQueryParametersPerLine(dbCommandFactory, lineNo, account, postPeriod, branch, company, dept, lineAmount - originalLineAmount, category);
						}
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
						category = gL.AH_TransactionCategory;
						#endregion

						SetQueryParametersPerLine(dbCommandFactory, lineNo, account, postPeriod, branch, company, dept, lineAmount, category);
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

		#endregion
	}
}