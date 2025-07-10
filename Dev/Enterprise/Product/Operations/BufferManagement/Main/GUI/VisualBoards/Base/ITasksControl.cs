using System.Collections.Generic;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	public interface ITasksControl
	{
		CellContent Cell { get; }
		void SetupTasks(bool disposeExistingTickets);
	}

	static class ITasksControlExtensions
	{
		public static ICardContent[] SortCards(this ITasksControl control, IEnumerable<ICardContent> cards, BMBoardSectionViewModel viewModel)
		{
			return (viewModel.IsBucket && !viewModel.ShowWorkflowOrJobWorkflowCards)
				? TaskOrderSorter.SortByBuckettedTasks(cards, c => c.TaskOrderable, control.Cell.CardSortType)
				: TaskOrderSorter.SortByTask(cards, c => c.TaskOrderable, control.Cell.CardSortType);
		}
	}
}
