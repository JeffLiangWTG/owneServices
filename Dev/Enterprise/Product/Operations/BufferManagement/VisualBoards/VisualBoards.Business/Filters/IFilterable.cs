using System.Collections.Generic;

namespace Enterprise.VisualBoards.Business
{
	public interface IFilterable
	{
		string Name { get; }

		IFilterable Parent { get; }
		IEnumerable<IFilterable> Children { get; }
		FilterManager FilterManager { get; }

		void RefreshFilters(bool requiresFullRedraw = false);
		void RefreshSelectedFilters(IEnumerable<IBoardFilter> filters);
	}
}
