using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public static class ComponentGridHelper
	{
		internal static string GetTimeUnit(double timeValue, char timeUnit, int ageIndex)
		{
			var multiplier = ageIndex < 0 ? ageIndex : ageIndex + 1;
			return string.Format(CultureInfo.InvariantCulture, "{0}{1}", (timeValue * multiplier).ToString("0.#", CultureInfo.InvariantCulture), timeUnit);
		}

		internal static bool IsOneCell(BMComponentSectionConfiguration sectionConfiguration)
		{
			return sectionConfiguration.CellsPerSubsection == 1 && sectionConfiguration.Subsections == 1;
		}

		internal static bool HasSubComponents(BMComponentSectionConfiguration sectionConfiguration, bool? isInConstrainedMode = null)
		{
			return !sectionConfiguration.IsWrapped && (isInConstrainedMode ?? sectionConfiguration.Section.IsInConstrainedMode);
		}

		internal static decimal GetPercentPerCell(BMComponentSectionConfiguration sectionConfiguration)
		{
			var totalCells = sectionConfiguration.CellsPerSubsection * sectionConfiguration.Subsections - sectionConfiguration.CellsInZoneZero;
			return totalCells > 0 ? 1 / (ZDecimal)totalCells * 100m : 0m;
		}

		internal static IEnumerable<BMComponent> GetBufferSubComponents(BMComponent buffer)
		{
			return buffer.ChildComponents.Where(c => c.IsBuffer);
		}

		internal static int GetZoneForAgeIndex(int ageIndex, BMComponentSectionConfiguration sectionConfiguration)
		{
			var elapsedZones = ageIndex / Math.Max(1, GetCellsPerZone(sectionConfiguration));
			return Math.Max(0, BMConstants.NumberOfZones - elapsedZones);
		}

		internal static int GetCellsPerZone(BMComponentSectionConfiguration sectionConfiguration)
		{
			var totalCells = sectionConfiguration.CellsPerSubsection * sectionConfiguration.Subsections;
			totalCells -= sectionConfiguration.CellsInZoneZero;

			return totalCells / BMConstants.NumberOfZones;
		}

		internal static ZInt ConvertSecondaryAxisToTimeIndex(BMComponentSectionConfiguration sectionConfiguration, int secondaryAxis)
		{
			return sectionConfiguration.FlowsInSameDirectionAsAxis()
				? secondaryAxis - 1 // -1 to exclude row 0 i.e. channel-header
				: (sectionConfiguration.CellsPerSubsection - 1) - (secondaryAxis - 1); // -1 to exclude row 0 i.e. channel-header on both CellsPerSubsection and secondaryAxis
		}

		#region Constraints

		public static void SetConstraintLine(bool isInConstrainedMode, BMComponentSectionConfiguration sectionConfiguration, BMComponent buffer, ComponentGrid grid)
		{
			if (isInConstrainedMode && sectionConfiguration.ShowZones)
			{
				var borderThickness = GetConstraintBorderThickness(sectionConfiguration.FlowDirection);
				var cellsToRemoveConstraintLineFrom = grid.Cells.Where(c => c.ContentType != CellContentType.ZoneHeading && c.BorderThickness != null);

				foreach (var cell in cellsToRemoveConstraintLineFrom)
				{
					cell.BorderThickness = null;
				}

				foreach (var constraint in buffer.ChildComponents.Where(c => c.FC_Type == BMComponentTypeList.Codes.Constraint))
				{
					var constraintTimeIndex = GetConstraintTimeIndex(sectionConfiguration, constraint, buffer);
					if (constraintTimeIndex != null && constraintTimeIndex > 0)
					{
						var adjustedIndex = constraintTimeIndex - 1; // index is reduced by one because line needs to appear on cell *before* the constraint.
						var cellsToAddConstraintLineTo = grid.Cells.Where(c => c.ContentType != CellContentType.ZoneHeading && c.TimeIndex == adjustedIndex);

						foreach (var cell in cellsToAddConstraintLineTo)
						{
							cell.BorderThickness = borderThickness;
						}
					}
				}
			}
		}

		public static string GetConstraintBorderThickness(ZString flowDirection)
		{
			switch (flowDirection)
			{
				case FlowDirectionList.Codes.Left:
					return string.Format(CultureInfo.InvariantCulture, "{0} 0 0 0", BorderThickness);

				case FlowDirectionList.Codes.Up:
					return string.Format(CultureInfo.InvariantCulture, "0 {0} 0 0", BorderThickness);

				case FlowDirectionList.Codes.Right:
					return string.Format(CultureInfo.InvariantCulture, "0 0 {0} 0", BorderThickness);

				case FlowDirectionList.Codes.Down:
					return string.Format(CultureInfo.InvariantCulture, "0 0 0 {0}", BorderThickness);

				default:
					throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "FlowDirection {0} is not supported", flowDirection));
			}
		}

		internal static int? GetConstraintTimeIndex(BMComponentSectionConfiguration sectionConfiguration, BMComponent constraint, BMComponent buffer)
		{
			var minutesPerCell = GetTimePerCellForBuffer(sectionConfiguration, buffer).GetMinutesFromDateTimeSpan();

			if (minutesPerCell != 0)
			{
				return (int)(constraint.FC_OffsetInMinutes / minutesPerCell);
			}

			return null;
		}

		public static ZDateTime GetTimePerCellForBuffer(BMComponentSectionConfiguration config, BMComponent buffer)
		{
			var cellTimeSpan = Math.Max(1.0d, (double)buffer.FC_BufferTimespanInMinutes / Math.Max(1, (config.CellsPerSubsection * config.Subsections) - config.CellsInZoneZero));
			return ZInt.Zero.GetDateTimeFromMinutes().ToDateTime().AddMinutes(cellTimeSpan);
		}

		public const int BorderThickness = 5;

		#endregion

		#region Flatten

		internal static CellContent[] FlattenSequence(CellContent[][] cellContent, BMComponentSectionConfiguration sectionConfiguration)
		{
			var isWrapped = sectionConfiguration.IsWrapped;
			var lastCell = sectionConfiguration.LastCell;

			return FlattenSequence(cellContent, isWrapped, lastCell, sectionConfiguration.FlowDirection);
		}

		static CellContent[] FlattenSequence(CellContent[][] cellContent, bool isWrapped, ZString lastCell, ZString flowDirection)
		{
			switch (flowDirection)
			{
				case FlowDirectionList.Codes.Right:
					{
						return FlowDirectionRightFlatten(cellContent, isWrapped, lastCell);
					}
				case FlowDirectionList.Codes.Left:
					{
						return FlowDirectionLeftFlatten(cellContent, isWrapped, lastCell);
					}
				case FlowDirectionList.Codes.Down:
					{
						return FlowDirectionDownFlatten(cellContent, isWrapped, lastCell);
					}
				case FlowDirectionList.Codes.Up:
					{
						return FlowDirectionUpFlatten(cellContent, isWrapped, lastCell);
					}
			}

			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Flow direction [{0}] is not supported", flowDirection));
		}

		static CellContent[] FlowDirectionRightFlatten(CellContent[][] cellContent, bool isWrapped, ZString lastCell)
		{
			var shouldReverse = isWrapped && lastCell == LastCellList.Codes.Top;
			return (shouldReverse ? cellContent.Reverse() : cellContent).SelectMany(c => c).ToArray();
		}

		static CellContent[] FlowDirectionLeftFlatten(CellContent[][] cellContent, bool isWrapped, ZString lastCell)
		{
			var shouldReverse = isWrapped && lastCell == LastCellList.Codes.Top;
			return (shouldReverse ? cellContent.Reverse() : cellContent).SelectMany(c => c.Reverse()).ToArray();
		}

		static CellContent[] FlowDirectionDownFlatten(CellContent[][] cellContent, bool isWrapped, ZString lastCell)
		{
			var shouldReverse = isWrapped && lastCell == LastCellList.Codes.Left;
			var result = cellContent.SelectMany(c => c);
			return (shouldReverse ? result.OrderByDescending(c => c.Column) : result.OrderBy(c => c.Column)).ThenBy(c => c.Row).ToArray();
		}

		static CellContent[] FlowDirectionUpFlatten(CellContent[][] cellContent, bool isWrapped, ZString lastCell)
		{
			var shouldReverse = isWrapped && lastCell == LastCellList.Codes.Left;
			var result = cellContent.SelectMany(c => c);
			return (shouldReverse ? result.OrderByDescending(c => c.Column) : result.OrderBy(c => c.Column)).ThenByDescending(c => c.Row).ToArray();
		}

		#endregion

#if DEBUG
		#region GridAsASCIIArt

		public static string GetGridAsASCIIArt(CellContent[][] gridContent)
		{
			var builder = new StringBuilder();
			for (var row = 0; row < gridContent.Length; row++)
			{
				for (var col = 0; col < gridContent[row].Length; col++)
				{
					builder.Append(GetSymbol(gridContent[row][col]));
				}
				builder.AppendLine();
			}
			return builder.ToString();
		}

		static string GetSymbol(CellContent content)
		{
			var contentType = content.ContentType;
			switch (contentType)
			{
				case CellContentType.AgeHeading:
					return "A";
				case CellContentType.CCRHeading:
					return "R";
				case CellContentType.ChannelHeading:
					return "H";
				case CellContentType.Label:
					return "L";
				case CellContentType.SubComponentZoneHeading:
					return "S";
				case CellContentType.ZoneHeading:
					return content.Zone.ToString();
				case CellContentType.Cards:
					return "C";
				default:
					return "X";
			}
		}

		#endregion
#endif
	}
}
