using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class AsycudaBillNotifyPartyCollection : ASYCUDA.Business.AsycudaBillNotifyPartyCollection
	{
		public AsycudaBillNotifyPartyCollection(BusinessObjectFactory factory, AsycudaBill bill) : base(factory, bill)
		{
			this.bill = bill;
		}

		readonly AsycudaBill bill;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (bill.ShouldShowNotifyPartyRegNoType && (!bill.ABL_NotifyPartyRegNoType.IsEmpty || !bill.ABL_NotifyPartyRegNo.IsEmpty))
			{
				var header = child as OrgHeader;
				var mainAddress = header?.MainAddress;
				mainAddress?.CustomsCodes.AddNew(bill.ABL_NotifyPartyRegNoType, bill.ABL_NotifyPartyRegNo, Core.Constants.CountryCodes.Japan);
			}
		}
	}
}
