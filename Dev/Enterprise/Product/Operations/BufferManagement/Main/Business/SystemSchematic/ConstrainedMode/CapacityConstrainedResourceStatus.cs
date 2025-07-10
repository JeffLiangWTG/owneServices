using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public class CapacityConstrainedResourceStatus
	{
		internal CapacityConstrainedResourceStatus(string staffCode, bool isDesignatedCCR, bool isPersistentlyOverloaded, ZDateTime capacityConstraintDetectedUtc)
		{
			StaffCode = staffCode;
			IsDesignatedCCR = isDesignatedCCR;
			IsPersistentlyOverloaded = isPersistentlyOverloaded;
			CapacityConstraintDetectedUtc = capacityConstraintDetectedUtc;
		}

		public string StaffCode { get; }
		public bool IsDesignatedCCR { get; }
		public bool IsPersistentlyOverloaded { get; }
		public ZDateTime CapacityConstraintDetectedUtc { get; }

		public ZString TimeConsideredCCR => CapacityConstraintDetectedUtc.ToFriendlyTimeOverAgoString();
	}
}
