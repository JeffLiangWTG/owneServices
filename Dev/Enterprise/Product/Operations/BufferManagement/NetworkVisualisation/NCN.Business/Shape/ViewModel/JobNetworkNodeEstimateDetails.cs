using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class JobNetworkNodeEstimateDetails
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		public JobNetworkNodeEstimateDetails(BMNCNShape shape, decimal totalEstimateHours, decimal completedEstimateHours)
		{
			if (totalEstimateHours <= 0)
			{
				throw new ArgumentException("Total estimate hours cannot equal to zero", nameof(totalEstimateHours));
			}

			Shape = shape;
			TotalEstimateHours = totalEstimateHours;
			CompletedEstimateHours = completedEstimateHours;
			PercentComplete = completedEstimateHours / totalEstimateHours;

			RiskReason = GetRiskReason(this, shape);
		}

		public BMNCNShape Shape { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		public decimal TotalEstimateHours { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		public decimal CompletedEstimateHours { get; }
		public decimal PercentComplete { get; }
		public RiskReason RiskReason { get; }

		static RiskReason GetRiskReason(JobNetworkNodeEstimateDetails estimateDetails, BMNCNShape shape)
		{
			if (shape.RootShape.IsScaled)
			{
				if (IsActualDurationGreaterThanPlannedDuration(estimateDetails))
				{
					return RiskReason.OverEstimate;
				}
				else if (IsStartableAndScheduledToStartButNotReleased(estimateDetails))
				{
					return RiskReason.LateRelease;
				}
			}

			return RiskReason.None;
		}

		static bool IsActualDurationGreaterThanPlannedDuration(JobNetworkNodeEstimateDetails estimateDetails)
		{
			return estimateDetails.Shape.ProcessHeader != null && estimateDetails.Shape.ProcessHeader.TotalActualHoursIncludingChildren > estimateDetails.Shape.ExplicitDurationHours;
		}

		static bool IsStartableAndScheduledToStartButNotReleased(JobNetworkNodeEstimateDetails estimateDetails)
		{
			var processHeader = estimateDetails.Shape.ProcessHeader;

			if (processHeader != null && !processHeader.HasOpenPrerequisites && estimateDetails.Shape.ScheduledStartTimeUtc < ZDateTime.UtcNow)
			{
				var jobHeader = processHeader as ProcessJobHeader;

				if (jobHeader != null)
				{
					return !jobHeader.ProcessHeaders.Any(w => w.IsReleased);
				}
				else
				{
					return !processHeader.IsReleased;
				}
			}

			return false;
		}
	}
}
