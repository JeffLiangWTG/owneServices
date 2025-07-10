using System.Collections.Immutable;
using CargoWise.Types;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IT.Business;

public static class CurrenciesHelper
{
	static readonly ImmutableArray<string> CurrenciesRecognizedByCustoms = ImmutableArray.Create(
			CurrencyCodes.EuropeanUnion,
			CurrencyCodes.UnitedStates,
			CurrencyCodes.Japan,
			CurrencyCodes.CzechRepublic,
			CurrencyCodes.Denmark,
			CurrencyCodes.UnitedKingdom,
			CurrencyCodes.Hungary,
			CurrencyCodes.Poland,
			CurrencyCodes.Romania,
			CurrencyCodes.Sweden,
			CurrencyCodes.Switzerland,
			CurrencyCodes.Norway,
			CurrencyCodes.Croatia,
			CurrencyCodes.Russia,
			CurrencyCodes.Turkey,
			CurrencyCodes.Australia,
			CurrencyCodes.Brazil,
			CurrencyCodes.Canada,
			CurrencyCodes.China,
			CurrencyCodes.HongKong,
			CurrencyCodes.Israel,
			CurrencyCodes.India,
			CurrencyCodes.NewZealand,
			CurrencyCodes.SouthAfrica,
			CurrencyCodes.KoreaRepublicOf,
			CurrencyCodes.Mexico,
			CurrencyCodes.Malaysia,
			CurrencyCodes.Philippines,
			CurrencyCodes.Singapore,
			CurrencyCodes.Thailand
	   );

	public static ZBool IsCurrencyRecognizedByCustoms(ZString currencyCode) => CurrenciesRecognizedByCustoms.Contains(currencyCode);
}
