using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Business
{
	public class UPEOrgStaffAssignmentsLookupsImplementer : OrgStaffAssignmentsLookupsImplementer
	{
		public UPEOrgStaffAssignmentsLookupsImplementer(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ReadOnlyCodeDescriptionPairList GetNewStaffRoles()
		{
			return new UPEStaffRoles();
		}
	}
}
