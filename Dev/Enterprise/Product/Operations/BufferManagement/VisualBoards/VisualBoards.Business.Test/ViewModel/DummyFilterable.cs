using System.Collections.Generic;
using System.Linq;

namespace Enterprise.VisualBoards.Business.Test
{
	public class DummyFilterable : IFilterable
	{
		public DummyFilterable()
		{
			Children = Enumerable.Empty<IFilterable>();
		}

		public FilterManager FilterManager
		{
			get { return filterManager ?? (filterManager = new FilterManager(this)); }
			set { filterManager = value; }
		}

		FilterManager filterManager;

		public IFilterable Parent { get; set; }

		public IEnumerable<IFilterable> Children { get; set; }

		public int refreshCount;

		public void RefreshFilters(bool requiresFullRedraw)
		{
			refreshCount++;
		}

		public void RefreshSelectedFilters(IEnumerable<IBoardFilter> filters)
		{
			RefreshFilters(false);
		}

		internal void SetChild(DummyFilterable childFilterable)
		{
			if (childFilterable.Parent != null)
			{
				((DummyFilterable)childFilterable.Parent).Children = childFilterable.Parent.Children.Where(x => x != childFilterable);
			}
			childFilterable.Parent = this;
			Children = new[] { childFilterable }.Union(Children ?? Enumerable.Empty<IFilterable>());
		}

		public string Name
		{
			get { return GetType().Name; }
		}
	}
}
