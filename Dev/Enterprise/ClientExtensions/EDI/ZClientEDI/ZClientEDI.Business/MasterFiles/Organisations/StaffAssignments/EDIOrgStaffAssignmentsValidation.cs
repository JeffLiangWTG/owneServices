using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business;

public class EDIOrgStaffAssignmentsValidation : OrgStaffAssignmentsValidation
{
	protected readonly new EDIOrgStaffAssignments Parent;

	public EDIOrgStaffAssignmentsValidation(EDIOrgStaffAssignments parent)
		: base(parent)
	{
		Parent = parent;
	}

	protected override void CheckO8_Product()
	{
		base.CheckO8_Product();
		ListValidation.ErrorIfInvalidCode(Parent.O8_ProductInfo);
	}
}
