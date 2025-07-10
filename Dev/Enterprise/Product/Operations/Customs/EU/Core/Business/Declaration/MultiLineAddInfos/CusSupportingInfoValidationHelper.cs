using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	sealed class CusSupportingInfoValidationHelper
	{
		public static void ValidateRuleC0612(string code, ZPropertyInfo propertyInfo)
		{
			if (IsCodeInvalidForRuleC0612(code))
			{
				propertyInfo.AddMessageError(
					Res.GetString(
						"986CF259-0EFD-458D-A5B3-6EC05BD1B8EF",
						"[C0612] A CERTEX certificate of type {0} can only be declared on the invoice line level.",
						code));
			}
		}

		static bool IsCodeInvalidForRuleC0612(string code)
		{
			switch (code)
			{
				case UniversalReferenceConstants.CusSupportingInfoTypes.C057:
				case UniversalReferenceConstants.CusSupportingInfoTypes.C079:
				case UniversalReferenceConstants.CusSupportingInfoTypes.C082:
				case UniversalReferenceConstants.CusSupportingInfoTypes.C085:
				case UniversalReferenceConstants.CusSupportingInfoTypes.C640:
				case UniversalReferenceConstants.CusSupportingInfoTypes.C644:
				case UniversalReferenceConstants.CusSupportingInfoTypes.C678:
				case UniversalReferenceConstants.CusSupportingInfoTypes.E013:
				case UniversalReferenceConstants.CusSupportingInfoTypes.L100:
				case UniversalReferenceConstants.CusSupportingInfoTypes.N853:
				case UniversalReferenceConstants.CusSupportingInfoTypes.Y120:
				case UniversalReferenceConstants.CusSupportingInfoTypes.Y121:
				case UniversalReferenceConstants.CusSupportingInfoTypes.Y123:
				case UniversalReferenceConstants.CusSupportingInfoTypes.Y124:
				case UniversalReferenceConstants.CusSupportingInfoTypes.Y125:
				case UniversalReferenceConstants.CusSupportingInfoTypes.Y951:
				case UniversalReferenceConstants.CusSupportingInfoTypes.Y986:
					{
						return true;
					}
				default:
					return false;
			}
		}
	}
}
