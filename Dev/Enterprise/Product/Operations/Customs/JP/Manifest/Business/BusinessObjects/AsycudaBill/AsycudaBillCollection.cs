using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class AsycudaBillCollection : ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>
	{
		public AsycudaBillCollection(AsycudaManifestHeader master) : base(master)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var bill = (AsycudaBill)child;
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			using (bill.SuspendSettingHasChanges())
			{
				if (bill.ShouldShowConsigneeRegNoType)
				{
					bill.ABL_ConsigneeRegNoType = OrgCusCode.JapanCodeTypes.LPC;
				}

				if (bill.ShouldShowShipperRegNoType)
				{
					bill.ABL_ShipperRegNoType = bill.IsNVC ? OrgCusCode.JapanCodeTypes.LPC : OrgCusCode.JapanCodeTypes.FSB;
				}

				if (bill.ShouldShowNotifyPartyRegNoType)
				{
					bill.ABL_NotifyPartyRegNoType = OrgCusCode.JapanCodeTypes.LPC;
				}
			}
		}
	}
}
