using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.H7.Business
{
	public static class CusGoodsLocationValidationHelper
	{
		public static void ValidateWithQualifierAndAdditionalDeclarationType(ZString shipmentType, ZString qualifier, ZPropertyInfo info, string descrption = null)
		{
			if (shipmentType == EntrySubStyleList.Codes.NormalDeclaration)
			{
				ValidateWithQualifier(qualifier, info, descrption);
			}

			if (shipmentType == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA)
			{
				ValidateWhenAdditionalDeclarationTypeIsD(info, descrption);
			}
		}

		public static void ValidateWithQualifier(ZString qualifier, ZPropertyInfo info, string description = null)
		{
			if (qualifier == CusGoodsLocationQualifierList.Codes.UnLocode && info.Value.IsEmpty)
			{
				if (description == null)
				{
					info.AddMessageError(Res.GetString("86ff2824-20f9-445e-8e52-19add0c263d6", "Required when qualifier is 'U'."));
				}
				else
				{
					info.AddMessageError(Res.GetString("0c747229-b55b-458d-b762-43b862851f2d", "{0} required when qualifier is 'U'.", description));
				}
			}
		}

		public static void ValidateWhenAdditionalDeclarationTypeIsD(ZPropertyInfo info, string description = null)
		{
			if (!info.Value.IsEmpty)
			{
				if (description == null)
				{
					info.AddMessageError(Res.GetString("5bfdf2b0-fae1-4144-8c08-1128cab9924f", "Shall not be populated for additional declaration type 'D'."));
				}
				else
				{
					info.AddMessageError(Res.GetString("676b6d2e-80c4-429b-9738-65818eeb7389", "{0} shall not be populated for additional declaration type 'D'.", description));
				}
			}
		}

		public static void ValidateInnerGoodsLocation(ICusGoodsLocationProvider locationProvider)
			=> EU.Business.CusGoodsLocationValidationHelper.ValidateInnerGoodsLocation(locationProvider);
	}
}
