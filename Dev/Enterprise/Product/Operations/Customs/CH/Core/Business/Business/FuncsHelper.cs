using CargoWise.Types;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.CH.Business;

public static class FuncsHelper
{
	public static bool IsCHNE015V3Active => IsFuncsActive(FunctionalityTypes.CHNE015V3);

	public static bool IsCHNT015V4Active => IsFuncsActive(FunctionalityTypes.CHNT015V4);

	public static bool IsCHNT515V4Active => IsFuncsActive(FunctionalityTypes.CHNT515V4);

	public static bool IsCHNT044V4Active => IsFuncsActive(FunctionalityTypes.CHNT044V4);

	static bool IsFuncsActive(string code)
	{
		return ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(code, Core.Constants.CountryCodes.Switzerland, ZDateTime.Now);
	}
}
