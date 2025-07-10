using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ResourceStrings.Module
{
	public class LocalLanguagesFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			filters.AddTextFilter("Language Code", RefLocalLanguageSchema.RA_Code).MultilingualDescription = ResString.GetMultilingualString("85a33493-2b9a-4f79-813b-114860958c85", "Language Code");
			filters.AddTextFilter("Country Code", RefLocalLanguageSchema.RA_RN_NKCountryCode).MultilingualDescription = ResString.GetMultilingualString("20abaf92-7ecc-4e45-9CC6-360C2e003e19", "Country/Region Code");
			filters.AddTextFilter("Full Language Code", GetFullLanguageCodeQuery()).MultilingualDescription = ResString.GetMultilingualString("1eef8428-6c6b-407f-87b0-31afbae5117b", "Full Language Code");
			filters.AddFiltersForTranslatableText("Language Name", RefLocalLanguageSchema.RA_Description, typeof(RefLocalLanguage), ResString.GetMultilingualString("7fcbb0bd-7d93-44e5-b14b-6716a9816112", "Language Name"));
			return filters;
		}

		GetTextQueryWithOperator GetFullLanguageCodeQuery()
		{
			return delegate(SQLComparisonOperator comparisonOperator, ZString value)
			{
				var query = new ZDBOnlyQuery(typeof(RefLocalLanguage));
				var sqlText = string.Format(CultureInfo.InvariantCulture,
					"{0} + case({1}) when '' then '' else '-' + {1} end {2} '{3}'", RefLocalLanguageSchema.Constants.RA_Code, RefLocalLanguageSchema.Constants.RA_RN_NKCountryCode,
					comparisonOperator.ComparisonText(value), comparisonOperator.ValueForLiteralADO(value));
				query.AddFilterAndZSQLParameterCollection(sqlText, new ZSqlParameterCollection());
				return query;
			};
		}
	}
}
