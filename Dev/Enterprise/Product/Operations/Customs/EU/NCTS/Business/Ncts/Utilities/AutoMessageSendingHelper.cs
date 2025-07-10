using System;
using System.Collections.Immutable;

namespace Enterprise.Customs.EU.NCTS.Business;

public static class AutoMessageSendingHelper
{
	public static bool SupportsNctsAutomaticMessageSending(string countryCode) => GetCountriesSupportingAutoMessageSending().Contains(countryCode);

	static ImmutableHashSet<string> GetCountriesSupportingAutoMessageSending()
	{
		return _countriesSupportingAutoMessageSending ??= ImmutableHashSet.Create(
			Core.Constants.CountryCodes.Germany,
			Core.Constants.CountryCodes.France,
			Core.Constants.CountryCodes.UnitedKingdom,
			Core.Constants.CountryCodes.Ireland,
			Core.Constants.CountryCodes.Netherlands,
			Core.Constants.CountryCodes.Norway
		);
	}

	[ThreadStatic]
	static ImmutableHashSet<string> _countriesSupportingAutoMessageSending;
}
