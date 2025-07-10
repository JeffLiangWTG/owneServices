using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	class TaskControlFilterApplicator
	{
		public TaskControlFilterApplicator(BMBoardSectionViewModel viewModel, CellContent cellContent)
		{
			sectionViewModel = viewModel;
			cell = cellContent;
			filters = sectionViewModel.FilterManager.AppliedAndInheritedFilters.OfType<IFilterApplicator>().ToArray();
			requiresRedraw = sectionViewModel.FilterManager.AppliedAndInheritedFilters.Any(f => f.RequiresRedraw);
			filterMap = sectionViewModel.ComponentGrid.FilterMap;
		}

		readonly FilterMap filterMap;
		readonly BMBoardSectionViewModel sectionViewModel;
		readonly CellContent cell;
		readonly IFilterApplicator[] filters;
		readonly bool requiresRedraw;

		internal bool IsVisible(ICardContent card)
		{
			return filterMap.IsVisible(cell, card);
		}

		internal bool IsRedrawFilterApplicable(ICardContent cardContent)
		{
			return requiresRedraw && filters.Where(f => f.RequiresRedraw).Any(f => f.IsApplicable(cardContent, cell, sectionViewModel));
		}

		internal void Apply(TaskCardControl taskCard, ICardContent cardContent)
		{
			Apply(filters, taskCard, cardContent, cell, sectionViewModel);
		}

		static void Apply(IEnumerable<IFilterApplicator> filters, Control control, ICardContent cardContent, CellContent cell, BMBoardSectionViewModel viewModel)
		{
			var appliedFilters = filters.Select(applicator => new AppliedFilter(applicator, applicator.IsApplicable(cardContent, cell, viewModel))).ToArray();

			viewModel.Cache.GetCachedValue(cardContent.Identifier, TaskJobWorkflowCacheHelper.CacheConstants.IsVisibleAfterFilterApplication, () => appliedFilters.All(a => a.IsApplicable));

			foreach (var appliedFilter in appliedFilters)
			{
				appliedFilter.Applicator.Apply(control, appliedFilter.IsApplicable, appliedFilters.Where(a => a != appliedFilter));
			}
		}
	}
}
