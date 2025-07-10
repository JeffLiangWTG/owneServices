using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.VisualBoards.Business
{
	public sealed class FiltersChangedEventArgs : EventArgs
	{
		public FiltersChangedEventArgs(IEnumerable<IBoardFilter> addedFilters, IEnumerable<IBoardFilter> removedFilters)
		{
			AddedFilters = addedFilters ?? Enumerable.Empty<IBoardFilter>();
			RemovedFilters = removedFilters ?? Enumerable.Empty<IBoardFilter>();
		}

		public IEnumerable<IBoardFilter> AddedFilters { get; }
		public IEnumerable<IBoardFilter> RemovedFilters { get; }
	}
}
