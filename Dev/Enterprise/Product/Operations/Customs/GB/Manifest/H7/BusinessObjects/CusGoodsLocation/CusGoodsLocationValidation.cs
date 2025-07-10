using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.H7.Business
{
	public class CusGoodsLocationValidation : EU.H7.Business.CusGoodsLocationValidation
	{
		public CusGoodsLocationValidation(CusGoodsLocation parent) : base(parent)
		{
		}
		protected override void CheckCGL_Type()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CGL_TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CGL_TypeInfo);
		}

		protected override void CheckCGL_Qualifier()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CGL_QualifierInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CGL_QualifierInfo);
		}

		protected override void CheckCGL_AdditionalIdentifier()
		{
			EU.H7.Business.CusGoodsLocationValidationHelper.ValidateWithQualifier(Parent.CGL_Qualifier, Parent.CGL_AdditionalIdentifierInfo, UnlocodeCaption);

			if (Parent.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.UnLocode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.UnlocodeInfo);
			}
		}
	}
}
