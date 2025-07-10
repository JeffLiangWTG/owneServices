using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business;

public class EDIOrgStaffAssignments : OrgStaffAssignments
{
	public EDIOrgStaffAssignments(BusinessObjectFactory factory, DataRow dataRow)
		: base(factory, dataRow)
	{
	}

	#region Product

	[List("Lookups.ProductList")]
	public override ZString O8_Product
	{
		get => base.O8_Product;
		set => base.O8_Product = value;
	}

	public override ZString ProductDescription => Lookups.ProductList.GetDescriptionFromCode(O8_Product);

	public ZPropertyInfo ProductDescriptionInfo => GetZPropertyInfo(nameof(ProductDescription));

	#endregion

	#region Role

	[List("Lookups.StaffRoles")]
	public override ZString O8_Role
	{
		get => base.O8_Role;
		set => base.O8_Role = value;
	}

	public override ZString RoleDescription => Lookups.StaffRoles.GetDescriptionFromCode(O8_Role);

	#endregion

	public new EDIOrgHeader Header => (EDIOrgHeader)base.Header;

	protected override OrgStaffAssignmentsValidation GetNewValidation() => new EDIOrgStaffAssignmentsValidation(this);

	public new EDIOrgStaffAssignmentsLookups Lookups => (EDIOrgStaffAssignmentsLookups)base.Lookups;

	protected override OrgStaffAssignmentsLookups GetNewLookups() => new EDIOrgStaffAssignmentsLookups(this);
}
