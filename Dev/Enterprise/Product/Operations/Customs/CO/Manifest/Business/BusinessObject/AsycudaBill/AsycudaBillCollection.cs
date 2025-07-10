using CargoWise.EntityFramework;

namespace Enterprise.Customs.CO.Manifest.Business
{
	public class AsycudaBillCollection : ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>
	{
		public AsycudaBillCollection(AsycudaManifestHeader master)
			: base(master)
		{ }

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var bill = (AsycudaBill)child;
			bill.ABL_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bill.ABL_RX_NKFreightValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
		}
	}
}
