namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CusGoodsLocationAddressValidation : EU.NCTS.Business.CusGoodsLocationAddressValidation
	{
		public CusGoodsLocationAddressValidation(CusGoodsLocationAddress parent) : base(parent)
		{
		}

		protected override bool ApplyC0065Rule => false;
	}
}
