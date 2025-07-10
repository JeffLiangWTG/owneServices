using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.UserManagement.Module
{
	public class EdiUserAgreementFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddDateFilters(filters);
			AddFlagsFilters(filters);
			AddNumberRangeFilters(filters);
			AddRelatedItemFilters(filters);
			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(FilterDescription.Title, EdiUserAgreementSchema.ERA_Title).MultilingualDescription = ResString.GetMultilingualString("UserManagement|EdiUserAgreementFilter|Title", FilterDescription.Title);
			filters.AddTextFilter(FilterDescription.Type, EdiUserAgreementSchema.ERA_Type, new EdiUserAgreementTypes()).MultilingualDescription = ResString.GetMultilingualString("UserManagement|EdiUserAgreementFilter|Type", FilterDescription.Type);
			filters.AddTextFilter(FilterDescription.Content, EdiUserAgreementSchema.ERA_Content).MultilingualDescription = ResString.GetMultilingualString("UserManagement|EdiUserAgreementFilter|Content", FilterDescription.Content);
		}

		#endregion

		#region Date

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(FilterDescription.EffectiveDate, GetEffectiveDateFilter, convertFromLocalToUTC: true).MultilingualDescription = ResString.GetMultilingualString("UserManagement|EdiUserAgreementFilter|EffectiveDate", FilterDescription.EffectiveDate);
		}

		ZQuery GetEffectiveDateFilter(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			var sql = @"
ERA_PK IN
(
	SELECT ERA_PK FROM
	(
		SELECT
			agreement.ERA_PK,
			EndTimeUtc =
				(
					SELECT
						MIN(sucAg.ERA_EffectiveTimeUtc)
					FROM
						dbo.EdiUserAgreement sucAg
					WHERE
						sucAg.ERA_Type = agreement.ERA_Type
						AND sucAg.ERA_RN_NKCountryCode = agreement.ERA_RN_NKCountryCode
						AND sucAg.ERA_EffectiveTimeUtc > agreement.ERA_EffectiveTimeUtc
				)
		FROM dbo.EdiUserAgreement agreement
		WHERE
			agreement.ERA_EffectiveTimeUtc <= @DateTo
	) agreementWithEndTimeUtc
	WHERE
		agreementWithEndTimeUtc.EndTimeUtc IS NULL
		OR agreementWithEndTimeUtc.EndTimeUtc > @dateFrom
)
";
			var parameterCollection = new ZSqlParameterCollection();
			parameterCollection.Add("@DateFrom", dateFrom, EdiUserAgreementSchema.ERA_EffectiveTimeUtc);
			parameterCollection.Add("@DateTo", dateTo, EdiUserAgreementSchema.ERA_EffectiveTimeUtc);

			var query = new ZDBOnlyQuery(typeof(EdiUserAgreement));
			query.AddFilterAndZSQLParameterCollection(sql, parameterCollection);
			return query;
		}

		#endregion

		#region Flag

		void AddFlagsFilters(ModuleFilterCollection filters)
		{
			var isPublishedFilter = filters.AddFlagsFilter(FilterDescription.IsActive, new string[] { ResString.GetMultilingualString("UserManagement|EdiUserAgreementFilter|IsActive", FilterDescription.IsActive) }, new GetFlagsQuery[] { GetIsActiveFilter });
			isPublishedFilter.MultilingualDescription = ResString.GetMultilingualString("UserManagement|EdiUserAgreementFilter|IsActive", FilterDescription.IsActive);
		}

		ZQuery GetIsActiveFilter(ZBool value)
		{
			return new ZQuery(EdiUserAgreementSchema.ERA_IsActive, value);
		}

		#endregion

		#region Related Item

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			filters.AddNkFilter(FilterDescription.Country, EdiUserAgreementSchema.ERA_RN_NKCountryCode, ModuleIDs.RefCountry, new RefCountryCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("UserManagement|EdiUserAgreementFilter|Country", FilterDescription.Country);
		}

		#endregion

		#region Number Range

		void AddNumberRangeFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberRangeFilter(FilterDescription.VersionNumber, EdiUserAgreementSchema.ERA_VersionNumber).MultilingualDescription = ResString.GetMultilingualString("UserManagement|EdiUserAgreementFilter|VersionNumber", FilterDescription.VersionNumber);
		}

		#endregion

		#region Filter Descriptions

		public static class FilterDescription
		{
			#region SuppressResourceStringsCheckRegion

			public const string Title = "Title";
			public const string Type = "Type";
			public const string Content = "Content";
			public const string Country = "Country/Region";
			public const string EffectiveDate = "Effective Date";
			public const string IsActive = "Active";
			public const string VersionNumber = "Version Number";

			#endregion
		}

		#endregion
	}
}
