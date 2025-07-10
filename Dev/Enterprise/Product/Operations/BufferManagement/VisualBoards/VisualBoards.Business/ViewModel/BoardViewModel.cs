using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.VisualBoards.Business
{
	public class BoardViewModel : IFilterable, IBoardRefreshable
	{
		public BoardViewModel(IBMBoard board, BoardSlideshowViewModel slideShowViewModel)
		{
			BoardPK = board.Identifier;
			SystemPK = board.MB_FS_System;
			BoardName = board.MB_Name;
			BoardHumanReadableShortcutName = board.HumanReadableShortcutName;
			SlideShowViewModel = slideShowViewModel;

			if (!slideShowViewModel.IsPreview)
			{
				PopulateEstimatedLoadTimeCache();
			}
		}

		#region Properties

		public ZGuid BoardPK { get; }
		public ZGuid SystemPK { get; }

		public string BoardName { get; }

		public string BoardHumanReadableShortcutName { get; }

		public int NumberOfTicketsToShow => Sections.Sum(section => section.NumberOfTicketsToShow);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int IntervalInSlideShowSeconds { get; set; }

		public BoardSlideshowViewModel SlideShowViewModel { get; }

		public BoardFactoryProvider FactoryProvider => SlideShowViewModel.FactoryProvider;

		#endregion

		#region Events

		public event EventHandler FullRefreshForced;

		#endregion

		#region Sections

		readonly List<BoardSectionViewModel> sections = new List<BoardSectionViewModel>();

		internal List<BoardSectionViewModel> Sections
		{
			get { return sections; }
		}

		public IEnumerable<BoardSectionViewModel> GetSections()
		{
			return Sections;
		}

		public BoardDataSource Build(IBMBoard board)
		{
			return new BoardDataSource(this, board);
		}

		#endregion

		#region Filters

		public FilterManager FilterManager
		{
			get { return filterManager ?? (filterManager = new FilterManager(this)); }
			set { filterManager = value; }
		}
		FilterManager filterManager;

		#endregion

		#region IFilterable Members

		string IFilterable.Name
		{
			get { return Res.GetString("96d93d46-edf0-4f10-a3a7-ea8bc86826e4", "Visual Board"); }
		}

		void IFilterable.RefreshFilters(bool requiresFullRedraw)
		{
			foreach (var child in ((IFilterable)this).Children)
			{
				child.RefreshFilters(requiresFullRedraw);
			}
		}

		void IFilterable.RefreshSelectedFilters(IEnumerable<IBoardFilter> filters)
		{
			foreach (var child in ((IFilterable)this).Children)
			{
				child.RefreshSelectedFilters(filters);
			}
		}

		IFilterable IFilterable.Parent
		{
			get { return SlideShowViewModel; }
		}

		IEnumerable<IFilterable> IFilterable.Children
		{
			get { return Sections; }
		}

		#endregion

		#region Estimated Load Time Cache

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void PopulateEstimatedLoadTimeCache()
		{
			var statisticsMode = ObjectFactory.Get<ISystemDataRegistry>().StatisticsCollectionEnabled;
			var statisticsHistoryRange = ObjectFactory.Get<IBMSRegistry>().SectionProgressStatisticsHistoryRange;

			if (statisticsMode != nameof(EnabledState.Disabled) && statisticsHistoryRange > 0)
			{
				string sql = @"SELECT
							CONVERT(UNIQUEIDENTIFIER, SUBSTRING(SubName, 1, 36)) SectionPK,
							AVG(mean) AvgLoadTime
						FROM dbo.vw_StatisticsMeasurementsIncludingChildren WITH (NOLOCK)
						WHERE name = 'RefreshBoardSection'
						AND DateToUTC > GetUtcDate() - @StatisticsHistoryRange
						AND SubName LIKE @BoardName
						GROUP BY SubName";

				using (var cmd = Db.Connection.Command(sql)) // Direct SQL to calculate int value from the view
				{
					cmd.AddParameter("@StatisticsHistoryRange", SqlDbType.Int, statisticsHistoryRange);
					cmd.AddParameter("@BoardName", SqlDbType.VarChar, string.Format(CultureInfo.InvariantCulture, "%|{0}|%", BoardName.Replace("'", "''")));

					using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
					{
						while (reader.Read())
						{
							var sectionPK = reader.GetGuid(0);
							var loadTime = (decimal)reader.GetDouble(1);

							Cache.OverwriteCachedValue(sectionPK, EstimatedLoadingTimeCacheKey, loadTime);
						}
					}
				}
			}
		}

		public const string EstimatedLoadingTimeCacheKey = "EstimatedLoadingTime";

		public PropertyCache Cache
		{
			get { return cache ?? (cache = new PropertyCache()); }
		}
		PropertyCache cache;

		#endregion

		#region IBoardRefreshable Members

		IBoardRefreshable IBoardRefreshable.Parent
		{
			get { return null; }
		}

		IEnumerable<IBoardRefreshable> IBoardRefreshable.Children
		{
			get { return Sections; }
		}

		public BoardRefreshType LastBoardRefreshType { get; private set; }

		void IBoardRefreshable.PerformRefreshAction(IBoardRefreshContext refreshContext)
		{
			LastBoardRefreshType = refreshContext?.RefreshType ?? BoardRefreshType.None;

			if (refreshContext is ForcedReloadOperation)
			{
				FullRefreshForced?.Invoke(this, EventArgs.Empty);
			}
		}

		void IBoardRefreshable.AddAggregator(IBoardRefreshContext context, HashSet<IBoardRefreshOperationAggregator> aggregators)
		{
		}

		string IBoardRefreshable.Name => BoardName;

		void IBoardRefreshable.BeforeDataRefresh()
		{
			SlideShowViewModel.RefreshServices(BoardServiceStalenessPolicy.StaleBeforeSavingTicket);
		}

		#endregion
	}
}
