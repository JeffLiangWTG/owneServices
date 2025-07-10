using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Business.Res;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceKeyFilterBusinessObject : FilterStripBusinessObject
	{
		LicenceFilters LicenceFilters
		{
			get
			{
				if (licenceFilters == null)
				{
					licenceFilters = new LicenceFilters(this, StatusActive, StatusInactive, StatusAll);
				}

				return licenceFilters;
			}
		}
		LicenceFilters licenceFilters;

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddLicenceFilters(filters);
			AddLocationFilters(filters);

			ModuleFilter filter = filters.AddTextFilter("Organisation Active", GetOrgActiveQuery, OrganisationActiveStatusList);
			filter.Category = FilterCategories.Organisations;

			LicenceFilters.AddCoreFilters(filters, null);

			return filters;
		}

		#endregion

		#region Filters Sub Groups

		class LicenceDatabaseFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery licenceQuery = new ZDBOnlyQuery(typeof(LicenceHeader));
				ZDBOnlySubQuery databaseSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceHeaderSchema.LA_LD);
				databaseSubQuery.AddToFilter(filter);
				licenceQuery.AddSubQuery(databaseSubQuery, JoinCondition.And);
				return licenceQuery;
			}
		}

		class LicenceModuleFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery licenceQuery = new ZDBOnlyQuery(typeof(LicenceHeader));
				ZDBOnlySubQuery moduleSubQuery = new ZDBOnlySubQuery(typeof(LicenceModules), LicenceModulesSchema.LM_LA);
				moduleSubQuery.AddToFilter(filter);
				licenceQuery.AddSubQuery(moduleSubQuery, JoinCondition.And);
				return licenceQuery;
			}
		}

		#endregion

		#region Version and Licence Filters

		void AddLicenceFilters(ModuleFilterCollection filters)
		{
			LicenceDatabaseFilterSubGroup databaseSubGroup = new LicenceDatabaseFilterSubGroup();
			LicenceModuleFilterSubGroup moduleSubGroup = new LicenceModuleFilterSubGroup();

			ModuleFilter filter = filters.AddTextFilter("Licence Company Code", LicenceFilters.GetLicenceCompanyCodeQuery);
			filter.Category = LicenceFilters.VersionAndLicence;
			filter.MaxLength = LicenceCompanySchema.LC_CompanyCode.MaxLength;

			LicenceFilters.AddLicenceHeaderFilters(filters, null);
			LicenceFilters.AddLicenceDatabaseFilters(filters, databaseSubGroup);
			LicenceFilters.AddLicenceModuleFilters(filters, moduleSubGroup);
		}

		#endregion

		#region Location Filters

		public LocationCollection Locations
		{
			get { return new LocationCollection(Factory, Env.Security.OrganisationAllowSearchOutsideLoginCountry.IsAllowed); }
		}

		public RefCountryCollection Countries
		{
			get
			{
				return new RefCountryCollection(Factory);
			}
		}

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			ModuleNkFilter closestPortFilter = filters.AddNkFilter(OrgConstants.FilterControl.UNLOCOType.OrgPort, GetMainUNLOCOQuery, ModuleIDs.Location, Locations);
			closestPortFilter.Category = FilterCategories.Locations;
			closestPortFilter.MultilingualDescription = ResString.GetMultilingualString("LicenceKeyFilter|OrgPort", "Main UNLOCO");

			if (!Env.Security.OrganisationAllowSearchOutsideLoginCountry.IsAllowed)
			{
				closestPortFilter.Visibility = FilterVisibility.AlwaysVisible;
				closestPortFilter.DefaultProperty = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				closestPortFilter.PropertyValidation = ClosestPortCountryFilterValidation;
			}

			ModuleNkFilter companyCountryFilter = filters.AddNkFilter("Licence Country", GetCompanyCountryQuery, ModuleIDs.RefCountry, Countries);
			companyCountryFilter.Category = FilterCategories.Locations;
		}

		void ClosestPortCountryFilterValidation(ZPropertyInfo info)
		{
			if (!((ZString)info.Value).StartsWith(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, System.StringComparison.OrdinalIgnoreCase))
			{
				var errorMessage = Res.GetString("613ee18b-0059-4248-b5a2-2958b9b6f7c7", @"Your current security rights only allow you to view organizations based in your current login country/region ({0}).
If you think this is incorrect, please contact your {1} administrator.", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, BrandingFactory.Instance.ProductName);
				info.AddError(errorMessage);
			}
		}

		protected ZQuery GetMainUNLOCOQuery(ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(LicenceHeader));
			ZDBOnlySubQuery companySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceHeaderSchema.LA_LC);
			ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), LicenceCompanySchema.LC_OH);
			orgSubQuery.AddToFilter(LocationHelper.GetLocationFilter(Factory, value, OrgHeaderSchema.OH_RL_NKClosestPort, typeof(OrgHeader)));
			companySubQuery.AddSubQuery(orgSubQuery, JoinCondition.And);
			query.AddSubQuery(companySubQuery, JoinCondition.And);
			return query;
		}

		protected ZQuery GetCompanyCountryQuery(ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(LicenceHeader));
			ZDBOnlySubQuery companySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceHeaderSchema.LA_LC);
			companySubQuery.AddToFilter(LicenceCompanySchema.LC_CompanyCountry, value);
			query.AddSubQuery(companySubQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region Code Filters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter("Organisation", ModuleIDs.Organisation, LicenceFilters.GetOrgPKQuery, OrganisationList);
			filters.AddTextFilter("Organisation Name", LicenceFilters.GetOrgNameQuery)
				.WithMaxLengthOf<ModuleTextFilter>(OrgHeaderSchema.OH_FullName);
			filters.AddGuidFilter("Enterprise Code", ClientModuleRegistration.LicenceEnterprise, LicenceFilters.GetEnterprisePKQuery, new LicenceEnterpriseCollectionForEntCodeFilter(Factory));
			filters.AddGuidFilter("Enterprise ID", ClientModuleRegistration.LicenceEnterprise, LicenceFilters.GetEnterprisePKQuery, LicenceEnterpriseList);
			filters.AddGuidFilter("Database Code", ClientModuleRegistration.LicenceDatabase, LicenceHeaderSchema.LA_LD, LicenceDatabaseList);

			filters.AddTextFilter("Tag/Note", GetTagNoteQuery)
				.WithMaxLengthOf<ModuleTextFilter>(ClientLicenceHeaderExSchema.L0_TagNote);
		}

		ZQuery GetOrgActiveQuery(ZString orgActive)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(LicenceHeader));
			ZDBOnlySubQuery companySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceHeaderSchema.LA_LC);
			ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), LicenceCompanySchema.LC_OH);
			LicenceFilters.AddStatusFilter(orgSubQuery, OrgHeaderSchema.OH_IsActive, orgActive);
			companySubQuery.AddSubQuery(orgSubQuery, JoinCondition.And);
			query.AddSubQuery(companySubQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetTagNoteQuery(SQLComparisonOperator comparisonOperator, ZString note)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(LicenceHeader));
			var exSubQuery = new ZDBOnlySubQuery(typeof(ClientLicenceHeaderEx), ClientLicenceHeaderExSchema.L0_LA);
			exSubQuery.AddToFilter(ClientLicenceHeaderExSchema.L0_TagNote, comparisonOperator, note);
			query.AddSubQuery(exSubQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region Lookups

		#region Organisations

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

		public virtual CodeDescriptionPairList OrganisationActiveStatusList
		{
			get
			{
				if (organisationActiveStatusList == null)
				{
					organisationActiveStatusList = new CodeDescriptionPairList();
					organisationActiveStatusList.AddPair(StatusActive, LicenceFilters.OrganisationActiveStringYes);
					organisationActiveStatusList.AddPair(StatusInactive, LicenceFilters.OrganisationActiveStringNo);
				}
				return organisationActiveStatusList;
			}
		}
		CodeDescriptionPairList organisationActiveStatusList;

		#region Licence Databases

		public LicenceDatabaseNonDependentCollection LicenceDatabaseList
		{
			get
			{
				if (licenceDatabaseList == null)
				{
					licenceDatabaseList = new LicenceDatabaseNonDependentCollection(Factory);
				}
				return licenceDatabaseList;
			}
		}
		LicenceDatabaseNonDependentCollection licenceDatabaseList;

		#endregion

		#region Licence Enterprises

		public LicenceEnterpriseCollection LicenceEnterpriseList
		{
			get
			{
				if (licenceEnterpriseList == null)
				{
					licenceEnterpriseList = new LicenceEnterpriseCollection(Factory);
				}
				return licenceEnterpriseList;
			}
		}
		LicenceEnterpriseCollection licenceEnterpriseList;
		#endregion

		#endregion
	}
}
