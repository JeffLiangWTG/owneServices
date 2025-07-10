using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	internal class AsycudaBillSellerCollection : OrgHeaderCollection
	{
		readonly AsycudaBill bill;

		public AsycudaBillSellerCollection(BusinessObjectFactory factory, AsycudaBill bill) : base(factory)
		{
			this.bill = bill;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			IAddressDetails addressDetails = bill?.SellerABLAddress;

			if (!addressDetails.AreEmpty())
			{
				addressDetails.CopyTo(child as OrgHeader);
			}
		}
	}
}
