using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using Enterprise.Core.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Security
{
	class ZSecurityFactory : IZSecurityFactory
	{
		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		public IZSecurity NewSecurityInstance(IZGlbSecurityCollection securityData, object staffOrGroup, Guid branchPK, Guid departmentPK, Guid companyPK, bool reloadStaffOrGroupObject = true)
		{
			GlbStaff staff = null;
			GlbGroup group = null;
			if (staffOrGroup is GlbStaff)
			{
				staff = (GlbStaff)staffOrGroup;
			}
			else if (staffOrGroup is GlbGroup)
			{
				group = (GlbGroup)staffOrGroup;
			}
			else if (staffOrGroup != null)
			{
				Guid staffOrGroupGuid = (Guid)staffOrGroup;
				BusinessObjectFactory factory = new BusinessObjectFactory();
				factory.NameForDebugging = "Staff Security";
				staff = (GlbStaff)factory.Load(typeof(GlbStaff), staffOrGroupGuid);
				if (staff == null)
				{
					group = (GlbGroup)factory.Load(typeof(GlbGroup), staffOrGroupGuid);
				}
			}

			GlbSecurityCollection securityDataAsGlbSecurityCollection = (GlbSecurityCollection)securityData;

			return group != null ?
				new ZSecurity(securityDataAsGlbSecurityCollection, group, branchPK, departmentPK, companyPK, reloadStaffOrGroupObject) :
				new ZSecurity(securityDataAsGlbSecurityCollection, staff, branchPK, departmentPK, companyPK, reloadStaffOrGroupObject);
		}
	}
}
