using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business
{
	public static class EURefCusMapper
	{
		public static ZString MapCW1AuthorisationCodeToCustomsCode(BusinessObjectFactory factory, ZString code)
		{
			return MapCW1CodeToCustomsCode(factory, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, code);
		}

		static ZString MapCW1CodeToCustomsCode(BusinessObjectFactory factory, ZString type, ZString code)
		{
			if (code.IsEmpty)
			{
				return code;
			}

			var customsCode = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, type, code, ZDateTime.Today);
			return !customsCode.IsEmpty ? customsCode : code;
		}
	}
}
