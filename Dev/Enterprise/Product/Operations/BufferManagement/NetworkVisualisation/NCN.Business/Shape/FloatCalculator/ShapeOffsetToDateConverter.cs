using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class ShapeOffsetToDateConverter : OffsetToDateConverter
	{
		internal ShapeOffsetToDateConverter(ShapeNetworkEntity rootShapeEntity, WorkingTimeContext context, BusinessObjectFactory factory)
			: this(rootShapeEntity.Shape.ScheduledStartTimeUtc, rootShapeEntity.Shape.ScheduledFinishTimeUtc, rootShapeEntity.Scale, rootShapeEntity.EntityDurationMinutes, context, factory)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		internal ShapeOffsetToDateConverter(ZDateTime scheduledStartTimeUtc, ZDateTime scheduledFinishTimeUtc, ZDateTime scale, int diagramDurationInMinutes, WorkingTimeContext context, BusinessObjectFactory factory)
			: base(context, factory)
		{
			this.diagramDurationInHours = diagramDurationInMinutes / 60m;
			this.scale = scale;
			this.scheduledStartTimeUtc = scheduledStartTimeUtc;
			this.scheduledFinishTimeUtc = scheduledFinishTimeUtc;
		}

		readonly ZDateTime scale;
		readonly ZDateTime scheduledStartTimeUtc;
		readonly ZDateTime scheduledFinishTimeUtc;
		readonly decimal? diagramDurationInHours;

		ZDateTime LocalStartTime
		{
			get
			{
				if (localStartTime == null)
				{
					localStartTime = Context.ToLocalTime(scheduledStartTimeUtc, Factory);
				}

				return localStartTime.Value;
			}
		}

		ZDateTime? localStartTime;

		ZDateTime LocalFinishTime
		{
			get
			{
				if (localFinishTime == null)
				{
					localFinishTime = Context.ToLocalTime(scheduledFinishTimeUtc, Factory);
				}

				return localFinishTime.Value;
			}
		}

		ZDateTime? localFinishTime;

		public static double GetMinutesForScaleSize(ZDateTime scale, double scaledSize)
		{
			var scaleMinutes = scale.GetMinutesFromDateTimeSpan();
			var minutes = (scaledSize / CCPMConstants.ScaledModeDiagramPixelsPerScaleUnit * scaleMinutes);
			return Math.Min(int.MaxValue, minutes);
		}

		public static double GetSizeForMinutes(ZDateTime scale, double minutes)
		{
			var scaleMinutes = scale.GetMinutesFromDateTimeSpan();
			var size = CCPMConstants.ScaledModeDiagramPixelsPerScaleUnit * minutes / scaleMinutes;
			return Math.Min(int.MaxValue, size);
		}

		public ZDateTime GetTimeInUtcForScalePosition(double position)
		{
			var hoursOffset = GetHoursOffsetForPosition(position);
			return GetTimeInUtcForTimeOffset(hoursOffset);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		public decimal GetHoursOffsetForPosition(double position)
		{
			return !scale.IsEmpty ? (decimal)GetMinutesForScaleSize(scale, position) / 60m : decimal.Zero;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		public override ZDateTime GetTimeInUtcForTimeOffset(decimal hoursOffset)
		{
			if (scheduledStartTimeUtc.IsValid)
			{
				var futureTime = new ZDateTime(WorkingDays.GetDateTimeInWorkingHoursFutureOrPast(LocalStartTime.ToDateTime(), (double)hoursOffset), DateTimeKind.Local);

				return futureTime.ToUniversalBranchTime(Context.Branch);
			}
			else if (scheduledFinishTimeUtc.IsValid)
			{
				var actualOffset = GetOffsetFromAgreedDeliveryDate(hoursOffset);
				var timeToUse = LocalFinishTime.ToDateTime();

				var pastTime = new ZDateTime(WorkingDays.GetWorkingDayFromHours(timeToUse, -(int)actualOffset), DateTimeKind.Local);

				return pastTime.ToUniversalBranchTime(Context.Branch);
			}
			else
			{
				// When the diagram has no date, don't store offsets from the current time. That leads to excessive updates every time a scaled diagram with no date is saved.
				return ZDateTime.Empty;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		decimal GetOffsetFromAgreedDeliveryDate(decimal hoursOffset)
		{
			if (diagramDurationInHours.HasValue)
			{
				return diagramDurationInHours.Value - hoursOffset;
			}
			else
			{
				return hoursOffset;
			}
		}
	}
}
