using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBMComponentReleaseGroupLink : IBusiness
	{
		ZGuid PK { get; }
		ZBool FO_IsConstrainedMode { get; set; }
		ZDateTime FO_AutoAssignTasksAge { get; set; }
		ZString FO_ReleaseGateMode { get; set; }
		ZBool FO_ReleaseWhenInProgress { get; set; }
		ZByte FO_ResetTaskPenetrationThrottleFactor { get; set; }
		ZBool FO_ResetTaskPenetrationInsideGroup { get; set; }
		ZBool FO_ResetTaskPenetrationOutsideGroup { get; set; }
		IBMComponent Component { get; }
		IGlbGroup ReleaseGroup { get; }
	}
}
