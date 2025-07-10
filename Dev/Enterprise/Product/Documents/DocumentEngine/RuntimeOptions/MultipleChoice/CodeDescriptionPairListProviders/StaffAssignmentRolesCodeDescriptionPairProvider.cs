using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class StaffAssignmentRolesCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return DataRegistry.Instance.OrgStaffMemberAssignmentRoles;
		}
	}
}
