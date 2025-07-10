using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.BufferManagement.Business;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.GUI
{
	public class CurrentTaskFilterMenuItem : BasicCheckedFilterMenuItem
	{
		public CurrentTaskFilterMenuItem(BMBoardSectionViewModel sectionViewModel)
			: this()
		{
			this.sectionViewModel = sectionViewModel;
			UpdateAppliedVisualState();
		}

		public CurrentTaskFilterMenuItem(BoardViewModel boardViewModel)
			: this()
		{
			this.boardViewModel = boardViewModel;
			UpdateAppliedVisualState();
		}

		CurrentTaskFilterMenuItem()
			: base(Res.GetString("ea7cf370-5877-42ac-9c2c-611131c51d6c", "Show Startable Items"), Res.GetString("bec5efba-ecec-49b3-8ae7-47b0759a3b69", "Hides all tasks except those which can be started now."))
		{
		}

		readonly BMBoardSectionViewModel sectionViewModel;
		readonly BoardViewModel boardViewModel;

		protected override IEnumerable<IFilterable> GetFilterables()
		{
			if (sectionViewModel != null)
			{
				yield return sectionViewModel;
			}
			else if (boardViewModel != null)
			{
				yield return boardViewModel;
			}
		}

		protected override IBoardFilter GetOrCreateFilter()
		{
			var filter = GetCurrentFilter(typeof(CurrentTaskFilter)) ?? new CurrentTaskFilter();
			filter.RestoreVisualStateAfterFilterRemovedAction = UpdateAppliedVisualState;
			return filter;
		}

		protected override IBoardFilter GetCurrentFilter(Type filterType)
		{
			var filterManager = sectionViewModel != null ? sectionViewModel.FilterManager : boardViewModel.FilterManager;
			return filterManager.AppliedFilters.FirstOrDefault(filterType.IsInstanceOfType);
		}
	}
}
