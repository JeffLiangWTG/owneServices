using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business
{
	public partial class FRDomesticOverseasTerritories
	{
		public static ZString[] GetRegionOrTerritoryOfDestinationCombined(ZString code)
		{
			switch (code)
			{
				case Codes.CORSE:
				case Codes.CONTI:
					return new ZString[] { code, FRLocalGroups.Codes.METRO };

				case Codes.MARTI:
				case Codes.GUADE:
				case Codes.REUNI:
					return new ZString[] { code, FRLocalGroups.Codes.DPDOM, FRLocalGroups.Codes.MGPRE };

				case Codes.GUYAN:
				case Codes.MAYOT:
					return new ZString[] { code, FRLocalGroups.Codes.DPDOM };

				default:
					return new ZString[] { code };
			}
		}

		public static Dictionary<ZString, ZString[]> GetTerritoriesFromCountryCode()
		{
			return new Dictionary<ZString, ZString[]>
			{
				{ Core.Constants.CountryCodes.France, new ZString[] { FRDomesticOverseasTerritories.Codes.CONTI, FRDomesticOverseasTerritories.Codes.CORSE } },
				{ Core.Constants.CountryCodes.FrenchGuyana, new ZString[] { FRDomesticOverseasTerritories.Codes.GUYAN } },
				{ Core.Constants.CountryCodes.Guadeloupe, new ZString[] { FRDomesticOverseasTerritories.Codes.GUADE } },
				{ Core.Constants.CountryCodes.Martinique, new ZString[] { FRDomesticOverseasTerritories.Codes.MARTI } },
				{ Core.Constants.CountryCodes.Mayotte, new ZString[] { FRDomesticOverseasTerritories.Codes.MAYOT } },
				{ Core.Constants.CountryCodes.Reunion, new ZString[] { FRDomesticOverseasTerritories.Codes.REUNI } },
				{ Core.Constants.CountryCodes.SaintMartin, new ZString[] { FRDomesticOverseasTerritories.Codes.GUADE } }
			};
		}

		public static Dictionary<ZString, ZString> GetTerritoryFromState()
		{
			return new Dictionary<ZString, ZString>
			{
				{ "2A", FRDomesticOverseasTerritories.Codes.CORSE },
				{ "2B", FRDomesticOverseasTerritories.Codes.CORSE }
			};
		}
	}
}
