using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	internal abstract class ComponentGridBuilderBase
	{
		internal ComponentGridBuilderBase(ComponentGrid grid,
			IEnumerable<IChannel> primaryChannels,
			IEnumerable<IChannel> secondaryChannels,
			BMBoardSection section)
		{
			this.grid = grid;
			sectionConfiguration = section.SectionConfiguration;

			PrimaryChannels = primaryChannels.ToList();
			SecondaryChannels = secondaryChannels.ToList();

			remainingPrimaryAxisChannels = new Queue<IChannel>(PrimaryChannels);
			remainingSecondaryAxisChannels = new Queue<IChannel>(SecondaryChannels);

			gridList = new List<List<CellContent>>();
		}

		IEnumerable<IChannel> PrimaryChannels { get; }
		IEnumerable<IChannel> SecondaryChannels { get; }
		protected bool IsChanneled => PrimaryChannels.Any();

		bool HasSecondaryChannels => sectionConfiguration.CellsPerSubsection == 1 && sectionConfiguration.ChannelSecondaryBy != BMConstants.ChannelByTimeCode && SecondaryChannels.Any();

		protected bool HasSecondaryChannelsOrderedByTimeCode => (sectionConfiguration.CellsPerSubsection > 1 && sectionConfiguration.ChannelSecondaryBy == BMConstants.ChannelByTimeCode) || SecondaryChannels.Any();

		protected readonly ComponentGrid grid;
		readonly List<List<CellContent>> gridList;
		protected readonly BMComponentSectionConfiguration sectionConfiguration;
		protected readonly Queue<IChannel> remainingPrimaryAxisChannels;
		readonly Queue<IChannel> remainingSecondaryAxisChannels;

		bool primaryChannelPlaced;
		bool secondaryChannelPlaced;

		protected abstract CellContentType GetCellType(int primaryIndex, int secondaryIndex);

		protected int GetHeadingsPlacedCount(CellContentType contentType)
		{
			return (gridList.SelectMany(g => g).Where(w => w.ContentType == contentType)
					.GroupBy(c => sectionConfiguration.OrientationValue == BMBoardSectionOrientation.Vertical ? c.Column : c.Row)
					.Count());
		}

		internal virtual void Build()
		{
			var secondaryMax = GetMaximumSecondaryChannels();

			for (var subsection = 0; subsection < sectionConfiguration.Subsections; subsection++)
			{
				var subsectionOffset = gridList.Count;

				for (var primaryIndex = 0; true; primaryIndex++)
				{
					CellContent cell = null;
					var secondaryIndexList = new List<CellContent>();
					primaryChannelPlaced = false;
					secondaryChannelPlaced = false;

					for (var secondaryIndex = 0; secondaryIndex < secondaryMax; secondaryIndex++)
					{
						var cellType = GetCellType(primaryIndex, secondaryIndex);

						if (cellType != CellContentType.None)
						{
							var row = GetRow(primaryIndex, secondaryIndex, subsectionOffset);
							var col = GetColumn(primaryIndex, secondaryIndex, subsectionOffset);

							cell = cellType == CellContentType.CCRHeading ? new CCRHeadingCellContent(row, col, sectionConfiguration) : GetCellContent(row, col, cellType);
							SetCellProperties(cell, secondaryIndexList);

							secondaryIndexList.Add(cell);

							if (secondaryChannelPlaced && remainingSecondaryAxisChannels.Any())
							{
								remainingSecondaryAxisChannels.Dequeue();
							}
						}
						else
						{
							break;
						}
					}

					if (primaryChannelPlaced && remainingPrimaryAxisChannels.Any())
					{
						remainingPrimaryAxisChannels.Dequeue();
					}

					if (cell != null)
					{
						gridList.Add(secondaryIndexList);
					}

					if (cell == null)
					{
						break;
					}
				}

				PopulateGridCellContent();
			}

			SetCellPropertiesAfterCellAllocation();
		}

		protected virtual CellContent GetCellContent(int row, int col, CellContentType cellType)
		{
			return new CellContent(row, col, cellType, sectionConfiguration);
		}

		int GetMaximumSecondaryChannels()
		{
			var secondaryMax = IsChanneled ? sectionConfiguration.CellsPerSubsection + 1 : (int)sectionConfiguration.CellsPerSubsection;

			if (remainingSecondaryAxisChannels.Count != 0)
			{
				secondaryMax = remainingSecondaryAxisChannels.Count + 1;
			}

			return secondaryMax;
		}

		void PopulateGridCellContent()
		{
			var primaryListCount = gridList.Count;
			var firstSecondaryListEntry = gridList.FirstOrDefault();
			var secondaryListCount = firstSecondaryListEntry != null ? firstSecondaryListEntry.Count : 0;
			grid.TotalRows = GetRow(primaryListCount, secondaryListCount);
			grid.TotalColumns = GetColumn(primaryListCount, secondaryListCount);

			grid.CellContent = new CellContent[grid.TotalRows][];

			for (var row = 0; row < grid.TotalRows; row++)
			{
				grid.CellContent[row] = new CellContent[grid.TotalColumns];

				for (var col = 0; col < grid.TotalColumns; col++)
				{
					grid.CellContent[row][col] = grid.Orientation == BMBoardSectionOrientation.Horizontal ? gridList[row][col] : gridList[col][row];
				}
			}
		}

		#region Set Cell Properties

		void SetCellProperties(CellContent cell, List<CellContent> secondaryIndexList)
		{
			SetCellTimespan(cell);
			SetCellChannel(cell, secondaryIndexList);
			SetCardSortType(cell);
		}

		#region Set Cell Channel

		void SetCellChannel(CellContent cell, List<CellContent> secondaryIndexList)
		{
			SetCellPrimaryChannel(cell, secondaryIndexList);
			SetCellSecondaryChannel(cell);
		}

		void SetCellPrimaryChannel(CellContent cell, List<CellContent> secondaryIndexList)
		{
			if (IsChanneled)
			{
				switch (cell.ContentType)
				{
					case CellContentType.ChannelHeading:
						if (GetSecondaryIndex(cell.Row, cell.Column) == 0)
						{
							if (remainingPrimaryAxisChannels.Any())
							{
								cell.Channel = remainingPrimaryAxisChannels.Peek() as IVisualBoardChannel;
								primaryChannelPlaced = true;
							}
						}
						break;
					case CellContentType.Cards:
						cell.Channel = GetPrimaryChannelForCell(secondaryIndexList);
						break;
				}
			}
		}

		void SetCellSecondaryChannel(CellContent cell)
		{
			if (HasSecondaryChannelsOrderedByTimeCode)
			{
				switch (cell.ContentType)
				{
					case CellContentType.ChannelHeading:
						if (GetPrimaryIndex(cell.Row, cell.Column) == 0)
						{
							if (remainingSecondaryAxisChannels.Any())
							{
								cell.SecondaryChannel = remainingSecondaryAxisChannels.Peek() as IVisualBoardChannel;
								secondaryChannelPlaced = true;
							}
						}
						break;
					case CellContentType.Cards:
						cell.SecondaryChannel = GetSecondaryChannelForCell(cell, gridList);
						break;
				}
			}
		}

		IVisualBoardChannel GetPrimaryChannelForCell(List<CellContent> secondaryIndexList)
		{
			return secondaryIndexList[0].Channel;
		}

		IVisualBoardChannel GetSecondaryChannelForCell(CellContent cell, List<List<CellContent>> list)
		{
			if (HasSecondaryChannels)
			{
				return list[0][GetSecondaryIndex(cell.Row, cell.Column)].SecondaryChannel;
			}

			return null;
		}

		#endregion

		#region Cell Lookup Helpers

		public CellContent GetCell(int primaryIndex, int secondaryIndex)
		{
			if (primaryIndex >= 0
				&& secondaryIndex >= 0
				&& primaryIndex < gridList.Count
				&& secondaryIndex < gridList[primaryIndex].Count)
			{
				return gridList[primaryIndex][secondaryIndex];
			}
			else
			{
				return null;
			}
		}

		public int GetPrimaryIndex(int row, int col)
		{
			return grid.Orientation == BMBoardSectionOrientation.Horizontal ? row : col;
		}

		public int GetSecondaryIndex(int row, int col)
		{
			return grid.Orientation == BMBoardSectionOrientation.Horizontal ? col : row;
		}

		public int GetRow(int primaryIndex, int secondaryIndex, int subsectionOffset = 0)
		{
			return grid.Orientation == BMBoardSectionOrientation.Horizontal ? primaryIndex + subsectionOffset : secondaryIndex;
		}

		public int GetColumn(int primaryIndex, int secondaryIndex, int subsectionOffset = 0)
		{
			return grid.Orientation == BMBoardSectionOrientation.Horizontal ? secondaryIndex : primaryIndex + subsectionOffset;
		}

		#endregion

		#region Set Cell Timespan

		void SetCellTimespan(CellContent cell)
		{
			if (CellShouldHaveTimeSpanSet(cell.ContentType))
			{
				cell.TimeInCell = sectionConfiguration.TimeSpanPerCell;
			}
		}

		static bool CellShouldHaveTimeSpanSet(CellContentType contentType)
		{
			return contentType == CellContentType.Cards || contentType == CellContentType.AgeHeading;
		}

		#endregion

		#region Set Card Sort Type

		protected abstract void SetCardSortType(CellContent cell);

		#endregion

		#region Set Cell Colors

		void SetCellColors(CellContent cell)
		{
			cell.BackColor = GetBackgroundColor(cell);
			cell.ForeColor = GetForegroundColor(cell);
		}

		protected virtual Color GetBackgroundColor(CellContent cell)
		{
			if (cell.TimeIndex < 0 && sectionConfiguration.OverdueBackgroundColorValue != null)
			{
				return sectionConfiguration.OverdueBackgroundColorValue.Value;
			}
			else
			{
				return sectionConfiguration.Section.BackgroundColorValue;
			}
		}

		protected virtual Color GetForegroundColor(CellContent cell)
		{
			if (cell.TimeIndex < 0 && sectionConfiguration.OverdueForegroundColorValue != null)
			{
				return sectionConfiguration.OverdueForegroundColorValue.Value;
			}
			else if (sectionConfiguration.Section.ForegroundColorValue != null)
			{
				return sectionConfiguration.Section.ForegroundColorValue.Value;
			}
			else if (cell.BackColor != null)
			{
				return cell.BackColor.Value.GetBestTextColorForBackground();
			}
			else
			{
				return Color.Black;
			}
		}

		#endregion

		#endregion

		#region Set Properties On Flattened

		void SetCellPropertiesAfterCellAllocation()
		{
			var flattenedSequence = ComponentGridHelper.FlattenSequence(grid.CellContent, sectionConfiguration);

			SetAgeHeadingAndCardCellAgeDetails(flattenedSequence);
			SetPropertiesOnFlattened(flattenedSequence);
			SetFirstAndLastFlags(sectionConfiguration, flattenedSequence);
			SetLabelOnCells(flattenedSequence);
			SetBackForeColors(flattenedSequence);
		}

		protected virtual void SetPropertiesOnFlattened(CellContent[] flattenedSequence)
		{
		}

		internal void SetBackForeColors(CellContent[] flattenedSequence = null)
		{
			if (flattenedSequence == null)
			{
				flattenedSequence = ComponentGridHelper.FlattenSequence(grid.CellContent, sectionConfiguration);
			}

			flattenedSequence.ForEach(SetCellColors);
		}

		#region Implementation

		#region Set First And Last Flags

		static void SetFirstAndLastFlags(BMComponentSectionConfiguration sectionConfiguration, CellContent[] flattenedSequence)
		{
			var flattenedSequenceExcludingChannelHeaders = flattenedSequence.Where(c => c.ContentType == CellContentType.AgeHeading ||
													c.ContentType == CellContentType.Cards);

			if (flattenedSequenceExcludingChannelHeaders.Any())
			{
				var lastAgeIndex = sectionConfiguration.TimeProgressionMode == TimeProgressionModeList.Codes.Age
					? flattenedSequenceExcludingChannelHeaders.Max(c => c.TimeIndex)
					: flattenedSequenceExcludingChannelHeaders.Min(c => c.TimeIndex);

				foreach (var cell in flattenedSequenceExcludingChannelHeaders.Where(c => c.TimeIndex == lastAgeIndex))
				{
					cell.IsLastCell = true;
				}

				var firstAgeIndex = sectionConfiguration.TimeProgressionMode == TimeProgressionModeList.Codes.Age
					? flattenedSequenceExcludingChannelHeaders.Min(c => c.TimeIndex)
					: flattenedSequenceExcludingChannelHeaders.Max(c => c.TimeIndex);

				foreach (var cell in flattenedSequenceExcludingChannelHeaders.Where(c => c.TimeIndex == firstAgeIndex))
				{
					cell.IsFirstCell = true;
				}
			}
		}

		#endregion

		#region Set Age Heading And Card Cell Age Details

		void SetAgeHeadingAndCardCellAgeDetails(CellContent[] flattenedSequence)
		{
			var ageHeadingCells = flattenedSequence.Where(c => c.ContentType == CellContentType.AgeHeading).ToArray();
			var cardCells = flattenedSequence.Where(c => c.ContentType == CellContentType.Cards).ToArray();
			var cellsGroupedBySecondaryAxis = flattenedSequence.GroupBy(c => c.SecondaryAxis).ToArray();

			var startingIndex = sectionConfiguration.TimeProgressionMode == TimeProgressionModeList.Codes.Age
				? 0 - sectionConfiguration.MaxOverdueSlots
				: (sectionConfiguration.CellsPerSubsection * sectionConfiguration.Subsections) - sectionConfiguration.MaxOverdueSlots - 1;

			var ageIndex = startingIndex;

			for (var i = 0; i < cardCells.Length; i++)
			{
				var cardCell = cardCells[i];

				var ageHeadingCell = i < ageHeadingCells.Length
					? ageHeadingCells[i]
					: null;

				var resetAgeIndex = IsChanneled && Math.Abs(ageIndex - startingIndex) == (sectionConfiguration.CellsPerSubsection * sectionConfiguration.Subsections);

				if (resetAgeIndex)
				{
					ageIndex = startingIndex;
				}

				cardCell.TimeIndex = ageIndex;
				cardCell.TimePercent = ComponentGridHelper.GetPercentPerCell(sectionConfiguration) * (cardCell.TimeIndex + 1);

				if (ageHeadingCell != null)
				{
					ageHeadingCell.TimeIndex = ageIndex;
					ageHeadingCell.TimePercent = cardCell.TimePercent;
					SetNonCardTimeIndexAndTimePercent(cardCell.SecondaryAxis, flattenedSequence, cardCell.TimeIndex, cardCell.TimePercent);
				}

				if (sectionConfiguration.ShowZones)
				{
					cardCell.Zone = ComponentGridHelper.GetZoneForAgeIndex(ageIndex, sectionConfiguration);
					var cellsPerZone = ComponentGridHelper.GetCellsPerZone(sectionConfiguration);
					if (cellsPerZone > 0)
					{
						if (!cardCell.IsLastAgeIndexForZone && (ageIndex + 1) % cellsPerZone == 0)
						{
							SetIsLastAgeIndexForZone(
								cellsGroupedBySecondaryAxis
									.Single(g => g.Key == cardCell.SecondaryAxis)
									.Where(c => c.ContentType.In(CellContentType.Cards, CellContentType.AgeHeading))
							);
						}
					}

					if (ageHeadingCell != null)
					{
						ageHeadingCell.Zone = cardCell.Zone;
					}
				}

				if (sectionConfiguration.TimeProgressionMode == TimeProgressionModeList.Codes.Age)
				{
					ageIndex++;
				}
				else
				{
					ageIndex--;
				}
			}
		}

		static void SetNonCardTimeIndexAndTimePercent(int secondaryAxis, CellContent[] flattenedSequence, int cellTimeIndex, decimal cellTimePercent)
		{
			var nonCardCells = flattenedSequence
				.Where(c =>
					c.ContentType != CellContentType.Cards
					&& c.ContentType != CellContentType.AgeHeading // AgeHeading cell with different secondaryAxis has different cellTimeIndex
					&& c.SecondaryAxis == secondaryAxis
					&& c.TimeIndex == -1);
			foreach (var nonCardCell in nonCardCells)
			{
				nonCardCell.TimeIndex = cellTimeIndex;
				nonCardCell.TimePercent = cellTimePercent;
			}
		}

		static void SetIsLastAgeIndexForZone(IEnumerable<CellContent> cells)
		{
			foreach (var cell in cells)
			{
				cell.IsLastAgeIndexForZone = true;
			}
		}

		#endregion

		#region Set Label On Cells

		protected virtual void SetLabelOnCells(CellContent[] flattenedSequence)
		{
			var ageHeadingCells = flattenedSequence.Where(l => l.ContentType == CellContentType.AgeHeading).OrderBy(o => o.TimeIndex).ToArray();

			foreach (var cell in ageHeadingCells)
			{
				cell.Label = GetAgeIndexLabel(cell);
			}

			if (ageHeadingCells.Length > 1)
			{
				ageHeadingCells[ageHeadingCells.Length - 1].Label = ageHeadingCells[ageHeadingCells.Length - 2].Label + '+';
			}

			var cardCells = flattenedSequence.Where(l => l.ContentType == CellContentType.Cards);

			foreach (var cell in cardCells)
			{
				cell.Label = GetLabelForCardCell(ageHeadingCells, cell);
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		string GetAgeIndexLabel(CellContent cell)
		{
			if (sectionConfiguration.ChannelSecondaryBy != BMConstants.ChannelByTimeCode || ComponentGridHelper.IsOneCell(sectionConfiguration))
			{
				return string.Empty;
			}

			string result;

			if (sectionConfiguration.ShowZones)
			{
				result = cell.TimePercent.ToString("0.#") + " %";
			}
			else
			{
				var totalMinutesPerCell = sectionConfiguration.TimePerCell.GetMinutesFromDateTimeSpan();
				var totalHoursPerCell = totalMinutesPerCell / 60.0;
				var totalDaysPerCell = totalHoursPerCell / BMConstants.WorkingHoursPerDay;

				if (totalDaysPerCell >= BMConstants.WorkingDaysPerWeek)
				{
					result = ComponentGridHelper.GetTimeUnit(totalDaysPerCell / BMConstants.WorkingDaysPerWeek, 'w', cell.TimeIndex);
				}
				else if (totalDaysPerCell >= 1)
				{
					result = ComponentGridHelper.GetTimeUnit(totalDaysPerCell, 'd', cell.TimeIndex);
				}
				else if (totalHoursPerCell >= 1)
				{
					result = ComponentGridHelper.GetTimeUnit(totalHoursPerCell, 'h', cell.TimeIndex);
				}
				else
				{
					result = ComponentGridHelper.GetTimeUnit(totalMinutesPerCell, 'm', cell.TimeIndex);
				}
			}

			return result;
		}

		string GetLabelForCardCell(CellContent[] ageHeadingCells, CellContent cell)
		{
			var primaryAxisChannelName = cell.Channel != null ? cell.Channel.GetChannelName(DisplayNameType.FullName) : string.Empty;
			var secondaryAxisChannelName = cell.SecondaryChannel != null ? cell.SecondaryChannel.GetChannelName(DisplayNameType.FullName) : string.Empty;
			var cellLabel = new ZStringBuilder();
			var secondaryAxisLabel = !string.IsNullOrWhiteSpace(secondaryAxisChannelName) ? secondaryAxisChannelName : GetMatchingAgeHeadingTextForGivenCardCell(ageHeadingCells, cell);

			if (!string.IsNullOrEmpty(primaryAxisChannelName))
			{
				cellLabel.Append(primaryAxisChannelName);

				if (!string.IsNullOrWhiteSpace(secondaryAxisLabel))
				{
					cellLabel.Append(", ");
				}
			}
			cellLabel.Append(secondaryAxisLabel);

			return cellLabel.ToString();
		}

		string GetMatchingAgeHeadingTextForGivenCardCell(CellContent[] ageHeadingCells, CellContent cell)
		{
			var ageHeading = ageHeadingCells.SingleOrDefault(l => l.TimeIndex == cell.TimeIndex);

			if (ageHeading != null)
			{
				return ageHeading.Label;
			}

			return string.Empty;
		}

		#endregion

		#endregion

		#endregion
	}
}
