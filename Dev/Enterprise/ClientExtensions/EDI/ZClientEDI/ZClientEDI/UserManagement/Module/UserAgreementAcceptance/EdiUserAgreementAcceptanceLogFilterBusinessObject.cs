using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.UserManagement.Module
{
	public class EdiUserAgreementAcceptanceLogFilterBusinessObject : FilterStripBusinessObject
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

		ZDBOnlyQuery GetQueryBase(ZDBOnlySubQuery subQuery)
		{
			var query = new ZDBOnlyQuery(typeof(EdiUserAgreementAcceptanceLog));
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		FilterCategory acceptanceCategory => FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("C914D888-7BD8-4B9F-882F-1E950CB79871", "Acceptance"));
		FilterCategory productCategory => FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("3EE3DC36-EA50-4A7B-938D-BAE04EE5BD28", "Product"));
		FilterCategory userAgreementCategory => FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("9D6BB203-E6C2-4380-96B7-FBC2A4DC8FE6", "User Agreement"));
		FilterCategory systemUserAccountCategory => FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("09B48486-79FB-474D-A7D2-2A5C0AFF174A", "System User Account"));

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var titleFilter = filters.AddTextFilter(FilterDescription.Title, GetUserAgreementTitleQuery);
			titleFilter.MultilingualDescription = ResString.GetMultilingualString("ACB3D847-3305-4024-8E93-6F9A0D12C920", FilterDescription.Title);
			titleFilter.Category = userAgreementCategory;

			var typeFilter = filters.AddTextFilter(FilterDescription.Type, GetUserAgreementTypeQuery);
			typeFilter.MultilingualDescription = ResString.GetMultilingualString("60071522-9BAC-48A6-A15C-4EA934B03581", FilterDescription.Type);
			typeFilter.Category = userAgreementCategory;

			var contentFilter = filters.AddTextFilter(FilterDescription.Content, GetUserAgreementContentQuery);
			contentFilter.MultilingualDescription = ResString.GetMultilingualString("3CE0C739-9266-4EBD-92BB-636E9DFA95DF", FilterDescription.Content);
			contentFilter.Category = userAgreementCategory;

			var contactNameFilter = filters.AddTextFilter(FilterDescription.ContactName, GetUserAgreementContactNameQuery);
			contactNameFilter.MultilingualDescription = ResString.GetMultilingualString("380FE358-D420-4E44-ABA7-29DE4A16DC20", FilterDescription.ContactName);
			contactNameFilter.Category = systemUserAccountCategory;

			var organisationNameFilter = filters.AddTextFilter(FilterDescription.OrganisationName, GetUserAgreementOrganizationNameQuery);
			organisationNameFilter.MultilingualDescription = ResString.GetMultilingualString("F36440A9-DCC3-4C3A-9B13-2D91DEA71B34", "Organization Name");
			organisationNameFilter.Category = systemUserAccountCategory;

			var productCodeFilter = filters.AddTextFilter(FilterDescription.ProductCode, GetUserAgreementProductCodeQuery, ProductTypeList);
			productCodeFilter.MultilingualDescription = ResString.GetMultilingualString("A86E57A5-BE25-4F6E-B84F-0F96BCA6ED85", FilterDescription.ProductCode);
			productCodeFilter.Category = productCategory;
		}

		ZQuery GetUserAgreementTitleQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (!value.IsEmpty)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(EdiUserAgreement), EdiUserAgreementAcceptanceLogSchema.EUL_ERA);
				subQuery.AddToFilter(EdiUserAgreementSchema.ERA_Title, comparisonOperator, value);
				return GetQueryBase(subQuery);
			}

			return null;
		}

		ZQuery GetUserAgreementTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (!value.IsEmpty)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(EdiUserAgreement), EdiUserAgreementAcceptanceLogSchema.EUL_ERA);
				subQuery.AddToFilter(EdiUserAgreementSchema.ERA_Type, comparisonOperator, value);
				return GetQueryBase(subQuery);
			}

			return null;
		}

		ZQuery GetUserAgreementContentQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (!value.IsEmpty)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(EdiUserAgreement), EdiUserAgreementAcceptanceLogSchema.EUL_ERA);
				subQuery.AddToFilter(EdiUserAgreementSchema.ERA_Content, comparisonOperator, value);
				return GetQueryBase(subQuery);
			}

			return null;
		}

		ZQuery GetUserAgreementContactNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (!value.IsEmpty)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(EdiCustomerUserAccount), EdiUserAgreementAcceptanceLogSchema.EUL_EUA);
				var contactSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact);
				contactSubQuery.AddToFilter(OrgContactSchema.OC_ContactName, comparisonOperator, value);
				subQuery.AddSubQuery(contactSubQuery, JoinCondition.And);
				return GetQueryBase(subQuery);
			}

			return null;
		}

		ZQuery GetUserAgreementOrganizationNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (!value.IsEmpty)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(EdiCustomerUserAccount), EdiUserAgreementAcceptanceLogSchema.EUL_EUA);
				var contactSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact);
				var organizationSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgContactSchema.OC_OH);
				organizationSubQuery.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, value);
				contactSubQuery.AddSubQuery(organizationSubQuery, JoinCondition.And);
				subQuery.AddSubQuery(contactSubQuery, JoinCondition.And);
				return GetQueryBase(subQuery);
			}

			return null;
		}

		ZQuery GetUserAgreementProductCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (!value.IsEmpty)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(EdiCustomerUserAccount), EdiUserAgreementAcceptanceLogSchema.EUL_EUA);
				var databaseSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), EdiCustomerUserAccountSchema.EUA_LD);
				databaseSubQuery.AddToFilter(LicenceDatabaseSchema.LD_Product, comparisonOperator, value);
				subQuery.AddSubQuery(databaseSubQuery, JoinCondition.And);
				return GetQueryBase(subQuery);
			}

			return null;
		}

		CodeDescriptionPairList ProductTypeList
		{
			get
			{
				if (productTypeList == null)
				{
					productTypeList = new LicenceDatabaseLookups(null).ProductTypeList;
				}

				return productTypeList;
			}
		}
		CodeDescriptionPairList productTypeList;

		#endregion

		#region Date

		void AddDateFilters(ModuleFilterCollection filters)
		{
			var effectiveDateFilter = filters.AddDateFilter(FilterDescription.EffectiveDate, GetEffectiveDateFilter, convertFromLocalToUTC: true);
			effectiveDateFilter.MultilingualDescription = ResString.GetMultilingualString("79466D8F-92CC-4D93-B4E5-5EB4288D3ED4", FilterDescription.EffectiveDate);
			effectiveDateFilter.Category = userAgreementCategory;

			var acceptanceDateFilter = filters.AddDateFilter(FilterDescription.AcceptanceDate, EdiUserAgreementAcceptanceLogSchema.EUL_AcceptanceTimeUtc, convertFromLocalToUTC: true);
			acceptanceDateFilter.MultilingualDescription = ResString.GetMultilingualString("99A502E4-A370-4B46-BA7E-45623D5C0FA7", FilterDescription.AcceptanceDate);
			acceptanceDateFilter.Category = acceptanceCategory;
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
			AND agreement.ERA_EffectiveTimeUtc >= @DateFrom
	) agreementWithEndTimeUtc
	WHERE
		agreementWithEndTimeUtc.EndTimeUtc IS NULL
		OR agreementWithEndTimeUtc.EndTimeUtc > @DateFrom
)
";
			var parameterCollection = new ZSqlParameterCollection();
			parameterCollection.Add("@DateFrom", dateFrom, EdiUserAgreementSchema.ERA_EffectiveTimeUtc);
			parameterCollection.Add("@DateTo", dateTo, EdiUserAgreementSchema.ERA_EffectiveTimeUtc);

			var subQuery = new ZDBOnlySubQuery(typeof(EdiUserAgreement), EdiUserAgreementAcceptanceLogSchema.EUL_ERA);
			subQuery.AddFilterAndZSQLParameterCollection(sql, parameterCollection);
			return GetQueryBase(subQuery);
		}

		#endregion

		#region Flag

		void AddFlagsFilters(ModuleFilterCollection filters)
		{
			var activeMultilingualString = ResString.GetMultilingualString("EdiUserAgreementAcceptanceLog|EdiUserAgreementFilter|IsActive", FilterDescription.IsActive);
			var isActiveFilter = filters.AddFlagsFilter(FilterDescription.IsActive, new string[] { activeMultilingualString }, new GetFlagsQuery[] { GetIsActiveFilter });
			isActiveFilter.MultilingualDescription = activeMultilingualString;
			isActiveFilter.Category = userAgreementCategory;
		}

		ZQuery GetIsActiveFilter(ZBool value)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(EdiUserAgreement), EdiUserAgreementAcceptanceLogSchema.EUL_ERA);
			subQuery.AddToFilter(EdiUserAgreementSchema.ERA_IsActive, value);
			return GetQueryBase(subQuery);
		}

		#endregion

		#region Related Item

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			var agreementCountryFilter = filters.AddNkFilter(FilterDescription.AgreementCountry, GetAgreementCountryCodeQuery, ModuleIDs.RefCountry, new RefCountryCollection(Factory));
			agreementCountryFilter.MultilingualDescription = ResString.GetMultilingualString("24A1D80C-F49C-4026-8727-71176608994E", FilterDescription.AgreementCountry);
			agreementCountryFilter.Category = userAgreementCategory;

			var contactFilter = new ContactAcceptanceLogModuleFilter(ModuleIDs.OrgContacts, OrgContactSchema.PK, EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, Factory, typeof(EdiUserAgreementAcceptanceLog));
			contactFilter.MultilingualDescription = ResString.GetMultilingualString("37C0FB52-48CB-4085-BD0E-4D396AD6CCC9", "Contacts");
			contactFilter.Category = systemUserAccountCategory;
			filters.AddFilter(contactFilter);

			var organisationFilter = new OrganisationAcceptanceLogModuleFilter(ModuleIDs.Organisation, EdiUserAgreementAcceptanceLogSchema.PK, OrgContactSchema.OC_OH, Factory, typeof(EdiUserAgreementAcceptanceLog));
			organisationFilter.MultilingualDescription = ResString.GetMultilingualString("C5739CA5-ADF9-4F7D-B0F2-989C840BC8B7", "Organizations");
			organisationFilter.Category = systemUserAccountCategory;
			filters.AddFilter(organisationFilter);

			var userCountryFilter = filters.AddNkFilter(FilterDescription.UserCountry, GetUserCountryFilterCodeQuery, ModuleIDs.RefCountry, new RefCountryCollection(Factory));
			userCountryFilter.MultilingualDescription = ResString.GetMultilingualString("F834E792-D0FF-4AB1-806E-E903222438A3", FilterDescription.UserCountry);
			userCountryFilter.Category = systemUserAccountCategory;
		}

		ZQuery GetAgreementCountryCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (!value.IsEmpty)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(EdiUserAgreement), EdiUserAgreementAcceptanceLogSchema.EUL_ERA);
				subQuery.AddToFilter(EdiUserAgreementSchema.ERA_RN_NKCountryCode, comparisonOperator, value);
				return GetQueryBase(subQuery);
			}

			return null;
		}

		ZQuery GetUserCountryFilterCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (!value.IsEmpty)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(EdiCustomerUserAccount), EdiUserAgreementAcceptanceLogSchema.EUL_EUA);
				subQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_RN_NKCountry, comparisonOperator, value);
				return GetQueryBase(subQuery);
			}

			return null;
		}

		#endregion

		#region Number Range

		void AddNumberRangeFilters(ModuleFilterCollection filters)
		{
			var versionNumberFilter = filters.AddNumberRangeFilter(FilterDescription.VersionNumber, GetUserAgreementVersionNumberQuery);
			versionNumberFilter.MultilingualDescription = ResString.GetMultilingualString("EdiUserAgreementAcceptanceLog|EdiUserAgreementFilter|VersionNumber", FilterDescription.VersionNumber);
			versionNumberFilter.Category = userAgreementCategory;
		}

		ZQuery GetUserAgreementVersionNumberQuery(INumericZType value1, INumericZType value2)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(EdiUserAgreement), EdiUserAgreementAcceptanceLogSchema.EUL_ERA);
			subQuery.AddToFilter(ModuleNumberRangeFilter.AddToFilters(new ZQuery(), EdiUserAgreementSchema.ERA_VersionNumber, value1, value2));
			return GetQueryBase(subQuery);
		}

		#endregion

		#region Filter Descriptions

		public static class FilterDescription
		{
			#region SuppressResourceStringsCheckRegion

			public const string Title = "Title";
			public const string Type = "Type";
			public const string Content = "Content";
			public const string AgreementCountry = "Agreement Country/Region";
			public const string EffectiveDate = "Effective Date";
			public const string IsActive = "Active";
			public const string VersionNumber = "Version Number";
			public const string Contact = "Contact";
			public const string ContactName = "Contact Name";
			public const string Organisation = "Organisation";
			public const string OrganisationName = "Organisation Name";
			public const string ProductCode = "Product";
			public const string AcceptanceDate = "Acceptance Date";
			public const string UserCountry = "User Country/Region";

			#endregion
		}

		#endregion
	}
}
