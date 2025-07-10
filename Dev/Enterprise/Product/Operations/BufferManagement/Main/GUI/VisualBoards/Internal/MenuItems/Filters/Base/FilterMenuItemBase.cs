using System;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public abstract class FilterMenuItemBase : ZToolStripMenuItem
	{
		protected FilterMenuItemBase(string text)
			: base(text)
		{
		}

		IBoardFilter filter;

		protected IBoardFilter Filter
		{
			get
			{
				return filter ?? (filter = GetOrCreateFilter());
			}
		}

		protected abstract IBoardFilter GetOrCreateFilter();

		protected abstract IBoardFilter GetCurrentFilter(Type filterType);

		#region Life Cycle

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				Filter.RestoreVisualStateAfterFilterRemovedAction = null;
			}
		}

		#endregion
	}
}
