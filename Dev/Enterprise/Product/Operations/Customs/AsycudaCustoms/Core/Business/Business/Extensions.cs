using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.AsycudaCustoms.Business;

public static class Extensions
{
	public static bool IsBLNSCountry(this ZString countryCode, BusinessObjectFactory factory)
	{
		var blnsCountries = factory.GetCachedValue("AsycudaCustoms.BLNSCountries", () =>
		{
			return new ZString[]
			{
				Core.Constants.CountryCodes.Botswana,
				Core.Constants.CountryCodes.Lesotho,
				Core.Constants.CountryCodes.Namibia,
				Core.Constants.CountryCodes.Swaziland
			}.ToList();
		});
		return blnsCountries.Contains(countryCode);
	}

	public static bool IsRiskManagementEnabled(ZString countryCode, BusinessObjectFactory factory)
	{
		var today = ZDateTime.Today;
		return factory.GetCachedValue($"AsycudaCustoms_IsRiskManagementEnabled_{countryCode}_{today}", // Cache key
			() =>
			{
				return ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(
					Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, countryCode,
					today);
			});
	}
}
