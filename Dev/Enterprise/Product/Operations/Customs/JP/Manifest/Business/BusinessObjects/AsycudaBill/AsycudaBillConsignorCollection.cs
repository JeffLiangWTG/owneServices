using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class AsycudaBillConsignorCollection : ASYCUDA.Business.AsycudaBillConsignorCollection
	{
		public AsycudaBillConsignorCollection(BusinessObjectFactory factory, AsycudaBill bill) : base(factory, bill)
		{
			this.bill = bill;
		}

		readonly AsycudaBill bill;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (bill.ShouldShowShipperRegNoType && (!bill.ABL_ShipperRegNoType.IsEmpty || !bill.ABL_ShipperRegNo.IsEmpty))
			{
				var header = child as OrgHeader;
				var mainAddress = header?.MainAddress;
				mainAddress?.CustomsCodes.AddNew(bill.ABL_ShipperRegNoType, bill.ABL_ShipperRegNo, Core.Constants.CountryCodes.Japan);
			}
		}
	}
}
