using System.Drawing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public class TaskCardContent : CardContentBase<ProcessTask>
	{
		public TaskCardContent(ProcessTask task, BMBoardSectionViewModel viewModel)
			: base(task, task, task.GetProcessHeader(), viewModel)
		{
		}

		public override ZString NoteText => Task.IsDeleted ? ZString.Empty : Task.VisualBoardNoteText;
		public override bool IsCurrent => Task.IsStartable(Cache);
		public override ProcessTask Task => Parent;
		public override ProcessHeader Workflow => Parent.IsDeleted ? null : Parent.GetProcessHeader();
		public override CardType CardType => CardType.Task;
		public override Color BorderColor => GetBorderColor(Task, Workflow, this, ViewModel, Cache);
		public override SizedButtonBorderStyle BorderStyle => GetBorderStyle(Task, Workflow, this, ViewModel, Cache);

		public static Color GetBorderColor(ProcessTask task, ProcessHeader workflow, ICardContent cardContent, BMBoardSectionViewModel viewModel, PropertyCache cache)
		{
			var tagBorderColor = TagProvider.GetBorderColor(cardContent.Definitions, cardContent.ApplicableTagMagnitudes);
			if (tagBorderColor.HasValue)
			{
				return tagBorderColor.Value;
			}

			return Color.Black;
		}

		public static SizedButtonBorderStyle GetBorderStyle(ProcessTask task, ProcessHeader workflow, ICardContent cardContent, BMBoardSectionViewModel viewModel, PropertyCache cache)
		{
			var tagBorderStyle = TagProvider.GetBorderStyle(cardContent.Definitions, cardContent.ApplicableTagMagnitudes);
			if (tagBorderStyle != null)
			{
				return tagBorderStyle;
			}

			return SizedButtonBorderStyle.Default;
		}
	}
}
