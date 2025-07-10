using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	internal class BufferGridBuilder : ComponentGridBuilderBase
	{
		internal BufferGridBuilder(ComponentGrid grid,
			IEnumerable<IChannel> primaryChannels,
			IEnumerable<IChannel> secondaryChannels,
			BMBoardSection section,
			bool isPreview,
			bool isInConstrainedMode)
			: base(grid, primaryChannels, secondaryChannels, section)
		{
			IsPreview = isPreview;
			IsInConstrainedMode = isInConstrainedMode;
			this.section = section;
		}

		bool IsPreview { get; }
		bool IsInConstrainedMode { get; }
		readonly BMBoardSection section;

		protected override CellContentType GetCellType(int primaryIndex, int secondaryIndex)
		{
			var previousCell = GetCell(primaryIndex - 1, secondaryIndex);
			var previousCellContentType = previousCell != null ? previousCell.ContentType : CellContentType.None;
			var isCCR = GetCurrentChannelIsCCRLazy();
			var previousWasCCR = GetPreviousChannelIsCCRLazy(primaryIndex, secondaryIndex);

			if (secondaryIndex == 0 && IsChanneled)
			{
				return GetHeaderCellType(primaryIndex, previousCellContentType, isCCR, previousWasCCR);
			}
			else
			{
				return GetNonHeaderCellType(primaryIndex, previousCellContentType, isCCR, previousWasCCR);
			}
		}

		protected override CellContent GetCellContent(int row, int col, CellContentType cellType)
		{
			return new CellContent(row, col, cellType, sectionConfiguration, PreConstraintPKs, PostConstraintPKs, ConstraintPKs);
		}

		List<ZGuid> PreConstraintPKs
		{
			get
			{
				if (preConstraintPKs == null)
				{
					preConstraintPKs = section.ApplicableComponents.SelectMany(c => c.GetBuffers(ConstraintStatus.PreConstraint)).Select(b => b.PK).ToList();
				}
				return preConstraintPKs;
			}
		}
		List<ZGuid> preConstraintPKs;

		List<ZGuid> PostConstraintPKs
		{
			get
			{
				if (postConstraintPKs == null)
				{
					postConstraintPKs = section.ApplicableComponents.SelectMany(c => c.GetBuffers(ConstraintStatus.PostConstraint)).Select(b => b.PK).ToList();
				}
				return postConstraintPKs;
			}
		}
		List<ZGuid> postConstraintPKs;

		List<ZGuid> ConstraintPKs
		{
			get
			{
				if (constraintPKs == null)
				{
					constraintPKs = PreConstraintPKs.Concat(PostConstraintPKs).ToList();
				}
				return constraintPKs;
			}
		}
		List<ZGuid> constraintPKs;

		#region CellContentHelpers

		#region Header Cell Type

		CellContentType GetHeaderCellType(int primaryIndex, CellContentType previousCellContentType, Lazy<bool> isCCR, Lazy<bool?> previousWasCCR)
		{
			if (ShouldPlaceAgeHeading(primaryIndex)
				|| ShouldPlaceZoneOrCCRHeaderLabel(primaryIndex)
				|| ShouldPlaceLabelAboveSubComponentZoneHeading(primaryIndex, previousCellContentType)
				|| ShouldPlaceStartOfDifferentResourceBlockLabel(isCCR, previousWasCCR))
			{
				return CellContentType.Label;
			}

			if (AllHeadersPlaced(primaryIndex))
			{
				return CellContentType.None;
			}

			if (ShouldPlaceChannelHeader(previousCellContentType, isCCR, previousWasCCR)) //No change in CCRness = another channel. CCR headings and Zone headings are followed by channels.
			{
				return CellContentType.ChannelHeading;
			}

			return CellContentType.Label;
		}

		bool ShouldPlaceStartOfDifferentResourceBlockLabel(Lazy<bool> isCCR, Lazy<bool?> previousWasCCR)
		{
			if (IsInConstrainedMode && sectionConfiguration.ShowZones) //We only show sub component zone header / CCR targets when we are in constrained mode.
			{
				if (remainingPrimaryAxisChannels.Count != 0 && isCCR.Value != previousWasCCR.Value) //If the previous primary index cell channel had a different ccrness / wasn't a channel.
				{
					if (!isCCR.Value)
					{
						if (previousWasCCR.Value != null) //The previous cell was a channel
						{
							return true; //we should place a zone header here.
						}
					}
				}
			}

			return false;
		}

		bool ShouldPlaceZoneOrCCRHeaderLabel(int primaryIndex)
		{
			return primaryIndex == 1 && sectionConfiguration.ShowZones;
		}

		bool AllHeadersPlaced(int primaryIndex)
		{
			return remainingPrimaryAxisChannels.Count == 0 || (!IsChanneled && ((sectionConfiguration.ShowZones && primaryIndex == 2) || (!sectionConfiguration.ShowZones && primaryIndex == 1)));
		}

		bool ShouldPlaceChannelHeader(CellContentType previousCellContentType, Lazy<bool> isCCR, Lazy<bool?> previousWasCCR)
		{
			return previousCellContentType == CellContentType.ZoneHeading
				|| isCCR.Value == previousWasCCR.Value
				|| previousCellContentType == CellContentType.CCRHeading
				|| previousCellContentType == CellContentType.Label
				|| (previousCellContentType == CellContentType.ChannelHeading && (!IsInConstrainedMode || !sectionConfiguration.ShowZones));
		}

		#endregion

		CellContentType GetNonHeaderCellType(int primaryIndex, CellContentType previousCellContentType, Lazy<bool> isCCR, Lazy<bool?> previousWasCCR)
		{
			if (ShouldPlaceAgeHeading(primaryIndex))
			{
				return CellContentType.AgeHeading;
			}

			if (ShouldPlaceZoneHeading(primaryIndex, isCCR.Value))
			{
				return CellContentType.ZoneHeading;
			}

			if (ShouldPlaceSubComponentZoneHeading(previousCellContentType))
			{
				return CellContentType.SubComponentZoneHeading;
			}

			if (AllCellsPlaced(previousCellContentType))
			{
				return CellContentType.None;
			}

			if (ShouldPlaceCCRHeader(previousCellContentType, isCCR.Value)) //Sub component zone headings are followed by a CCR heading if there is a CCR channel.
			{
				return CellContentType.CCRHeading;
			}

			if (ShouldPlaceCards(previousCellContentType, isCCR, previousWasCCR)) //No change in CCRness = another channel. CCR headings and Zone headings are followed by channels.
			{
				return CellContentType.Cards;
			}

			if (StartOfDifferentResourceBlock(previousCellContentType, isCCR, previousWasCCR) && sectionConfiguration.ShowZones) //If we've gone from CCR's to non CCRs the we need the respective heading, if the previous was an age heading we need to start the section with a heading.
			{
				return isCCR.Value ? CellContentType.CCRHeading : CellContentType.ZoneHeading; //If the CCRness has changed, we need the respective block starting heading.
			}

			return CellContentType.Label;
		}

		bool StartOfDifferentResourceBlock(CellContentType previousCellContentType, Lazy<bool> isCCR, Lazy<bool?> previousWasCCR)
		{
			return IsInConstrainedMode && (previousCellContentType == CellContentType.Cards && isCCR.Value != previousWasCCR.Value || previousCellContentType == CellContentType.AgeHeading);
		}

		bool ShouldPlaceCards(CellContentType previousCellContentType, Lazy<bool> isCCR, Lazy<bool?> previousWasCCR)
		{
			return (previousCellContentType == CellContentType.Cards && (!IsInConstrainedMode || isCCR.Value == previousWasCCR.Value || !sectionConfiguration.ShowZones)) ||
				previousCellContentType.In(CellContentType.ZoneHeading, CellContentType.SubComponentZoneHeading) ||
				(!sectionConfiguration.ShowZones && previousCellContentType == CellContentType.AgeHeading) ||
				previousCellContentType == CellContentType.CCRHeading;
		}

		bool ShouldPlaceZoneHeading(int primaryIndex, bool isCCR)
		{
			return primaryIndex == 1 && sectionConfiguration.ShowZones && (!isCCR || !IsInConstrainedMode);
		}

		static bool ShouldPlaceAgeHeading(int primaryIndex)
		{
			return primaryIndex == 0;
		}

		bool ShouldPlaceCCRHeader(CellContentType previousCellContentType, bool isCCR)
		{
			return IsInConstrainedMode && isCCR && sectionConfiguration.ShowZones && (previousCellContentType == CellContentType.AgeHeading || previousCellContentType == CellContentType.Label);
		}

		bool ShouldHaveSubComponentZoneHeaders(bool isSubComponentHeadingPlaceable, List<ZGuid> subComponentPKs)
		{
			if (!isSubComponentHeadingPlaceable || !sectionConfiguration.ShowZones)
			{
				return false;
			}

			var subComponents = sectionConfiguration.Section.ApplicableComponents.SelectMany(c => ComponentGridHelper.GetBufferSubComponents(c));

			if (!(subComponents.Any() && IsInConstrainedMode))
			{
				return false;
			}

			var showChildComponentZones = sectionConfiguration.ShowChildComponentZones;
			var requiredSubComponentZoneHeadingCount = GetRequiredSubComponentZoneHeadingCount(subComponents, subComponentPKs, showChildComponentZones);

			if (requiredSubComponentZoneHeadingCount == 0)
			{
				return false;
			}

			var constraintHeadingsPlaced = GetHeadingsPlacedCount(CellContentType.SubComponentZoneHeading);
			var zoneHeadingsPlaced = GetHeadingsPlacedCount(CellContentType.ZoneHeading);

			return constraintHeadingsPlaced < requiredSubComponentZoneHeadingCount * (showChildComponentZones ? 1 : zoneHeadingsPlaced);
		}

		int GetRequiredSubComponentZoneHeadingCount(IEnumerable<BMComponent> subComponents, List<ZGuid> subComponentPKs, bool showChildComponentZones)
		{
			if (!subComponents.Any())
			{
				return 0;
			}

			if (showChildComponentZones)
			{
				return subComponents.Max(c => GetSubComponentStackPosition(c)) + 1;
			}

			return subComponents.Count(c => c.PK.In(subComponentPKs));
		}

		bool ShouldPlaceSubComponentZoneHeading(CellContentType previousCellContentType)
		{
			var isSubComponentHeadingPlaceable = sectionConfiguration.ShowChildComponentZones
				? remainingPrimaryAxisChannels.Count == 0
				: previousCellContentType.In(CellContentType.ZoneHeading, CellContentType.SubComponentZoneHeading);

			return ShouldHaveSubComponentZoneHeaders(isSubComponentHeadingPlaceable, ConstraintPKs);
		}

		bool ShouldPlaceLabelAboveSubComponentZoneHeading(int primaryIndex, CellContentType previousCellContentType)
		{
			var isSubComponentHeadingPlaceable = sectionConfiguration.ShowChildComponentZones
				? remainingPrimaryAxisChannels.Count == 0
				: previousCellContentType == CellContentType.Label && GetCell(primaryIndex - 1, 1).ContentType.In(CellContentType.ZoneHeading, CellContentType.SubComponentZoneHeading);

			return ShouldHaveSubComponentZoneHeaders(isSubComponentHeadingPlaceable, ConstraintPKs);
		}

		bool AllCellsPlaced(CellContentType previousCellContentType)
		{
			if (IsChanneled)
			{
				return remainingPrimaryAxisChannels.Count == 0;
			}
			else //If there are no primary channels, then we only place one card cell.
			{
				return previousCellContentType == CellContentType.Cards; //If there are no channels and we don't need a sub component zone heading, we will only have one card cell.
			}
		}

		Lazy<bool> GetCurrentChannelIsCCRLazy()
		{
			return new Lazy<bool>(GetCurrentChannelIsCCR, isThreadSafe: false);
		}

		bool GetCurrentChannelIsCCR()
		{
			var currentChannel = remainingPrimaryAxisChannels.Count != 0 ? remainingPrimaryAxisChannels.Peek() as IVisualBoardChannel : null;
			return currentChannel != null && !IsPreview && currentChannel.IsCCRChannel(sectionConfiguration.Section, IsInConstrainedMode);
		}

		Lazy<bool?> GetPreviousChannelIsCCRLazy(int primaryIndex, int secondaryIndex)
		{
			return new Lazy<bool?>(() => GetPreviousChannelIsCCR(primaryIndex, secondaryIndex), isThreadSafe: false);
		}

		bool? GetPreviousChannelIsCCR(int primaryIndex, int secondaryIndex)
		{
			var previousCell = GetCell(primaryIndex - 1, secondaryIndex);
			if (previousCell == null || previousCell.Channel == null)
			{
				return null;
			}
			else
			{
				return !IsPreview && previousCell.Channel.IsCCRChannel(sectionConfiguration.Section, IsInConstrainedMode);
			}
		}

		#endregion

		#region Set Cell Properties

		protected override void SetCardSortType(CellContent cell)
		{
			cell.CardSortType = CardSortType.BufferWorkSequence;
		}

		protected override Color GetBackgroundColor(CellContent cell)
		{
			if (cell.ContentType == CellContentType.AgeHeading || !sectionConfiguration.ShowZones)
			{
				return base.GetBackgroundColor(cell);
			}

			var zone = GetZone(cell);
			return sectionConfiguration.Section.GetZoneColor(zone);
		}

		protected override Color GetForegroundColor(CellContent cell)
		{
			if (cell.ContentType == CellContentType.AgeHeading)
			{
				if (sectionConfiguration.Section.ForegroundColor.IsEmpty)
				{
					return cell.BackColor.Value.GetBestTextColorForBackground();
				}
				return ColorList.ColorFromName(sectionConfiguration.Section.ForegroundColor);
			}

			return base.GetForegroundColor(cell);
		}

		int? GetZone(CellContent cell)
		{
			if (cell.ZoneOverride != null)
			{
				return cell.ZoneOverride;
			}

			if (!IsPreview && IsCellInCCRHeaderZone(cell))
			{
				return cell.CCRHeaderZone;
			}

			return GetZoneOutsideCCR(cell);
		}

		bool IsCellInCCRHeaderZone(CellContent cell)
		{
			return cell.ContentType == CellContentType.Cards
				&& cell.Channel != null
				&& cell.Channel.IsCCRChannel(sectionConfiguration.Section, IsInConstrainedMode);
		}

		int? GetZoneOutsideCCR(CellContent cell)
		{
			if (!cell.SubComponentZones.Any())
			{
				return cell.Zone;
			}

			var showChildComponentZones = sectionConfiguration.ShowChildComponentZones;

			if (showChildComponentZones
				&& cell.ContentType.In(CellContentType.Cards, CellContentType.SubComponentZoneHeading))
			{
				return cell.SubComponentZones.MinBySafe(z => z.Value).Value;
			}
			else if (!showChildComponentZones
				&& cell.ContentType.In(CellContentType.Cards)
				&& grid.CurrentlySelectedComponent != null
				&& grid.CurrentlySelectedComponent.IsChildBuffer)
			{
				var constraintZones = cell.SubComponentZones.Where(w => w.Key.In(cell.ConstraintPKs));
				if (constraintZones.Any())
				{
					return constraintZones.MinBySafe(z => z.Value).Value;
				}
			}

			return cell.Zone;
		}

		#endregion

		#region Set Properties On Flattened

		protected override void SetPropertiesOnFlattened(CellContent[] flattenedSequence)
		{
			base.SetPropertiesOnFlattened(flattenedSequence);

			SetZonesAndAgeIndexes(flattenedSequence);
			SetConstraintBorderThickness();
		}

		#region Zones and Age Indexes

		protected override void SetLabelOnCells(CellContent[] flattenedSequence)
		{
			base.SetLabelOnCells(flattenedSequence);

			foreach (var cell in flattenedSequence)
			{
				if (cell.ContentType == CellContentType.SubComponentZoneHeading)
				{
					cell.Label = Res.GetString("dfdcdb78-9240-4d33-b94b-63b6ec0ebbd3", "Zone {0}", cell.Zone);
				}
			}
		}

		void SetZonesAndAgeIndexes(CellContent[] flattenedSequence)
		{
			SetZoneHeadingZoneNumbers(flattenedSequence);

			if (sectionConfiguration.ShowChildComponentZones)
			{
				SetVisibleChildComponentHeadingDetails(flattenedSequence);
			}
			else
			{
				SetHiddenSubComponentConstraintHeadingDetails(flattenedSequence);
			}

			SetCCRHeadingZoneAndCCRChannelZone(flattenedSequence);
			SetAgingOnCCRHeading(flattenedSequence);
		}

		#region Set Zone Heading Zone Numbers

		void SetZoneHeadingZoneNumbers(CellContent[] flattenedSequence)
		{
			if (sectionConfiguration.Subsections > 1)
			{
				var zoneCells = flattenedSequence.Where(c => c.ContentType == CellContentType.ZoneHeading).ToArray();
				var ageHeadingCells = flattenedSequence.Where(c => c.ContentType == CellContentType.AgeHeading).ToArray();

				for (var i = 0; i < zoneCells.Length; i++)
				{
					var zone = ComponentGridHelper.GetZoneForAgeIndex(i, sectionConfiguration);
					zoneCells[i].Zone = zone;
					ageHeadingCells[i].Zone = zone;
				}
			}
			else
			{
				var zoneCells = flattenedSequence.Where(c => c.ContentType == CellContentType.ZoneHeading).GroupBy(c => c.PrimaryAxis);

				if (zoneCells.Any())
				{
					var firstZoneCellGroup = zoneCells.First().ToArray();

					for (var i = 0; i < firstZoneCellGroup.Length; i++)
					{
						var zone = ComponentGridHelper.GetZoneForAgeIndex(i, sectionConfiguration);
						SetPrimaryComponentZoneNumber(firstZoneCellGroup[i], flattenedSequence, zone);
					}
				}
			}
		}

		static void SetPrimaryComponentZoneNumber(CellContent cellContent, CellContent[] flattenedSequence, int zone)
		{
			var cellsNeedingZoneNumberSet = flattenedSequence.Where(c =>
				c.SecondaryAxis == cellContent.SecondaryAxis
				&& c.ContentType.In(CellContentType.ZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.AgeHeading));

			foreach (var cell in cellsNeedingZoneNumberSet)
			{
				cell.Zone = zone;
			}
		}

		#endregion

		#region Set Hidden Sub Component Constraint Heading Details

		void SetHiddenSubComponentConstraintHeadingDetails(CellContent[] flattenedSequence)
		{
			var subComponentHeadingCells = flattenedSequence.Where(c => c.ContentType == CellContentType.SubComponentZoneHeading).ToArray();

			var pks = ConstraintPKs.Distinct();
			if (subComponentHeadingCells.Length > 0)
			{
				var subComponents = sectionConfiguration.Section.ApplicableComponents.SelectMany(c => ComponentGridHelper.GetBufferSubComponents(c))
					.Where(c => c.PK.In(pks)).ToArray();

				int subIndex = 0;

				for (var subComponentHeadingCellPosition = 0; subComponentHeadingCellPosition < subComponentHeadingCells.Length; subComponentHeadingCellPosition++)
				{
					var cellPosition = subComponentHeadingCellPosition;

					var subComponentHeadingCell = subComponentHeadingCells[subComponentHeadingCellPosition];

					if (subComponentHeadingCellPosition != 0 && subComponentHeadingCellPosition % sectionConfiguration.CellsPerSubsection == 0)
					{
						subIndex = (subIndex + 1) % subComponents.Length;
					}

					var ageInMinutes = GetAgeInMinutes(sectionConfiguration, subComponents[subIndex].ParentComponent, cellPosition);
					var zoneNumber = GetSubComponentZone(sectionConfiguration, subComponents[subIndex].ParentComponent, subComponents[subIndex].FC_BufferTimespanInMinutes, subComponents[subIndex].FC_OffsetInMinutes, ageInMinutes);
					subComponentHeadingCell.Zone = zoneNumber;
					subComponentHeadingCell.SubComponentHeadingPK = subComponents[subIndex].PK;

					foreach (var cardSubComponent in subComponents)
					{
						ageInMinutes = GetAgeInMinutes(sectionConfiguration, cardSubComponent.ParentComponent, cellPosition);
						SetSubComponentHeadingAndSameSecondaryAxisCards(sectionConfiguration, subComponentHeadingCell, cardSubComponent, ageInMinutes);
					}
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		static double GetAgeInMinutes(BMComponentSectionConfiguration sectionConfiguration, BMComponent buffer, int subComponentZoneCellIndex)
		{
			var ageIndex = subComponentZoneCellIndex % sectionConfiguration.CellsPerSubsection;
			return ageIndex * ComponentGridHelper.GetTimePerCellForBuffer(sectionConfiguration, buffer).GetMinutesFromDateTimeSpan();
		}

		void SetSubComponentHeadingAndSameSecondaryAxisCards(BMComponentSectionConfiguration sectionConfiguration, CellContent subComponentHeadingCell, BMComponent subComponent, double ageInMinutes)
		{
			var zoneNumber = GetSubComponentZone(sectionConfiguration, subComponent.ParentComponent, subComponent.FC_BufferTimespanInMinutes, subComponent.FC_OffsetInMinutes, ageInMinutes);
			SetSubComponentHeadingAndSameSecondaryAxisCards(sectionConfiguration, subComponentHeadingCell, zoneNumber, subComponent);
		}

		void SetSubComponentHeadingAndSameSecondaryAxisCards(BMComponentSectionConfiguration sectionConfiguration, CellContent subComponentHeadingCell, int zone, BMComponent subComponent)
		{
			if (!subComponentHeadingCell.SubComponentZones.ContainsKey(subComponent.PK))
			{
				subComponentHeadingCell.SubComponentZones.Add(subComponent.PK, zone);
			}

			SetSameSecondaryAxisCards(sectionConfiguration, subComponentHeadingCell, zone, subComponent);
		}

		void SetSameSecondaryAxisCards(BMComponentSectionConfiguration sectionConfiguration, CellContent subComponentHeadingCell, int zone, BMComponent subComponent)
		{
			var cardCellsInSameSecondaryAxis = grid.CardCells.Where(c => c.SecondaryAxis == subComponentHeadingCell.SecondaryAxis);
			var constraintStatus = ConstrainedModeHelper.GetConstraintStatus(subComponent);

			foreach (var cardCell in cardCellsInSameSecondaryAxis)
			{
				if (!cardCell.SubComponentZones.ContainsKey(subComponent.PK))
				{
					cardCell.SubComponentZones.Add(subComponent.PK, zone);
					if (sectionConfiguration.ShowChildComponentZones)
					{
						cardCell.Zone = zone;
						cardCell.SubComponentHeadingPK = subComponent.PK;
					}
				}

				if (cardCell.Channel != null && !IsPreview && cardCell.Channel.IsCCRChannel(sectionConfiguration.Section))
				{
					cardCell.ConstraintStatus = ConstraintStatus.ReadyForConstraint;
				}
				else
				{
					cardCell.ConstraintStatus = constraintStatus;
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		static int GetSubComponentZone(BMComponentSectionConfiguration sectionConfiguration, BMComponent buffer, int subComponentTimespanInMinutes, int subComponentOffsetInMinutes, double cellPositionInMinutes)
		{
			var primaryBuffer = sectionConfiguration.ShowChildComponentZones ? buffer.ParentComponent : buffer;
			var minutesPerCell = (int)ComponentGridHelper.GetTimePerCellForBuffer(sectionConfiguration, primaryBuffer).GetMinutesFromDateTimeSpan();

			var cellPositionInIndex = (int)(cellPositionInMinutes / minutesPerCell);
			var subComponentOffsetInIndex = subComponentOffsetInMinutes / minutesPerCell;
			var subComponentPositionInIndex = cellPositionInIndex - subComponentOffsetInIndex;

			var subComponentTimespanInIndex = subComponentTimespanInMinutes / minutesPerCell;
			var cellsCountInZone = GetSubComponentCellsCountPerZone(subComponentTimespanInIndex);

			var zoneNumber = subComponentPositionInIndex >= subComponentTimespanInIndex ? 0
				: subComponentPositionInIndex >= cellsCountInZone.CellsCountInZone3 + cellsCountInZone.CellsCountInZone2 ? 1
				: subComponentPositionInIndex >= cellsCountInZone.CellsCountInZone3 ? 2
				: 3;

			return zoneNumber;
		}

		static CellsCountInZone GetSubComponentCellsCountPerZone(int subComponentTimeSpanInTimeIndex)
		{
			var cellsCountInZone = new CellsCountInZone();

			cellsCountInZone.CellsCountInZone3 = cellsCountInZone.CellsCountInZone2 = cellsCountInZone.CellsCountInZone1 = subComponentTimeSpanInTimeIndex / BMConstants.NumberOfZones;

			if (subComponentTimeSpanInTimeIndex % BMConstants.NumberOfZones == 2)
			{
				cellsCountInZone.CellsCountInZone1++;
				cellsCountInZone.CellsCountInZone3++;
			}
			else if (subComponentTimeSpanInTimeIndex % BMConstants.NumberOfZones == 1)
			{
				cellsCountInZone.CellsCountInZone2++;
			}

			return cellsCountInZone;
		}

		#endregion

		#region Set Visible Child Component Heading Details

		void SetVisibleChildComponentHeadingDetails(CellContent[] flattenedSequence)
		{
			var subComponentHeadingCells = flattenedSequence.Where(c => c.ContentType == CellContentType.SubComponentZoneHeading).ToArray();
			if (subComponentHeadingCells.Any())
			{
				var subComponents = sectionConfiguration.Section.ApplicableComponents
					.OrderByDescending(c => c.FC_BufferTimespanInMinutes)
					.SelectMany(c => ComponentGridHelper.GetBufferSubComponents(c))
					.Where(c => c.PK.In(ConstraintPKs.Distinct())).ToArray();
				var maxSubcomponentStack = subComponents.Max(c => GetSubComponentStackPosition(c)) + 1;

				var nonZoneZeroCellFound = false;
				BMComponent lastSubComponent = null;
				var cellsInASubComponentBlock = 0;

				for (var subComponentHeadingCellPosition = 0; subComponentHeadingCellPosition < subComponentHeadingCells.Length; subComponentHeadingCellPosition++)
				{
					var cellPosition = GetCurrentCellPosition(sectionConfiguration, subComponentHeadingCellPosition, maxSubcomponentStack, cellsInASubComponentBlock);

					if (cellPosition % sectionConfiguration.CellsPerSubsection == 0)
					{
						lastSubComponent = null;
						nonZoneZeroCellFound = false;
					}

					var subComponentHeadingCell = subComponentHeadingCells[subComponentHeadingCellPosition];

					var result = SetChildComponentHeadingAndCards(sectionConfiguration, nonZoneZeroCellFound, subComponentHeadingCell, cellPosition, subComponents, lastSubComponent);
					lastSubComponent = result.Item1;
					nonZoneZeroCellFound = result.Item2;
				}
			}
		}

		static int GetCurrentCellPosition(BMComponentSectionConfiguration sectionConfiguration, int subComponentHeadingCellPosition, int maxSubcomponentStack, int cellsInASubComponentBlock)
		{
			if (subComponentHeadingCellPosition >= sectionConfiguration.CellsPerSubsection * maxSubcomponentStack)
			{
				// NB: the integer division of subComponentHeadingCellPosition / cellsInASubComponentBlock is intentioal
				return subComponentHeadingCellPosition - cellsInASubComponentBlock * (subComponentHeadingCellPosition / cellsInASubComponentBlock);
			}

			return subComponentHeadingCellPosition;
		}

		(BMComponent, bool) SetChildComponentHeadingAndCards(BMComponentSectionConfiguration sectionConfiguration, bool nonZoneZeroCellFound, CellContent subComponentHeadingCell, int cellPosition, BMComponent[] subComponents, BMComponent lastSubComponent)
		{
			var ageInMinutes = GetAgeInMinutes(sectionConfiguration, cellPosition);
			var subComponent = GetSubComponentForZoneCellIndex(sectionConfiguration, cellPosition, ageInMinutes, subComponents);

			if (subComponent != null)
			{
				lastSubComponent = subComponent;
				SetChildComponentHeadingAndSameSecondaryAxisCards(sectionConfiguration, subComponentHeadingCell, subComponent, ageInMinutes);
			}

			if (!subComponentHeadingCell.SubComponentZones.Any())
			{
				if (nonZoneZeroCellFound && lastSubComponent != null)
				{
					SetChildComponentHeadingAndSameSecondaryAxisCardsByZone(sectionConfiguration, subComponentHeadingCell, 0, lastSubComponent);
					subComponentHeadingCell.Zone = 0;
				}
			}
			else
			{
				nonZoneZeroCellFound = true;
			}

			return (lastSubComponent, nonZoneZeroCellFound);
		}

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		static double GetAgeInMinutes(BMComponentSectionConfiguration sectionConfiguration, int subComponentZoneCellIndex)
		{
			var ageIndex = subComponentZoneCellIndex % sectionConfiguration.CellsPerSubsection;
			return ageIndex * sectionConfiguration.TimePerCell.GetMinutesFromDateTimeSpan();
		}

		void SetChildComponentHeadingAndSameSecondaryAxisCards(BMComponentSectionConfiguration sectionConfiguration, CellContent subComponentHeadingCell, BMComponent subComponent, double ageInMinutes)
		{
			// We do not consider Zone 0 of the sub-component in this pass; we do another pass for Zone 0's later.

			var zoneNumber = GetSubComponentZone(sectionConfiguration, subComponent, subComponent.FC_BufferTimespanInMinutes, subComponent.FC_OffsetInMinutes, ageInMinutes);

			if (zoneNumber > 0)
			{
				SetChildComponentHeadingAndSameSecondaryAxisCardsByZone(sectionConfiguration, subComponentHeadingCell, zoneNumber, subComponent);
			}
		}

		void SetChildComponentHeadingAndSameSecondaryAxisCardsByZone(BMComponentSectionConfiguration sectionConfiguration, CellContent subComponentHeadingCell, int zone, BMComponent component)
		{
			if (!subComponentHeadingCell.SubComponentZones.ContainsKey(component.PK))
			{
				subComponentHeadingCell.SubComponentZones.Add(component.PK, zone);
				subComponentHeadingCell.SubComponentHeadingPK = component.PK;
				subComponentHeadingCell.Zone = zone;
			}

			SetSameSecondaryAxisCards(sectionConfiguration, subComponentHeadingCell, zone, component);
		}

		BMComponent GetSubComponentForZoneCellIndex(BMComponentSectionConfiguration sectionConfiguration, int subComponentZoneCellIndex, double ageInMinutes, BMComponent[] subComponents)
		{
			var subComponentsAtOffset = subComponents.Where(c => c.IsOverlapped(ageInMinutes, sectionConfiguration)).ToArray();

			if (subComponentsAtOffset.Any())
			{
				var indexStackPosition = subComponentZoneCellIndex / sectionConfiguration.CellsPerSubsection;
				return subComponentsAtOffset.FirstOrDefault(c => indexStackPosition == GetSubComponentStackPosition(c));
			}

			return null;
		}

		int GetSubComponentStackPosition(BMComponent subComponent)
		{
			var stackPosition = 0;
			var orderedSubComponents = ComponentGridHelper.GetBufferSubComponents(sectionConfiguration.Section.Component).OrderBy(c => c.FC_DisplaySequence).ToArray();

			if (!orderedSubComponents.IsNullOrEmpty())
			{
				var previousSubComponent = orderedSubComponents[0];

				if (previousSubComponent != subComponent)
				{
					for (int i = 1; i < orderedSubComponents.Length; i++)
					{
						var currentSubComponent = orderedSubComponents[i];
						if (previousSubComponent.IsOverlapped(currentSubComponent.FC_OffsetInMinutes))
						{
							stackPosition++;
						}

						if (currentSubComponent == subComponent)
						{
							break;
						}

						previousSubComponent = currentSubComponent;
					}
				}
			}

			return stackPosition;
		}

		#endregion

		#region SetCCRHeadingZoneNumber

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		static void SetAgingOnCCRHeading(CellContent[] flattenedSequence)
		{
			var ageHeadings = flattenedSequence.Where(c => c.ContentType == CellContentType.AgeHeading).ToArray();
			var ccrHeadings = flattenedSequence.Where(c => c.ContentType == CellContentType.CCRHeading && c.Zone != 0).ToArray();

			var referenceTimePercent = SetCCRHeadingsAgingTimePercentAndGetItsReference(ageHeadings, ccrHeadings);

			if (referenceTimePercent != null)
			{
				foreach (var ccrHeadingPerChannel in ccrHeadings.GroupBy(h => h.PrimaryAxis))
				{
					foreach (var group in GroupCellByZone(ccrHeadingPerChannel))
					{
						var sortedGroup = group.OrderBy(h => h.TimePercent);
						var minAge = (sortedGroup.First().TimePercent - referenceTimePercent.Value).ToString("0.#") + " %";
						var maxAge = (sortedGroup.Last().TimePercent).ToString("0.#") + " %";

						foreach (var cell in group)
						{
							cell.Label = Res.GetString("2B6ABF4E-8E6A-4184-97D2-4BFFAE0B2024", "CCRs {0} to {1}", minAge, maxAge);
						}
					}
				}
			}
		}

		static ZDecimal? SetCCRHeadingsAgingTimePercentAndGetItsReference(CellContent[] ageHeadings, CellContent[] ccrHeadings)
		{
			ZDecimal? referenceTimePercent = null;

			foreach (var ageHeading in ageHeadings)
			{
				var ccrHeadingCells = ccrHeadings.Where(h => h.SecondaryAxis == ageHeading.SecondaryAxis);
				foreach (var ccrHeading in ccrHeadingCells)
				{
					ccrHeading.TimePercent = ageHeading.TimePercent;
					if (referenceTimePercent == null)
					{
						referenceTimePercent = ageHeading.TimePercent;
					}
				}
			}
			return referenceTimePercent;
		}

		static IEnumerable<IEnumerable<CellContent>> GroupCellByZone(IEnumerable<CellContent> ccrHeadingPerChannel)
		{
			var groupId = 0;
			var ccrHeadingPerChannelSequenced = ccrHeadingPerChannel.Select((h, index) => new { Header = h, GroupId = (index > 0 && h.Zone != ccrHeadingPerChannel.ElementAt(index - 1).Zone) ? ++groupId : groupId });
			var ccrHeadingsPerChannelGroupByZone = ccrHeadingPerChannelSequenced.GroupBy(g => g.GroupId);
			return ccrHeadingsPerChannelGroupByZone.Select(g => g.Select(h => h.Header));
		}

		void SetCCRHeadingZoneAndCCRChannelZone(CellContent[] flattenedSequence)
		{
			var constraintSecondaryAxis = GetLowestConstraintSecondaryAxis(sectionConfiguration, grid.Cells);
			if (constraintSecondaryAxis != null)
			{
				var ccrHeadingCells = flattenedSequence
					.Where(c => c.ContentType == CellContentType.CCRHeading)
					.Select(c => c as CCRHeadingCellContent);

				foreach (var ccrHeadingCell in ccrHeadingCells)
				{
					ccrHeadingCell.Zone = GetCCRHeadingCellZone(sectionConfiguration, ccrHeadingCell, constraintSecondaryAxis.Value);
					ccrHeadingCell.CCRStatus = sectionConfiguration.GetCCRStatus(ccrLineSecondaryAxis: constraintSecondaryAxis.Value, cellSecondaryAxis: ccrHeadingCell.SecondaryAxis);

					SetCCRChannelCellsZone(ccrHeadingCell);
				}
			}
		}

		void SetCCRChannelCellsZone(CCRHeadingCellContent ccrHeadingCell)
		{
			var ccrChannelPrimaryAxis = ccrHeadingCell.PrimaryAxis + 1;
			CellContent ccrCell = null;
			do
			{
				ccrCell = GetCell(ccrChannelPrimaryAxis, ccrHeadingCell.SecondaryAxis);

				if (ccrCell != null && ccrCell.Channel != null && ccrCell.Channel.IsCCRChannel(sectionConfiguration.Section))
				{
					ccrCell.CCRHeaderZone = ccrHeadingCell.Zone;
				}

				ccrChannelPrimaryAxis++;
			} while (ccrCell != null);
		}

		static int GetCCRHeadingCellZone(BMComponentSectionConfiguration sectionConfiguration, CellContent cell, int constraintSecondaryAxis)
		{
			var cellTimeIndex = cell.TimeIndex % sectionConfiguration.CellsPerSubsection;
			var constraintStatus = sectionConfiguration.GetCCRStatus(constraintSecondaryAxis, cell.SecondaryAxis);
			var bufferSize = sectionConfiguration.CellsPerSubsection - sectionConfiguration.CellsInZoneZero;
			var offsetIndex = ComponentGridHelper.ConvertSecondaryAxisToTimeIndex(sectionConfiguration, constraintSecondaryAxis);

			if (constraintStatus == ConstraintStatus.PreConstraint)
			{
				if (cellTimeIndex >= Convert.ToInt32((offsetIndex * 2.0) / 3.0))
				{
					return 2; //inside CCR Target
				}
				return 3;
			}
			else if (constraintStatus == ConstraintStatus.PostConstraint && cellTimeIndex < bufferSize)
			{
				if (cellTimeIndex < Convert.ToInt32(offsetIndex + ((bufferSize) - offsetIndex) / 3.0))
				{
					return 2; //inside CCR Target
				}
				return 1;
			}
			return 0;
		}

		ZInt? GetLowestConstraintSecondaryAxis(BMComponentSectionConfiguration sectionConfiguration, IEnumerable<CellContent> cells)
		{
			if (IsInConstrainedMode)
			{
				var lowestConstraintTimeIndex = sectionConfiguration.Section.Component.ChildComponents
					.Where(c => c.FC_Type == BMComponentTypeList.Codes.Constraint)
					.Select(c => ComponentGridHelper.GetConstraintTimeIndex(sectionConfiguration, c, sectionConfiguration.Section.Component))
					.Where(constraintTimeIndex => constraintTimeIndex != null)
					.MinBySafe(i => i.Value);

				if (lowestConstraintTimeIndex != null)
				{
					return cells
						.Where(c => c.ContentType == CellContentType.Cards && c.TimeIndex == lowestConstraintTimeIndex)
						.Select(c => c.SecondaryAxis)
						.FirstOrDefault();
				}
			}

			return null;
		}

		#endregion

		#endregion

		#region Constraints

		void SetConstraintBorderThickness()
		{
			var buffer = sectionConfiguration.Section.Component;
			ComponentGridHelper.SetConstraintLine(section.IsInConstrainedMode, sectionConfiguration, buffer, grid);
		}

		#endregion

		#endregion

		#region Implementation

		struct CellsCountInZone
		{
			public int CellsCountInZone1;
			public int CellsCountInZone2;
			public int CellsCountInZone3;
		}

		#endregion

	}
}
