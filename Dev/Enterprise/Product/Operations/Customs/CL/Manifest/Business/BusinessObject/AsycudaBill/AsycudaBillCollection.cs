using CargoWise.EntityFramework;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class AsycudaBillCollection : ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>
	{
		public AsycudaBillCollection(AsycudaManifestHeader master) : base(master) { }

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var bill = (AsycudaBill)child;
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
		}
	}
}
