using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace ZClientEDI.GUI.UserManagement
{
	public class CustomerUserAccountWizardFilterBusinessObject : EdiCustomerUserAccountFilterBusinessObject
	{
		public CustomerUserAccountWizardFilterBusinessObject()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "EdiCustomerUserAccountWizard";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			AddTextFilters(filters);

			var organisationSubGroup = new OrganisationFilterSubGroup();
			AddOrganisationFilters(filters, organisationSubGroup);

			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var enterpriseCodeFilter = filters.AddTextFilter("Enterprise Code", GetLicenceEnterpriseCode);
			enterpriseCodeFilter.Category = licenceDatabaseCategory;
			enterpriseCodeFilter.MaxLength = LicenceEnterpriseSchema.LE_EnterpriseCode.MaxLength;

			var enterpriseIdFilter = filters.AddTextFilter("Enterprise ID", GetLicenceEnterpriseID);
			enterpriseIdFilter.Category = licenceDatabaseCategory;
			enterpriseIdFilter.MaxLength = LicenceEnterpriseSchema.LE_EnterpriseID.MaxLength;

			var databaseNumberFilter = filters.AddNumberRangeFilter("Database Number", GetLicenceDatabaseNumber);
			databaseNumberFilter.Decimals = 0;
			databaseNumberFilter.Category = licenceDatabaseCategory;
			databaseNumberFilter.DefaultPropertySearch = ModuleNumberRangeFilter.SearchTexts.EqualTo.GetUnresolvedString();
			databaseNumberFilter.PropertySearch = databaseNumberFilter.DefaultPropertySearch;
			databaseNumberFilter.MaxValue = int.MaxValue;
			databaseNumberFilter.MinValue = int.MinValue;

			var licenceTypeFilter = filters.AddTextFilter("Licence Type", GetLicenceType, new DatabaseTypes());
			licenceTypeFilter.Category = licenceDatabaseCategory;
			licenceTypeFilter.MaxLength = LicenceDatabaseSchema.LD_LicenceType.MaxLength;
		}

		ZQuery GetLicenceEnterpriseCode(SQLComparisonOperator comparison, ZString key)
		{
			return GetLicenceEnterpriseBaseQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, comparison, key);
		}

		ZQuery GetLicenceEnterpriseID(SQLComparisonOperator comparison, ZString key)
		{
			return GetLicenceEnterpriseBaseQuery(LicenceEnterpriseSchema.LE_EnterpriseID, comparison, key);
		}

		ZQuery GetLicenceEnterpriseBaseQuery(SchemaColumn schemaColumn, SQLComparisonOperator comparison, ZString key)
		{
			var licenceEnterpriseSubQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceEnterpriseSchema.PK);
			licenceEnterpriseSubQuery.AddToFilter(JoinCondition.And, schemaColumn, comparison, key);

			var licenceDatabaseSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
			licenceDatabaseSubQuery.AddSubQuery(LicenceDatabaseSchema.LD_LE, licenceEnterpriseSubQuery, JoinCondition.And);

			var ediCustomerUserAccountQuery = new ZDBOnlyQuery(typeof(EdiCustomerUserAccount));
			ediCustomerUserAccountQuery.AddSubQuery(EdiCustomerUserAccountSchema.EUA_LD, licenceDatabaseSubQuery, JoinCondition.And);

			var query = new ZQuery();
			query.AddToFilter(ediCustomerUserAccountQuery);
			return query;
		}

		ZQuery GetLicenceDatabaseNumber(INumericZType value1, INumericZType value2)
		{
			var licenceDatabaseSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
			licenceDatabaseSubQuery.AddToFilter(ModuleNumberRangeFilter.AddToFilters(new ZQuery(), LicenceDatabaseSchema.LD_DatabaseNumber, value1, value2), JoinCondition.And);

			var ediCustomerUserAccountQuery = new ZDBOnlyQuery(typeof(EdiCustomerUserAccount));
			ediCustomerUserAccountQuery.AddSubQuery(EdiCustomerUserAccountSchema.EUA_LD, licenceDatabaseSubQuery, JoinCondition.And);

			var query = new ZQuery();
			query.AddToFilter(ediCustomerUserAccountQuery);
			return query;
		}

		ZQuery GetLicenceType(SQLComparisonOperator comparison, ZString key)
		{
			var licenceDatabaseSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
			licenceDatabaseSubQuery.AddToFilter(JoinCondition.And, LicenceDatabaseSchema.LD_LicenceType, comparison, key);

			var ediCustomerUserAccountQuery = new ZDBOnlyQuery(typeof(EdiCustomerUserAccount));
			ediCustomerUserAccountQuery.AddSubQuery(EdiCustomerUserAccountSchema.EUA_LD, licenceDatabaseSubQuery, JoinCondition.And);

			var query = new ZQuery();
			query.AddToFilter(ediCustomerUserAccountQuery);
			return query;
		}

		void AddOrganisationFilters(ModuleFilterCollection filters, OrganisationFilterSubGroup subgroup)
		{
			var orgFilter = filters.AddGuidFilter("Organisation", ModuleIDs.Organisation, OrgHeaderSchema.PK, OrganisationList);
			orgFilter.Category = FilterCategories.Organisations;
			orgFilter.SubGroup = subgroup;

			var orgNameFilter = filters.AddTextFilter("Organisation Name", GetOrganisationName);//OrgHeaderSchema.OH_FullName);
			orgNameFilter.Category = FilterCategories.Organisations;
			orgNameFilter.MaxLength = OrgHeaderSchema.OH_FullName.MaxLength;
			orgNameFilter.SubGroup = subgroup;
		}

		ZQuery GetOrganisationName(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var isNegativeOperator = comparisonOperator.IsNegativeSQLOperator();
			var joinCondition = isNegativeOperator ? JoinCondition.And : JoinCondition.Or;

			ZDBOnlySubQuery GetMainAddressSubQuery(bool notIn = false)
			{
				var mainAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH, notIn || isNegativeOperator);
				mainAddressSubQuery.AddSubQuery(OrgAddressSchema.PK, GetOrgAddressCapabilitySubQuery(), JoinCondition.And);
				mainAddressSubQuery.AddToFilter(OrgAddressSchema.OA_CompanyNameOverride, comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), value.SubstringSafe(0, OrgAddressSchema.OA_CompanyNameOverride.MaxLength));

				return mainAddressSubQuery;
			}

			var query = new ZDBOnlyQuery(typeof(OrgContact));

			// Contact has address
			// Contact address company name meets condition
			var orgAddressSubQuery1 = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK, isNegativeOperator);
			orgAddressSubQuery1.AddToFilter(OrgAddressSchema.OA_CompanyNameOverride, comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), value.SubstringSafe(0, OrgAddressSchema.OA_CompanyNameOverride.MaxLength));

			// Contact has no address
			// Org main address company name meets condition
			var orgMainAddressSubQuery = GetMainAddressSubQuery();
			orgMainAddressSubQuery.AddToFilter(JoinCondition.And, OrgContactSchema.OC_OA_OrgAddress, SQLComparisonOperator.Equal, DBNull.Value);

			// Contact has no address
			// Org full name meets condition
			// Org main address company name doesn't meet condition
			var orgHeaderSubQuery1 = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK, isNegativeOperator);
			orgHeaderSubQuery1.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), value.SubstringSafe(0, OrgHeaderSchema.OH_FullName.MaxLength));
			orgHeaderSubQuery1.AddToFilter(JoinCondition.And, OrgContactSchema.OC_OA_OrgAddress,
				SQLComparisonOperator.Equal, DBNull.Value);
			orgHeaderSubQuery1.AddSubQuery(GetMainAddressSubQuery(true), JoinCondition.And);

			// Contact has address
			// Contact address has blank company name
			// Org full name meets condition
			var contactSubQuery1 = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.PK, isNegativeOperator);
			var orgHeaderSubQuery2 = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgHeaderSubQuery2.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), value.SubstringSafe(0, OrgHeaderSchema.OH_FullName.MaxLength));
			var orgAddressSubQuery2 = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressSubQuery2.AddToFilter(OrgAddressSchema.OA_CompanyNameOverride, string.Empty);
			contactSubQuery1.AddSubQuery(OrgContactSchema.OC_OH, orgHeaderSubQuery2, JoinCondition.And);
			contactSubQuery1.AddSubQuery(OrgContactSchema.OC_OA_OrgAddress, orgAddressSubQuery2, JoinCondition.And);

			query.AddSubQuery(OrgContactSchema.OC_OA_OrgAddress, orgAddressSubQuery1, joinCondition);
			query.AddSubQuery(OrgContactSchema.OC_OH, orgMainAddressSubQuery, joinCondition);
			query.AddSubQuery(OrgContactSchema.OC_OH, orgHeaderSubQuery1, joinCondition);
			query.AddSubQuery(OrgContactSchema.PK, contactSubQuery1, joinCondition);
			return query;
		}

		ZDBOnlySubQuery GetOrgAddressCapabilitySubQuery() =>
			new ZDBOnlySubQuery(typeof(OrgAddressCapability), OrgAddressCapabilitySchema.PZ_OA)
				.AddToFilter(JoinCondition.And, OrgAddressCapabilitySchema.PZ_AddressType, OrgAddressType.Office.Code)
				.AddToFilter(JoinCondition.And, OrgAddressCapabilitySchema.PZ_IsMainAddress, ZBool.True) as ZDBOnlySubQuery;

		#region Organisation

		class OrganisationFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(EdiCustomerUserAccount));
				var enterpriseSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact);
				var orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgContactSchema.OC_OH);
				orgSubQuery.AddToFilter(filter);
				enterpriseSubQuery.AddSubQuery(orgSubQuery, JoinCondition.And);
				query.AddSubQuery(enterpriseSubQuery, JoinCondition.And);
				return query;
			}
		}

		public OrganisationsFindBoxCollection OrganisationList
		{
			get
			{
				if (organisationList == null)
				{
					organisationList = new OrganisationsFindBoxCollection(Factory);
				}
				return organisationList;
			}
		}
		OrganisationsFindBoxCollection organisationList;

		#endregion
	}
}
