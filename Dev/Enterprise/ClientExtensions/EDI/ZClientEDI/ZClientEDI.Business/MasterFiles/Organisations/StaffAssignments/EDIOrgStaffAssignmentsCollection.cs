using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business;

public class EDIOrgStaffAssignmentsCollection : OrgStaffAssignmentsCollection
{
	public EDIOrgStaffAssignmentsCollection(EDIOrgHeader organisation)
		: base(organisation)
	{
	}

	public EDIOrgStaffAssignmentsCollection(EDIOrgHeader organisation, GlbCompany company)
		: base(organisation, company)
	{
	}

	#region Implementation

	public new EDIOrgStaffAssignments this[int index]
	{
		get { return (EDIOrgStaffAssignments)Elements[index]; }
	}

	public virtual new EDIOrgStaffAssignments AddNew()
	{
		return (EDIOrgStaffAssignments)base.AddNew();
	}

	#endregion
}
