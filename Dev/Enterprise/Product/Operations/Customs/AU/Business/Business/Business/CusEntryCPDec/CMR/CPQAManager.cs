using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public enum MessageTypesForCPQAGenerator { Original, Amend, Withdraw }

	public class CPQAManager
	{
		public CPQAManager(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		public void GenerateQuestionsForOriginalOrAmendment()
		{
			LineMerger.CacheCPDecQuestions(declaration);
			GenerateQuestions(false, false);
		}

		public void GenerateQuestionsForWithdrawal()
		{
			declaration.CachedQuestions.ClearQuestions();
			GenerateQuestions(true, false);
		}

		public void GenerateQuestionsForConsolidatedEntryOriginalOrAmendment()
		{
			LineMerger.CacheCPDecQuestions(declaration);
			GenerateQuestions(false, true);
		}

		public void GenerateQuestionsForConsolidatedEntryWithdrawal()
		{
			declaration.CachedQuestions.ClearQuestions();
			GenerateQuestions(true, true);
		}

		#region Implementation

		internal void GenerateQuestions(bool isWithdrawal, bool isAggregateDeclaration)
		{
			if (declaration.IsMergeDone)
			{
				if (!declaration.IsSAC && !isWithdrawal)
				{
					GenerateCPDecQuestions();
					foreach (CusEntryHeader entryHeader in declaration.CustomsEntryHeaders)
					{
						entryHeader.AllCPDecQuestions.Load();
						entryHeader.CPDecQuestionsViewCollection.Rebuild();
					}
				}

				if (isAggregateDeclaration || !ConsolidatedDeclaration.IsConsolidated(declaration))
				{
					GenerateLodgementQuestions(isWithdrawal);
				}

				#pragma warning disable IDE0001 // Prevent simplification to base class
				using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
				#pragma warning restore IDE0001 // Prevent simplification to base class
				{
					CMRReferenceFileUpdateLog log = new CMRReferenceFileUpdateLog(declaration.Factory);
					log.LoadLastSuccessfulUpdate();
					declaration.AddInfo.ZA_CPQuestionGenDate_Hidden = log.SuccessfulUpdateFileTimeStamp.ToZDateTime();
				}
			}
		}

		protected
#if DEBUG
 virtual
#endif
 void GenerateCPDecQuestions()
		{
			CMRCPDecQuestionGenerator cPDecGenerator = new CMRCPDecQuestionGenerator(declaration);
			cPDecGenerator.GenerateQuestions();
		}

		protected
#if DEBUG
 virtual
#endif
 void GenerateLodgementQuestions(bool isWithdrawal)
		{
			CMRLodgementQuestionGenerator lodgementGenerator = new CMRLodgementQuestionGenerator(declaration);
			lodgementGenerator.GenerateQuestions(isWithdrawal);
		}

		readonly JobDeclaration declaration;

		#endregion
	}
}
