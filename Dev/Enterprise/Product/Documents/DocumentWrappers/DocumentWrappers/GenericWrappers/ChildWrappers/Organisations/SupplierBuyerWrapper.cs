using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class SupplierBuyerLinkWrapper : GenericWrapper
	{
		public SupplierBuyerLinkWrapper(OrgSupplierBuyerLink orgSupplierBuyerLink, BusinessObjectFactory factory)
			: base(orgSupplierBuyerLink, factory)
		{
		}

		public static SupplierBuyerLinkWrapper New(OrgSupplierBuyerLink orgSupplierBuyerLink, BusinessObjectFactory factory)
		{
			return orgSupplierBuyerLink != null ? new SupplierBuyerLinkWrapper(orgSupplierBuyerLink, factory) : null;
		}

		OrgSupplierBuyerLink OrgSupplierBuyerLink
		{
			get { return (OrgSupplierBuyerLink)WrappedObject; }
		}

		#region VendorID

		public ZString VendorID
		{
			get { return OrgSupplierBuyerLink != null ? OrgSupplierBuyerLink.OL_VendorID : ZString.Empty; }
		}

		#endregion

	}
}
