using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.CN.Business
{
	public static class CNRefCusMapper
	{
		public static ZString MapCW1StateCodeToCustomsCode(BusinessObjectFactory factory, ZString code)
		{
			return MapCW1CodeToCustomsCode(factory, code, RefCusCodeListTypes.Codes.CNCIQStates);
		}
		public static ZString MapCW1CurrencyCodeToCustomsCode(BusinessObjectFactory factory, ZString code)
		{
			return MapCW1CodeToCustomsCode(factory, code, RefCusMapTypeList.Codes.Currency);
		}

		public static ZString MapCW1CountryCodeToCustomsCode(BusinessObjectFactory factory, ZString code)
		{
			return MapCW1CodeToCustomsCode(factory, code, RefCusMapTypeList.Codes.Country);
		}

		static ZString MapCW1CodeToCustomsCode(BusinessObjectFactory factory, ZString code, ZString type)
		{
			return code.IsEmpty ? ZString.Empty : ZZRefCusMapCombined.MapCW1CodeToCustomsCode(factory, Core.Constants.CountryCodes.China, type, code, ZDateTime.Today);
		}
	}
}
