using System.Drawing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public class PopulateTaskCardStrategy : PopulateCardContentStrategy
	{
		protected internal override ZGuid GetIdentifier(ProcessTask task, ProcessHeader workflow)
		{
			return task.PK;
		}

		protected internal override CardType CardType
		{
			get { return CardType.Task; }
		}

		protected internal override ZString GetNoteText(ProcessTask task, PropertyCache cache) => task.VisualBoardNoteText;

		protected internal override bool GetIsCurrent(ICardContent cardContent, BMBoardSectionViewModel viewModel)
		{
			return viewModel.Cache.GetCachedValue<bool>(cardContent.Identifier, TaskJobWorkflowCacheHelper.CacheConstants.IsCurrent);
		}

		protected internal override SizedButtonBorderStyle GetBorderStyle(ICardContent cardContent, ProcessTask task, ProcessHeader workflow, BMBoardSectionViewModel viewModel)
		{
			return TaskCardContent.GetBorderStyle(task, workflow, cardContent, viewModel, viewModel.Cache);
		}

		protected internal override Color GetBorderColor(ICardContent cardContent, ProcessTask task, ProcessHeader workflow, BMBoardSectionViewModel viewModel)
		{
			return TaskCardContent.GetBorderColor(task, workflow, cardContent, viewModel, viewModel.Cache);
		}
	}
}
