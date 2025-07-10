using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public class EDIStaffAssignmentsModule : StaffAssignmentsModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EDIStaffAssignmentsFilterBusinessObject();
		}
	}
}
