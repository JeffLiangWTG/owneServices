using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public static class ImportMappingHelper
	{
		public static ZString MapDeclarantTypeToMessaging(this ZString value)
		{
			var result = MapDeclarantTypeToMessagingWithoutIndirect(value);
			if (result.IsEmpty && value == RepresentationTypeList.Codes._3Indirect)
			{
				result = "2";
			}
			return result;
		}

		public static ZString MapDeclarantTypeToMessagingWithoutIndirect(this ZString value)
		{
			switch (value)
			{
				case RepresentationTypeList.Codes._1Self:
					return "0";
				case RepresentationTypeList.Codes._2Direct:
					return "1";
			}
			return ZString.Empty;
		}

		public static ZString GetDestinationFederalState(JobDeclaration declaration, ZString destinationCountry)
		{
			var result = ZString.Empty;
			if (destinationCountry != Core.Constants.CountryCodes.Germany)
			{
				result = "25";
			}
			else
			{
				var finalDestination = declaration.JE_RL_NKFinalDestination;
				if (!finalDestination.IsEmpty)
				{
					var destinationCountryStateCode = new RefUNLOCO.Loader(declaration.Factory).Load(finalDestination)?.CountryStates?.RW_Code ?? ZString.Empty;
					if (!destinationCountryStateCode.IsEmpty && refCountryStatesMappingDictionary.TryGetValue(destinationCountryStateCode, out var mappedValue))
					{
						result = mappedValue;
					}
				}
			}
			return result;
		}

		static readonly ImmutableDictionary<string, string> refCountryStatesMappingDictionary = ImmutableDictionary.CreateRange(new Dictionary<string, string>()
		{
			{ "BW", "08" },
			{ "BY", "09" },
			{ "BE", "11" },
			{ "BB", "12" },
			{ "HB", "04" },
			{ "HH", "02" },
			{ "HE", "06" },
			{ "MV", "13" },
			{ "NI", "03" },
			{ "NW", "05" },
			{ "RP", "07" },
			{ "SL", "10" },
			{ "SN", "14" },
			{ "ST", "15" },
			{ "SH", "01" },
			{ "TH", "16" }
		});
	}
}
