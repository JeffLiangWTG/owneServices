using CargoWise.EntityFramework;

namespace Enterprise.Customs.CN.Business
{
	public class CNOrgSupplierBuyerLinkAddInfoCollection : NonPersistentBusinessObjectCollection<CNOrgSupplierBuyerLinkAddInfo>
	{
		public CNOrgSupplierBuyerLinkAddInfoCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotSupportedException();
		}

		protected override bool AllowNewCore => false;
	}
}
