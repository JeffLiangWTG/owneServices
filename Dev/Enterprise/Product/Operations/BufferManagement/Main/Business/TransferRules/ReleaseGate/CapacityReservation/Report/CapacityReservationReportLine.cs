using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public sealed class CapacityReservationReportLine
	{
		internal CapacityReservationReportLine(CapacityReservationReport report, GlbStaff resource, IResourceCapacity availableCapacityAtThisPointInQueue, int? placeInQueue)
		{
			Report = Argument.NotNull(report, nameof(report));
			Resource = Argument.NotNull(resource, nameof(resource));

			IsNominatedCCRInConstrainedModeReleaseGroup = ConstrainedModeHelper.IsDesignatedCapacityConstrainedResourceInConstrainedReleaseGroup(report.Buffer.Factory, resource.GS_Code, report.Buffer);

			AvailableCapacityAtThisPointInQueue_ForDisplayOnly = ResourceUsedNonCCRTemporaryOverloadMultiplier
				? availableCapacityAtThisPointInQueue.AvailableCapacityForWorkInvolvingCCR
				: availableCapacityAtThisPointInQueue.AvailableCapacity;

			AvailableCapacityAtThisPointInQueueIncludingAllowedOverload_ForEvaluatingCapacity = report.GetRelevantAvailableCapacity(resource, availableCapacityAtThisPointInQueue).EffectiveAvailableCapacityConsideringAllowedOverload;

			PlaceInQueue = placeInQueue;
		}

		public CapacityReservationReport Report { get; }
		public GlbStaff Resource { get; }

		public bool IsNominatedCCRInConstrainedModeReleaseGroup { get; }

		public ZDateTime ResourceReturningFromLeaveTime { get; internal set; }
		public ZDateTime EarliestTimeWorkCanBeReleasedToResource { get; internal set; }

		public decimal AvailableCapacityAtThisPointInQueue_ForDisplayOnly { get; }
		public decimal AvailableCapacityAtThisPointInQueueIncludingAllowedOverload_ForEvaluatingCapacity { get; }
		public bool ResourceUsedNonCCRTemporaryOverloadMultiplier => !IsNominatedCCRInConstrainedModeReleaseGroup && Report.WorkflowInvolvesNominatedCCR;
		public int? PlaceInQueue { get; }

		public bool ThisResourceBlocksWorkflowRelease { get; internal set; }
		public bool ThisResourcesLeaveBlocksWorkflowRelease { get; internal set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public decimal CapacityHoursRequiredForDirectlyAssignedTasks { get; internal set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public decimal CapacityHoursRequiredForCapabilityTasks { get; internal set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public decimal TotalCapacityHoursRequired => CapacityHoursRequiredForDirectlyAssignedTasks + CapacityHoursRequiredForCapabilityTasks;

		public ICollection<GlbStaff> OtherResourcesConsidered => Report.ResourceCapacityReportLines.Select(l => l.Resource).Except(Resource).ToArray();

		public ICollection<TaskLogDetails> DirectlyAssignedTasks { get; } = new List<TaskLogDetails>();
		public ICollection<TaskLogDetails> CapabilityTasks { get; } = new List<TaskLogDetails>();
	}
}
