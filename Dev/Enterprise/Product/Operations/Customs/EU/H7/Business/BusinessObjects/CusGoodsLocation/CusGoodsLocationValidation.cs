using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.H7.Business
{
	public class CusGoodsLocationValidation : EU.Business.CusGoodsLocationValidation
	{
		public CusGoodsLocationValidation(CusGoodsLocation parent) : base(parent)
		{
		}
		protected AsycudaBill bill => Parent.Parent as AsycudaBill;

		protected virtual ZString ShipmentType => bill.ABL_ShipmentType;

		protected virtual IMultilingualString InvalidCodeErrorMessage => null;

		protected override void CheckCGL_Type()
		{
			ValidateWithAdditionalDeclarationType(Parent.CGL_TypeInfo);
		}

		protected override void CheckCGL_Qualifier()
		{
			ValidateWithAdditionalDeclarationType(Parent.CGL_QualifierInfo);
		}

		protected override void CheckCGL_AdditionalIdentifier()
		{
			CusGoodsLocationValidationHelper.ValidateWithQualifierAndAdditionalDeclarationType(ShipmentType, Parent.CGL_Qualifier, Parent.CGL_AdditionalIdentifierInfo, UnlocodeCaption);
			if (ShipmentType == EU.Business.EntrySubStyleList.Codes.NormalDeclaration
				&& Parent.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.UnLocode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.UnlocodeInfo);
			}
		}

		void ValidateWithAdditionalDeclarationType(ZPropertyInfo info)
		{
			if (ShipmentType == EntrySubStyleList.Codes.NormalDeclaration)
			{
				MandatoryValidation.MessageErrorIfNotEntered(info);
				if (InvalidCodeErrorMessage == null)
				{
					ListValidation.ErrorIfInvalidCode(info);
				}
				else
				{
					ListValidation.ErrorIfInvalidCode(InvalidCodeErrorMessage, info);
				}
			}

			if (ShipmentType == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA)
			{
				ListValidation.ErrorIfInvalidCode(ResString.GetMultilingualString("014f992f-48e1-4a62-9109-6f72b85ca27c", "The value entered is invalid and should be removed for additional declaration type 'D'."), info);
				if (info.Notifications.Count() == 0)
				{
					CusGoodsLocationValidationHelper.ValidateWhenAdditionalDeclarationTypeIsD(info);
				}
			}
		}

		protected static string UnlocodeCaption => Res.GetString("93fd1520-dee8-4237-906b-b8089d91d702", "UNLOCODE");
	}
}
