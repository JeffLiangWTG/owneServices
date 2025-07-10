namespace Enterprise.Customs.EU.H7.Business
{
	public class CusGoodsLocationAddressValidation : EU.Business.CusGoodsLocationAddressValidation
	{
		public CusGoodsLocationAddressValidation(EU.Business.CusGoodsLocationAddress parent) : base(parent)
		{
		}

		protected new CusGoodsLocationAddress Parent => (CusGoodsLocationAddress)base.Parent;

		protected CusGoodsLocation CusGoodsLocation => Parent.GoodsLocation;

		protected AsycudaBill Bill => CusGoodsLocation.Parent;
	}
}
