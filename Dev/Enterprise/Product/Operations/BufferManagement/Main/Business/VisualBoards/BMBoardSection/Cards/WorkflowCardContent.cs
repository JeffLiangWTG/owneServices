using System.Drawing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using static Enterprise.BufferManagement.Business.TaskJobWorkflowCacheHelper;

namespace Enterprise.BufferManagement.Business
{
	public class WorkflowCardContent : CardContentBase<ProcessHeader>
	{
		public WorkflowCardContent(ProcessHeader workflow, ProcessTask task, BMBoardSectionViewModel viewModel)
			: base(workflow, task, workflow, viewModel)
		{
			this.task = task;
		}

		readonly ProcessTask task;

		public override ProcessTask Task
		{
			get { return task; }
		}

		public override ProcessHeader Workflow
		{
			get { return Parent; }
		}

		public override ZString NoteText
		{
			get { return ZString.Empty; }
		}

		public override bool IsCurrent
		{
			get { return !Cache.GetCachedValue<bool>(TaskIdentifier, CacheConstants.WorkflowHasOpenPrerequisites, () => Workflow.HasOpenPrerequisites); }
		}

		public override CardType CardType
		{
			get { return CardType.Workflow; }
		}

		public override Color BorderColor
		{
			get { return GetBorderColor(this); }
		}

		public static Color GetBorderColor(ICardContent cardContent)
		{
			return TagProvider.GetBorderColor(cardContent.Definitions, cardContent.ApplicableTagMagnitudes) ?? Color.Black;
		}

		public override SizedButtonBorderStyle BorderStyle
		{
			get { return GetBorderStyle(this); }
		}

		public static SizedButtonBorderStyle GetBorderStyle(ICardContent cardContent)
		{
			return TagProvider.GetBorderStyle(cardContent.Definitions, cardContent.ApplicableTagMagnitudes) ?? SizedButtonBorderStyle.Default;
		}
	}
}
