using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceDatabaseFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			LocalFilterSubGroup localSubGroup = new LocalFilterSubGroup();
			var organisationSubGroup = new OrganisationFilterSubGroup();
			var clientCompanySubGroup = new ClientCompanyFilterSubGroup(localSubGroup);

			AddOrganisationFilters(filters, organisationSubGroup);
			AddStaffOwnerFilter(filters);
			AddTextFilters(filters);
			LicenceFilters.AddLicenceDatabaseFilters(filters, localSubGroup);
			LicenceFilters.AddClientCompanyFilters(filters, clientCompanySubGroup);
			new StlPriceListFilters().AddPriceListFilters(filters, new EdiPriceHeaderLinkFilterSubGroup(), new EdiLicenceSettingFilterSubGroup());
			LicenceFilters.AddPremiumServiceFilters(filters, new ClientPremiumServiceFilterSubGroup());
			AddFlagsFilters(filters);
			AddTokenAuthenticationFilter(filters);

			return filters;
		}

		#region Filters

		void AddTokenAuthenticationFilter(ModuleFilterCollection filters)
		{
			var filterDesc = ResString.GetMultilingualString("F4CBF195-66FD-495C-AA51-C90C016D73A0", "Token Authentication");
			var filter = filters.AddTextFilter(filterDesc.EnglishText, GetTokenAuthenticationStatusFilter, new TokenAuthenticationStatusList());
			filter.Category = FilterCategories.Other;
			filter.MultilingualDescription = filterDesc;
		}

		ZQuery GetTokenAuthenticationStatusFilter(ZString value)
		{
			if (value == TokenAuthenticationStatusList.Codes.Enabled)
			{
				return new ZQuery(LicenceDatabaseSchema.LD_TokenAuthenticationEnabled, true);
			}

			if (value == TokenAuthenticationStatusList.Codes.NotEnabled)
			{
				return new ZQuery(LicenceDatabaseSchema.LD_TokenAuthenticationEnabled, false);
			}

			return new ZQuery();
		}

		void AddOrganisationFilters(ModuleFilterCollection filters, OrganisationFilterSubGroup subgroup)
		{
			var orgFilter = filters.AddGuidFilter("Organisation", ModuleIDs.Organisation, OrgHeaderSchema.PK, OrganisationList);
			orgFilter.Category = FilterCategories.Organisations;
			orgFilter.SubGroup = subgroup;
			var orgNameFilter = filters.AddTextFilter("Organisation Name", OrgHeaderSchema.OH_FullName);
			orgNameFilter.Category = FilterCategories.Organisations;
			orgNameFilter.MaxLength = OrgHeaderSchema.OH_FullName.MaxLength;
			orgNameFilter.SubGroup = subgroup;
		}

		void AddStaffOwnerFilter(ModuleFilterCollection filters)
		{
			filters.AddNkFilter("Staff Owner", LicenceDatabaseSchema.LD_GS_NKOwner, ModuleIDs.GlbStaff, new GlbStaffCollection(Factory));
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter("Enterprise Code", ClientModuleRegistration.LicenceEnterprise, GetEnterpriseCodeQuery, new LicenceEnterpriseCollectionForEntCodeFilter(Factory));
			filters.AddGuidFilter("Enterprise ID", ClientModuleRegistration.LicenceEnterprise, GetEnterpriseCodeQuery, LicenceEnterpriseList).SupportsBlankComparisonOperators = false;
			filters.AddGuidFilter("Trusted System", ClientModuleRegistration.EdiTrustedSystem, LicenceDatabaseSchema.LD_ETS_TrustedSystem, new EdiTrustedSystemCollection(Factory));
		}

		void AddFlagsFilters(ModuleFilterCollection filters)
		{
			var enterpriseRegistrationFilter = filters.AddFlagsFilter("Databases Not registered with an Enterprise ID", new string[] { "Yes" },
				new GetFlagsQuery[] { GetEnterpriseRegistrationFilter });
			enterpriseRegistrationFilter.Category = FilterCategories.StatusAndFlags;
		}

		#endregion

		#region Lists

		#region Licence Filters

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

		#endregion

		#region Organisation List

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

		#region LicenceEnterprise List

		public LicenceEnterpriseCollection LicenceEnterpriseList
		{
			get
			{
				return new LicenceEnterpriseCollection(Factory);
			}
		}

		#endregion

		#endregion

		#region Queries

		#region Enterprise Code

		public static ZQuery GetEnterpriseCodeQuery(SQLComparisonOperator comparisonOperator, object pkValue)
		{
			var query = new ZDBOnlyQuery(typeof(LicenceDatabase));
			var enterpriseSubQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceDatabaseSchema.LD_LE);

			if (comparisonOperator == SQLComparisonOperator.IsBlank)
			{
				enterpriseSubQuery.AddToFilter(LicenceEnterpriseSchema.LE_EnterpriseCode, SQLComparisonOperator.Equal, ZString.Empty);
			}
			else if (comparisonOperator == SQLComparisonOperator.IsNotBlank)
			{
				enterpriseSubQuery.AddToFilter(LicenceEnterpriseSchema.LE_EnterpriseCode, SQLComparisonOperator.NotEqual, ZString.Empty);
			}
			else
			{
				enterpriseSubQuery.AddToFilter(LicenceEnterpriseSchema.PK, comparisonOperator, pkValue);
			}

			query.AddSubQuery(enterpriseSubQuery, JoinCondition.And);
			return query;
		}

		#region Enterprise Registration

		ZQuery GetEnterpriseRegistrationFilter(ZBool value)
		{
			if (value)
			{
				var query = new ZDBOnlyQuery(typeof(LicenceDatabase));
				var enterpriseSubQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceDatabaseSchema.LD_LE);
				enterpriseSubQuery.AddToFilter(LicenceEnterpriseSchema.LE_EnterpriseID, EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.Value);
				query.AddSubQuery(enterpriseSubQuery, JoinCondition.And);
				return query;
			}
			else
			{
				return new ZQuery();
			}
		}

		#endregion

		#endregion

		#endregion

		#region Sub Groups

		#region database

		class LocalFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery licenceDatabaseQuery = new ZDBOnlyQuery(typeof(LicenceDatabase));
				licenceDatabaseQuery.AddToFilter(filter);
				return licenceDatabaseQuery;
			}
		}

		class ClientCompanyFilterSubGroup : ModuleFilterSubGroup
		{
			public ClientCompanyFilterSubGroup(ModuleFilterSubGroup parent)
					: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(LicenceDatabase));
				ZDBOnlySubQuery clientCompanySubQuery = new ZDBOnlySubQuery(typeof(ClientCompany), ClientCompanySchema.LCC_LD);
				clientCompanySubQuery.AddToFilter(filter);
				query.AddSubQuery(clientCompanySubQuery, JoinCondition.And);
				return query;
			}
		}

		#endregion

		#region organisation

		class OrganisationFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(LicenceDatabase));
				var enterpriseSubQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceDatabaseSchema.LD_LE);
				var orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), LicenceEnterpriseSchema.LE_OH);
				orgSubQuery.AddToFilter(filter);
				enterpriseSubQuery.AddSubQuery(orgSubQuery, JoinCondition.And);
				query.AddSubQuery(enterpriseSubQuery, JoinCondition.And);
				return query;
			}
		}

		#endregion

		public class ClientPremiumServiceFilterSubGroup : ModuleFilterSubGroup
		{
			public ClientPremiumServiceFilterSubGroup()
			{ }

			public ClientPremiumServiceFilterSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(LicenceDatabase));
				var clientPremiumServiceSubQuery = new ZDBOnlySubQuery(typeof(ClientPremiumService), ClientPremiumServiceSchema.CPS_LD);
				clientPremiumServiceSubQuery.AddToFilter(filter);
				query.AddSubQuery(clientPremiumServiceSubQuery, JoinCondition.And);
				return query;
			}
		}

		public class EdiLicenceSettingFilterSubGroup : ModuleFilterSubGroup
		{
			public EdiLicenceSettingFilterSubGroup()
			{ }

			public EdiLicenceSettingFilterSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(LicenceDatabase));
				var ediLicenceSettingSubQuery = new ZDBOnlySubQuery(typeof(EdiLicenceSetting), EdiLicenceSettingSchema.LS9_LD);
				ediLicenceSettingSubQuery.AddToFilter(filter);
				query.AddSubQuery(ediLicenceSettingSubQuery, JoinCondition.And);
				return query;
			}
		}

		public class EdiPriceHeaderLinkFilterSubGroup : ModuleFilterSubGroup
		{
			public EdiPriceHeaderLinkFilterSubGroup()
			{ }

			public EdiPriceHeaderLinkFilterSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(LicenceDatabase));
				var ediPriceHeaderLinkSubQuery = new ZDBOnlySubQuery(typeof(EdiPriceHeaderLink), EdiPriceHeaderLinkSchema.PHL_LD);
				ediPriceHeaderLinkSubQuery.AddToFilter(filter);
				query.AddSubQuery(ediPriceHeaderLinkSubQuery, JoinCondition.And);
				return query;
			}
		}

		#endregion
	}
}
