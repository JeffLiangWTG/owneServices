using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public static class NctsLookupsHelper
{
	public static CodeDescriptionPairList TransportTypeOfIdList(BusinessObjectFactory factory) => factory.GetCachedValue("CH.NCTS.TransportTypeOfIdList", () =>
	{
		var list = new NctsTransportTypeOfIdList();
		list.RemoveCode(NctsTransportTypeOfIdList.Codes._11);
		list.RemoveCode(NctsTransportTypeOfIdList.Codes._20);
		list.RemoveCode(NctsTransportTypeOfIdList.Codes._31);
		return list;
	});

	public static ZZRefCusCodeListCombinedCollection NationalityList(BusinessObjectFactory factory) => factory.GetCachedValue("CH.NCTS.NationalityList", () => factory.GetNCNATCountryList());

	public static TariffView LoadTariffForTransit(BusinessObjectFactory factory, ZString tariffCode, ZDateTime effectiveDate)
	{
		TariffView tariff = null;

		if (tariffCode.Length == HSCodeLength)
		{
			tariff = new TariffView.Loader(factory).LoadMostRecentCachedTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, Constants.TariffTypes.HarmonizedSystem, tariffCode, effectiveDate);
		}
		else if (tariffCode.Length == SwissTariffCodeLength)
		{
			tariff = factory.GetCachedValue(string.Join("_", "LoadTariffForTransit", tariffCode, effectiveDate), () =>
			{
				var query = TariffView.Loader.GetEffectiveTariffFilter(factory, Core.Constants.CountryCodes.Switzerland, Constants.TariffTypes.Export, new[] { tariffCode }, effectiveDate, SQLComparisonOperator.StartsWith);
				query.OrderBy = TariffView.Schema.ZZ1_TariffCode;
				return factory.LoadTop1<TariffView>(query);
			});
		}
		else if (tariffCode.Length == SwissTariffAndStatisticalCodeLength)
		{
			tariff = new TariffView.Loader(factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Switzerland, Constants.TariffTypes.Export, tariffCode, effectiveDate);
		}

		return tariff;
	}

	const int HSCodeLength = 6;
	const int SwissTariffCodeLength = 8;
	const int SwissTariffAndStatisticalCodeLength = 11;
}
