using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Pipes;
using Enterprise.BufferManagement.Integration;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Telemetry;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public interface IBoardSectionRefreshable
	{
		void NotifyComponentFinishRefreshed(BoardRefreshEventArgs refreshArgs);
		void NotifyRefreshFinished();
		void ShowLoadingSectionOverlayControl(LoadingIndicatorEventArgs args);
		bool WrapDrawCardsWithLoadingCursor(Func<bool> onRefreshComponentCore, bool isLoading, BoardRefreshEventArgs eventArgs);
		void ShowLoadingIndicator(bool isLoading, BoardRefreshEventArgs eventAgs);
		void DisplayAcceptabilityBandResults(IEnumerable<BoardSectionAcceptabilityBandResult> results);
	}

	public class BoardSectionRefreshPipeEngine : PipeEngine
	{
		public static BoardSectionRefreshPipeEngine Create(BoardRefreshEventArgs refreshArgs, LoadCardContentSetup setup, IBoardSectionRefreshable componentControl, TaskChannelMap taskChannelMapOverride = null)
		{
			var options = BMSRegistry.Instance.BoardOnSecondaryServer.Value ? EngineConfigOptions.DisableAsync : EngineConfigOptions.None;
			var engine = new BoardSectionRefreshPipeEngine(options, refreshArgs, setup, componentControl, taskChannelMapOverride);

			engine.Initialise();
			return engine;
		}

		protected BoardSectionRefreshPipeEngine(EngineConfigOptions engineConfigOptions, BoardRefreshEventArgs refreshArgs, LoadCardContentSetup setup, IBoardSectionRefreshable control, TaskChannelMap taskChannelMapOverride = null)
			: base(engineConfigOptions, id: PipeEngineHelper.GetID())
		{
			this.control = control;
			this.setup = setup;
			this.taskChannelMapOverride = taskChannelMapOverride;
			this.refreshArgs = refreshArgs;
		}

		readonly LoadCardContentSetup setup;
		readonly TaskChannelMap taskChannelMapOverride;
		readonly BoardRefreshEventArgs refreshArgs;
		readonly IBoardSectionRefreshable control;
		readonly CardAllocationMap existingAllocationMap;

		protected BoardRefreshEventArgs RefreshArgs => refreshArgs;

		public void Initialise()
		{
			var engine = this;

			var activity = TelemetryService.ActivitySource?.CreateActivity($"{nameof(BoardSectionRefreshPipeEngine)}.Execute", ActivityKind.Internal);
			engine.AddSync(() => activity?.Start());

			if (IsLoading || DidUserForceRefresh)
			{
				ShowLoadingRefreshIndicator = engine.AddSync(OnShowLoadingIndicator).SetName("LoadingRefreshIndicator");
			}

			ViewModel = engine.AddSync(() => setup.ViewModel);
			CardFactory = engine.AddAsync(CreateCardFactory(setup)).ThreadSentryHandover(t => t);

			TaskChannelMap = GetTaskChannelMapWrapper(engine);
			TaskChannelMap.SetName(nameof(TaskChannelMap));

			CardAllocationMapWrapper = GetCardAllocationMapWrapper(this);

			if (CardAllocationMapWrapper == null)
			{
				ErrorReporter.ReportOnce(
					string.Format(
						CultureInfo.InvariantCulture,
						"CardAllocationMap is null on board '{0}'.",
						setup.ViewModel.BoardViewModel.BoardName));
			}
			else
			{
				UpdateBackgroundsAndFades = engine
					.AddAsync(OnUpdateBackgroundsAndFades, CardAllocationMapWrapper, ViewModel)
					.SetName(nameof(UpdateBackgroundsAndFades));

				ChannelViewModelsPipe = engine.AddAsync(
					(allocationMap, viewModel, update) =>
					{
						using var activity = TelemetryService.ActivitySource?.StartActivity($"{nameof(BoardSectionRefreshPipeEngine)}.ChannelViewModelsPipe");
						return allocationMap.Failure == null
						? viewModel.AllChannels.MakeViewModelSet(
								viewModel.FactoryProvider.GetNewBackgroundThreadLoaderFactory((NoResString)"ID: " + engine.ID + (NoResString)", ChannelViewModelsPipe"), // Internal Debug info
								viewModel,
								allocationMap.AllocationMap,
								allocationMap.Cache)
						: null;
					},
					CardAllocationMapWrapper,
					ViewModel, UpdateBackgroundsAndFades).SetName(nameof(ChannelViewModelsPipe));

				AcceptabilityBandResults = engine.AddAsync(GetAcceptabilityBandResults, ViewModel, CardAllocationMapWrapper);

				var loaded = engine.AddSync(t => RemoveLoadingIndicator(), engine.CardAllocationMapWrapper);
				var displayedABs = engine.AddSync(DisplayAcceptabilityBandResults, AcceptabilityBandResults);
				var refreshHeaders = engine.AddSync(RefreshHeadings, ViewModel, ChannelViewModelsPipe);
				RefreshComponentGrid = engine.AddSync(OnRefreshComponent, CardFactory, ViewModel, CardAllocationMapWrapper).SetName("DrawCards");

				engine.AddPipeEnd((refreshedGrid, refreshedAbs, refreshedHeaders) =>
				{
					control?.NotifyComponentFinishRefreshed(RefreshArgs);
					activity?.Dispose();
				}, RefreshComponentGrid, displayedABs, refreshHeaders);
			}

#if DEBUG
			var bmsRegistry = ObjectFactory.Get<IBMSRegistry>();
			if (bmsRegistry.DisallowDBHitsOnBoardGUIThread)
			{
				TableHitCounter.ReportHitsOnAllTablesExceptSpecifiedTables(bmsRegistry.AllowedTablesDBHitsOnBoardGUIThread.ToList());
			}
#endif
		}

		protected bool IsLoading
		{
			get { return RefreshArgs.IsInitialLoad || RefreshArgs.IsReloading && !RefreshArgs.TriggeredBySlideshowTimer; }
		}

		bool DidUserForceRefresh
		{
			get { return RefreshArgs.TriggeredByShortcut && !RefreshArgs.IsReloading || RefreshArgs.TriggeredByUserRefreshingBoardOrSection; }
		}

		IPipe<TaskChannelMapWrapper> GetTaskChannelMapWrapper(BoardSectionRefreshPipeEngine engine)
		{
			if (taskChannelMapOverride != null)
			{
				return engine.AddSync(() => new TaskChannelMapWrapper(taskChannelMapOverride));
			}
			else if (IsNewControlForPreLoadedSectionContent)
			{
				return engine.AddSync(() => (TaskChannelMapWrapper)null);
			}
			else
			{
				return engine.AddAsync(
							func: (viewModel, cardFactory) => LoadCardContents.MakeTaskChannelMap(cardFactory, viewModel),
							dataSource1: ViewModel,
							dataSource2: CardFactory);
			}
		}

		IPipe<CardAllocationMapResult> GetCardAllocationMapWrapper(BoardSectionRefreshPipeEngine engine)
		{
			if (IsNewControlForPreLoadedSectionContent)
			{
				return engine.AddSync(viewModel => new CardAllocationMapResult { AllocationMap = existingAllocationMap, Cache = viewModel.Cache }, ViewModel);
			}
			else
			{
				return engine.AddAsync(LoadCardContents.CreateAllocationMap, CardFactory, ViewModel, TaskChannelMap)
					.SetName(nameof(CardAllocationMapResult));
			}
		}

		bool RefreshHeadings(BMBoardSectionViewModel viewModel, ChannelHeadingViewModelSet channelViewModels)
		{
			using var activity = TelemetryService.ActivitySource?.StartActivity($"{nameof(BoardSectionRefreshPipeEngine)}.{nameof(RefreshHeadings)}");
			if (channelViewModels != null)
			{
				viewModel.ComponentGrid.RefreshHeadings(channelViewModels);
			}
			return true;
		}

		bool OnUpdateBackgroundsAndFades(CardAllocationMapResult cardAllocationMapWrapper, BMBoardSectionViewModel viewModel)
		{
			using var activity = TelemetryService.ActivitySource?.StartActivity($"{nameof(BoardSectionRefreshPipeEngine)}.{nameof(OnUpdateBackgroundsAndFades)}");
			if (cardAllocationMapWrapper.Failure == null)
			{
				var factory = viewModel.FactoryProvider.GetBestFactory(nameof(OnUpdateBackgroundsAndFades));
				var cardAllocationMap = cardAllocationMapWrapper.AllocationMap;
				var section = BMBoardSection.Load(factory, viewModel.SectionPK);
				if (section != null)
				{
					viewModel.ComponentGrid.UpdateBackgroundColoursAndFades(viewModel, cardAllocationMap, section);
				}

				return true;
			}

			return false;
		}

		static Func<BusinessObjectFactory> CreateCardFactory(LoadCardContentSetup setup)
		{
			return () =>
			{
				var factory = setup.GetFactoryInstance();
				factory.ThreadSentry.RelinquishThreadOwnership();
				return factory;
			};
		}

		bool OnRefreshComponent(BusinessObjectFactory factory, BMBoardSectionViewModel viewModel, CardAllocationMapResult newSnapshot)
		{
			using var activity = TelemetryService.ActivitySource?.StartActivity($"{nameof(BoardSectionRefreshPipeEngine)}.{nameof(OnRefreshComponent)}");
			bool OnRefreshComponentCore()
			{
				if (newSnapshot.Failure != null)
				{
					viewModel.ComponentGrid.ShowLoadFailure(newSnapshot.Failure);
				}
				else
				{
					viewModel.Cache.ReplaceWith(newSnapshot.Cache);
					viewModel.ComponentGrid.RefreshComponent(factory, viewModel, newSnapshot.AllocationMap, !RefreshArgs.IsDifferentialRefresh);
				}
				return true;
			}

			if (control != null)
			{
				return control.WrapDrawCardsWithLoadingCursor(OnRefreshComponentCore, IsLoading, RefreshArgs);
			}
			else
			{
				return OnRefreshComponentCore();
			}
		}

		bool OnShowLoadingIndicator()
		{
			using var activity = TelemetryService.ActivitySource?.StartActivity($"{nameof(BoardSectionRefreshPipeEngine)}.{nameof(OnShowLoadingIndicator)}");
			control?.ShowLoadingIndicator(IsLoading, RefreshArgs);
			return true;
		}

		bool RemoveLoadingIndicator()
		{
			using var activity = TelemetryService.ActivitySource?.StartActivity($"{nameof(BoardSectionRefreshPipeEngine)}.{nameof(RemoveLoadingIndicator)}");
			control?.NotifyRefreshFinished();
			return true;
		}

		#region Implementation

		bool IsNewControlForPreLoadedSectionContent => existingAllocationMap != null && RefreshArgs.CachedLayoutWasDisposed;

		#endregion

		#region Acceptability Bands

		BoardSectionAcceptabilityBandResult[] GetAcceptabilityBandResults(BMBoardSectionViewModel viewModel, CardAllocationMapResult map)
		{
			using var activity = TelemetryService.ActivitySource?.StartActivity($"{nameof(BoardSectionRefreshPipeEngine)}.{nameof(GetAcceptabilityBandResults)}");
			if (map.Failure == null && !viewModel.SuppressAcceptabilityBandVisualisation)
			{
				var sectionBands = viewModel.BoardSectionAcceptabilityBands;
				if (sectionBands.Length > 0)
				{
					var factory = new BusinessObjectFactory { NameForDebugging = GetType().Name + ": UpdateAcceptabilityBands" };

					return viewModel.GetAcceptabilityBandResults(factory) ?? Array.Empty<BoardSectionAcceptabilityBandResult>();
				}
			}
			return Array.Empty<BoardSectionAcceptabilityBandResult>();
		}

		bool DisplayAcceptabilityBandResults(IEnumerable<BoardSectionAcceptabilityBandResult> results)
		{
			using var activity = TelemetryService.ActivitySource?.StartActivity($"{nameof(BoardSectionRefreshPipeEngine)}.{nameof(DisplayAcceptabilityBandResults)}");
			control?.DisplayAcceptabilityBandResults(results);
			return true;
		}

		#endregion

		public IPipe<bool> ShowLoadingRefreshIndicator { get; private set; }
		public IPipe<BMBoardSectionViewModel> ViewModel { get; private set; }
		public IPipe<bool> UpdateBackgroundsAndFades { get; private set; }
		public IPipe<TaskChannelMapWrapper> TaskChannelMap { get; private set; }
		public IPipe<CardAllocationMapResult> CardAllocationMapWrapper { get; private set; }
		public IPipe<BoardSectionAcceptabilityBandResult[]> AcceptabilityBandResults { get; private set; }
		public IPipe<bool> RefreshComponentGrid { get; private set; }
		public IPipeHandoverWrapper<BusinessObjectFactory> CardFactory { get; private set; }
		public IPipe<ChannelHeadingViewModelSet> ChannelViewModelsPipe { get; private set; }
	}
}
