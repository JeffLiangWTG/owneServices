using System;
using System.Data;
using CargoWise.Data;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;

namespace Enterprise.Accounting.Business.Aggregator
{
	public class GLReverseOnlineAggregator : GLBaseOnlineAggregator
	{
		public GLReverseOnlineAggregator(GLJournal gL, GLJournal originalGL)
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
				dbCommandFactory.AppendQueryLine(lineNo, GetQueryPerLine(lineNo, gL.GLJournalLines[lineNo], gL.GLJournalLines[lineNo], BizOState.Added));
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
				SetExtraQueryParametersPerLine(dbCommandFactory, lineNo, gL.GLJournalLines[lineNo].AL_ReverseDatePeriod,
					-gL.GLJournalLines[lineNo].AL_LocalExTaxAmount * amountMultiplier);
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
					//					if (NewLineNo == GL.GLJournalLines.Count)
					//					{	
					//						NewLineNo--;
					//					}

					if (lineState != BizOState.Deleted)
					{
						dbCommandFactory.AppendQueryLine(lineNo, GetQueryPerLine(lineNo, gL.GLJournalLines[newLineNo], originalGL.GLJournalLines[lineNo], lineState));
						newLineNo++;
					}
					else
					{
						dbCommandFactory.AppendQueryLine(lineNo, GetQueryPerLine(lineNo, originalGL.GLJournalLines[lineNo], originalGL.GLJournalLines[lineNo], lineState));
						maxLineNo++;
					}
				}
				else
				{
					if (newLineNo < gL.GLJournalLines.Count)
					{
						dbCommandFactory.AppendQueryLine(lineNo, GetQueryPerLine(lineNo, gL.GLJournalLines[newLineNo], gL.GLJournalLines[newLineNo], BizOState.Added));
						newLineNo++;
					}
				}

				lineNo++;
			}
		}

		protected void SetSQLParams(AggregatorDbCommandFactory dbCommandFactory, int maxLineNo, GLJournal gL, GLJournal originalGL)
		{
			#region Local Var Declaration
			decimal originalLineAmount = 0M;
			Guid originalBranch = Guid.Empty;
			Guid originalCompany = Guid.Empty;
			Guid originalDept = Guid.Empty;
			Guid originalAccount = Guid.Empty;
			int originalPostPeriod = 0;
			int originalReversePeriod = 0;
			string originalCategory = "";

			decimal lineAmount = 0M;
			Guid branch = Guid.Empty;
			Guid company = Guid.Empty;
			Guid dept = Guid.Empty;
			Guid account = Guid.Empty;
			int postPeriod = 0;
			int reversePeriod = 0;
			string category = "";
			BizOState lineState = BizOState.Unchanged;
			#endregion

			int newLineNo = 0;
			int lineNo = 0;

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
							SetQueryParametersPerLine(dbCommandFactory, lineNo, account, postPeriod, branch, company, dept, lineAmount, category);
							SetExtraQueryParametersPerLine(dbCommandFactory, lineNo, reversePeriod, -lineAmount);
							break;
						case BizOState.Deleted:
							SetQueryParametersPerLine(dbCommandFactory, lineNo, originalAccount, originalPostPeriod, originalBranch, originalCompany, originalDept, -originalLineAmount, originalCategory);
							SetExtraQueryParametersPerLine(dbCommandFactory, lineNo, originalReversePeriod, originalLineAmount);
							break;
						case BizOState.Modified:
							{
								if (HasNaturalKeyChanged(gL.GLJournalLines[newLineNo], originalGL.GLJournalLines[lineNo]))
								{
									SetQueryParametersPerLine(dbCommandFactory, lineNo, originalAccount, originalPostPeriod, originalBranch, originalCompany, originalDept, -originalLineAmount, originalCategory);
									SetQueryParametersPerNewLine(dbCommandFactory, lineNo, account, postPeriod, branch, company, dept, lineAmount, category);
									SetExtraQueryParametersPerLine(dbCommandFactory, lineNo, originalReversePeriod, originalLineAmount);
									SetExtraQueryParametersPerNewLine(dbCommandFactory, lineNo, reversePeriod, -lineAmount);
								}
								else
								{
									SetQueryParametersPerLine(dbCommandFactory, lineNo, account, postPeriod, branch, company, dept, lineAmount - originalLineAmount, category);
									SetExtraQueryParametersPerLine(dbCommandFactory, lineNo, reversePeriod, -(lineAmount - originalLineAmount));
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

						SetQueryParametersPerLine(dbCommandFactory, lineNo, account, postPeriod, branch, company, dept, lineAmount, category);
						SetExtraQueryParametersPerLine(dbCommandFactory, lineNo, reversePeriod, -lineAmount);
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

		protected void SetExtraQueryParametersPerLine(AggregatorDbCommandFactory dbCommandFactory, int lineNo, int aL_ReversePeriod, decimal aL_ReverseLineAmount)
		{
			DbCommand command = dbCommandFactory.GetCommandForLine(lineNo);
			command.AddParameter("@ReversePeriod" + lineNo, SqlDbType.Int, aL_ReversePeriod);
			command.AddParameter("@ReverseLineAmount" + lineNo, SqlDbType.Money, aL_ReverseLineAmount);
		}

		protected void SetExtraQueryParametersPerNewLine(AggregatorDbCommandFactory dbCommandFactory, int lineNo, int aL_ReversePeriod, decimal aL_ReverseLineAmount)
		{
			DbCommand command = dbCommandFactory.GetCommandForLine(lineNo);
			command.AddParameter("@NewReversePeriod" + lineNo, SqlDbType.Int, aL_ReversePeriod);
			command.AddParameter("@NewReverseLineAmount" + lineNo, SqlDbType.Money, aL_ReverseLineAmount);
		}

		protected string GetQueryPerLine(int lineNo, GLJournalLine line, GLJournalLine originalLine, BizOState state)
		{
			string sQL = "";

			if (state != BizOState.Unchanged)
			{
				if (state != BizOState.Deleted && HasNaturalKeyChanged(line, originalLine))
				{
					sQL = " INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
						" VALUES (newid(), @LineAmount" + lineNo + ", @PostPeriod" + lineNo + ", @GLAccountPK" + lineNo + ", @BranchPK" + lineNo + ", @CompanyPK" + lineNo + ", @DepartmentPK" + lineNo + ", @TransactionCategory" + lineNo + ") " +
						" ; " +
						" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
						" VALUES (newid(), @ReverseLineAmount" + lineNo + ", @ReversePeriod" + lineNo + ", @GLAccountPK" + lineNo + ", @BranchPK" + lineNo + ", @CompanyPK" + lineNo + ", @DepartmentPK" + lineNo + ", @TransactionCategory" + lineNo + ") " +
						" ; " +
						" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
						" VALUES (newid(), @NewLineAmount" + lineNo + ", @NewPostPeriod" + lineNo + ", @NewGLAccountPK" + lineNo + ", @NewBranchPK" + lineNo + ", @NewCompanyPK" + lineNo + ", @NewDepartmentPK" + lineNo + ", @NewTransactionCategory" + lineNo + ") " +
						" ; " +
						" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
						" VALUES (newid(), @NewReverseLineAmount" + lineNo + ", @NewReversePeriod" + lineNo + ", @NewGLAccountPK" + lineNo + ", @NewBranchPK" + lineNo + ", @NewCompanyPK" + lineNo + ", @NewDepartmentPK" + lineNo + ", @NewTransactionCategory" + lineNo + ") " +
						" ; ";
				}
				else
				{
					sQL = " INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
						" VALUES (newid(), @LineAmount" + lineNo + ", @PostPeriod" + lineNo + ", @GLAccountPK" + lineNo + ", @BranchPK" + lineNo + ", @CompanyPK" + lineNo + ", @DepartmentPK" + lineNo + ", @TransactionCategory" + lineNo + ") " +
						" ; " +
						" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
						" VALUES (newid(), @ReverseLineAmount" + lineNo + ", @ReversePeriod" + lineNo + ", @GLAccountPK" + lineNo + ", @BranchPK" + lineNo + ", @CompanyPK" + lineNo + ", @DepartmentPK" + lineNo + ", @TransactionCategory" + lineNo + ") " +
						" ; ";
				}
			}
			return sQL;
		}

		#endregion
	}
}