using System.Drawing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public class PopulateWorkflowCardStrategy : PopulateCardContentStrategy
	{
		protected internal override ZGuid GetIdentifier(ProcessTask task, ProcessHeader workflow)
		{
			return workflow.PK;
		}

		protected internal override CardType CardType
		{
			get { return CardType.Workflow; }
		}

		protected internal override ZString GetNoteText(ProcessTask task, PropertyCache cache)
		{
			return ZString.Empty;
		}

		protected internal override bool GetIsCurrent(ICardContent cardContent, BMBoardSectionViewModel viewModel)
		{
			return !viewModel.Cache.GetCachedValue<bool>(cardContent.Identifier, TaskJobWorkflowCacheHelper.CacheConstants.WorkflowHasOpenPrerequisites);
		}

		protected internal override SizedButtonBorderStyle GetBorderStyle(ICardContent cardContent, ProcessTask task, ProcessHeader workflow, BMBoardSectionViewModel viewModel)
		{
			return WorkflowCardContent.GetBorderStyle(cardContent);
		}

		protected internal override Color GetBorderColor(ICardContent cardContent, ProcessTask task, ProcessHeader workflow, BMBoardSectionViewModel viewModel)
		{
			return WorkflowCardContent.GetBorderColor(cardContent);
		}
	}
}
