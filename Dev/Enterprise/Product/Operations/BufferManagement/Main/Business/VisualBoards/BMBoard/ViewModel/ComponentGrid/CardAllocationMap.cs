using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.BufferManagement.Business.TaskJobWorkflowCacheHelper;

namespace Enterprise.BufferManagement.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Coupling in Type Estimates and Allocation enables ReadOnly API.")]
	public class CardAllocationMap
	{
		CardAllocationMap(Dictionary<CellContent, List<ICardContent>> cardsByCell, Dictionary<ZGuid, List<CellContent>> cellsByCard)
		{
			CardsByCell = cardsByCell;
			CellsByCard = cellsByCard;
		}

		Dictionary<CellContent, List<ICardContent>> CardsByCell { get; }
		Dictionary<ZGuid, List<CellContent>> CellsByCard { get; }

		#region Accessors

		public ICollection<CellContent> GetCells() => CardsByCell.Keys;

		public ICollection<ZGuid> GetCardPKs() => CellsByCard.Keys;

		public IReadOnlyCollection<CellContent> GetCells(ICardContent card) => GetCells(card.Identifier);

		public IReadOnlyCollection<CellContent> GetCells(ZGuid identifier)
		{
			if (CellsByCard.TryGetValue(identifier, out var cells))
			{
				return new ReadOnlyCollection<CellContent>(cells);
			}
			else
			{
				return Array.Empty<CellContent>();
			}
		}

		public IReadOnlyCollection<ICardContent> GetCards(CellContent cell)
		{
			if (CardsByCell.TryGetValue(cell, out var cards))
			{
				return new ReadOnlyCollection<ICardContent>(cards);
			}
			else
			{
				return new ReadOnlyCollection<ICardContent>(new List<ICardContent>());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Returned type should not be extended.")]
		public IEnumerable<KeyValuePair<CellContent, IEnumerable<ICardContent>>> GetCardsByCells()
		{
			foreach (var pair in CardsByCell)
			{
				yield return new KeyValuePair<CellContent, IEnumerable<ICardContent>>(pair.Key, new ReadOnlyCollection<ICardContent>(pair.Value));
			}
		}

		public CardAllocationMap Clone()
		{
			var clonedCardsByCell = CardsByCell.ToDictionary(entry => entry.Key, entry => entry.Value.ToList());
			var clonedCellsByCard = CellsByCard.ToDictionary(entry => entry.Key, entry => entry.Value.ToList());

			return new CardAllocationMap(clonedCardsByCell, clonedCellsByCard);
		}

		#endregion

		#region Allocation

		public static CardAllocationMap Failure(string text, Exception exception = null)
		{
			var result = new CardAllocationMap(new Dictionary<CellContent, List<ICardContent>>(), new Dictionary<ZGuid, List<CellContent>>());
			result.FailureDetails = new LoadFailureDetails(text, exception);
			return result;
		}

		public static CardAllocationMap NewAllocationMap(BMBoardSection section, BMBoardSectionViewModel viewModel, TaskChannelMap tasks)
		{
#if DEBUG
			viewModel.ExecuteBeforeCardAllocation_ForTest();
#endif
			try
			{
				return AllocateTasksAndGetNewMap(tasks, section, viewModel);
			}
			catch (WorkingTimeNotAvailableException ex)
			{
				return new CardAllocationMap(null, null) { FailureDetails = LoadCardContents.GetUnexpectedFailure(ex) };
			}
		}

		public LoadFailureDetails FailureDetails { get; private set; }

		static CardAllocationMap AllocateTasksAndGetNewMap(TaskChannelMap taskChannelMap, BMBoardSection section, BMBoardSectionViewModel viewModel)
		{
			var cardsByCell = new Dictionary<CellContent, List<ICardContent>>();
			var cellsByCard = new Dictionary<ZGuid, List<CellContent>>();
			var context = WorkingTimeContext.Create(section);
			var createdCards = UseBizoCardContents.Value ? new Dictionary<ZGuid, ICardContent>() : GetFactorylessCardContentFromChannelMap(taskChannelMap, viewModel);

			Func<IEnumerable<ProcessTask>, CellContent, IEnumerable<ProcessTask>> getTasksInCellFunc = null;

			var sectionConfiguration = section.SectionConfiguration;

			if (viewModel.IsBuffer && sectionConfiguration.TimeField == TimeProgressionFieldList.Codes.TransferTime)
			{
				var penetrations = GetBufferPenetrations(taskChannelMap, viewModel, context);
				var percentPerCell = ComponentGridHelper.GetPercentPerCell(sectionConfiguration);

				getTasksInCellFunc = (tasks, cell) => tasks.Where(task => DoesAgePercentMatchCell(cell, task.PK, percentPerCell, penetrations));
			}
			else if (sectionConfiguration.TimeField == TimeProgressionFieldList.Codes.WorkingTimeSinceStartable)
			{
				getTasksInCellFunc = (tasks, cell) => tasks.Where(task => DoesAgeIndexMatchCell(cell, sectionConfiguration, task));
			}
			else
			{
				var cachedWorkflowTimes = new Dictionary<ZGuid, TimeSpan>();
				getTasksInCellFunc = (tasks, cell) => tasks.Where(task => DoesAgeIndexMatchCell(cell, sectionConfiguration, task, context, cachedWorkflowTimes));
			}

			foreach (var cell in viewModel.ComponentGrid.CardCells)
			{
				var tasksInCellChannels = GetTasksInCellChannels(taskChannelMap, cell);
				var tasksInCell = getTasksInCellFunc(tasksInCellChannels, cell).ToArray();

				foreach (var task in tasksInCell)
				{
					var card = GetOrCreateCardContents(viewModel, createdCards, task);
					cellsByCard.GetOrAdd(card.Identifier, () => new List<CellContent>()).Add(cell);
				}

				var cardsInCell = tasksInCell.Select(task => GetOrCreateCardContents(viewModel, createdCards, task)).Distinct().ToList();
				cardsByCell[cell] = cardsInCell;
			}

			return new CardAllocationMap(cardsByCell, cellsByCard);
		}

		static IEnumerable<ProcessTask> GetTasksInCellChannels(TaskChannelMap taskMap, CellContent cell)
		{
			IEnumerable<ProcessTask> tasks;

			if (cell.Channel == null && cell.SecondaryChannel == null)
			{
				tasks = taskMap.AllTasks;
			}
			else
			{
				tasks = new[] { cell.Channel, cell.SecondaryChannel }
					.WhereNotNull()
					.Select(c => taskMap.TasksByChannel[c])
					.Aggregate((set1, set2) => set1.Intersect(set2));
			}

#if NETFRAMEWORK
			return tasks.DistinctBy(t => t.PK).Where(t => !t.IsDeleted);
#else
			return IEnumerableExtensions.DistinctBy(tasks, t => t.PK).Where(t => !t.IsDeleted);
#endif
		}

		#region GetFactorylessCardContentFromChannelMap

		static Dictionary<ZGuid, ICardContent> GetFactorylessCardContentFromChannelMap(TaskChannelMap taskChannelMap, BMBoardSectionViewModel viewModel)
		{
			var definitions = viewModel.Cache.GetCachedValue(ZGuid.Empty, BMBoardSectionViewModel.CacheConstants.TagDefinitions, () => viewModel.TagDefinitionCache);
			var staticControlPropertyCache = new PropertyCache();
			var showJobCards = viewModel.ShowJobCards;
			var strategy = viewModel.GetNewCardContentStrategy();
			var keyAndFactorylessCardContentDataPair = new Dictionary<ZGuid, FactorylessCardContentDto>();

			AddFetchHints(taskChannelMap, viewModel);

			foreach (var groupedTasks in taskChannelMap.AllTasks.Where(x => !x.IsDeleted && x.P9_FH_ProcessHeader.IsValid).GroupBy(t => viewModel.GetSummaryCard(t.WorkflowType)))
			{
				var taskWorkflowTuples = groupedTasks
					.Select(task => Tuple.Create(task, strategy.GetProcessHeaderForCardType(task, showJobCards)))
					.Where(tuple => tuple.Item1 != null && tuple.Item2 != null).ToArray();

				var customisationData = new CustomisedControlDataCache(
					groupedTasks.Key,
					staticControlPropertyCache,
					taskWorkflowTuples.Select(tuple => tuple.Item1).Distinct().ToArray(),
					taskWorkflowTuples.Select(tuple => tuple.Item2).Distinct().ToArray(),
					showJobCards,
					viewModel.ShowWorkflowCards
				);

				foreach (var (task, workflow) in taskWorkflowTuples)
				{
					var key = GetKey(viewModel, task);

					if (!keyAndFactorylessCardContentDataPair.ContainsKey(key))
					{
						var factorylessCardContentDto = new FactorylessCardContentDto(workflow, task, viewModel, customisationData, definitions, strategy);
						keyAndFactorylessCardContentDataPair.Add(key, factorylessCardContentDto);
					}
				}
			}

			return keyAndFactorylessCardContentDataPair.ToDictionary(entry => entry.Key, entry => (ICardContent)new FactorylessCardContent(entry.Value));
		}

		static void AddFetchHints(TaskChannelMap tasks, BMBoardSectionViewModel viewModel)
		{
			if (viewModel.ShowJobCards)
			{
				foreach (var processJobHeader in tasks.AllWorkflows)
				{
					tasks.Factory.AddFetchHint(typeof(ProcessTask), processJobHeader.GetTasksQuery());
				}
			}
		}

		static ICardContent GetOrCreateCardContents(BMBoardSectionViewModel viewModel, Dictionary<ZGuid, ICardContent> createdCards, ProcessTask task)
		{
			var key = GetKey(viewModel, task);
			return createdCards.GetOrAdd(key, () => CreateCardContents(viewModel, task));
		}

		static ZGuid GetKey(BMBoardSectionViewModel viewModel, ProcessTask task)
		{
			return viewModel.ShowWorkflowCards
				? task.P9_FH_ProcessHeader
				: viewModel.ShowJobCards
					? task.ProcessHeader.FH_FH_ParentHeader
					: task.PK;
		}

		static ICardContent CreateCardContents(BMBoardSectionViewModel viewModel, ProcessTask task)
		{
			if (viewModel.ShowWorkflowOrJobWorkflowCards)
			{
				return new WorkflowCardContent(task.GetProcessHeaderForCardType(viewModel.ShowJobCards), task, viewModel);
			}
			else
			{
				return new TaskCardContent(task, viewModel);
			}
		}

		#endregion

		#region DoesAgePercentMatchCell

		static Dictionary<ZGuid, decimal> GetBufferPenetrations(TaskChannelMap taskChannelMap, BMBoardSectionViewModel viewModel, WorkingTimeContext context)
		{
			var penetrations = new Dictionary<ZGuid, decimal>();
			var calculateForTask = !viewModel.ShowWorkflowOrJobWorkflowCards;
			var cache = viewModel.Cache;

			foreach (var task in taskChannelMap)
			{
#if DEBUG
				viewModel.ExecuteBeforeGetCardPenetration_ForTest(task);
#endif

				var processHeader = task.GetProcessHeaderForCardType(viewModel.ShowJobCards);

				if (processHeader == null || processHeader.IsDeleted)
				{
					continue;
				}

				var penetration = GetBufferPenetration(cache, context, processHeader, task, calculateForTask);

				penetrations.Add(task.PK, penetration);
			}

			return penetrations;
		}

		static decimal GetBufferPenetration(PropertyCache cache, WorkingTimeContext context, ProcessHeader processHeader, ProcessTask task, bool calculateForTask)
		{
			var key = calculateForTask ? task.PK : processHeader.PK;

			return cache.GetCachedValue(key, CacheConstants.Penetration, () =>
			{
				return calculateForTask ? processHeader.CalculatePenetrationPercentage(task, context) : processHeader.CalculatePenetrationPercentage(context);
			}) * 100;
		}

		public static decimal GetBufferPenetration(PropertyCache cache, WorkingTimeContext context, ProcessHeader processHeader)
		{
			return GetBufferPenetration(cache, context, processHeader, null, calculateForTask: false);
		}

		static bool DoesAgePercentMatchCell(CellContent cell, ZGuid taskPK, decimal percentPerCell, Dictionary<ZGuid, decimal> penetrations)
		{
			if (!penetrations.TryGetValue(taskPK, out var bufferPenetration))
			{
				return false;
			}

			return (cell.TimePercent - percentPerCell <= bufferPenetration || cell.IsFirstCell) && (bufferPenetration < cell.TimePercent || cell.IsLastCell);
		}

		#endregion

		#region DoesAgeIndexMatchCell

		public static double GetTimeIndex(BMComponentSectionConfiguration sectionConfiguration, IProcessHeader processHeader, WorkingTimeContext context, TimeSpan timePerCell, Dictionary<ZGuid, TimeSpan> cachedWorkflowTimes)
		{
			var timeIndexUnitsSinceSpecifiedDate = GetTimespanBetweenSpecifiedDateAndNow(sectionConfiguration, processHeader, context, cachedWorkflowTimes);
			return Math.Floor(timeIndexUnitsSinceSpecifiedDate.TotalMinutes / Math.Max(1, timePerCell.TotalMinutes));
		}

		public static double GetTimeIndex(BMComponentSectionConfiguration sectionConfiguration, ProcessTask task, TimeSpan timePerCell)
		{
			if (task.ProcessHeader != null)
			{
				task.ProcessHeader.OverrideContextProvider = sectionConfiguration.Section;
			}

			var timeIndexUnitsSinceSpecifiedDate = task.WorkingTimeSinceBecomingStartable;
			return Math.Floor(timeIndexUnitsSinceSpecifiedDate.TotalMinutes / Math.Max(1, timePerCell.TotalMinutes));
		}

		static bool DoesAgeIndexMatchCell(CellContent cell, BMComponentSectionConfiguration sectionConfiguration, ProcessTask task, WorkingTimeContext context, Dictionary<ZGuid, TimeSpan> cachedWorkflowTimes)
		{
			var timeIndexUnitsSinceSpecifiedDate = GetTimeIndex(sectionConfiguration, task.ProcessHeader, context, cell.TimeInCell, cachedWorkflowTimes);
			return DoesAgeIndexMatchCell(sectionConfiguration, cell, timeIndexUnitsSinceSpecifiedDate);
		}

		static bool DoesAgeIndexMatchCell(CellContent cell, BMComponentSectionConfiguration sectionConfiguration, ProcessTask task)
		{
			var timeIndexUnitsSinceSpecifiedDate = GetTimeIndex(sectionConfiguration, task, cell.TimeInCell);
			return DoesAgeIndexMatchCell(sectionConfiguration, cell, timeIndexUnitsSinceSpecifiedDate);
		}

		static bool DoesAgeIndexMatchCell(BMComponentSectionConfiguration sectionConfiguration, CellContent cell, double timeIndexUnitsSinceSpecifiedDate)
		{
			if (timeIndexUnitsSinceSpecifiedDate == cell.TimeIndex)
			{
				return true;
			}
			else if (cell.IsLastCell)
			{
				return sectionConfiguration.TimeProgressionMode == TimeProgressionModeList.Codes.Age
						? timeIndexUnitsSinceSpecifiedDate > cell.TimeIndex
						: timeIndexUnitsSinceSpecifiedDate < cell.TimeIndex;
			}
			else if (cell.IsFirstCell)
			{
				return sectionConfiguration.TimeProgressionMode == TimeProgressionModeList.Codes.Age
						? timeIndexUnitsSinceSpecifiedDate < cell.TimeIndex
						: timeIndexUnitsSinceSpecifiedDate > cell.TimeIndex;
			}

			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1068:Do Not Use Math.Round", Justification = "I'm working with doubles")]
		static TimeSpan GetTimespanBetweenSpecifiedDateAndNow(BMComponentSectionConfiguration sectionConfiguration, IProcessHeader processHeader, WorkingTimeContext workTimeContext, Dictionary<ZGuid, TimeSpan> cachedWorkflowTimes)
		{
			var timespan = TimeSpan.Zero;

			if (processHeader == null)
			{
				return timespan;
			}

			if (cachedWorkflowTimes.ContainsKey(processHeader.PK))
			{
				return cachedWorkflowTimes[processHeader.PK];
			}
			else
			{
				var dateInUTC = sectionConfiguration.GetTimeFieldValue(processHeader);
				if (dateInUTC.IsValid)
				{
					var now = workTimeContext.GetCurrentLocalTime(processHeader.Factory).ToDateTime();
					var date = workTimeContext.ToLocalTime(dateInUTC, processHeader.Factory).ToDateTime();
					var workTimeArithmetic = workTimeContext.GetWorkTimeArithmetic(processHeader.Factory);

					switch (sectionConfiguration.TimeProgressionMode)
					{
						case TimeProgressionModeList.Codes.Age:
							if (sectionConfiguration.TimeField == TimeProgressionFieldList.Codes.TransferTime)
							{
								var component = sectionConfiguration.Section.Component;
								if (component != null && component.IsChildBuffer && component.FC_OffsetInMinutes > 0)
								{
									date = workTimeArithmetic.GetDateTimeInWorkingHoursFutureOrPast(date, component.FC_OffsetInMinutes / 60.0);
								}
							}

							if (date < now)
							{
								timespan = workTimeArithmetic.TimeDifference(date, now);
							}
							else
							{
								timespan = workTimeArithmetic.TimeDifference(now, date);
								timespan = TimeSpan.FromMinutes(-timespan.TotalMinutes);
							}
							break;

						case TimeProgressionModeList.Codes.Due:
							if (now < date)
							{
								timespan = workTimeArithmetic.TimeDifference(now, date);
							}
							else
							{
								timespan = workTimeArithmetic.TimeDifference(date, now);
								timespan = TimeSpan.FromMinutes(-timespan.TotalMinutes);
							}
							break;
					}
				}

				cachedWorkflowTimes[processHeader.PK] = timespan = TimeSpan.FromMinutes(Math.Round(timespan.TotalMinutes)); // I'm working with doubles

				return timespan;
			}
		}

		#endregion

		#endregion

		#region Task Estimates

		internal decimal GetTaskEstimates(HashSet<int> primaryAxes, HashSet<int> secondaryAxes, HashSet<ZString> staffCodes, CCRFilter ccrFilter, bool includePenetratedComponents, params ZGuid[] components)
		{
			var taskViewModelsMatchingConditions = CardsByCell.Where(pair => (primaryAxes.IsNullOrEmpty() || primaryAxes.Contains(pair.Key.PrimaryAxis)) && (secondaryAxes.IsNullOrEmpty() || secondaryAxes.Contains(pair.Key.SecondaryAxis)))
				.SelectMany(s => s.Value).Select(c => c.CapacityDto).Distinct()
				.Where(card => (staffCodes.IsNullOrEmpty() || staffCodes.Contains(card.AssignedResourceCode) || card.AssignedStaff.Any(staffCode => staffCodes.Contains(staffCode)))
					&& (components.Length == 0 || !components.Any(c => !c.IsEmpty) || components.Contains(card.ComponentPK) || (includePenetratedComponents && components.Intersect(card.PenetratedComponentsPK).Any())));

			return GetTaskEstimatesWithFilter(taskViewModelsMatchingConditions, ccrFilter);
		}

		internal decimal GetCurrentStartableTaskStackEstimates(IVisualBoardChannel channel, params ZGuid[] components)
		{
			var total = 0m;
			if (channel.EntityType == ChannelTypeList.Codes.Resource && channel.EntityPK.IsValid)
			{
				var resourceCode = channel.ChannelEntityCode;

				var viewModelsInCurrentComponent = CardsByCell.SelectMany(c => c.Value).Where(t =>
					(components.Length == 0 || components.Contains(t.CapacityDto.ComponentPK)));

				var applicableWorkflows = viewModelsInCurrentComponent.Where(t =>
					t.CapacityDto.AssignedResourceCode == resourceCode
					&& t.IsCurrent).Select(t => t.WorkflowIdentifier);

				var workflowsToEstimate = viewModelsInCurrentComponent.Where(t => applicableWorkflows.Any(w => w == t.WorkflowIdentifier)).GroupBy(t => t.WorkflowIdentifier).ToArray();

				total = workflowsToEstimate.Sum(g => GetApplicableTaskEstimates(resourceCode, g));
			}
			return total;
		}

		static decimal GetApplicableTaskEstimates(ZString resourceCode, IGrouping<ZGuid, ICardContent> viewModels)
		{
			var nonStaffTask = viewModels.FirstOrDefault(t => t.CapacityDto.AssignedResourceCode != resourceCode);
			Func<ICardContent, bool> whereFunc = null;

			if (nonStaffTask != null)
			{
				whereFunc = t => t.CapacityDto.AssignedResourceCode == resourceCode && t.CapacityDto.Sequence <= nonStaffTask.CapacityDto.Sequence;
			}
			else
			{
				whereFunc = t => true;
			}

			return viewModels.Where(whereFunc).Sum(t => t.CapacityDto.RelevantEstimateHours);
		}

		decimal GetTaskEstimatesWithFilter(IEnumerable<ICardCapacityDto> tasks, CCRFilter ccrFilter)
		{
			var estimates = 0m;
			switch (ccrFilter)
			{
				case CCRFilter.CCR:
					estimates = tasks.Where(t => t.IsAssignedToCCR).Sum(t => t.RelevantEstimateHours);
					break;
				case CCRFilter.NonCCR:
					estimates = tasks.Where(t => !t.IsAssignedToCCR).Sum(t => t.RelevantEstimateHours);
					break;
				case CCRFilter.InCCRWorkflow:
					estimates = tasks.Where(t => t.IsInCCRWorkflow).Sum(t => t.RelevantEstimateHours);
					break;
				default:
					estimates = tasks.Sum(t => t.RelevantEstimateHours);
					break;
			}
			return estimates;
		}

		internal bool DoesAnyCurrentTaskMatch(ZString staffCode, int secondaryAxis)
		{
			return CardsByCell.Where(pair => pair.Key.SecondaryAxis == secondaryAxis)
				.SelectMany(pair => pair.Value)
				.Any(card => card.CapacityDto.AssignedResourceCode == staffCode && card.IsCurrent);
		}

		#endregion

		#region Merging Maps

		internal Tuple<CardAllocationMap, HashSet<CellContent>> MergeIntoAndFindCellsToRefresh(CardAllocationMap newAllocationMap, ZGuid[] relatedEntities, IEnumerable<CellContent> additionalModifiedCells)
		{
			var oldAllocationMap = this;
			var allocationMapForMerge = oldAllocationMap.Clone();
			var modifiedCells = new HashSet<CellContent>();

			var itemsInCellsPreviously = relatedEntities.Where(pk => oldAllocationMap.CellsByCard.ContainsKey(pk)).ToHashSet();
			var itemsInCellsNow = relatedEntities.Where(pk => newAllocationMap.CellsByCard.ContainsKey(pk)).ToHashSet();

			foreach (var itemPK in itemsInCellsNow)
			{
				AddItemToMap(modifiedCells, allocationMapForMerge, itemPK, oldAllocationMap, newAllocationMap);
			}

			foreach (var itemPK in itemsInCellsPreviously.Where(pk => !itemsInCellsNow.Contains(pk)))
			{
				RemoveItemNoLongerInMap(modifiedCells, allocationMapForMerge, itemPK);
			}

			foreach (var cell in additionalModifiedCells.Where(c => oldAllocationMap.CardsByCell.ContainsKey(c)))
			{
				modifiedCells.Add(cell);
			}

			return Tuple.Create(allocationMapForMerge, modifiedCells);
		}

		static void AddItemToMap(HashSet<CellContent> modifiedCells, CardAllocationMap allocationMapForMerge, ZGuid itemPK, CardAllocationMap oldAllocationMap, CardAllocationMap newAllocationMap)
		{
			var newCells = newAllocationMap.CellsByCard[itemPK].ToHashSet();

			foreach (var cell in newCells)
			{
				modifiedCells.Add(cell);

				var setToAdd = newAllocationMap.CardsByCell[cell].ToHashSet();
				var listToModify = allocationMapForMerge.CardsByCell[cell];

				listToModify.RemoveAll(c => setToAdd.Contains(c));
				listToModify.AddRange(setToAdd);
			}

			var oldCells = oldAllocationMap.CellsByCard.TryGetValue(itemPK, out var oldCellList)
				? oldCellList.ToHashSet()
				: new HashSet<CellContent>();
			var cellsToModify = oldCells.Where(c => !newCells.Contains(c)).ToHashSet();
			var cells = allocationMapForMerge.CellsByCard.GetOrAdd(itemPK, () => new List<CellContent>());

			var cellsLookup = cells.ToHashSet();
			cells.AddRange(newCells.Where(c => !cellsLookup.Contains(c)));
			cells.RemoveAll(c => cellsToModify.Contains(c));

			foreach (var cell in cellsToModify)
			{
				modifiedCells.Add(cell);
				allocationMapForMerge.CardsByCell[cell].RemoveAll(c => c.Identifier == itemPK);
			}
		}

		static void RemoveItemNoLongerInMap(HashSet<CellContent> modifiedCells, CardAllocationMap allocationMapForMerge, ZGuid itemPK)
		{
			var cells = allocationMapForMerge.CellsByCard[itemPK];
			allocationMapForMerge.CellsByCard.Remove(itemPK);
			foreach (var cell in cells)
			{
				modifiedCells.Add(cell);
				allocationMapForMerge.CardsByCell[cell].RemoveAll(c => c.Identifier == itemPK);
			}
		}

		#endregion

		#region Test

		public readonly static Overridable<bool> UseBizoCardContents = new Overridable<bool>(false);

#if DEBUG

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Dictionary<CellContent, List<ICardContent>> CardsByCell_ForTest => CardsByCell;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Dictionary<ZGuid, List<CellContent>> CellsByCard_ForTest => CellsByCard;

		public void AddCard_ForTest(CellContent cellContent, ICardContent taskCardContent, bool unique = true)
		{
			if (unique)
			{
				var cellContents = CellsByCard.GetOrAdd(taskCardContent.Identifier, () => new List<CellContent>());

				if (!cellContents.Contains(cellContent))
				{
					cellContents.Add(cellContent);
				}

				var cardContents = CardsByCell.GetOrAdd(cellContent, () => new List<ICardContent>());

				if (!cardContents.Contains(taskCardContent, new CardEqualityComparer()))
				{
					cardContents.Add(taskCardContent);
				}
			}
			else
			{
				CellsByCard.GetOrAdd(taskCardContent.Identifier, () => new List<CellContent>()).Add(cellContent);
				CardsByCell.GetOrAdd(cellContent, () => new List<ICardContent>()).Add(taskCardContent);
			}
		}

		class CardEqualityComparer : IEqualityComparer<ICardContent>
		{
			bool IEqualityComparer<ICardContent>.Equals(ICardContent x, ICardContent y)
			{
				return x.Identifier == y.Identifier;
			}

			int IEqualityComparer<ICardContent>.GetHashCode(ICardContent obj)
			{
				return obj.Identifier.GetHashCode();
			}
		}

#endif
		#endregion
	}
}
