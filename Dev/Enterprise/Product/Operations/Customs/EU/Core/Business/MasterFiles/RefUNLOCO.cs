using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.MasterFiles
{
	public static class RefUNLOCOExtension
	{
		public static ZBool IsInEUSpecialTerritory(this RefUNLOCO loco)
		{
			var result = ZBool.False;
			if (loco != null)
			{
				var specialTerritoryFullCodes = new List<ZString> { "DEHGL", //Heligoland (Germany)
																	"GBSRX" }; // Sark - Guernsey, not UK
				var specialTerritoryCountryCodes = new List<ZString> {  Core.Constants.CountryCodes.FrenchGuyana,
																		Core.Constants.CountryCodes.Guernsey,
																		Core.Constants.CountryCodes.Guadeloupe,
																		Core.Constants.CountryCodes.IsleOfMan,
																		Core.Constants.CountryCodes.Jersey,
																		Core.Constants.CountryCodes.Martinique,
																		Core.Constants.CountryCodes.Reunion,
																		"IC",  // reserved for future use as Canary Islands (Spain)
																		Core.Constants.CountryCodes.AlandIslands //Aland islands (Finland) (Ahvenanmaan)
																		};
				result = specialTerritoryFullCodes.Contains(loco.RL_Code) || specialTerritoryCountryCodes.Contains(loco.RL_Code.Left(2)) || UnlocoIsInSpecialState(loco);
			}
			return result;
		}

		static ZBool UnlocoIsInSpecialState(RefUNLOCO refUNLOCO)
		{
			var stateCeutaSpain_RWPK = new ZGuid("CF7A91CE-4EC1-4B58-9210-BC39D33430C5");
			var stateLasPalmasSpain_RWPK = new ZGuid("895F820D-7CEC-4C9C-A31D-39B02D187ECA");
			var stateMelillaSpain_RWPK = new ZGuid("E441F246-8592-4859-BA35-1E3CC7E2E9AF");
			var stateTenerifeSpain_RWPK = new ZGuid("0FEEC1E8-93A2-491A-8A14-01A9202A004B");
			var stateArlandFinland_RWPK = new ZGuid("EC4C896C-416D-455F-9CBB-9B5E50FFF1A8");  //Ahvenanmaan Maakunta or Ahvenanmaan lääni = Aland
			var allStates = new List<ZGuid> { stateCeutaSpain_RWPK, stateLasPalmasSpain_RWPK, stateMelillaSpain_RWPK, stateTenerifeSpain_RWPK, stateArlandFinland_RWPK };
			return allStates.Contains(refUNLOCO.RL_RW);
		}
	}
}
