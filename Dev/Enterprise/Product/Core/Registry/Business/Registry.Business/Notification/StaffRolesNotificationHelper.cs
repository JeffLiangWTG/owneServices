using System.Collections;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public static class StaffRolesNotificationHelper
	{
		public static CodeDescriptionBoolDisallowNewCollection GetRoles()
		{
			return new CodeDescriptionBoolDisallowNewCollection(DataRegistry.Instance.OrgStaffMemberAssignmentRoles);
		}

		public static void SetAllBoolsTo(CodeDescriptionBoolDisallowNewCollection roles, bool newValue)
		{
			foreach (CodeDescriptionBool element in roles)
			{
				element.Bool = newValue;
			}
		}

		public static void SetBoolTo(ICodeDescriptionBoolList roles, bool newValue, params string[] codes)
		{
			foreach (CodeDescriptionBool element in roles)
			{
				if (((IList)codes).Contains(element.Code.ToString()))
				{
					element.Bool = newValue;
				}
			}
		}
	}
}
