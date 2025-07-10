using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class CusGoodsLocationAddressValidation : EU.NCTS.Business.CusGoodsLocationAddressValidation
	{
		public CusGoodsLocationAddressValidation(CusGoodsLocationAddress parent)
			: base(parent)
		{
		}

		new CusGoodsLocationAddress Parent => (CusGoodsLocationAddress)base.Parent;

		protected override void CheckE2_Phone()
		{
			base.CheckE2_Phone();

			var parent = Parent;
			if (!parent.E2_Contact.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.E2_PhoneInfo);
			}
		}

		protected override void CheckE2_GovRegNum()
		{
			if (Parent.GoodsLocation is CusGoodsLocation location && location.ParentIsArrivalMovementHeader)
			{
				base.CheckE2_GovRegNum();
			}
		}
	}
}
