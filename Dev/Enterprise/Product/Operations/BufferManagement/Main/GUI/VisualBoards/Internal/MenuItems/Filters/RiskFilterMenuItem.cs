using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Enterprise.BufferManagement.Business;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.GUI
{
	class RiskFilterMenuItem : BasicCheckedFilterMenuItem
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		internal RiskFilterMenuItem(BMBoardSectionViewModel sectionViewModel)
			: base(RiskFilter.Name, Res.GetString("6dde4c84-eeda-403d-86e2-eb1b7a588630", "Hides all tasks except those considered to be at risk of late delivery."))
		{
			this.sectionViewModel = sectionViewModel;

			UpdateAppliedVisualState();
		}

		readonly BMBoardSectionViewModel sectionViewModel;

		protected override IEnumerable<IFilterable> GetFilterables()
		{
			yield return sectionViewModel;
		}

		protected override IBoardFilter GetOrCreateFilter()
		{
			var filter = GetCurrentFilter(typeof(BoardMeetingModeFilter)) ?? new RiskFilter();
			filter.RestoreVisualStateAfterFilterRemovedAction = UpdateAppliedVisualState;
			return filter;
		}

		protected override IBoardFilter GetCurrentFilter(Type filterType)
		{
			return sectionViewModel.FilterManager.AppliedFilters.FirstOrDefault(filterType.IsInstanceOfType);
		}
	}
}
