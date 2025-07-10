using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocOrgStaffAssignments : DocumentWrapper
	{
		DocOrgStaffAssignments(OrgStaffAssignments orgStaffAssignments, BusinessObjectFactory factoryForWrapper)
			: base(orgStaffAssignments, factoryForWrapper)
		{
		}

		public static DocOrgStaffAssignments New(OrgStaffAssignments orgStaffAssignments, BusinessObjectFactory factoryForWrapper)
		{
			if (orgStaffAssignments == null)
			{
				return null;
			}
			else
			{
				return new DocOrgStaffAssignments(orgStaffAssignments, factoryForWrapper);
			}
		}

		OrgStaffAssignments OrgStaffAssignments
		{
			get { return (OrgStaffAssignments)WrappedObject; }
		}

		public ZString Email
		{
			get
			{
				return OrgStaffAssignments.Email;
			}
		}

		public ZString Name
		{
			get
			{
				return OrgStaffAssignments.Name;
			}
		}
		public ZString O8_Department
		{
			get
			{
				return OrgStaffAssignments.O8_Department;
			}
		}

		public ZString O8_Role
		{
			get
			{
				return OrgStaffAssignments.O8_Role;
			}
		}

		public ZGuid O8_GC
		{
			get
			{
				return OrgStaffAssignments.O8_GC;
			}
		}

		public ZString ResponsiblePersonLoginName
		{
			get
			{
				return OrgStaffAssignments.ResponsiblePersonLoginName;
			}
		}

		public ZString ResponsiblePersonName
		{
			get
			{
				return OrgStaffAssignments.ResponsiblePersonName;
			}
		}

		public ZString RoleDescription
		{
			get
			{
				return OrgStaffAssignments.RoleDescription;
			}
		}
	}
}
