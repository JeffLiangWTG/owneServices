using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business
{
	public class EUCustomsOfficeCodeCollection : Customs.Business.CustomsOfficeCodeCollection
	{
		public EUCustomsOfficeCodeCollection(BusinessObjectFactory factory, IEnumerable<ZString> dataGroupingCodes, ZString codeType, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters, ZString countryFilter)
			: base(factory, dataGroupingCodes, codeType, attributeFilters, countryFilter)
		{
		}

		public EUCustomsOfficeCodeCollection(BusinessObjectFactory factory, ZString parentDataGroupingCode, ZString codeType, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters, ZString countryFilter, ZString[] otherCountries)
			: base(factory, parentDataGroupingCode, codeType, attributeFilters, countryFilter, otherCountries)
		{
		}

		public static EUCustomsOfficeCodeCollection CustomsOfficesWithRequiredRoles(BusinessObjectFactory factory, ZString[] dataGroupingCodes, params ZString[] roles)
			=> new EUCustomsOfficeCodeCollection(factory, dataGroupingCodes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, CreateRoleAttributeFilters(roles), ZString.Empty);

		public static EUCustomsOfficeCodeCollection AllEuropeanUnionCustomsOfficesWithRequiredRoles(BusinessObjectFactory factory, params ZString[] roles)
			=> GetAllEuropeanUnionCustomsOfficesWithRequiredRolesExceptLocal(factory, ZString.Empty, [], roles);

		public static EUCustomsOfficeCodeCollection AllEuropeanUnionAndOtherCountriesCustomsOfficesWithRequiredRoles(BusinessObjectFactory factory, ZString[] otherCountries, params ZString[] roles)
			=> GetAllEuropeanUnionCustomsOfficesWithRequiredRolesExceptLocal(factory, ZString.Empty, otherCountries, roles);

		public static EUCustomsOfficeCodeCollection AllEuropeanUnionCustomsOfficesWithRequiredRolesExceptLocal(BusinessObjectFactory factory, ZString dataGroupingCode, params ZString[] roles)
			=> GetAllEuropeanUnionCustomsOfficesWithRequiredRolesExceptLocal(factory, dataGroupingCode, [], roles);

		public static EUCustomsOfficeCodeCollection AllEuropeanUnionAndCtCountriesCustomsOfficesWithRequiredRoles(BusinessObjectFactory factory, params ZString[] roles) => AllEuropeanUnionAndCtCountriesCustomsOfficesWithRequiredRolesExceptLocal(factory, ZString.Empty, roles);

		public static EUCustomsOfficeCodeCollection AllEuropeanUnionAndCtCountriesCustomsOfficesWithRequiredRolesExceptLocal(BusinessObjectFactory factory, ZString localDataGroupingCode, params ZString[] roles)
		{
			var dataGroupingCodes = factory.GetEuropeanUnionAndCtCountries().Select(x => new ZString(x)).Concat(new ZString[] { Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes }).ToArray();
			return new EUCustomsOfficeCodeCollection(factory, dataGroupingCodes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, CreateRoleAttributeFilters(roles), localDataGroupingCode);
		}

		static EUCustomsOfficeCodeCollection GetAllEuropeanUnionCustomsOfficesWithRequiredRolesExceptLocal(BusinessObjectFactory factory, ZString dataGroupingCode, ZString[] otherCountries, params ZString[] roles)
		{
			var includeCountries = new List<ZString>(otherCountries ?? []);
			includeCountries.Add(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes);
			return new EUCustomsOfficeCodeCollection(factory, EconomicGroupList.Codes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, CreateRoleAttributeFilters(roles), dataGroupingCode, includeCountries.Distinct().ToArray());
		}
	}
}
