using System;

namespace Enterprise.Core.Environment
{
	public interface IZSecurityFactory
	{
		IZSecurity NewSecurityInstance(IZGlbSecurityCollection securityData, object staffOrGroup, Guid branchPK, Guid departmentPK, Guid companyPK, bool reloadStaffOrGroupObject = true);
	}
}
