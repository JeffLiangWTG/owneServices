using CargoWise.EntityFramework;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class AsycudaBillCollection : ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>
	{
		public AsycudaBillCollection(AsycudaManifestHeader master)
			: base(master)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var bill = (AsycudaBill)child;

			if (Master != null)
			{
				var eDep = Master.AMA_E_DEP;
				if (!eDep.IsEmpty)
				{
					bill.ABL_E_DEP = eDep;
				}
			}

			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;

			bill.ABL_RX_NKFreightValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bill.ABL_RX_NKTransportValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bill.ABL_RX_NKInsuranceValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bill.DiscountValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bill.OtherChargesValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bill.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
		}
	}
}
