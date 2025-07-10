using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class ComponentGrid
	{
		public ComponentGrid(IEnumerable<IChannel> primaryChannels,
			IEnumerable<IChannel> secondaryChannels,
			BMBoardSection section,
			bool isPreview,
			bool isInConstrainedMode
			)
		{
			Orientation = section.SectionConfiguration.OrientationValue;
			IsInConstrainedMode = isInConstrainedMode;
			ComponentGridBuilderProvider.Build(this, primaryChannels, secondaryChannels, section, isPreview, IsInConstrainedMode);
		}

		ComponentGrid()
		{
			CellContent = Array.Empty<CellContent[]>();
		}

		public CellContent this[int row, int col]
		{
			get { return CellContent[row][col]; }
		}

		public IEnumerable<CellContent> Cells
		{
			get { return CellContent.SelectMany(c => c); }
		}

		public CardAllocationMap CardAllocationMap { get; private set; }
		public FilterMap FilterMap { get; private set; }

		internal CellContent[][] CellContent { get; set; }

		public int TotalRows { get; internal set; }
		public int TotalColumns { get; internal set; }
		public BMBoardSectionOrientation Orientation { get; private set; }

		public BMComponent CurrentlySelectedComponent { get; set; }

		bool IsInConstrainedMode { get; }

		internal static ComponentGrid Empty
		{
			get { return new ComponentGrid(); }
		}

		#region Load Conditions

		#region Refresh Headings

		public void RefreshHeadings(ChannelHeadingViewModelSet channelViewModels)
		{
			if (HeadingsRefreshed != null)
			{
				HeadingsRefreshed(this, new RefreshHeadingsEventArgs(channelViewModels));
			}
		}

		public EventHandler<RefreshHeadingsEventArgs> HeadingsRefreshed;

		public class RefreshHeadingsEventArgs : EventArgs
		{
			public RefreshHeadingsEventArgs(ChannelHeadingViewModelSet channelViewModels)
			{
				ChannelViewModels = channelViewModels;
			}

			public ChannelHeadingViewModelSet ChannelViewModels { get; }
		}

		#endregion

		#region Success

		public void RefreshComponent(BusinessObjectFactory factory, BMBoardSectionViewModel viewModel, CardAllocationMap allocationMap, bool requiresFullRedraw)
		{
			CardAllocationMap = allocationMap;
			ShowAllocatedTasks(factory, viewModel, allocationMap, requiresFullRedraw: requiresFullRedraw);
		}

		public void ShowAllocatedTasks(BusinessObjectFactory factory, BMBoardSectionViewModel viewModel, CardAllocationMap cardAllocation, IEnumerable<CellContent> cellsOverride = null, bool requiresFullRedraw = true)
		{
			CardAllocationMap = cardAllocation;
			var filters = viewModel.FilterManager.AppliedAndInheritedFilters;
			RefreshCells(factory, viewModel, cardAllocation, cellsOverride, filters, requiresFullRedraw: requiresFullRedraw);
		}

		void RefreshCells(BusinessObjectFactory factory, BMBoardSectionViewModel viewModel, CardAllocationMap cardAllocation, IEnumerable<CellContent> cellsOverride, IEnumerable<IBoardFilter> filters, bool requiresFullRedraw)
		{
			var cells = cellsOverride ?? cardAllocation.GetCells();

			if (cells != null && cardAllocation != null)
			{
				var filterApplicator = new CardVisibilityFilterApplicator(factory, viewModel, filters);
				FilterMap = filterApplicator.PopulateFilterMap(FilterMap, cardAllocation, cells);

				using (CellsRefreshing())
				{
					foreach (var cell in cells)
					{
						cell.OnCardsRefreshed(requiresFullRedraw);
					}
				}
			}
		}

		IDisposable CellsRefreshing()
		{
			OnCellRefreshStarted();
			return new DisposableList(new[] { new DisposableAction(OnCellRefreshFinished) });
		}

		void OnCellRefreshStarted()
		{
			CellRefreshStarted?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler CellRefreshStarted;

		void OnCellRefreshFinished()
		{
			CellRefreshFinished?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler CellRefreshFinished;

		#endregion

		#region Failure

		public void ShowLoadFailure(LoadFailureDetails failure)
		{
			if (LoadFailed != null)
			{
				LoadFailed(this, new LoadFailureEventArgs(failure));
			}
		}

		public event EventHandler<LoadFailureEventArgs> LoadFailed;

		public class LoadFailureEventArgs : EventArgs
		{
			public LoadFailureEventArgs(LoadFailureDetails failure)
			{
				Failure = failure;
			}

			public LoadFailureDetails Failure { get; private set; }
		}

		#endregion

		#endregion

		#region ColorFade

		internal void SetColorFades(BMComponentSectionConfiguration sectionConfiguration, CardAllocationMap allocationMap)
		{
			if (sectionConfiguration.FadeBackgroundAtPercentage > 0)
			{
				var cells = ComponentGridHelper.FlattenSequence(CellContent, sectionConfiguration);

				SetColorFadesForChannel(sectionConfiguration, allocationMap, cells);
				SetColorFadesForAgeHeading(sectionConfiguration, allocationMap, cells);
				SetColorFadesForSubComponentHeading(sectionConfiguration, allocationMap, cells);
				SetColorFadesForCCRHeading(sectionConfiguration, allocationMap, cells);
			}
		}

		void SetColorFadesForChannel(BMComponentSectionConfiguration sectionConfiguration, CardAllocationMap allocationMap, CellContent[] cells)
		{
			foreach (var channelCellGroup in cells.Where(c => c.ContentType == CellContentType.Cards).GroupBy(c => c.Channel))
			{
				var firstCell = channelCellGroup.First();
				var totalWorkForChannel = GetTaskEstimatesInCells(allocationMap, firstCell.PrimaryAxis.WrapWithEnumerable());

				SetColorFadesCore(channelCellGroup, totalWorkForChannel, sectionConfiguration, allocationMap);
			}
		}

		void SetColorFadesForAgeHeading(BMComponentSectionConfiguration sectionConfiguration, CardAllocationMap allocationMap, CellContent[] cells)
		{
			var ageHeaders = cells.Where(c => c.ContentType == CellContentType.AgeHeading).ToArray();
			if (ageHeaders.Length > 0)
			{
				var totalWork = GetTaskEstimatesInCells(allocationMap, secondaryAxes: ageHeaders.Select(c => c.SecondaryAxis));
				SetColorFadesCore(ageHeaders, totalWork, sectionConfiguration, allocationMap);
			}
		}

		void SetColorFadesForSubComponentHeading(BMComponentSectionConfiguration sectionConfiguration, CardAllocationMap allocationMap, CellContent[] cells)
		{
			if (sectionConfiguration.ShowChildComponentZones)
			{
				SetColorFadesForSubComponentHeadingCore(sectionConfiguration, allocationMap, cells, c => c.ContentType == CellContentType.SubComponentZoneHeading && c.SubComponentHeadingPK.IsValid);
			}
			else
			{
				SetColorFadesForSubComponentHeadingCore(sectionConfiguration, allocationMap, cells, c => c.ContentType == CellContentType.SubComponentZoneHeading && c.SubComponentHeadingPK.In(c.PreConstraintPKs));
				SetColorFadesForSubComponentHeadingCore(sectionConfiguration, allocationMap, cells, c => c.ContentType == CellContentType.SubComponentZoneHeading && c.SubComponentHeadingPK.In(c.PostConstraintPKs));
			}
		}

		IEnumerable<IGrouping<ZGuid, CellContent>> GroupBySubComponentHeadingCells(IGrouping<int, CellContent> subComponentHeadingCells, BMComponentSectionConfiguration sectionConfiguration)
		{
			return sectionConfiguration.ShowChildComponentZones
				? subComponentHeadingCells.GroupBy(c => c.SubComponentHeadingPK, c => c)
				: subComponentHeadingCells.GroupBy(c => (c.SubComponentHeadingPK.In(c.PreConstraintPKs) ? c.PreConstraintPKs : c.PostConstraintPKs).First(), c => c);
		}

		void SetColorFadesForSubComponentHeadingCore(BMComponentSectionConfiguration sectionConfiguration, CardAllocationMap allocationMap, CellContent[] cells, Func<CellContent, bool> cellFilterPredicate)
		{
			var cellsGroupedByPrimaryAxis = cells
					.Where(c => cellFilterPredicate(c))
					.GroupBy(g => g.PrimaryAxis);

			foreach (var subComponentHeadingCells in cellsGroupedByPrimaryAxis)
			{
				var nonCCRsInBlock = GetNonCCRsForSubComponentHeading(cells, subComponentHeadingCells.First(), sectionConfiguration.Section);

				foreach (var subComponentHeadingCellsGroupedBySubComponentPK in GroupBySubComponentHeadingCells(subComponentHeadingCells, sectionConfiguration))
				{
					var subComponentPK = subComponentHeadingCellsGroupedBySubComponentPK.Key;

					var secondaryAxes = sectionConfiguration.FlowsInSameDirectionAsAxis()
						? Enumerable.Range(0, subComponentHeadingCells.MaxOrDefault(c => c.SecondaryAxis) + 1)
						: Enumerable.Range(subComponentHeadingCells.MinOrDefault(c => c.SecondaryAxis), sectionConfiguration.CellsPerSubsection).Reverse();

					var subComponentPrimaryAxis = subComponentHeadingCellsGroupedBySubComponentPK.Select(c => c.PrimaryAxis).First();

					var totalEstimates =
						GetTaskEstimatesInCells(
							allocationMap,
							secondaryAxes: secondaryAxes,
							staffCodes: nonCCRsInBlock,
							ccrFilter: CCRFilter.InCCRWorkflow,
							includePenetratedComponents: true,
							components: subComponentPK);

					var subComponentHeadingCellsForCalculation = GetSubComponentHeadingCellsForFadeCalculation(sectionConfiguration, secondaryAxes, subComponentHeadingCells, subComponentPrimaryAxis);

					SetColorFadesCore(
						subComponentHeadingCellsForCalculation,
						totalEstimates,
						sectionConfiguration,
						allocationMap,
						ccrFilter: CCRFilter.InCCRWorkflow,
						includePenetratedComponents: true,
						componentPK: subComponentPK,
						staffCodes: nonCCRsInBlock);
				}
			}
		}

		IEnumerable<CellContent> GetSubComponentHeadingCellsForFadeCalculation(BMComponentSectionConfiguration sectionConfiguration, IEnumerable<int> secondaryAxes, IEnumerable<CellContent> subComponentHeadingCells, int subComponentPrimaryAxis)
		{
			var subComponentHeadingCellsForCalculation = new List<CellContent>();
			var subComponentHeadingCellsForThisPrimaryAxis = subComponentHeadingCells.Where(c => c.PrimaryAxis == subComponentPrimaryAxis);

			foreach (var secondaryAxis in secondaryAxes)
			{
				var cellExist = subComponentHeadingCellsForThisPrimaryAxis.FirstOrDefault(c => c.SecondaryAxis == secondaryAxis);

				if (cellExist != null)
				{
					subComponentHeadingCellsForCalculation.Add(cellExist);
				}
				else
				{
					var row = sectionConfiguration.OrientationValue == BMBoardSectionOrientation.Vertical
						? secondaryAxis
						: subComponentPrimaryAxis;

					var col = sectionConfiguration.OrientationValue == BMBoardSectionOrientation.Vertical
						? subComponentPrimaryAxis
						: secondaryAxis;

					// Includes tasks in cell without corresponding subcomponent heading
					var imaginaryCellForCalculation = new CellContent(row, col, CellContentType.SubComponentZoneHeading, sectionConfiguration);
					imaginaryCellForCalculation.BackColor = Color.Blue; //Assign any color so it can be used for calculation
					subComponentHeadingCellsForCalculation.Add(imaginaryCellForCalculation);
				}
			}

			return subComponentHeadingCellsForCalculation;
		}

		IEnumerable<ZString> GetNonCCRsForSubComponentHeading(IEnumerable<CellContent> cells, CellContent subComponentHeadingCell, BMBoardSection section)
		{
			var nonCCRs = new List<ZString>();

			var channelHeadingCells = cells
				.Where(c => c.ContentType == CellContentType.ChannelHeading && c.PrimaryAxis < subComponentHeadingCell.PrimaryAxis)
				.OrderByDescending(c => c.PrimaryAxis);

			foreach (var headingCell in channelHeadingCells)
			{
				if (headingCell.Channel != null && !headingCell.Channel.IsCCRChannel(section))
				{
					var channel = headingCell.Channel;
					if (channel.EntityType == ChannelTypeList.Codes.Resource && !channel.ChannelEntityCode.IsEmpty)
					{
						nonCCRs.Add(channel.ChannelEntityCode);
					}
				}
				else
				{
					break;
				}
			}
			return nonCCRs;
		}

		void SetColorFadesForCCRHeading(BMComponentSectionConfiguration sectionConfiguration, CardAllocationMap allocationMap, CellContent[] cells)
		{
			var ccrHeadingsList = cells.Where(c => c.ContentType == CellContentType.CCRHeading).GroupBy(g => g.PrimaryAxis);

			foreach (var ccrHeading in ccrHeadingsList)
			{
				var ccrCellContents = ccrHeading.ToArray();
				if (ccrCellContents.Length > 0)
				{
					var ccrsInBlock = new List<ZString>();
					bool nextChannelIsCCR = true;

					while (nextChannelIsCCR)
					{
						var firstHeadingCell = ccrCellContents[0];
						var nextCell = GetCell(firstHeadingCell.PrimaryAxis + ccrsInBlock.Count + 1, firstHeadingCell.SecondaryAxis);

						if (nextCell == null || nextCell.Channel == null)
						{
							break;
						}

						var channel = nextCell.Channel;
						nextChannelIsCCR = channel.EntityType == ChannelTypeList.Codes.Resource
							&& !channel.ChannelEntityCode.IsEmpty
							&& ConstrainedModeHelper.IsDesignatedCapacityConstrainedResourceInConstrainedReleaseGroup(sectionConfiguration.Section.Component.Factory, channel.ChannelEntityCode, sectionConfiguration.Section.Component);
						if (nextChannelIsCCR)
						{
							ccrsInBlock.Add(channel.ChannelEntityCode);
						}
					}

					decimal totalWork = GetTaskEstimatesInCells(
						allocationMap,
						secondaryAxes: ccrCellContents.Select(c => c.SecondaryAxis),
						staffCodes: ccrsInBlock,
						ccrFilter: CCRFilter.CCR);

					SetColorFadesCore(ccrCellContents, totalWork, sectionConfiguration, allocationMap, ccrFilter: CCRFilter.CCR, staffCodes: ccrsInBlock);
				}
			}
		}

		CellContent GetCell(int primaryIndex, int secondaryIndex)
		{
			var i = Orientation == BMBoardSectionOrientation.Vertical ? secondaryIndex : primaryIndex;
			var j = Orientation == BMBoardSectionOrientation.Vertical ? primaryIndex : secondaryIndex;

			var array = i < CellContent.Length ? CellContent[i] : null;
			return array != null && j < array.Length ? array[j] : null;
		}

		static void SetColorFadesCore(IEnumerable<CellContent> cells, decimal totalWork, BMComponentSectionConfiguration sectionConfiguration, CardAllocationMap allocationMap, CCRFilter ccrFilter = CCRFilter.None, IEnumerable<ZString> staffCodes = null, bool includePenetratedComponents = false, ZGuid componentPK = default(ZGuid))
		{
			if (totalWork > 0)
			{
				var foundFadeCell = false;
				var runningTotal = 0.0m;
				CellContent skipTillCell = null;

				foreach (var cell in cells.Where(c => c.BackColor.HasValue))
				{
					var canFade = CanFade(cell, componentPK, sectionConfiguration);

					if (canFade)
					{
						if (cell.ContentType == CellContentType.AgeHeading)
						{
							cell.IsBoldLabel = false;
						}
						cell.BackgroundFadeColor = null;
					}

					if (!foundFadeCell)
					{
						if (skipTillCell != null && skipTillCell != cell)
						{
							continue;
						}

						runningTotal += GetRunningTotal(sectionConfiguration, allocationMap, ccrFilter, staffCodes, cell, includePenetratedComponents, componentPK);

						var startFading = runningTotal / totalWork * 100 > sectionConfiguration.FadeBackgroundAtPercentage;
						if (startFading && canFade)
						{
							if (cell.CollapsedToCell == null)
							{
								foundFadeCell = true;
								if (cell.ContentType == CellContentType.AgeHeading)
								{
									cell.IsBoldLabel = true;
								}

								cell.BackgroundFadeColor = cell.BackColor.Value.FadeTowardsWhite();
								skipTillCell = null;
							}
							else
							{
								skipTillCell = cell.CollapsedToCell;
							}
						}
					}
					else
					{
						if (canFade)
						{
							cell.BackColor = cell.BackColor.Value.FadeTowardsWhite();
						}
					}
				}
			}
			else
			{
				foreach (var cell in cells)
				{
					if (CanFade(cell, componentPK, sectionConfiguration))
					{
						cell.BackgroundFadeColor = null;
					}
				}
			}
		}

		static bool CanFade(CellContent cell, ZGuid componentPK, BMComponentSectionConfiguration sectionConfiguration)
		{
			if (cell.ContentType == CellContentType.SubComponentZoneHeading)
			{
				return sectionConfiguration.ShowChildComponentZones
					? cell.SubComponentHeadingPK == componentPK && cell.Zone != null
					: cell.SubComponentZones.Any(z => z.Key == componentPK);
			}

			return true;
		}

		static decimal GetRunningTotal(BMComponentSectionConfiguration sectionConfiguration, CardAllocationMap allocationMap, CCRFilter ccrFilter, IEnumerable<ZString> staffCodes, CellContent cell, bool includePenetratedComponents = false, ZGuid componentPK = default(ZGuid))
		{
			var runningTotal = 0m;

			if (cell.ContentType.IsHeadingType())
			{
				if (staffCodes != null)
				{
					runningTotal += GetTaskEstimatesInCells(
						allocationMap,
						secondaryAxes: cell.SecondaryAxis.WrapWithEnumerable(),
						staffCodes: staffCodes,
						ccrFilter: ccrFilter,
						includePenetratedComponents: includePenetratedComponents,
						components: componentPK);
				}
				else
				{
					if (cell.GetHeaderOrientation(sectionConfiguration) == cell.Orientation)
					{
						runningTotal += GetTaskEstimatesInCells(
							allocationMap,
							primaryAxes: cell.PrimaryAxis.WrapWithEnumerable(),
							ccrFilter: ccrFilter,
							includePenetratedComponents: includePenetratedComponents,
							components: componentPK);
					}
					else
					{
						runningTotal += GetTaskEstimatesInCells(
							allocationMap,
							secondaryAxes: cell.SecondaryAxis.WrapWithEnumerable(),
							ccrFilter: ccrFilter,
							includePenetratedComponents: includePenetratedComponents,
							components: componentPK);
					}
				}
			}
			else
			{
				runningTotal += GetTaskEstimatesInCells(allocationMap,
					primaryAxes: cell.PrimaryAxis.WrapWithEnumerable(),
					secondaryAxes: cell.SecondaryAxis.WrapWithEnumerable(),
					ccrFilter: ccrFilter,
					includePenetratedComponents: includePenetratedComponents,
					components: componentPK);
			}

			return runningTotal;
		}

		public string GetWorkingZoneStatus(IVisualBoardChannel channel, BMBoardSection section, CardAllocationMap allocationMap)
		{
			var status = string.Empty;
			var subcomponentStatus = string.Empty;

			var componentZones = GetAllComponentZones(channel, section, allocationMap);
			var primaryComponentZones = componentZones.Where(c => c.IsPrimary).ToArray();

			if (primaryComponentZones.IsNullOrEmpty())
			{
				return status;
			}

			var zone = GetChannelZoneCore(componentZones);

			if (IsInConstrainedMode && !channel.IsCCRChannel(section))
			{
				var subcomponentZones = componentZones.OfType<ComponentZoneInfo>().ToArray();
				if (subcomponentZones.Length > 0)
				{
					if (componentZones.All(c => c.Zone <= 1))
					{
						subcomponentStatus = subcomponentZones.MinBy(c => c.OffsetInMinutes).Name;
					}
					else if (componentZones.All(c => c.Zone.IsInRange(2, 3)))
					{
						subcomponentStatus = string.Empty;

						var primaryComponentZone = primaryComponentZones.FirstOrDefault();
						if (primaryComponentZone != null)
						{
							zone = primaryComponentZone.Zone;
						}
					}
					else
					{
						subcomponentStatus = GetWorstSubcomponentName(subcomponentZones, primaryComponentZones.FirstOrDefault());
					}
				}
			}

			status = string.IsNullOrWhiteSpace(subcomponentStatus)
				? Res.GetString("acc3af07-d2f5-455d-b6e1-031c0dd99931", "Zone {0}", zone)
				: Res.GetString("913CC159-2492-454B-B09D-4C7E899FFC99", "Zone {0} ({1})", zone, subcomponentStatus);

			return status;
		}

		public string GetSubComponentTooltipStatus(IVisualBoardChannel channel, BMBoardSection section, CardAllocationMap allocationMap)
		{
			string subComponentStatus = string.Empty;

			if (IsInConstrainedMode && !channel.IsCCRChannel(section))
			{
				ComponentZoneInfo subComponent = GetSubComponent(channel, section, allocationMap);

				if (subComponent != null)
				{
					subComponentStatus = Res.GetString("64f7d2e9-fada-48d0-9bcb-ed97ef206bd2", "{0} percent of work in this channel falls within zone {1} of the {2} buffer.",
						section.SectionConfiguration.FadeBackgroundAtPercentage, subComponent.Zone, subComponent.Name);

					if (CurrentlySelectedComponent == null || CurrentlySelectedComponent.FC_Name != subComponent.Name)
					{
						subComponentStatus += " " + Res.GetString("0efda0cb-879f-4c94-b99d-2c410edbe8e5", "Use the Component Filter context menu to switch to this buffer, or click this message.");
					}
				}
			}

			return subComponentStatus;
		}

		public ZGuid? GetSubComponentPK(IVisualBoardChannel channel, BMBoardSection section, CardAllocationMap allocationMap)
		{
			ZGuid? subComponentPK = null;

			if (IsInConstrainedMode && !channel.IsCCRChannel(section))
			{
				var subComponent = GetSubComponent(channel, section, allocationMap);
				if (subComponent != null)
				{
					subComponentPK = subComponent.PK;
				}
			}

			return subComponentPK;
		}

		ComponentZoneInfo GetSubComponent(IVisualBoardChannel channel, BMBoardSection section, CardAllocationMap allocationMap)
		{
			ComponentZoneInfo subComponent = null;

			var subcomponentZones = GetSubComponentZones(channel, section, allocationMap).Where(c => c.Zone <= 1).ToArray();
			if (subcomponentZones.Length > 0)
			{
				subComponent = subcomponentZones.Cast<ComponentZoneInfo>().MinBy(c => c.OffsetInMinutes);
			}

			return subComponent;
		}

		#endregion

		#region Zones

		public ZInt? GetChannelZone(IVisualBoardChannel channel, BMBoardSection section, CardAllocationMap allocationMap)
		{
			var componentZones = GetAllComponentZones(channel, section, allocationMap);
			return GetChannelZoneCore(componentZones);
		}

		ZInt? GetChannelZoneCore(List<ZoneInfo> componentZones)
		{
			return componentZones.Count > 0 ? componentZones.MinBy(c => c.Zone).Zone : null;
		}

		string GetWorstSubcomponentName(IEnumerable<ComponentZoneInfo> subcomponentZones, ZoneInfo primaryComponent)
		{
			var zone = subcomponentZones.Where(c => c.Zone < primaryComponent.Zone).MinBySafe(c => c.Zone);

			return zone != null ? zone.Name : string.Empty;
		}

		List<ZoneInfo> GetAllComponentZones(IVisualBoardChannel channel, BMBoardSection section, CardAllocationMap allocationMap)
		{
			var result = GetSubComponentZones(channel, section, allocationMap);
			var primaryZone = GetPrimaryComponentZone(channel);

			if (primaryZone != null)
			{
				result.Add(new ZoneInfo(primaryZone.Value));
			}

			return result;
		}

		List<ZoneInfo> GetSubComponentZones(IVisualBoardChannel channel, BMBoardSection section, CardAllocationMap allocationMap)
		{
			var result = new List<ZoneInfo>();

			if (IsInConstrainedMode)
			{
				foreach (var subComponent in section.AllComponents.SelectMany(s => s.ChildComponents).Where(b => b.FC_Type == BMComponentTypeList.Codes.Buffer))
				{
					var zone = GetSubComponentZone(section, channel, subComponent, allocationMap);
					if (zone != null)
					{
						result.Add(new ComponentZoneInfo(subComponent.FC_Name, zone.Value, subComponent.FC_OffsetInMinutes, subComponent.PK));
					}
				}
			}

			return result;
		}

		ZInt? GetSubComponentZone(BMBoardSection section, IVisualBoardChannel channel, BMComponent component, CardAllocationMap allocationMap)
		{
			int? zone = null;

			var sectionConfiguration = section.SectionConfiguration;
			int primaryAxis = CardCells.Where(c => c.HasSameChannel(channel)).Select(c => c.PrimaryAxis).FirstOrDefault();

			var total = GetTaskEstimatesInCells(allocationMap,
				primaryAxes: primaryAxis.WrapWithEnumerable(),
				includePenetratedComponents: true,
				components: component.PK);
			if (total > 0)
			{
				var runningTotal = 0.0m;
				var secondaryAxes = sectionConfiguration.OrientationValue == BMBoardSectionOrientation.Vertical
					? Enumerable.Range(0, TotalRows)
					: Enumerable.Range(0, TotalColumns);

				for (int secondaryAxis = secondaryAxes.First(); secondaryAxis <= secondaryAxes.Last(); secondaryAxis++)
				{
					runningTotal += GetTaskEstimatesInCells(allocationMap,
						primaryAxes: primaryAxis.WrapWithEnumerable(),
						secondaryAxes: secondaryAxis.WrapWithEnumerable(),
						includePenetratedComponents: true,
						components: component.PK);
					if (runningTotal / total * 100 > sectionConfiguration.FadeBackgroundAtPercentage)
					{
						var cell = CardCells.FirstOrDefault(c => c.PrimaryAxis == primaryAxis && c.SecondaryAxis == secondaryAxis);
						if (cell != null)
						{
							var ccrStatus = ConstrainedModeHelper.GetConstraintStatus(component);

							if (ccrStatus == ConstraintStatus.PreConstraint && cell.Zone <= 1)
							{
								zone = 0;
							}
							else if (ccrStatus == ConstraintStatus.PostConstraint && cell.Zone >= 2)
							{
								zone = 3;
							}
							else if (cell.SubComponentZones.ContainsKey(component.PK))
							{
								zone = cell.SubComponentZones[component.PK];
							}
						}
						break;
					}
				}
			}

			return zone;
		}

		public ZInt? GetPrimaryComponentZone(IVisualBoardChannel channel)
		{
			var fadeCell = CardCells.Where(c => c.HasSameChannel(channel)).FirstOrDefault(c => c.BackgroundFadeColor != null);

			if (fadeCell != null && fadeCell.Zone != null)
			{
				return fadeCell.Zone.Value;
			}
			else
			{
				return null;
			}
		}

		#endregion

		#region Risk States

		internal static bool IsInNotEnoughWorkRiskState(IVisualBoardChannel channel, BMBoardSection section, CardAllocationMap allocationMap)
		{
			var oneZoneConstraintOffsetInHourList = section
				.Component
				.ChildComponents
				.Where(component => component.IsConstraint)
				.Select(constraint => ((decimal)constraint.FC_OffsetInMinutes / BMConstants.NumberOfZones / 60));

			if (oneZoneConstraintOffsetInHourList.Any())
			{
				var oneZoneConstraintOffsetInHours = oneZoneConstraintOffsetInHourList.Min(i => i);
				var totalTaskEstimatesInHours = allocationMap.GetCurrentStartableTaskStackEstimates(channel);
				return totalTaskEstimatesInHours < oneZoneConstraintOffsetInHours;
			}

			return false;
		}

		internal static bool IsInOutsideTargetZoneRiskState(ZString ccrCode, ComponentGrid grid)
		{
			return grid.Cells.Any(cell =>
				cell.Channel != null
				&& cell.Channel.ChannelEntityCode == ccrCode
				&& cell.ContentType == CellContentType.Cards
				&& cell.BackgroundFadeColor != null
				&& (cell.CCRHeaderZone == 1 || cell.CCRHeaderZone == 0));
		}

		#endregion

		#region Filters

		internal void RefreshFilters(IEnumerable<IBoardFilter> filters, BMBoardSectionViewModel viewModel, bool requiresFullRedraw)
		{
			var factory = new ReadOnlyBusinessObjectFactory { NameForDebugging = GetType().Name + ".RefreshFilters", RefreshEnabled = false };
			RefreshCells(factory, viewModel, CardAllocationMap, CardCells, filters, requiresFullRedraw);
		}

		#endregion

		#region Task allocation

		public IEnumerable<CellContent> CardCells
		{
			get { return Cells.Where(c => c.ContentType == CellContentType.Cards); }
		}

		#endregion

		#region Task estimates

		public static decimal GetTaskEstimatesInCells(CardAllocationMap allocationMap, IEnumerable<int> primaryAxes = null, IEnumerable<int> secondaryAxes = null, IEnumerable<ZString> staffCodes = null, CCRFilter ccrFilter = CCRFilter.None, bool includePenetratedComponents = false, params ZGuid[] components)
		{
			return allocationMap?.GetTaskEstimates(primaryAxes?.ToHashSet(), secondaryAxes?.ToHashSet(), staffCodes?.ToHashSet(), ccrFilter, includePenetratedComponents, components) ?? 0;
		}

		public bool IsReadyToCalculateChannelCapacity
		{
			get { return CardAllocationMap != null; }
		}

		#endregion

		#region Background Colours

		public void UpdateBackgroundColoursAndFades(BMBoardSectionViewModel viewModel, CardAllocationMap allocationMap, BMBoardSection section)
		{
			ComponentGridBuilderProvider.SetBackgroundColoursAndFades(this, viewModel.PrimaryChannels, viewModel.SecondaryAxisChannels, section, allocationMap, IsInConstrainedMode);
		}

		#endregion

		class ComponentZoneInfo : ZoneInfo
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
			public ComponentZoneInfo(string name, ZInt zone, decimal offsetInMinutes, ZGuid pk)
				: base(zone)
			{
				Name = name;
				PK = pk;
				OffsetInMinutes = offsetInMinutes;
			}

			public override bool IsPrimary => false;
			public string Name { get; private set; }
			public ZGuid PK { get; private set; }

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
			public decimal OffsetInMinutes { get; private set; }
		}

		class ZoneInfo
		{
			public ZoneInfo(ZInt zone)
			{
				Zone = zone;
			}

			public virtual bool IsPrimary => true;
			public ZInt Zone { get; private set; }
		}
	}
}
