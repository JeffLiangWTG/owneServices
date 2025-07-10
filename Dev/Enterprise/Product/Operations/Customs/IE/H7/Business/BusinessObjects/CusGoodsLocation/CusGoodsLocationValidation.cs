using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.Business;

namespace Enterprise.Customs.IE.H7.Business
{
	public class CusGoodsLocationValidation : EU.H7.Business.CusGoodsLocationValidation
	{
		public CusGoodsLocationValidation(EU.H7.Business.CusGoodsLocation parent) : base(parent)
		{
		}

		protected new CusGoodsLocation Parent => (CusGoodsLocation)base.Parent;

		protected override IMultilingualString InvalidCodeErrorMessage => bill.ValidationConfiguration.ValidationMessage.InvalidValueRuleMessage;

		protected override void CheckCGL_AdditionalIdentifier()
		{
		}

		protected override void CheckCGL_CustomsOffice()
		{
			CusGoodsLocationValidationHelper.ValidateWithQualifierAndAdditionalDeclarationType(bill.ABL_ShipmentType, Parent.CGL_Qualifier, Parent.CGL_CustomsOfficeInfo, Res.GetString("536ac41c-238e-487c-bf66-119538da5716", "UNLOCODE"));
			if (bill.ABL_ShipmentType == EU.Business.EntrySubStyleList.Codes.NormalDeclaration
				&& Parent.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.UnLocode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.UnlocodeInfo, Parent.Parent.ValidationConfiguration.ValidationMessage.InvalidValueRuleMessage);
			}
		}
	}
}
