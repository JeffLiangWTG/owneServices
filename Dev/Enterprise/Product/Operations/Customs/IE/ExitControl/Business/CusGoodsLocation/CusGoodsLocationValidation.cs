using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class CusGoodsLocationValidation : Customs.Business.CusGoodsLocationValidation
	{
		public CusGoodsLocationValidation(CusGoodsLocation parent)
			: base(parent)
		{
		}

		protected new CusGoodsLocation Parent => (CusGoodsLocation)base.Parent;

		protected override void CheckCGL_Type()
		{
			base.CheckCGL_Type();

			if (Parent.IsPresentation)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CGL_TypeInfo);
			}
		}

		protected override void CheckCGL_AdditionalIdentifier()
		{
			if (Parent.IsPresentation)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CGL_AdditionalIdentifierInfo);
			}
		}
	}
}
