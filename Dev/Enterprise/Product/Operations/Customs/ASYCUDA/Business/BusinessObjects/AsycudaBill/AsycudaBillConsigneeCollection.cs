using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaBillConsigneeCollection : ConsigneeCollection
	{
		readonly AsycudaBill bill;

		public AsycudaBillConsigneeCollection(BusinessObjectFactory factory, AsycudaBill bill) : base(factory)
		{
			this.bill = bill;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			IAddressDetails addressDetails = bill?.ConsigneeABLAddress;

			if (!addressDetails.AreEmpty())
			{
				addressDetails.CopyTo(child as OrgHeader);
			}
		}
	}
}
