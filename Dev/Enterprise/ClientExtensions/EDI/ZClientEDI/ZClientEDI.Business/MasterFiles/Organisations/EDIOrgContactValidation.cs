using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgContactValidation : OrgContactValidation
	{
		public EDIOrgContactValidation(EDIOrgContact parent)
			: base(parent)
		{
		}

		new EDIOrgContact Parent => (EDIOrgContact)base.Parent;

		protected override void CheckInactiveAddressWhenContactIsActive(ZPropertyInfo propertyInfo)
		{
			if
			(
				Parent.IsInDatabase && !Parent.OC_OA_OrgAddressInfo.HasChanges
				&& Parent.OrgAddress != null && !Parent.OrgAddress.OA_IsActive && Parent.OC_IsActive
			)
			{
				propertyInfo.AddWarning(Res.GetString("8860501f-8cc1-47a4-ab5e-520c27079ab4", "Only Active address can be set as Contact's address"));
			}
			else
			{
				base.CheckInactiveAddressWhenContactIsActive(propertyInfo);
			}
		}
	}
}
