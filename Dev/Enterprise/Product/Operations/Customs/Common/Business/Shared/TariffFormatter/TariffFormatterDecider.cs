using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;

namespace Enterprise.Customs.Common
{
	public static class TariffFormatterDecider
	{
		public static ITariffFormatter GetByCountryCode(string countryCode)
		{
			countryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode);
			ITariffFormatter result;
			if (!Dictionary.TryGetValue(countryCode, out result))
			{
				var types = ObjectFactory.Get<Hashtable>("TariffFormatterDeciders");
				if (!string.IsNullOrEmpty(countryCode))
				{
					var objectHandle = (ObjectHandle)types[countryCode];
					result = (ITariffFormatter)objectHandle?.GetObject();

					if (result == null && (countryCode == EuropeanCustomsUnion || ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnion(countryCode)))
					{
						result = ObjectFactory.Get<ITariffFormatterDecider>("EU.ITariffFormatterDecider")?.GetTariffFormatter(countryCode);
					}
				}

				if (result == null)
				{
					var objectHandle = (ObjectHandle)types[Shared];
					result = (ITariffFormatter)objectHandle.GetObject();
				}
				Dictionary.Add(countryCode, result);
			}
			return result;
		}
		const string EuropeanCustomsUnion = "EUN";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "constant")]
		const string Shared = "Shared";

		static Dictionary<string, ITariffFormatter> Dictionary => dictionary ?? (dictionary = new Dictionary<string, ITariffFormatter>());

		[ThreadStatic]
		static Dictionary<string, ITariffFormatter> dictionary;
	}

	class CommonTariffFormatter : Integration.Customs.Shared.ICommonTariffFormatter
	{
		public string DisplayFormat(string countryCode, string unformattedTariff)
		{
			return TariffFormatterDecider.GetByCountryCode(countryCode).DisplayFormat(unformattedTariff);
		}
	}
}
