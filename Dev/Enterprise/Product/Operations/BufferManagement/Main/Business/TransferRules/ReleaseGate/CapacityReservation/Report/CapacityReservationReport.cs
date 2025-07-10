using System.Collections.Generic;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public sealed class CapacityReservationReport
	{
		internal CapacityReservationReport(BMComponent buffer, IWorkflow workflowBeingConsideredForRelease, bool workflowInvolvesNominatedCCR, decimal zoneMultiplier)
		{
			Buffer = buffer;
			WorkflowBeingConsideredForRelease = workflowBeingConsideredForRelease;
			WorkflowInvolvesNominatedCCR = workflowInvolvesNominatedCCR;
			ZoneMultiplier = zoneMultiplier == 0 ? 1 : zoneMultiplier;
		}

		public BMComponent Buffer { get; }
		public IWorkflow WorkflowBeingConsideredForRelease { get; }
		public bool WorkflowInvolvesNominatedCCR { get; }
		public decimal ZoneMultiplier { get; }

		public ICollection<CapacityReservationReportLine> ResourceCapacityReportLines { get; } = new List<CapacityReservationReportLine>();
		public ICollection<MissingCapabilityCapacityReservationReportLine> CapabilityTasksWithNoResourcesReportLines { get; } = new List<MissingCapabilityCapacityReservationReportLine>();

		public BufferReleaseOutcome BufferReleaseOutcome { get; internal set; }

		public ICollection<string> MissingResourceCodes { get; } = new List<string>();

		public bool WasWorkflowReleased => BufferReleaseOutcome.IsReleasable();

		public string CreateOutcomeLogMessage(bool includeOtherResourcesConsideredMessage = false)
		{
			return CapacityReservationLogCreator.CreateLogMessageForReport(this, Buffer, BufferReleaseOutcome.IsReleasable(), includeOtherResourcesConsideredMessage);
		}

		internal AvailableCapacity GetRelevantAvailableCapacity(GlbStaff resource, IResourceCapacity resourceCapacity)
		{
			return GetRelevantAvailableCapacity(resource, resourceCapacity.AvailableCapacity, resourceCapacity.AvailableCapacityForWorkInvolvingCCR);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		internal AvailableCapacity GetRelevantAvailableCapacity(GlbStaff resource, decimal availableCapacityHours, decimal availableCapacityHoursForCcrWork)
		{
			var shouldUseCapacityForCcrWork = WorkflowInvolvesNominatedCCR && !ConstrainedModeHelper.IsDesignatedCapacityConstrainedResourceInConstrainedReleaseGroup(Buffer.Factory, resource.GS_Code, Buffer);
			var relevantAvailableCapacity = shouldUseCapacityForCcrWork ? availableCapacityHoursForCcrWork : availableCapacityHours;

			if (relevantAvailableCapacity > 0)
			{
				// Because if someone has 0.5 hours available, we still allow 1 hour to be released to them and let their capacity get a bit negative.
				// They won't be shown as 'overloaded' unless it gets to 120% utilisation of their full capacity.

				return new AvailableCapacity(relevantAvailableCapacity, relevantAvailableCapacity * BMConstants.WeLetALittleBitMoreSqueezeThroughTheSystemMultiplier);
			}
			else
			{
				return new AvailableCapacity(relevantAvailableCapacity, relevantAvailableCapacity);
			}
		}
	}
}
