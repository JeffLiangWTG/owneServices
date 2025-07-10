using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.BufferManagement.Business
{
	[ThreadSafe]
	public class BMBoardSectionViewModel : BoardSectionViewModel, INotifyPropertyChanged
	{
		public BMBoardSectionViewModel(BMBoardSection section, BoardViewModel boardViewModel)
			: base(section, boardViewModel)
		{
			SetupProperties(section);

			Section = section;
			originalRowHeights = new float[ComponentGrid.TotalRows];
			originalColumnWidths = new float[ComponentGrid.TotalColumns];
		}

		#region Setup

		void SetupProperties(BMBoardSection section)
		{
			var config = section.SectionConfiguration;

			CellsPerSubsection = config.CellsPerSubsection;

			ComponentPK = section.MS_FC_Component;
			AllShownComponentPKs = section.AllShownComponentPKs;
			ReleaseGroupPK = config.ApplicableReleaseGroupPK;

			SectionPanelLayoutStyle = config.PanelLayoutStyle;

			IsBuffer = config.IsBuffer;
			IsBucket = config.IsBucket;
			IsInConstrainedMode = section.IsInConstrainedMode;

			TimeProgressionMode = config.TimeProgressionMode;
			TimeProgressionField = config.TimeField;

			Orientation = config.OrientationValue;

			shouldShowAcceptabilityBandHeading = !SuppressAcceptabilityBandVisualisation && config.HeadingAcceptabilityBands.Any();

			if (shouldShowAcceptabilityBandHeading)
			{
				subHeadingAppearance = new LoadingBandsSubHeadingAppearance();
			}

			if (!IsPreview)
			{
				HideCapabilityTasksFromResourceChannels = config.HideCapabilityTasksFromResourceChannels;
				HideResourceTasksFromCapabilityChannels = config.HideResourceTasksFromCapabilityChannels;
				EnableShowCurrentItemsFilterByDefault = config.EnableShowCurrentItemsFilterByDefault;
				ShowWorkflowCards = config.CardType == CardTypeList.Codes.Workflow;
				ShowJobCards = config.ShowJobWorkflowCards;

				CountdownTargetBorderStyle = config.CountdownTargetBorderStyleValue;
				CountdownStartableBorderStyle = config.CountdownStartableBorderStyleValue;
				CountdownTargetBorderColor = config.CountdownTargetBorderColorValue ?? Color.Empty;
				CountdownStartableBorderColor = config.CountdownStartableBorderColorValue ?? Color.Empty;

				summaryCardLayouts = section.GetSummaryCardCustomisedLayouts();
				detailedCardLayouts = section.GetDetailedCardCustomisedLayouts();

				LoadBoardSectionAcceptabilityBands(config);
			}

			DefaultChannelsProvider.RefreshChannels(config, section.Factory);

			LoadChannels(section);

			ComponentGrid = BuildGrid(section);
			CacheCustomisationLastEditTime(section);

			GradientAngle = GetGradientAngle(section);
		}

		internal void SetDetailsForLog(int numberOfTicketsToShow, (string Code, string Type, int? TasksCount)[] channels)
		{
			NumberOfTicketsToShow = numberOfTicketsToShow;
			SectionDetailsForLogFunc = () => GetDetailsForLog(numberOfTicketsToShow, channels);
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Used in logging only")]
		static string GetDetailsForLog(int numberOfTicketsToShow, (string Code, string Type, int? TasksCount)[] channels)
		{
			string sectionsDetaisForLog = $"Tickets: {numberOfTicketsToShow} ";  // Log Information

			if (channels == null)
			{
				return default;
			}

			var channelDetails = channels
				.OrderBy(channel => channel.Code)
				.Select(channel => $"Channel: Type: {channel.Type}, Code: {channel.Code}, Tasks: {channel.TasksCount}"); // Log Information

			sectionsDetaisForLog += string.Join(" ", channelDetails);

			return sectionsDetaisForLog;
		}

		#endregion

		#region Filters

		protected override void RefreshFiltersCore(IEnumerable<IBoardFilter> filters, bool requiresFullRedraw)
		{
			ComponentGrid.RefreshFilters(filters, this, requiresFullRedraw);
		}

		public ZQuery OverriddenWorkflowSectionFilter { get; set; }

		public ZQuery OverriddenTaskSectionFilter { get; set; }

		public bool AreWorkflowOrTaskFiltersRedefined => OverriddenWorkflowSectionFilter != null || OverriddenTaskSectionFilter != null;

		#endregion

		#region Properties

		public BMBoardSection Section { get; private set; }

		public int CellsPerSubsection { get; private set; }

		public ZGuid ComponentPK { get; private set; }
		public ICollection<ZGuid> AllShownComponentPKs { get; private set; }
		public ZGuid ReleaseGroupPK { get; private set; }

		public bool IsBuffer { get; private set; }
		public bool IsBucket { get; private set; }
		public bool IsInConstrainedMode { get; private set; }

		public string TimeProgressionMode { get; private set; }
		public string TimeProgressionField { get; private set; }

		public bool HideCapabilityTasksFromResourceChannels { get; private set; }
		public bool HideResourceTasksFromCapabilityChannels { get; private set; }
		public bool ShowWorkflowCards { get; private set; }
		public bool ShowJobCards { get; private set; }

		public bool EnableShowCurrentItemsFilterByDefault { get; private set; }

		public bool ShowWorkflowOrJobWorkflowCards
		{
			get { return ShowWorkflowCards || ShowJobCards; }
		}

#if DEBUG
		public PopulateCardContentStrategy DummyStrategy_ForTest { get; set; }
#endif

		public PopulateCardContentStrategy GetNewCardContentStrategy()
		{
			return
#if DEBUG
DummyStrategy_ForTest ??
#endif
				(ShowWorkflowOrJobWorkflowCards ? new PopulateWorkflowCardStrategy() : new PopulateTaskCardStrategy());
		}

		public BMBoardSectionOrientation Orientation { get; private set; }

		public SizedButtonBorderStyle CountdownTargetBorderStyle { get; private set; }
		public SizedButtonBorderStyle CountdownStartableBorderStyle { get; private set; }
		public Color CountdownTargetBorderColor { get; private set; }
		public Color CountdownStartableBorderColor { get; private set; }

		public ZString SectionPanelLayoutStyle { get; private set; }

		public bool SuppressAcceptabilityBandVisualisation
		{
			get { return !BMSRegistryProvider.IsBufferManagementWorkflowModeOrBetterEnabled || IsPreview || !BMSRegistry.Instance.ShowAcceptabilityBandsOnBoards.Value; }
		}

		bool shouldShowAcceptabilityBandHeading;

		public ISectionSubHeadingAppearance SubHeadingAppearance
		{
			get { return subHeadingAppearance ?? new EmptySectionSubHeadingAppearance(); }
			set
			{
				subHeadingAppearance = value;
				OnPropertyChanged();
			}
		}
		ISectionSubHeadingAppearance subHeadingAppearance;

		public HashSet<ZGuid> SectionWorkflowPKsWithTaskAndSectionFiltersApplied => GetAllSectionTaskCards().Select(x => x.WorkflowIdentifier).ToHashSet();

		List<ICardContent> GetAllSectionTaskCards()
		{
			var taskCards = new List<ICardContent>();

			foreach (var cell in ComponentGrid.CardCells)
			{
				var cards = ComponentGrid.CardAllocationMap?.GetCards(cell);
				if (cards != null)
				{
					taskCards.AddRange(cards);
				}
			}

			return taskCards;
		}

		#endregion

		#region Validation

		public bool IsValid(BMBoardSection section)
		{
			var factory = section.Factory;
			var isValidationSuspended = factory.IsValidationSuspended;
			if (isValidationSuspended)
			{
				factory.ResumeValidation();
			}
			section.Validation.ValidateAll();
			foreach (var component in section.SectionConfiguration.AdditionalComponents)
			{
				component.Validation.ValidateAll();
				if (component.HasErrors)
				{
					return false;
				}
			}
			section.SectionConfiguration.Validation.ValidateAll();
			if (isValidationSuspended)
			{
				factory.SuspendValidation();
			}
			return !section.HasErrors;
		}

		#endregion

		#region Refresh

		public override void RefreshForPreview(IBMBoardSection section)
		{
			base.RefreshForPreview(section);
			var concreteSection = (BMBoardSection)section;

			LoadChannels(concreteSection);
		}

		public HashSet<ZGuid> CachedWorkflowPks => Cache.GetCachedValue<HashSet<ZGuid>>(SectionPK, CacheConstants.AllWorkflowPKs);

		public override bool ReloadRequired(IBMBoardSection section)
		{
			var concreteSection = (BMBoardSection)section;

			return base.ReloadRequired(section) || concreteSection.GetLatestCustomisationEditTimeUTC() != currentSectionEditDate;
		}

		void CacheCustomisationLastEditTime(IBMBoardSection section)
		{
			var concreteSection = (BMBoardSection)section;
			currentSectionEditDate = concreteSection.GetLatestCustomisationEditTimeUTC();
		}

		ZDateTime currentSectionEditDate = ZDateTime.Empty;

		#endregion

		#region Acceptability Bands

		void LoadBoardSectionAcceptabilityBands(BMComponentSectionConfiguration config)
		{
			if (!BMSRegistryProvider.IsBufferManagementWorkflowModeOrBetterEnabled)
			{
				return;
			}

			var allSectionAcceptabilityBands = config.AcceptabilityBands.Cast<BoardSectionAcceptabilityBand>().ToArray();

			foreach (var sectionBand in allSectionAcceptabilityBands)
			{
				sectionBand.AddBMComponentAcceptabilityBandFetchHint();
			}

			boardSectionAcceptabilityBands = allSectionAcceptabilityBands.Where(x => x.AcceptabilityBand?.BAB_IsActive ?? false).ToArray();
		}

		public BoardSectionAcceptabilityBand[] BoardSectionAcceptabilityBands => boardSectionAcceptabilityBands;
		BoardSectionAcceptabilityBand[] boardSectionAcceptabilityBands;

		public void RefreshAcceptabilityBandSubHeading(BoardSectionAcceptabilityBandResult[] results)
		{
			if (shouldShowAcceptabilityBandHeading && (CellsPerSubsection == 0 || CachedWorkflowPks != null))
			{
				var details = new ComponentAcceptabilityStatusDetails(results);
				SubHeadingAppearance = details.HeadingAppearance;
			}
		}

		public HashSet<ZGuid> GetCachedWorkflowPKsForABs(BusinessObjectFactory factory) => AreWorkflowOrTaskFiltersRedefined || ExperimentalSettingsProvider.SimpleBoardQueryEnabled(factory, BoardViewModel.BoardPK) ? null : CachedWorkflowPks;

		public BoardSectionAcceptabilityBandResult[] GetAcceptabilityBandResults(BusinessObjectFactory factory)
		{
			if (!BMSRegistryProvider.IsBufferManagementWorkflowModeOrBetterEnabled)
			{
				return null;
			}

			var workflowPKs = GetCachedWorkflowPKsForABs(factory);

			return BoardAcceptabilityBandCalculator.Calculate(factory, SectionPK, ReleaseGroupPK, BoardSectionAcceptabilityBands, workflowPKs);
		}

		#endregion

		#region Grid

		public ComponentGrid ComponentGrid { get; private set; }

		ComponentGrid BuildGrid(BMBoardSection section)
		{
			return section.SectionConfiguration.CellsPerSubsection > 0 ? new ComponentGrid(PrimaryChannels, SecondaryAxisChannels, section, IsPreview, IsInConstrainedMode) : ComponentGrid.Empty;
		}

		public void RefreshGrid(params ICardContent[] cards)
		{
			var factory = FactoryProvider.GetNewBackgroundThreadLoaderFactory("RefreshGrid");
			var tasks = cards.Select(s => s.TaskIdentifier).ToArray();
			var workflows = cards.Select(s => s.WorkflowIdentifier).ToArray();

			this.RefreshAll(new WorkflowUpdatedOperation(tasks, workflows, factory));
		}

		public void SetOriginalRowHeight(int row, float height)
		{
			originalRowHeights[row] = height;
		}

		public void SetOriginalColumnWidth(int column, float width)
		{
			originalColumnWidths[column] = width;
		}

		public float GetOriginalRowHeight(int row)
		{
			return originalRowHeights[row];
		}

		public float GetOriginalColumnWidth(int column)
		{
			return originalColumnWidths[column];
		}

		readonly float[] originalRowHeights;
		readonly float[] originalColumnWidths;

		#endregion

		#region Channels

		void LoadChannels(BMBoardSection section)
		{
			PrimaryChannels = ChannelFactory.CreatePrimaryChannels(this, section);
			SecondaryAxisChannels = ChannelFactory.CreateSecondaryChannels(this, section);

			foreach (var channel in AllChannels
				.Where(channel => channel is VisualBoardChannel)
				.Cast<VisualBoardChannel>())
			{
				channel.SeedCacheWithChannelDescriptor(section.Factory);
			}

			BMBoardChannels = section.SectionConfiguration.IsReleaseScheduler ? Array.Empty<BMBoardSectionChannel>() : GetBMBoardChannels(section.SectionConfiguration.Channels);
		}

		IEnumerable<BMBoardSectionChannel> GetBMBoardChannels(IEnumerable<BMBoardSectionChannel> sectionConfigChannels)
		{
			if (!sectionConfigChannels.Any(channel => channel.IsCurrentUser))
			{
				return sectionConfigChannels;
			}

			var result = new List<BMBoardSectionChannel>();
			foreach (var channel in sectionConfigChannels)
			{
				result.Add(channel.IsCurrentUser ? BMBoardSectionChannel.GetCurrentUserChannel(channel) : channel);
			}

			return result;
		}

		public IEnumerable<IVisualBoardChannel> PrimaryChannels { get; private set; }
		public IEnumerable<IVisualBoardChannel> SecondaryAxisChannels { get; private set; }
		public IEnumerable<BMBoardSectionChannel> BMBoardChannels { get; private set; }
		public IEnumerable<IVisualBoardChannel> AllChannels => PrimaryChannels.Union(SecondaryAxisChannels);

#if DEBUG
		public IVisualBoardChannel GetOrCreateChannelForTest(BMBoardSectionChannel channel, BusinessObjectFactory factory)
		{
			return PrimaryChannels.FirstOrDefault(visualBoardChannel => visualBoardChannel.EntityPK == channel.EntityPK ||
			(visualBoardChannel is UnchanneledChannel && channel.IsUnChanneled && !channel.MSC_ChannelType.IsEmpty))
				?? this.CreateChannelForTest(channel, factory);
		}

		public void AddChannelForTest(IVisualBoardChannel channel)
		{
			PrimaryChannels = PrimaryChannels.Concat(new[] { channel }).ToArray();
		}
#endif
		#endregion

		#region Customised Cards

		public BMControlCustomisation GetSummaryCard(string jobType)
		{
			return GetCard(summaryCardLayouts, jobType);
		}

		public BMControlCustomisation GetDetailedCard(string jobType)
		{
			return GetCard(detailedCardLayouts, jobType);
		}

		BMControlCustomisation GetCard(Dictionary<string, BMControlCustomisation> candidateLayouts, string jobType)
		{
			BMControlCustomisation layout;

			if (!candidateLayouts.TryGetValue(jobType, out layout))
			{
				layout = candidateLayouts[string.Empty];
			}

			return layout;
		}

		Dictionary<string, BMControlCustomisation> summaryCardLayouts;
		Dictionary<string, BMControlCustomisation> detailedCardLayouts;

		#endregion

		#region GetGradientAngle

		public float GradientAngle { get; private set; }

		static float GetGradientAngle(BMBoardSection section)
		{
			switch (section.SectionConfiguration.FlowDirection)
			{
				case FlowDirectionList.Codes.Down:
					return 90f;
				case FlowDirectionList.Codes.Left:
					return 180f;
				case FlowDirectionList.Codes.Up:
					return 270f;

				case FlowDirectionList.Codes.Right:
				default:
					return 0f;
			}
		}

		#endregion

		#region CachedProperties

		#region Ticket Bitmaps

		Tuple<ZGuid, ZDateTime> GetBitmapCacheKey(ICardContent cardContent) => Tuple.Create(cardContent.Identifier, cardContent.LastEditTime);

		public CardBitmaps GetCardBitmaps(ICardContent cardContent)
		{
			var key = GetBitmapCacheKey(cardContent);

			return cardBitmaps.GetValueSafe(key);
		}

		public CardBitmaps GetOrCreateCardBitmaps(ICardContent cardContent, Func<CardBitmaps> bitmapCreator)
		{
			var key = GetBitmapCacheKey(cardContent);

			if (cardBitmaps.TryGetValue(key, out var bitmaps))
			{
				return bitmaps;
			}
			else
			{
				bitmaps = bitmapCreator();

				if (bitmaps != null)
				{
					return cardBitmaps[key] = bitmaps;
				}

				return null;
			}
		}

		public void DisposeAllBitmaps()
		{
			foreach (var bitmap in cardBitmaps.Values)
			{
				bitmap.Dispose();
			}

			cardBitmaps.Clear();
		}

		public void RemoveUnusedBitmapsFromCache(HashSet<CardBitmaps> usedBitmaps)
		{
			foreach (var kvp in cardBitmaps.Where(pair => !usedBitmaps.Contains(pair.Value)).ToArray())
			{
				cardBitmaps.Remove(kvp.Key);
				kvp.Value.Dispose();
			}
		}

		readonly Dictionary<Tuple<ZGuid, ZDateTime>, CardBitmaps> cardBitmaps = new Dictionary<Tuple<ZGuid, ZDateTime>, CardBitmaps>();

		#endregion

		#region Tags

		public TagDefinitionCache TagDefinitionCache
		{
			get { return tagDefinitionCache ?? (tagDefinitionCache = new TagDefinitionCache(FactoryProvider.GetBestFactory(nameof(TagDefinitionCache)))); }
			internal set { tagDefinitionCache = value; }
		}
		TagDefinitionCache tagDefinitionCache;

		#endregion

		#region Populate Cache

		public static void CreateAndPopulatePropertyCache(TaskChannelMap tasks, BMBoardSectionViewModel viewModel, WorkflowUpdatedOperation context = null) // TODO: make context mandatory when board refresh uses this mechanism.
		{
			var cache = new PropertyCache();

			PopulateCacheForAll(tasks, viewModel, cache);
			BMBoardSection section = null;

			if (tasks.Any())
			{
				var factory = tasks.First().Factory;
				section = factory.Load<BMBoardSection>(viewModel.SectionPK);
			}

			if (viewModel.ShowJobCards)
			{
				TaskJobWorkflowCacheHelper.PopulateCacheForJobWorkflow(tasks, section, cache);
				viewModel.PopulateCacheMethodName = nameof(TaskJobWorkflowCacheHelper.PopulateCacheForJobWorkflow);
			}
			else if (viewModel.ShowWorkflowCards)
			{
				TaskJobWorkflowCacheHelper.PopulateCacheForWorkflows(tasks, section, cache);
				viewModel.PopulateCacheMethodName = nameof(TaskJobWorkflowCacheHelper.PopulateCacheForWorkflows);
			}
			else
			{
				TaskJobWorkflowCacheHelper.PopulateCacheForTasks(tasks, section, cache, viewModel.BoardViewModel.SlideShowViewModel.ReplaceService);
				viewModel.PopulateCacheMethodName = nameof(TaskJobWorkflowCacheHelper.PopulateCacheForTasks);
			}

			viewModel.PopulateCacheHasStartableTaskOnChannel(tasks, cache);

			if (context == null || context.RefreshType != BoardRefreshType.Partial)
			{
				viewModel.Cache.ReplaceWith(cache);
			}
			else
			{
				viewModel.Cache.MergeWith(cache);
			}
		}

		public string PopulateCacheMethodName { get; private set; }

		internal void PopulateCacheHasStartableTaskOnChannel(TaskChannelMap tasks, PropertyCache cache)
		{
			foreach (var channel in AllChannels.Where(c => c is ResourceChannel))
			{
				var tasksInChannel = tasks.TasksByChannel[channel];
				var hasStartableTaskOnChannel = tasksInChannel.Any(t => t.IsStartable());
				cache.OverwriteCachedValue(channel.EntityPK, CacheConstants.HasStartableTaskOnChannel, hasStartableTaskOnChannel);
			}
		}

		static void PopulateCacheForAll(TaskChannelMap tasks, BMBoardSectionViewModel viewModel, PropertyCache cache)
		{
			if (tasks.Length > 0)
			{
				cache.GetCachedValue(ZGuid.Empty, CacheConstants.TagDefinitions, () => viewModel.TagDefinitionCache);

				var taskGroups = viewModel.ShowJobCards
					? tasks.GroupBy(t => t.P9_ParentID)
					: tasks.GroupBy(t => t.P9_FH_ProcessHeader);

				if (!taskGroups.IsNullOrEmpty())
				{
					foreach (var def in viewModel.TagDefinitionCache.AllDefinitionsRelevantToCurrentWorkflowManagementMode)
					{
						tasks.Factory.ImportFromAnotherFactorySafe(def);
					}
					foreach (var mag in viewModel.TagDefinitionCache.AllMagnitudes)
					{
						tasks.Factory.ImportFromAnotherFactorySafe(mag);
					}
				}

				foreach (var group in taskGroups)
				{
					cache.GetCachedValue(group.Key, CacheConstants.EffectiveNudge, () => group.First().GetEffectiveNudge(viewModel));
				}
			}

			if (tasks.AllWorkflows != null)
			{
				var workflowPKs = tasks.AllWorkflows.Select(x => x.PK).ToHashSet();
				cache.OverwriteCachedValue(viewModel.SectionPK, CacheConstants.AllWorkflowPKs, workflowPKs);
			}
			else
			{
				cache.Remove(viewModel.SectionPK, CacheConstants.AllWorkflowPKs);
			}
		}

		internal ImmutableDictionary<ZString, RoadRunnerDetails> PopulateRoadRunnerDetails(BusinessObjectFactory factory, PropertyCache cache, CardAllocationMap allocationMap, IEnumerable<IVisualBoardChannel> channels, ZGuid releaseGroupPK)
		{
			var detailsDictionary = new Dictionary<ZString, RoadRunnerDetails>();

			var resourcePks = channels.Where(c => c.EntityType == ChannelTypeList.Codes.Resource).Select(c => c.EntityPK);
			var resources = resourcePks.Any() ? factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.PK, resourcePks)) : Array.Empty<GlbStaff>();
			if (resources.Length > 0)
			{
				var roadRunnerCache = new RoadRunnerTaskCache(ComponentPK, resources);

				foreach (var resource in resources)
				{
					factory.AddFetchHint(BMComponentResourceLinkSchema.FD_GS_NKResource, resource.GS_Code);
					factory.AddFetchHint(GlbStaffHolidaySchema.GA_GS, resource.PK);

					var workTimeQuery = new ZQuery(GlbWorkTimeSchema.GW_ParentID, resource.PK);
					workTimeQuery.AddToFilter(GlbWorkTimeSchema.GW_ParentTableCode, new ZString(GlbStaffSchema.Constants.Prefix));
					factory.AddFetchHint(GlbWorkTimeSchema.Instance, workTimeQuery);
				}

				detailsDictionary = GetAndCacheRoadRunnerDetailsDictionary(resources, cache, factory, roadRunnerCache);
			}

			var resourceChannels = channels.OfType<ResourceChannel>().ToArray();

			if (!BMSRegistry.Instance.DisableCapacityCalculations.Value)
			{
				ResourceChannel.SeedCacheCapacity(factory, cache, AllShownComponentPKs, resourceChannels);
			}

			ResourceChannel.SeedCacheWithStatus(factory, cache, allocationMap, resourceChannels);

			return detailsDictionary.ToImmutableDictionary();
		}

		Dictionary<ZString, RoadRunnerDetails> GetAndCacheRoadRunnerDetailsDictionary(GlbStaff[] resources, PropertyCache propertyCache, BusinessObjectFactory factory, RoadRunnerTaskCache taskCache = null)
		{
			var roadRunnerDictionary = RoadRunnerStatusCalculator.GetResourceCodeAndRoadRunnerDetailsDictionary(resources, ComponentPK, propertyCache, factory, taskCache);

			foreach (var resource in resources)
			{
				propertyCache.GetCachedValue(resource.PK, CacheConstants.ChannelRoadRunnerStatus, () => roadRunnerDictionary[resource.GS_Code]);
			}

			return roadRunnerDictionary;
		}

		internal RoadRunnerDetails GetAndCacheSingleResourceRoadRunnerDetails(GlbStaff resource, BusinessObjectFactory factory)
		{
			GlbStaff[] resources = { resource };
			return GetAndCacheRoadRunnerDetailsDictionary(resources, Cache, factory).First().Value;
		}

		#endregion

		#region CacheConstants

		public static class CacheConstants
		{
			public const string TagDefinitions = "TagDefinitions";
			public const string EffectiveNudge = "EffectiveNudge";
			public const string ChannelRoadRunnerStatus = "ChannelRoadRunnerStatus";
			public const string StaffByWorkflow = "StaffByWorkflow";
			public const string AssignedStaff = "AssignedStaff";
			public const string WorkflowOrderable = "WorkflowOrderable";
			public const string CapacityDto = "CapacityDto";
			public const string AllWorkflowPKs = "AllWorkflowPKs";
			public const string AcceptabilityBandResults = "AcceptabilityBandResults";
			public const string HasStartableTaskOnChannel = "HasStartableTaskOnChannel";
		}

		#endregion

		#endregion

		#region INotifyPropertyChanged

		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Partial Refresh Implementation

		public bool DataRefreshBusSubscriberRefreshesTickets
		{
			get
			{
				if (dataRefreshBusSubscriberRefreshesTickets == null)
				{
					dataRefreshBusSubscriberRefreshesTickets = BMSRegistry.Instance.UpdateTicketsWithDataRefresh.Value;
				}

				return dataRefreshBusSubscriberRefreshesTickets.Value;
			}
		}

		bool? dataRefreshBusSubscriberRefreshesTickets;

		protected override void PerformRefreshActionCore(IBoardRefreshContext refreshContext)
		{
			HandleWorkflowUpdatedOperation(refreshContext);
		}

		protected override void AddAggregatorCore(IBoardRefreshContext operation, HashSet<IBoardRefreshOperationAggregator> aggregators)
		{
			base.AddAggregatorCore(operation, aggregators);

			if (operation.RefreshType == BoardRefreshType.Full)
			{
				aggregators.Add(new FullRefreshOperationAggregator());
			}
			else if (operation.RefreshType == BoardRefreshType.Reload)
			{
				aggregators.Add(new BoardReloadOperationAggregator());
			}
		}

		public event EventHandler<LoadingIndicatorEventArgs> LoadingIndicatorMessageUpdated;

		public void NotifyLoadingIndicatorNeedsUpdating(LoadingIndicatorEventArgs args)
		{
			LoadingIndicatorMessageUpdated?.Invoke(this, args);
		}

		void HandleWorkflowUpdatedOperation(IBoardRefreshContext refreshContext)
		{
			if (refreshContext is WorkflowUpdatedOperation context)
			{
				ExecutePartialRefreshPipeEngine(context);
			}
		}

		void ExecutePartialRefreshPipeEngine(WorkflowUpdatedOperation context)
		{
			if (BoardViewModel.SlideShowViewModel.Dispatcher == null)
			{
				throw new InvalidOperationException("SlideShowViewModel.Dispatcher needs to be set in order to have boards respond to data refresh.");
			}

			var engine = PartialRefreshPipeEngine.Create(this, context);
			engine.DisplayingUpdatedTickets += OnDisplayingUpdatedTickets;
			engine.ExecuteAll(AsyncStrategy.Default, BoardViewModel.SlideShowViewModel.Dispatcher);
		}

		public event EventHandler DisplayingUpdatedTickets;

		void OnDisplayingUpdatedTickets(object sender, EventArgs e)
		{
			DisplayingUpdatedTickets?.Invoke(sender, e);
		}

		#endregion

		#region For Test
#if DEBUG
		public void ExecuteBeforeCardAllocation_ForTest()
		{
			BeforeCardAllocation_ForTest?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler BeforeCardAllocation_ForTest;

		public void ExecuteBeforeGetCardPenetration_ForTest(ProcessTask task)
		{
			BeforeGetCardPenetration_ForTest?.Invoke(this, new GetPenetrationEventArgs(task));
		}

		public event EventHandler<GetPenetrationEventArgs> BeforeGetCardPenetration_ForTest;

		public class GetPenetrationEventArgs : EventArgs
		{
			public GetPenetrationEventArgs(ProcessTask task)
			{
				Task = task;
			}

			public ProcessTask Task { get; private set; }
		}
#endif
		#endregion
	}
}
