using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IFilterControl : IDisposable, IResultCountHandler
	{
		ZFilterGrid FilteredGrid { get; }
		Control OuterFilterControl { get; }
		ModuleIdentifier ParentModuleID { get; set; }

		event EventHandler<ZFilterStripEventArgs> FilterStripAdding;
		event EventHandler<PerformSearchEventArgs> PerformSearch;
		event EventHandler<PerformSearchAsyncEventArgs> PerformSearchAsync;
		void CommitAllFilters();
		void LoadRecentItems();
		void HookFormEvents();
	}

	public sealed class PerformSearchEventArgs : EventArgs
	{
		public PerformSearchEventArgs()
		{
		}
	}

	public sealed class PerformSearchAsyncEventArgs : EventArgs
	{
		public PerformSearchAsyncEventArgs(Action onAfterPerformSearch, Action onBeginPeformAsync, Action onAfterPerformAsync)
		{
			OnAfterPerformSearch = onAfterPerformSearch;
			OnBeginPerformSearchAsync = onBeginPeformAsync;
			OnAfterPerformSearchAsync = onAfterPerformAsync;
		}

		public Action OnAfterPerformSearch { get; }

		public Action OnBeginPerformSearchAsync { get; }

		public Action OnAfterPerformSearchAsync { get; }
	}
}
