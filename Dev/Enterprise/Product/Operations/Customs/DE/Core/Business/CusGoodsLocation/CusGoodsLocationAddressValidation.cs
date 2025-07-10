namespace Enterprise.Customs.DE.Business
{
	public class CusGoodsLocationAddressValidation : EU.Business.CusGoodsLocationAddressValidation
	{
		public CusGoodsLocationAddressValidation(CusGoodsLocationAddress parent)
			: base(parent)
		{
		}

		protected override bool ApplyC0065Rule => false;
	}
}
