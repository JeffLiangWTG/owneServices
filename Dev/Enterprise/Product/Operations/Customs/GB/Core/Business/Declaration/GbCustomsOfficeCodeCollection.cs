using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class GbCustomsOfficeCodeCollection : EUCustomsOfficeCodeCollection
	{
		public GbCustomsOfficeCodeCollection(BusinessObjectFactory factory, ZString dataGroupingCode, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters)
			: base(factory, new ZString[] { dataGroupingCode }, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, attributeFilters, ZString.Empty)
		{
			officeCodePrefix = dataGroupingCode;
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", dataGroupingCode));
		}

		readonly ZString officeCodePrefix;

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, SQLComparisonOperator.StartsWith, officeCodePrefix));
			return result;
		}

		public static GbCustomsOfficeCodeCollection AllUKCustomsOfficesWithRequiredRoles(BusinessObjectFactory factory, params ZString[] roles)
		{
			return new GbCustomsOfficeCodeCollection(factory, CountryCodes.UnitedKingdom, CreateRoleAttributeFilters(roles));
		}
	}
}
