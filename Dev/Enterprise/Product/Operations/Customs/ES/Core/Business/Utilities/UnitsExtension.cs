using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ES.Business
{
	public static class UnitsExtension
	{
		public static ZString ConvertCargoWiseToES(this ZString value, BusinessObjectFactory factory)
		{
			if (value.IsEmpty)
			{
				return value;
			}

			var refDbMappedCode = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(factory, Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, value, ZDateTime.Today);
			return refDbMappedCode.IsEmpty ? ConvertSpecificValues(value) : refDbMappedCode;

			ZString ConvertSpecificValues(ZString unitToConvert) => unitToConvert == ESConstants.UOM.PK || unitToConvert == ESConstants.UOM.GF ? (ZString)ESConstants.UOM.KN : unitToConvert;
		}

		public static ZString ConvertESToCargoWise(this ZString value, BusinessObjectFactory factory)
		{
			if (value.IsEmpty)
			{
				return value;
			}

			var refDbMappedCode = ZZRefCusMapCombined.MapCustomsCodeToCW1Code(factory, Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, value, ZDateTime.Today);
			return refDbMappedCode.IsEmpty ? value : refDbMappedCode;
		}
	}
}
