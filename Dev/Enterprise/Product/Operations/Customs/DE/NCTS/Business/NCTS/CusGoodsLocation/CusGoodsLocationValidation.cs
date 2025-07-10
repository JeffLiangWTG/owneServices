using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class CusGoodsLocationValidation : EU.NCTS.Business.CusGoodsLocationValidation
	{
		public CusGoodsLocationValidation(EU.NCTS.Business.CusGoodsLocation parent) : base(parent)
		{
		}

		protected override void CheckCGL_Type()
		{
		}

		new CusGoodsLocation Parent => (CusGoodsLocation)base.Parent;

		protected override void CheckCGL_AdditionalIdentifier()
		{
			base.CheckCGL_AdditionalIdentifier();

			if (Parent.ParentIsMovementHeader)
			{
				ListValidation.WarnIfInvalidCode(Parent.CGL_AdditionalIdentifierInfo, ResString.GetMultilingualString("C0CB4064-6E08-433D-AE7B-7C6E0EDF33D8", "The captured Additional Identifier is not a valid Location Code of the selected Authorization."));
			}
		}
	}
}
