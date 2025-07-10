using Enterprise.MasterFiles.GUI.Test.Organisation.StaffAssignments;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.GUI.Testing
{
	[TestedType(typeof(EDIStaffAssignmentsFilterBusinessObject))]
	public class EDIStaffAssignmentsFilterBusinessObjectTest : StaffAssignmentsFilterBusinessObjectTest
	{
		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EDIStaffAssignmentsFilterBusinessObject();
		}

		#endregion
	}
}
