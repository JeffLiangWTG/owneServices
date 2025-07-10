using System.Collections.Generic;
using CargoWise.Types;
using ContainerModes = Enterprise.Core.Constants.ContainerModes;

namespace Enterprise.Customs.CH.Business;

public static class JobDeclarationHelper
{
	public static bool IsContainerised(ZString containerMode) => containerMode.ToString() switch
	{
		ContainerModes.Containerised => true,
		ContainerModes.FCL => true,
		ContainerModes.LCL => true,
		_ => false
	};

	public static ZString GetTwoCharacterUnitType(ZString unitTypeCode)
	{
		Dictionary<ZString, ZString> unitTypeList = new Dictionary<ZString, ZString>()
			{
				{ "BAG", "BG" },
				{ "BLC", "BN" },
				{ "BND", "BE" },
				{ "BOX", "BX" },
				{ "BSK", "BK" },
				{ "CAS", "CS" },
				{ "CNT", "CN" },
				{ "COI", "CL" },
				{ "CRT", "CR" },
				{ "CTN", "CT" },
				{ "CYL", "CY" },
				{ "DRM", "DR" },
				{ "ENV", "EN" },
				{ "KEG", "KG" },
				{ "PAI", "PL" },
				{ "PLT", "PX" },
				{ "REL", "RL" },
				{ "RLL", "RO" },
				{ "SHT", "ST" },
				{ "SKD", "SI" },
				{ "SPL", "SO" },
				{ "TUB", "TU" }
			};

		var twoCharacterUnitType = ZString.Format("PK");

		if (!unitTypeCode.IsEmpty)
		{
			if (unitTypeCode.Length == 3 && unitTypeList.ContainsKey(unitTypeCode))
			{
				twoCharacterUnitType = unitTypeList[unitTypeCode];
			}
			else if (unitTypeCode.Length == 2 && unitTypeList.ContainsValue(unitTypeCode))
			{
				twoCharacterUnitType = unitTypeCode;
			}
		}
		return twoCharacterUnitType;
	}
}
