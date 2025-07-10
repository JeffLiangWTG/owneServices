using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public static class RefCusTradeGroupLoader
{
	public static CusRefTradeGroupView GetTradeGroup(BusinessObjectFactory factory, ZString tradeGroupCode, ZDateTime dateOfValuation)
	{
		return factory.GetCachedValue($"CHRefCusTradeGroup_{tradeGroupCode}_{dateOfValuation}",
			() => new CusRefTradeGroupView.Loader(factory).Load(Core.Constants.CountryCodes.Switzerland, tradeGroupCode, dateOfValuation));
	}

	public static bool IsAnyCountryPartOfTradeGroup(BusinessObjectFactory factory, ZString tradeGroupCode, IEnumerable<ZString> countryCodes, ZDateTime dateOfValuation)
	{
		var result = false;

		if (!tradeGroupCode.IsEmpty && !countryCodes.IsNullOrEmpty())
		{
			var tradeGroup = GetTradeGroup(factory, tradeGroupCode, dateOfValuation);
			if (tradeGroup != null)
			{
				result = tradeGroup.TradeGroupCountries.Any(x => countryCodes.Contains(x.ZZB_RN_NKTradeGroupCountryCode));
			}
		}

		return result;
	}

	public static bool IsAnyCountryPartOfDevelopingCountries(BusinessObjectFactory factory, IEnumerable<ZString> countryCodes, ZDateTime dateOfValuation)
	{
		return IsAnyCountryPartOfTradeGroup(factory, UniversalReferenceConstants.TradeGroup.DevelopingCountries, countryCodes, dateOfValuation);
	}

	public static bool IsCountryPartOfDevelopingCountries(BusinessObjectFactory factory, RefCountry country, ZDateTime dateOfValuation)
	{
		return country != null && IsAnyCountryPartOfTradeGroup(factory, UniversalReferenceConstants.TradeGroup.DevelopingCountries, new[] { country.Code }, dateOfValuation);
	}
}
