using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Enterprise.VisualBoards.Business
{
	public class FilterManager
	{
		public FilterManager(IFilterable filterable)
		{
			this.filterable = filterable;
		}

		readonly IFilterable filterable;
		readonly HashSet<IBoardFilter> filters = new HashSet<IBoardFilter>();

		protected IFilterable Filterable
		{
			get { return filterable; }
		}

		public void ApplyFilter(IBoardFilter filter, bool requiresFullRedraw = true)
		{
			if (!filter.AllowMultiple)
			{
				var existingFilters = filters.Where(f => f.GetType() == filter.GetType()).ToArray();
				foreach (var existingFilter in existingFilters)
				{
					RemoveFilter(existingFilter);
					OnFilterRemoved(existingFilter);
				}
				filters.Add(filter);
			}
			else
			{
				if (!filters.Contains(filter))
				{
					filters.Add(filter);
				}
			}

			RefreshFilters(new FiltersChangedEventArgs(new[] { filter }, null), requiresFullRedraw);
		}

		public void RemoveFilters(Type filterType, bool shouldRefresh = false)
		{
			foreach (var filter in filters.Where(f => f.GetType() == filterType).ToArray())
			{
				RemoveFilter(filter, shouldRefresh);
			}
		}

		public void RemoveFilter(IBoardFilter filter, bool shouldRefresh = false)
		{
			filter.RequiresRemoval = true;
			if (filters.Contains(filter))
			{
				var requiresFullRedraw = filters.Count > 0 && filters.Any(f => f.RequiresRedraw);
				OnRemovingFilters(new[] { filter });
				filters.Remove(filter);

				if (shouldRefresh)
				{
					RefreshFilters(new FiltersChangedEventArgs(null, new[] { filter }), requiresFullRedraw);
					OnFilterRemoved(filter);
				}
			}
			filter.RequiresRemoval = false;
		}

		public void Clear()
		{
			if (filters.Count > 0)
			{
				OnRemovingFilters(filters);

				var requiresFullRedraw = filters.Count > 0 && filters.Any(f => f.RequiresRedraw);

				var filtersCleared = filters.ToArray();
				filters.Clear();

				foreach (var filter in filtersCleared)
				{
					OnFilterRemoved(filter);
				}

				RefreshFilters(new FiltersChangedEventArgs(null, filtersCleared), requiresFullRedraw);
			}
		}

		protected virtual void OnRemovingFilters(IEnumerable<IBoardFilter> removedFilter)
		{
			UndoUndoableFilters(removedFilter);
		}

		void UndoUndoableFilters(IEnumerable<IBoardFilter> removedFilter)
		{
			var filtersToUndo = removedFilter.Where(f => !f.RequiresRedraw).OfType<IUndoableFilter>().ToList();
			if (filtersToUndo.Count > 0)
			{
				filtersToUndo.ForEach(f => f.RequiresUndo = true);
				try
				{
					Filterable.RefreshSelectedFilters(filtersToUndo);
				}
				finally
				{
					filtersToUndo.ForEach(f => f.RequiresUndo = false);
				}
			}
		}

		static void OnFilterRemoved(IBoardFilter filter)
		{
			filter.RestoreVisualStateAfterFilterRemovedAction?.Invoke();
		}

		public void ToggleFilter(IBoardFilter filter)
		{
			if (filters.Contains(filter))
			{
				RemoveFilter(filter, true);
			}
			else
			{
				ApplyFilter(filter);
			}
		}

		void RefreshFilters(FiltersChangedEventArgs args, bool requiresFullRedraw = false)
		{
			ApplyingFilters?.Invoke(this, EventArgs.Empty);

			try
			{
				filterable.RefreshFilters(requiresFullRedraw);
			}
			finally
			{
				var filterablesToNotify = args.AddedFilters.Concat(args.RemovedFilters).Any(f => Attribute.IsDefined(f.GetType(), typeof(DescendantRefreshableFilterAttribute), inherit: true))
					? AllAncestorFilterables.Concat(AllChildFilterables)
					: AllAncestorFilterables;

				foreach (var filterItem in filterablesToNotify)
				{
					filterItem.FilterManager.FiltersUpdated?.Invoke(this, args);
				}
			}
		}

		public event EventHandler ApplyingFilters;
		public event EventHandler<FiltersChangedEventArgs> FiltersUpdated;

		public IEnumerable<IBoardFilter> AppliedFilters
		{
			get { return filters; }
		}

		public IEnumerable<IBoardFilter> AppliedAndInheritedFilters
		{
			get { return filterable.Parent != null ? AppliedFilters.Union(filterable.Parent.FilterManager.AppliedAndInheritedFilters) : AppliedFilters; }
		}

		public IEnumerable<IBoardFilter> AppliedAndChildFilters
		{
			get { return AllChildFilterables.SelectMany(f => f.FilterManager.AppliedFilters).Union(filterable.FilterManager.AppliedFilters); }
		}

		public IEnumerable<IFilterable> AllAncestorFilterables
		{
			get
			{
				var filter = filterable;
				do
				{
					yield return filter;
					filter = filter.Parent;
				} while (filter != null);
			}
		}

		public IEnumerable<IFilterable> AllChildFilterables
		{
			get { return filterable.Children.Union(filterable.Children.SelectMany(c => c.FilterManager.AllChildFilterables)); }
		}

		public bool IsApplied(IBoardFilter filter)
		{
			return AppliedAndInheritedFilters.Contains(filter);
		}

		public bool IsApplied(Type filterType)
		{
			return AppliedAndInheritedFilters.Any(filterType.IsInstanceOfType);
		}

		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public void ClearFiltersOfType<T>()
			where T : IBoardFilter
		{
			foreach (var ancestorFilterable in AllAncestorFilterables)
			{
				var filtersToClear = ancestorFilterable.FilterManager.AppliedFilters.OfType<T>().ToArray();

				if (filtersToClear.Length > 0)
				{
					foreach (var filter in filtersToClear)
					{
						ancestorFilterable.FilterManager.RemoveFilter(filter);
					}
				}
			}
		}

		void ApplyToChildFilters(Action<IFilterable> action)
		{
			if (filterable.Children != null)
			{
				foreach (var childFilterable in filterable.Children)
				{
					childFilterable.FilterManager.ApplyToChildFilters(action);
				}
			}

			action(filterable);
		}

		public void ClearAllChildFilters()
		{
			ApplyToChildFilters(r => r.FilterManager.Clear());
		}

		public void ClearAllChildFiltersWithoutRefresh()
		{
			ApplyToChildFilters(r => r.FilterManager.filters.Clear());
		}
	}
}
