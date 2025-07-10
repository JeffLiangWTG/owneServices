using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public class MissingCapabilityCapacityReservationReportLine
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		internal MissingCapabilityCapacityReservationReportLine(GlbCapability capability, decimal capacityHoursRequired)
		{
			Capability = capability;
			CapacityHoursRequired = capacityHoursRequired;
		}

		public GlbCapability Capability { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public decimal CapacityHoursRequired { get; }

		public ICollection<TaskLogDetails> CapabilityTasks { get; } = new List<TaskLogDetails>();
	}
}
