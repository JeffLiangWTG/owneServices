using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BR.Business
{
	public static class BRRefCusMapper
	{
		static ZString MapCW1CodeToCustomsCode(BusinessObjectFactory factory, ZString code, ZString type)
		{
			return code.IsEmpty ? ZString.Empty : ZZRefCusMapCombined.MapCW1CodeToCustomsCode(factory, Core.Constants.CountryCodes.Brazil, type, code, ZDateTime.Today);
		}

		public static ZString MapCW1CurrencyCodeToCustomsCode(BusinessObjectFactory factory, ZString code)
		{
			return MapCW1CodeToCustomsCode(factory, code, RefCusMapTypeList.Codes.Currency);
		}

		public static ZString MapCW1CountryCodeToCustomsCode(BusinessObjectFactory factory, ZString code)
		{
			var result = MapCW1CodeToCustomsCode(factory, code, RefCusMapTypeList.Codes.Country);
			return result.IsEmpty ? ZString.Empty : result.PadLeft(3, '0'); 
		}

		public static ZString MapCustomsCodeCountryToCW1Code(BusinessObjectFactory factory, ZString code)
		{
			return MapCustomsCodeToCW1Code(factory, code, RefCusMapTypeList.Codes.Country);
		}

		public static ZString MapCW1ModalTransportCodeToCustomsCode(BusinessObjectFactory factory, ZString code)
		{
			return MapCW1CodeToCustomsCode(factory, code, Constants.RefCusMapType.ModalTransport);
		}

		static ZString MapCustomsCodeToCW1Code(BusinessObjectFactory factory, ZString code, ZString type)
		{
			return code.IsEmpty ? ZString.Empty : ZZRefCusMapCombined.MapCustomsCodeToCW1Code(factory, Core.Constants.CountryCodes.Brazil, type, code, ZDateTime.Today);
		}

		public static ZString MapCustomsCodeCurrencyToCW1Code(BusinessObjectFactory factory, ZString code)
		{
			return MapCustomsCodeToCW1Code(factory, code, RefCusMapTypeList.Codes.Currency);
		}

		public static Dictionary<ZString, ZString> GetTaxRevenueCodeMapping(BusinessObjectFactory factory)
		{
			return ZZRefCusMapCombined.GetCW1CodeToCustomsCodeMapping(factory, Core.Constants.CountryCodes.Brazil, RefCusMapTypeList.Codes.RCODE, ZDateTime.Today);
		}

		public static ZString MapCW1RateTypeToCustomsCode(BusinessObjectFactory factory, ZString code)
		{
			return MapCW1CodeToCustomsCode(factory, code, RefCusMapTypeList.Codes.RateType);
		}
	}
}
