using Enterprise.Core.Environment;

namespace Enterprise.Security.Testing
{
	public sealed class CRMSecurityForTest : CRMSecurity
	{
		public CRMSecurityForTest(IZSecurity securityInstance) : base(securityInstance)
		{
		}

		public void CreateViewCheckpointForTest(SecurityCheckpoint parent) => CreateViewCheckpoint(parent);
		public void CreateEditCheckpointsForTest(SecurityCheckpoint parent) => CreateEditCheckpoints(parent);
		public void CreateOSMGCheckpointForTest(SecurityCheckpoint viewParent) => CreateOSMGCheckpoint(viewParent);
		public void CreateTaskAssignmentCheckpointForTest(SecurityCheckpoint viewParent) => CreateTaskAssignmentCheckpoint(viewParent);
	}
}
