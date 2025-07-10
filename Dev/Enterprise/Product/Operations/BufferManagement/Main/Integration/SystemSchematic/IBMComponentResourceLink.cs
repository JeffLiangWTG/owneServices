using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBMComponentResourceLink : IBusiness
	{
		ZGuid FD_FC_Component { get; set; }
		ZString FD_GS_NKResource { get; set; }
		ZByte FD_CapacityLimitPercent { get; set; }
		ZBool FD_IsCapacityConstrained { get; set; }
		ZString FD_GS_NKDesignatedAsCapacityConstrainedBy { get; set; }
		ZBool FD_IsPersistentlyOverloaded { get; }
		ZDateTime FD_CapacityConstraintDetectedUtc { get; }
		ZShort FD_StaffStartableWorkflowLimit { get; set; }

		ZString SystemName { get; }
		ZString TimeConsideredCCR { get; }
		ZDateTime CapacityConstraintDetectedLocal { get; }
		ZString CapacityReservationDetails { get; }
		ZString MarkedAsCapacityConstrainedByFullName { get; }
	}
}
