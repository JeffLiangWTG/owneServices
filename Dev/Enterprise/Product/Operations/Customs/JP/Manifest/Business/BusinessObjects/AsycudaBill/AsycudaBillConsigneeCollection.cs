using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class AsycudaBillConsigneeCollection : ASYCUDA.Business.AsycudaBillConsigneeCollection
	{
		public AsycudaBillConsigneeCollection(BusinessObjectFactory factory, AsycudaBill bill) : base(factory, bill)
		{
			this.bill = bill;
		}

		readonly AsycudaBill bill;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (bill.ShouldShowConsigneeRegNoType && (!bill.ABL_ConsigneeRegNoType.IsEmpty || !bill.ABL_ConsigneeRegNo.IsEmpty))
			{
				var header = child as OrgHeader;
				var mainAddress = header?.MainAddress;
				mainAddress?.CustomsCodes.AddNew(bill.ABL_ConsigneeRegNoType, bill.ABL_ConsigneeRegNo, Core.Constants.CountryCodes.Japan);
			}
		}
	}
}
