using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public class CardVisibilityFilterApplicator
	{
		public CardVisibilityFilterApplicator(BusinessObjectFactory businessObjectFactory, BMBoardSectionViewModel sectionViewModel, IEnumerable<IBoardFilter> boardFilters)
		{
			viewModel = sectionViewModel;
			factory = businessObjectFactory;
			filters = boardFilters.Where(filter => !filter.RequiresRemoval).Select(filter => GetRelevantCardVisibilityFilter(filter) ?? filter).ToArray();

			RenderApplicableFilters = filters.Where(filter => !(filter is ICardVisibilityFilter)).ToArray();
			filterApplicators = filters.OfType<ICardVisibilityFilter>().Select(filter => filter.GetCellVisibilityApplicator(viewModel, factory)).ToArray();
		}

		readonly BMBoardSectionViewModel viewModel;
		readonly BusinessObjectFactory factory;
		readonly IBoardFilter[] filters;
		readonly CellVisibilityApplicator[] filterApplicators;

		public IEnumerable<IBoardFilter> RenderApplicableFilters { get; private set; }

		public void AddFetchHints(IEnumerable<ICardContent> cards)
		{
			filters.OfType<ICardVisibilityFilter>().ForEach(f => f.FetchForFilter(viewModel, factory, cards));
		}

		public bool IsApplicable(ICardContent cardContent, CellContent cell)
		{
			return filterApplicators.All(f => f(cardContent, cell));
		}

		internal FilterMap PopulateFilterMap(FilterMap map, CardAllocationMap cardAllocation, IEnumerable<CellContent> cells)
		{
			var filterMap = map ?? new FilterMap(cardAllocation);
			filterMap.PopulateFilter(this, cardAllocation, cells);
			return filterMap;
		}

		static IBoardFilter GetRelevantCardVisibilityFilter(IBoardFilter filter)
		{
			if (filter is ICardVisibilityFilter)
			{
				return filter;
			}

			if (filter is BoardMeetingModeFilter boardMeetingFilter)
			{
				return new BoardMeetingModeFilterApplicator(boardMeetingFilter);
			}

			if (filter is SearchFilter searchFilter)
			{
				return new SearchFilterApplicator(searchFilter);
			}

			return null;
		}
	}
}
