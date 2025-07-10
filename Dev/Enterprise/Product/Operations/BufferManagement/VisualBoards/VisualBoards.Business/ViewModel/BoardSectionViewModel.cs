using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.VisualBoards.Business
{
	public class BoardSectionViewModel : IFilterable, IBoardRefreshable
	{
		public BoardSectionViewModel(IBMBoardSection section, BoardViewModel boardViewModel)
		{
			Argument.NotNull(boardViewModel, "boardViewModel");
			Argument.NotNull(boardViewModel.FactoryProvider, "boardViewModel.factoryProvider");
			Argument.NotNull(section, "section");

			BoardViewModel = boardViewModel;

			SetupProperties(section);
		}

		public BoardViewModel BoardViewModel { get; }

		public BoardFactoryProvider FactoryProvider => BoardViewModel.FactoryProvider;

		#region Properties

		public ZGuid SectionPK { get; private set; }

		public int Row { get; private set; }
		public int Column { get; private set; }

		public string SectionName { get; private set; }
		public bool IsPreview => BoardViewModel.SlideShowViewModel.IsPreview;

		public string SectionType { get; private set; }

		public int NumberOfTicketsToShow { get; protected set; }

		#endregion

		#region Setup

		void SetupProperties(IBMBoardSection section)
		{
			SectionPK = section.Identifier;
			Row = section.Row;
			Column = section.Column;

			SectionName = section.SectionName;

			SectionType = section.MS_SectionType;
		}

		protected internal virtual void Initialise()
		{
		}

		#endregion

		#region Refresh

		public virtual void RefreshForPreview(IBMBoardSection section)
		{
		}

		public virtual bool ReloadRequired(IBMBoardSection section)
		{
			return false;
		}

		#endregion

		#region Filters

		public FilterManager FilterManager
		{
			get { return filterManager ?? (filterManager = new FilterManager(this)); }
		}

		FilterManager filterManager;

		string IFilterable.Name
		{
			get { return SectionName; }
		}

		IFilterable IFilterable.Parent
		{
			get { return BoardViewModel; }
		}

		IEnumerable<IFilterable> IFilterable.Children
		{
			get { return Enumerable.Empty<IFilterable>(); }
		}

		void IFilterable.RefreshFilters(bool requiresFullRedraw)
		{
			RefreshFiltersCore(FilterManager.AppliedAndInheritedFilters, requiresFullRedraw);
		}

		void IFilterable.RefreshSelectedFilters(IEnumerable<IBoardFilter> filters)
		{
			RefreshFiltersCore(filters, false);
		}

		protected virtual void RefreshFiltersCore(IEnumerable<IBoardFilter> filters, bool requiresFullRedraw)
		{
		}

		#endregion

		#region Cache

		public PropertyCache Cache
		{
			get { return cache ?? (cache = new PropertyCache()); }
		}

		PropertyCache cache;

		#endregion

		#region Performance

		public string SectionDetailsForLog => SectionDetailsForLogFunc?.Invoke();

		protected Func<string> SectionDetailsForLogFunc { get; set; }

		public Tuple<string, string> CreatePerformanceStatisticsNamePair()
		{
			return Tuple.Create(
				"RefreshBoardSection",
				string.Format(CultureInfo.InvariantCulture, "{0}|{1}|{2}", SectionPK, BoardViewModel.BoardName.Replace("|", ""), SectionName)
				);
		}

		#endregion

		#region IBoardRefreshable Members

		IBoardRefreshable IBoardRefreshable.Parent
		{
			get { return BoardViewModel; }
		}

		IEnumerable<IBoardRefreshable> IBoardRefreshable.Children
		{
			get { yield break; }
		}

		void IBoardRefreshable.AddAggregator(IBoardRefreshContext context, HashSet<IBoardRefreshOperationAggregator> aggregators)
		{
			AddAggregatorCore(context, aggregators);
		}

		void IBoardRefreshable.PerformRefreshAction(IBoardRefreshContext refreshContext)
		{
			PerformRefreshActionCore(refreshContext);

#if DEBUG
			PerformRefreshActionCompleted_ForTest?.Invoke(this, EventArgs.Empty);
#endif
		}

#if DEBUG
		public event EventHandler PerformRefreshActionCompleted_ForTest;
#endif

		protected virtual void PerformRefreshActionCore(IBoardRefreshContext refreshContext)
		{
		}

		protected virtual void AddAggregatorCore(IBoardRefreshContext context, HashSet<IBoardRefreshOperationAggregator> aggregators)
		{
		}

		string IBoardRefreshable.Name => SectionName;

		void IBoardRefreshable.BeforeDataRefresh()
		{
		}

		#endregion
	}
}
