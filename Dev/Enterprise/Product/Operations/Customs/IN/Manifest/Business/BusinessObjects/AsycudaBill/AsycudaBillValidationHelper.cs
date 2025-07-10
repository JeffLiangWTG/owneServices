using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Manifest.Business;

public static class AsycudaBillValidationHelper
{
	public static void CheckGoodsDescription(ZPropertyInfo propertyinfo)
	{
		MandatoryValidation.MessageErrorIfNotEntered(propertyinfo);

		const int MaxCharacterDisplayLimit = 30;
		if (propertyinfo.Value.ToString().Length > MaxCharacterDisplayLimit)
		{
			propertyinfo.AddWarning(Res.GetString("00A4B20F-1775-404E-980F-CA6780EA8602", "{0} is more than 30 Char. Only first 30 Characters will be used for CGM filing.", propertyinfo.HumanReadableName));
		}
	}
}
