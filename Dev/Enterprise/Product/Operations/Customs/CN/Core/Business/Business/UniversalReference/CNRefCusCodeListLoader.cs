using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;
using RefCusCodeListTypesCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.CN.Business
{
	public static class CNRefCusCodeListLoader
	{
		#region Get RefCusCodeList Object with given code

		public static ZZRefCusCodeListCombined GetAdditionalElement(BusinessObjectFactory factory, ZString code, ZDateTime date)
		{
			return GetCNCodeByType(factory, code, RefCusCodeListTypesCodes.CNAdditionalElements, date);
		}

		public static ZZRefCusCodeListCombined GetCustomsOffice(BusinessObjectFactory factory, ZString code, ZDateTime date)
		{
			return GetCNCodeByType(factory, code, RefCusCodeListTypesCodes.CustomsOffice, date);
		}

		public static ZZRefCusCodeListCombined GetPort(BusinessObjectFactory factory, ZString code, ZDateTime date)
		{
			return GetCNCodeByType(factory, code, RefCusCodeListTypesCodes.Port, date);
		}

		public static ZZRefCusCodeListCombined GetDistrict(BusinessObjectFactory factory, ZString code, ZDateTime date)
		{
			return GetCNCodeByType(factory, code, RefCusCodeListTypesCodes.DistrictCode, date);
		}

		public static ZZRefCusCodeListCombined GetRegion(BusinessObjectFactory factory, ZString code, ZDateTime date)
		{
			return GetCNCodeByType(factory, code, RefCusCodeListTypesCodes.CNCIQDistricts, date);
		}

		public static ZZRefCusCodeListCombined GetCurrency(BusinessObjectFactory factory, ZString code, ZDateTime date)
		{
			return GetCNCodeByType(factory, code, RefCusCodeListTypesCodes.Currency, date);
		}

		public static ZZRefCusCodeListCombined GetRequiredDocuments(BusinessObjectFactory factory, ZString code, ZDateTime date)
		{
			return GetCNCodeByType(factory, code, RefCusCodeListTypesCodes.CNRequiredDocuments, date);
		}

		public static ZZRefCusCodeListCombined GetCIQOffice(BusinessObjectFactory factory, ZString code, ZDateTime date)
		{
			return GetCNCodeByType(factory, code, RefCusCodeListTypesCodes.CNCIQOfficeCode, date);
		}

		public static ZZRefCusCodeListCombined GetCIQPortOffices(BusinessObjectFactory factory, ZString code, ZDateTime date)
		{
			return GetCNCodeByType(factory, code, RefCusCodeListTypesCodes.CNCIQPortOffices, date);
		}

		public static ZZRefCusCodeListCombined GetCIQState(BusinessObjectFactory factory, ZString code, ZDateTime date)
		{
			return GetCNCodeByType(factory, code, RefCusCodeListTypesCodes.CNCIQStates, date);
		}

		public static ZZRefCusCodeListCombined GetCustomsStatus(BusinessObjectFactory factory, ZString code, ZDateTime date)
		{
			return GetCNCodeByType(factory, code, RefCusCodeListTypesCodes.CustomsStatus, date);
		}

		public static void AddRefCusCodeListFetchHintIfNotEmpty(this BusinessObjectFactory factory, ZString type, ZString code, ZDateTime date)
		{
			if (!code.IsEmpty)
			{
				var query = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, code);
				query.AddToFilter(ZZRefCusCodeListCombined.Loader.GetFilter(factory, Core.Constants.CountryCodes.China, type, date, (ZQuery)null, true));
				factory.AddFetchHint(ZZRefCusCodeListCombinedSchema.Instance, query);
			}
		}

		#endregion

		#region Common

		public static ZZRefCusCodeListCombined GetCNCodeByType(BusinessObjectFactory factory, ZString code, ZString codeType, ZDateTime date)
		{
			return code.IsEmpty ? null : ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, Core.Constants.CountryCodes.China, codeType, date);
		}

		#endregion
	}
}
