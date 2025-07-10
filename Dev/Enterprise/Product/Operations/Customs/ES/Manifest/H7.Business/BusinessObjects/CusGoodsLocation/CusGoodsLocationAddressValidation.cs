namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class CusGoodsLocationAddressValidation : EU.H7.Business.CusGoodsLocationAddressValidation
	{
		public CusGoodsLocationAddressValidation(EU.H7.Business.CusGoodsLocationAddress parent) : base(parent)
		{
		}

		protected new CusGoodsLocationAddress Parent => (CusGoodsLocationAddress)base.Parent;

		protected new CusGoodsLocation CusGoodsLocation => Parent.GoodsLocation;

		protected new AsycudaBill Bill => CusGoodsLocation.Parent as AsycudaBill;

		protected AsycudaManifestHeader ManifestHeader => CusGoodsLocation.ManifestHeaderParent as AsycudaManifestHeader;

		protected override bool ApplyC0065Rule => false;
	}
}
