using System.Collections.Generic;
using System.ComponentModel;

namespace CargoWise.EntityFramework
{
	class SortState
	{
		public SortState()
		{
		}

		public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
		{
			if (history == null)
			{
				history = new List<ListSortDescription>();
			}
			if (clearHistoryOnNextSort)
			{
				history.Clear();
				clearHistoryOnNextSort = false;
			}
			else
			{
				for (int i = history.Count - 1; i >= 0; i--)
				{
					if (history[i].PropertyDescriptor.Name == property.Name)
					{
						history.RemoveAt(i);
						break;
					}
				}
			}

			history.Add(new ListSortDescription(property, direction));
		}

		public ListSortDescriptionCollection SortDescriptions
		{
			get
			{
				if (history == null)
				{
					history = new List<ListSortDescription>();
				}

				return new ListSortDescriptionCollection(history.ToArray());
			}
		}

		public void RemoveSort()
		{
			if (history != null)
			{
				history.Clear();
			}
		}

		public void ClearSortDescriptionsBeforeNextSort()
		{
			clearHistoryOnNextSort = true;
		}

		List<ListSortDescription> history;
		bool clearHistoryOnNextSort;
	}
}
