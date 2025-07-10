using System;
using Enterprise.Core.Environment;

namespace Enterprise.Security.Testing
{
	public sealed class SecurityForTest : SecurityCore
	{
		public SecurityForTest(IZGlbSecurityCollection securityCollection, object staffMember, Guid branchPK, Guid departmentPK, Guid companyPK)
			: base(securityCollection, staffMember, branchPK, departmentPK, companyPK)
		{
		}

		public IZSecurity ZSecurityInstance
		{
			get { return SecurityInstance; }
		}
	}
}
