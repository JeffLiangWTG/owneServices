using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.CalendarArithmetic;
using CargoWise.Common;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	class NetworkScaleDescriptor : INetworkScaleDescriptor
	{
		internal NetworkScaleDescriptor(JobNetwork network)
		{
			this.network = Argument.NotNull(network, "network");

			var scheduleBizo = DiagramShape.ScheduleBizo;
			isBranchOrDepartmentInvalidated = !scheduleBizo.Validation.IsBranchAndDepartmentValid;

			context = isBranchOrDepartmentInvalidated ? WorkingTimeContext.Create(DiagramShape.Factory) : WorkingTimeContext.Create(DiagramShape);
			workTimeArithmetic = context.GetWorkTimeArithmetic(DiagramShape.Factory);

			scaleStartTime = GetOrCalculateStartTime(DiagramShape, context);
		}

		readonly JobNetwork network;
		readonly IWorkTimeArithmetic workTimeArithmetic;
		readonly ZDateTime scaleStartTime;
		readonly WorkingTimeContext context;

		BMNCNShape DiagramShape => network.DiagramShape;
		IEnumerable<BMNCNLevelingRule> LevelingRules => network.DiagramEntity.LevelingRules;

		#region INetworkScaleDescriptor Members

		bool INetworkScaleDescriptor.IsScaleDescriptorInvalidated => DiagramShape.IsDeleted || isBranchOrDepartmentInvalidated;

		readonly bool isBranchOrDepartmentInvalidated;
		bool INetworkScaleDescriptor.IsBranchOrDepartmentInvalidated => isBranchOrDepartmentInvalidated;

		void INetworkScaleDescriptor.ClearCachedData()
		{
			mostOverloadedAffinitiesCache.Clear();
		}

		INetworkScaleSet INetworkScaleDescriptor.GetScaleSetForColumns(int columnAmount)
		{
			var scalePoints = new List<INetworkScalePoint>(columnAmount);
			var presentColumn = -1;

			for (int i = 0; i < columnAmount; i++)
			{
				if (presentColumn == -1 && IsFirstIndexInThePresent(i))
				{
					presentColumn = i;
				}
				scalePoints.Add(new NetworkScalePoint(i, GetLabel(i), GetTooltip(i)));
			}

			var backgrounds = new List<INetworkScaleBackground>();
			var lastColor = Color.Empty;
			var lastColorStart = -1;

			foreach (var entity in network.Entities.ShapeEntities)
			{
				entity.AppliedLevelingRuleViolationsAtIndex.Clear();
			}

			for (int i = 0; i < columnAmount; i++)
			{
				ApplyLevelingRuleViolationsToEntitiesAtIndex(i);
				var color = GetBackgroundColor(i);
				if (color != lastColor)
				{
					if (lastColor != Color.Empty)
					{
						backgrounds.Add(new NetworkScaleBackground(lastColor, lastColorStart, i));
					}

					lastColor = color;
					lastColorStart = i;
				}
			}

			foreach (var entity in network.Entities.ShapeEntities)
			{
				entity.UpdateLevelingRuleViolations();
			}

			if (lastColor != Color.Empty)
			{
				backgrounds.Add(new NetworkScaleBackground(lastColor, lastColorStart, columnAmount));
			}

			return new NetworkScaleSet(scalePoints.AsReadOnly(), backgrounds.AsReadOnly(), presentColumn);
		}

		Color GetBackgroundColor(int columnIndex)
		{
			return GetColor(columnIndex);
		}

		string GetLabel(int columnIndex)
		{
			if (scaleStartTime.IsValid)
			{
				var minutes = GetMinutesOffset(columnIndex);
				return GetTimeHeading(minutes, columnIndex);
			}
			else
			{
				var minutes = GetMinutesOffset(columnIndex + 1);
				if (minutes == 0)
				{
					return string.Empty;
				}
				else
				{
					return GetOffsetHeading(minutes);
				}
			}
		}

		string GetTooltip(int columnIndex)
		{
			var affinity = GetMostOverloadedAffinity(columnIndex);
			if (affinity != null)
			{
				return Res.GetString("30e163c6-c75c-434a-b10a-1675d3548556", "The affinity {0} is overloaded at this position.", affinity.Name);
			}

			return string.Empty;
		}

		bool IsFirstIndexInThePresent(int columnIndex)
		{
			var columnDate = scaleStartTime.IsValid ? GetTimeFromDiagramStart(GetMinutesOffset(columnIndex), columnIndex) : ZDateTime.Empty;

			return columnDate.IsValid
				&& columnDate.Date >= ZDateTime.Today.Date
				&& ((columnIndex == 0 || GetColor(columnIndex - 1) == ColumnIndexInThePastColor)
					&& GetColor(columnIndex) != ColumnIndexInThePastColor);
		}

		#endregion

		#region Implementation

		#region Start Time

		static ZDateTime GetOrCalculateStartTime(BMNCNShape diagramShape, WorkingTimeContext context)
		{
			var startTimeUtc = diagramShape.ScheduledStartTimeUtc;

			if (startTimeUtc.IsValid)
			{
				return context.ToLocalTime(startTimeUtc, diagramShape.Factory);
			}
			else
			{
				var endTimeUtc = diagramShape.ScheduledFinishTimeUtc;

				if (endTimeUtc.IsValid && diagramShape.ExplicitDurationMinutes > 0)
				{
					var endTimeInLocal = context.ToLocalTime(endTimeUtc, diagramShape.Factory);
					var workingTimeArithmetic = context.GetWorkTimeArithmetic(diagramShape.Factory);

					return workingTimeArithmetic.GetDateTimeInWorkingHoursFutureOrPast(endTimeInLocal.ToDateTime(), -(double)diagramShape.ExplicitDurationHours);
				}
			}

			return ZDateTime.Empty;
		}

		#endregion

		#region Headings

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		string GetTimeHeading(int minutes, int columnIndex)
		{
			var offsetTime = GetTimeFromDiagramStart(minutes, columnIndex);
			return offsetTime.ToLongTimeString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		static string GetOffsetHeading(int minutes)
		{
			if (minutes >= MinutesPerWorkWeek())
			{
				return GetHeading(minutes / MinutesPerWorkWeek(), TimeConstants.TimeStrings.Week, TimeConstants.TimeStrings.Weeks);
			}
			else if (minutes >= MinutesPerWorkDay())
			{
				return GetHeading(minutes / MinutesPerWorkDay(), TimeConstants.TimeStrings.Day, TimeConstants.TimeStrings.Days);
			}
			else if (minutes >= 60)
			{
				return GetHeading(minutes / 60, TimeConstants.TimeStrings.Hour, TimeConstants.TimeStrings.Hours);
			}
			else
			{
				return GetHeading(minutes, TimeConstants.TimeStrings.Minute, TimeConstants.TimeStrings.Minutes);
			}
		}

		static string GetHeading(double unitsValue, string unitsSingularName, string unitsPluralName)
		{
			return unitsValue.FormatWithNoMoreThanTwoDecimalPlaces() + " " + (unitsValue == 1 ? unitsSingularName : unitsPluralName);
		}

		static double MinutesPerWorkDay()
		{
			return 60 * Enterprise.BufferManagement.Business.BMConstants.WorkingHoursPerDay;
		}

		static double MinutesPerWorkWeek()
		{
			return 60 * Enterprise.BufferManagement.Business.BMConstants.WorkingHoursPerDay * Enterprise.BufferManagement.Business.BMConstants.WorkingDaysPerWeek;
		}

		#endregion

		#region Colors

		readonly Color ColumnIndexInThePastColor = Color.FromArgb(255, 221, 221, 221);

		Color GetColor(int columnIndex)
		{
			if (scaleStartTime.IsValid)
			{
				var now = context.GetCurrentLocalTime(DiagramShape.Factory);
				var scaleTime = GetTimeFromDiagramStart(GetMinutesOffset(columnIndex), columnIndex);

				if (scaleTime < now)
				{
					return GetColorOfPastIndex(columnIndex);
				}
			}

			return GetColorOfIncompleteIndex(columnIndex);
		}

		Color GetColorOfIncompleteIndex(int columnIndex)
		{
			var levelingRuleToApply = GetLevelingRuleToApplyToIndex(columnIndex);
			if (levelingRuleToApply != null)
			{
				return Color.FromName(levelingRuleToApply.ColorName);
			}
			else
			{
				var affinity = GetMostOverloadedAffinity(columnIndex);
				return affinity != null ? ((IAffinity)affinity).Colour : Color.Empty;
			}
		}

		Color GetColorOfPastIndex(int columnIndex)
		{
			var levelingRuleToApply = GetLevelingRuleToApplyToIndex(columnIndex);
			if (levelingRuleToApply != null)
			{
				return Color.FromName(levelingRuleToApply.ColorName);
			}
			else
			{
				var affinity = GetMostOverloadedAffinity(columnIndex);
				return affinity != null ? ((IAffinity)affinity).Colour : ColumnIndexInThePastColor;
			}
		}

		BMNCNLevelingRule GetLevelingRuleToApplyToIndex(int index)
		{
			if (LevelingRules != null && LevelingRules.Any())
			{
				return network.Entities.ShapeEntities.SelectMany(e => e.AppliedLevelingRuleViolationsAtIndex)
				   .Where(e => e.Item1 == index).Select(e => e.Item2).OrderBy(r => r.BNR_Name)
				   .FirstOrDefault();
			}

			return null;
		}

		void ApplyLevelingRuleViolationsToEntitiesAtIndex(int index)
		{
			var entities = Array.Empty<ShapeNetworkEntity>();

			if (LevelingRules != null && LevelingRules.Any())
			{
				foreach (var rule in LevelingRules)
				{
					var applicableChannelPKs = rule.ChannelLinks.Select(c => c.BNK_BNL_Channel);
					switch (rule.BNR_Type)
					{
						case LevelingRuleTypeList.Codes.MaximumConcurrentEntities:
							entities = LevelingRuleHelper.GetEntitiesThatViolateConcurrencyLevelingRule(index, rule.BNR_RuleValue, network.Entities.ShapeEntities, DiagramShape, applicableChannelPKs);
							break;

						case LevelingRuleTypeList.Codes.MinimumEntityStartGapSize:
							entities = LevelingRuleHelper.GetEntitiesThatViolateStartLevelingRule(index, rule.BNR_RuleValue, network.Entities.ShapeEntities, DiagramShape, applicableChannelPKs);
							break;

						case LevelingRuleTypeList.Codes.MinimumGapSizeBetweenEntities:
							entities = LevelingRuleHelper.GetEntitiesThatViolateGapLevelingRule(index, rule.BNR_RuleValue, network.Entities.ShapeEntities, DiagramShape, applicableChannelPKs);
							break;
					}

					foreach (var entity in entities)
					{
						if (!entity.AppliedLevelingRuleViolationsAtIndex.Any(r => r.Item1 == index && object.ReferenceEquals(r.Item2, rule)))
						{
							entity.AppliedLevelingRuleViolationsAtIndex.Add(index, rule);
						}
					}
				}
			}
		}

		ShapeAffinity GetMostOverloadedAffinity(int columnIndex)
		{
			return mostOverloadedAffinitiesCache.GetOrAdd(columnIndex, () =>
			{
				var cellRectangle = new Rectangle(columnIndex * DiagramShape.ScaleUnitPixelSize, 1, DiagramShape.ScaleUnitPixelSize, 1);
				var shapesInsideCell = (
					from entity in network.Entities.ShapeEntities
					where entity.Supports(NetworkActions.Affinities) && !entity.IsNonScheduled
					let shape = entity.AsShape()
					where !((IBufferedItem)shape).IsClosed
					where entity.GetScaleRectangle().IntersectsWith(cellRectangle)
					select entity
					).ToArray();

				Tuple<int, ShapeAffinity> mostOverloadedAffinity = null;

				foreach (ShapeAffinity affinity in shapesInsideCell.SelectMany(s => ((INetworkEntity)s).AppliedAffinities))
				{
					var overloadAmount = shapesInsideCell.Count(s => ((INetworkEntity)s).AppliedAffinities.Contains(affinity)) - affinity.AllowedConcurrency;
					if (overloadAmount > 0)
					{
						if (mostOverloadedAffinity == null || mostOverloadedAffinity.Item1 < overloadAmount)
						{
							mostOverloadedAffinity = Tuple.Create(overloadAmount, affinity);
						}
					}
				}

				return mostOverloadedAffinity != null ? mostOverloadedAffinity.Item2 : null;
			});
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		int GetMinutesOffset(int columnIndex)
		{
			if (DiagramShape.IsDeleted)
			{
				return 0;
			}

			var scaleMinutes = DiagramShape.Scale.GetMinutesFromDateTimeSpan();
			return (int)(columnIndex * scaleMinutes);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		ZDateTime GetTimeFromDiagramStart(int minutes, int columnIndex)
		{
			if (diagramStartWorkingMinutesCache.ContainsKey(minutes))
			{
				return diagramStartWorkingMinutesCache[minutes];
			}
			else
			{
				var startingTime = scaleStartTime;
				var additionalMinutes = minutes;

				if (columnIndex > 0)
				{
					var previousIndexMinutes = GetMinutesOffset(columnIndex - 1);

					if (diagramStartWorkingMinutesCache.ContainsKey(previousIndexMinutes))
					{
						startingTime = diagramStartWorkingMinutesCache[previousIndexMinutes];
						additionalMinutes = GetMinutesOffset(columnIndex: 1);
					}
				}

				return diagramStartWorkingMinutesCache[minutes] = new ZDateTime(workTimeArithmetic.GetDateTimeInWorkingHoursFutureOrPast(startingTime.ToDateTime(), additionalMinutes / 60.0));
			}
		}

		readonly Dictionary<int, ZDateTime> diagramStartWorkingMinutesCache = new Dictionary<int, ZDateTime>();
		readonly Dictionary<int, ShapeAffinity> mostOverloadedAffinitiesCache = new Dictionary<int, ShapeAffinity>();

		#endregion
	}
}
