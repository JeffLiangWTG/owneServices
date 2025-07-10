using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.VisualBoards.Business
{
	public class BoardFilterBusinessObjectCollection : NonPersistentBusinessObjectCollection<BoardFilterBusinessObject>
	{
		public BoardFilterBusinessObjectCollection(IFilterable filterable)
		{
			var filterBizos = GetBoardFilters(filterable).Concat(filterable.FilterManager.AllChildFilterables.SelectMany(f => GetBoardFilters(f))).ToArray();
			AddRange(filterBizos);
		}

		static IEnumerable<BoardFilterBusinessObject> GetBoardFilters(IFilterable filterable)
		{
			foreach (var filter in filterable.FilterManager.AppliedFilters)
			{
				yield return new BoardFilterBusinessObject(filter, filterable);
			}
		}

		#region NonPersistentBusinessObjectCollection Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override void OnRemoved(BusinessObject bizo)
		{
			base.OnRemoved(bizo);

			var filterBizo = (BoardFilterBusinessObject)bizo;
			filterBizo.Filterable.FilterManager.RemoveFilter(filterBizo.Filter, shouldRefresh: true);
		}

		#endregion
	}
}
