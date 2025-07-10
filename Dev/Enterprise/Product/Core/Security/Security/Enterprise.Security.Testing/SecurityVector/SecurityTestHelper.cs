using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security.Testing
{
	sealed class SecurityTestHelper
	{
		public SecurityTestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public GlbSecurity CreateSecurity(ISecurityCheckpoint checkpoint, BusinessObject target, GlbCompany company, GlbBranch branch, GlbDepartment department, bool isAllowed)
		{
			var security = factory.New<GlbSecurity>();

			if (target is GlbStaff)
			{
				security.GU_GS = target.PK;
			}
			else if (target is GlbGroup)
			{
				security.GU_GG = target.PK;
			}

			security.GU_SecurityRight = checkpoint.Code;
			security.GU_ItemGUID = checkpoint.ItemGuid;
			if (company != null)
			{
				security.GU_GC = company.PK;
			}

			if (branch != null)
			{
				security.GU_GB = branch.PK;
			}

			if (department != null)
			{
				security.GU_GE = department.PK;
			}

			security.GU_SecurityItemIsAllowed = isAllowed;

			return security;
		}

		readonly BusinessObjectFactory factory;
	}
}
