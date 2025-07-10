using Enterprise.Integration;

namespace Enterprise.BufferManagement.Integration
{
	public interface IProcessTaskBMSValidation
	{
		void CheckP9_EstDuration(IProcessTask task);
		void CheckP9_GS_NKAssignedStaffMember(IProcessTask task);
		void CheckP9_FH_ProcessHeader(IProcessTask task);
	}
}
