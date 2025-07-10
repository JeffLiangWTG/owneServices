using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface ICusRefTradeGroupViewLoader
			{
				ZBool IsCountryPartOfTradeGroupAny(ZString countryCode, ZString[] tradeGroups, ZString dataGrouping, ZDateTime dateOfValuation);
			}
		}
	}
}
