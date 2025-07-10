using System;
using CargoWise.Common;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobDeclarationConsolidatedEntryProvider : Customs.Business.JobDeclarationConsolidatedEntryProvider
	{
		public JobDeclarationConsolidatedEntryProvider(BaseJobDeclaration declaration, IConsolidatedEntryDeclarationRemover consolidatedEntryDeclarationRemover = null) : base(declaration, consolidatedEntryDeclarationRemover)
		{
		}

		new JobDeclaration declaration => (JobDeclaration)base.declaration;

		protected override IDisposable BeforeQueueForConsolidationValidation()
		{
			return new DisposableAction(SuspendQuestionsValidation, ResumeQuestionsValidation);
		}

		void SuspendQuestionsValidation()
		{
			var entryHeader = declaration.EntryHeader;
			if (entryHeader?.Questions != null)
			{
				foreach (var entryQuestion in entryHeader.Questions)
				{
					entryQuestion.Reload(); // Workaround to clear existing notifications because ClearAllNotifications() throws an exception.
					entryQuestion.SuspendValidation();
				}
			}
		}

		void ResumeQuestionsValidation()
		{
			declaration.EntryHeader?.Questions?.ResumeValidation();
		}
	}
}
