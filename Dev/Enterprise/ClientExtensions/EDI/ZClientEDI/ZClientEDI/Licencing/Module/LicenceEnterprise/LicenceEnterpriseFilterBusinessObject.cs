using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module
{
	public class LicenceEnterpriseFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var filter = filters.AddTextFilter("Enterprise Code", LicenceEnterpriseSchema.LE_EnterpriseCode);
			filter.IsCommon = true;

			filter = filters.AddTextFilter("Organisation Code", GetOrgCodeQuery);
			filter.IsCommon = true;
			filter.MaxLength = OrgHeaderSchema.OH_Code.MaxLength;

			filter = filters.AddTextFilter("Organisation Name", GetOrgNameQuery);
			filter.MaxLength = OrgHeaderSchema.OH_FullName.MaxLength;
			filter.IsCommon = true;

			filter = filters.AddTextFilter("Release Ring", GetReleaseRingQuery, ReleaseRings);
			filter.MaxLength = LicenceDatabaseSchema.LD_ReleaseRing.MaxLength;

			filters.AddTextFilter("Token Authentication", GetTokenAuthenticationStatusFilter, new TokenAuthenticationStatusList());
			return filters;
		}

		ZQuery GetTokenAuthenticationStatusFilter(ZString value)
		{
			if (value == TokenAuthenticationStatusList.Codes.Enabled)
			{
				return new ZQuery(LicenceEnterpriseSchema.LE_TokenAuthenticationEnabled, true);
			}

			if (value == TokenAuthenticationStatusList.Codes.NotEnabled)
			{
				return new ZQuery(LicenceEnterpriseSchema.LE_TokenAuthenticationEnabled, false);
			}

			return new ZQuery();
		}

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			var filter = new ModuleTextFilter("Enterprise ID", LicenceEnterpriseSchema.LE_EnterpriseID);
			filter.IsCommon = true;
			return filter;
		}

		ZQuery GetOrgCodeQuery(SQLComparisonOperator @operator, ZString value)
		{
			ZQuery query = new ZQuery();
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(LicenceEnterprise));
			ZDBOnlySubQuery orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), LicenceEnterpriseSchema.LE_OH);
			orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.OH_Code, @operator, value);

			ZDBOnlySubQuery orgHeaderLicCompanySubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), LicenceCompanySchema.LC_OH);
			orgHeaderLicCompanySubQuery.AddToFilter(OrgHeaderSchema.OH_Code, @operator, value);
			ZDBOnlySubQuery licCompanyQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceCompanySchema.LC_LE);
			licCompanyQuery.AddSubQuery(orgHeaderLicCompanySubQuery, JoinCondition.And);

			result.AddSubQuery(licCompanyQuery, JoinCondition.Or);
			result.AddSubQuery(orgHeaderSubQuery, JoinCondition.Or);
			query.AddToFilter(result);

			return query;
		}

		ZQuery GetOrgNameQuery(SQLComparisonOperator @operator, ZString value)
		{
			ZQuery query = new ZQuery();
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(LicenceEnterprise));
			ZDBOnlySubQuery orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), LicenceEnterpriseSchema.LE_OH);
			orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.OH_FullName, @operator, value);

			ZDBOnlySubQuery orgHeaderLicCompanySubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), LicenceCompanySchema.LC_OH);
			orgHeaderLicCompanySubQuery.AddToFilter(OrgHeaderSchema.OH_FullName, @operator, value);
			ZDBOnlySubQuery licCompanyQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceCompanySchema.LC_LE);
			licCompanyQuery.AddSubQuery(orgHeaderLicCompanySubQuery, JoinCondition.And);

			result.AddSubQuery(licCompanyQuery, JoinCondition.Or);
			result.AddSubQuery(orgHeaderSubQuery, JoinCondition.Or);
			query.AddToFilter(result);

			return query;
		}

		ZQuery GetReleaseRingQuery(ZString releaseRing)
		{
			ZQuery query = new ZQuery();
			if (releaseRing != "ALL")
			{
				ZDBOnlyQuery enterpriseQuery = new ZDBOnlyQuery(typeof(LicenceEnterprise));
				ZDBOnlySubQuery databaseSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.LD_LE);
				databaseSubQuery.AddToFilter(LicenceDatabaseSchema.LD_ReleaseRing, releaseRing);
				enterpriseQuery.AddSubQuery(databaseSubQuery, JoinCondition.And);
				query.AddToFilter(enterpriseQuery);
			}

			return query;
		}

		public ReleaseRingsList ReleaseRings
		{
			get
			{
				if (fReleaseRings == null)
				{
					fReleaseRings = new ReleaseRingsList();

					CodeDescriptionPair aLLOption = new CodeDescriptionPair("ALL", "All Releases");
					fReleaseRings.Insert(0, aLLOption);
				}
				return fReleaseRings;
			}
		}

		ReleaseRingsList fReleaseRings;
	}
}
