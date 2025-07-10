using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif

namespace Enterprise.BufferManagement.Business
{
	public static class TaskOrderSorter
	{
		public static T[] SortByTask<T>(IEnumerable<T> items, Func<T, ITaskOrderable> taskSelector, CardSortType sortType)
		{
			T[] orderedItems;

			switch (sortType)
			{
				case CardSortType.BufferWorkSequence:
					orderedItems = items.OrderBy(taskSelector, new VisualBoardTaskComparer()).ToArray();
					break;

				case CardSortType.ReleaseSequence:
					var itemTaskPair = items.Select(item => new { Item = item, Task = taskSelector(item) }).Where(x => x?.Task?.WorkflowOrderable?.Identifier != null);
					var itemByWorkflow = itemTaskPair.ToLookup(t => t.Task.WorkflowOrderable.Identifier);
					var orderedWorkflows = WorkflowTransferOrderSorter.Sort(itemTaskPair
						.Select(x => x.Task.WorkflowOrderable)
						.DistinctBy(w => w.Identifier));

					orderedItems = orderedWorkflows
						.SelectMany(workflow => itemByWorkflow[workflow.Identifier]
						.OrderBy(pair => pair.Task.Sequence)
						.ThenBy(pair => pair.Task.TaskID)
						.Select(pair => pair.Item))
						.ToArray();

					break;

				case CardSortType.LastTransferTime:
					orderedItems = items
						.OrderBy(item => taskSelector(item).WorkflowOrderable.ReleaseDateTime)
						.ThenBy(item => taskSelector(item).TaskID)
						.ToArray();
					break;

				default:
					throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "CardSortType {0} is not supported", sortType), nameof(sortType));
			}

			return orderedItems;
		}

		public static T[] SortByBuckettedTasks<T>(IEnumerable<T> items, Func<T, ITaskOrderable> taskSelector, CardSortType sortType)
		{
			var sortedItems = SortByTask(items, taskSelector, sortType);

			return sortedItems.OrderBy(taskSelector, new VisualBoardBucketTaskComparer()).ToArray();
		}
	}
}
