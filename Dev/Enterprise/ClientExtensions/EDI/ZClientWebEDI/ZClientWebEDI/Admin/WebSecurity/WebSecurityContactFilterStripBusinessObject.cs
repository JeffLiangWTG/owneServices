using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class WebSecurityContactFilterStripBusinessObject : MyAccountContactFilterStripBusinessObject
	{
		public WebSecurityContactFilterStripBusinessObject(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			var securityFilter = filters.AddTextFilter(FilterDescription.Security, GetSecurityQuery, GetSecurityList);
			securityFilter.MultilingualDescription = ResString.GetMultilingualString("1a2b01af-5b70-446d-8924-f1877bd8b294", "Security");
			securityFilter.Category = FilterCategories.Other;
			RemoveComparisonOperators(securityFilter);

			#region AlwaysAppliedAndHidden

			var webEnabledFilter = filters.AddFlagsFilter(FilterDescription.WebAccessEnabled, new[] { FilterDescription.WebAccessEnabled }, new[] { OrgContactSchema.OC_WebAccessEnabled });
			webEnabledFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			webEnabledFilter.DefaultProperties[FilterDescription.WebAccessEnabled] = true;

			#endregion AlwaysAppliedAndHidden

			return filters;
		}

		#region Lists

		CodeDescriptionPairList GetSecurityList() => MasterOrg.Factory.GetCachedValue("WebSecurityContactFilter.SecurityList", () =>
		{
			var list = new CodeDescriptionPairList();

			foreach (var security in MasterOrg.SecurityRights.OfType<OrgSecurity>().Where(x => x.OX_IsCustomerManaged))
			{
				list.AddPair(security.SecurityKey, security.SecurityItemNameForDisplay);
			}

			return list;
		});

		#endregion Lists

		#region Security

		ZQuery GetSecurityQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var security = MasterOrg.SecurityRightsView.OfType<OrgSecurity>().FirstOrDefault(x => x.OX_SecurityItemName.EqualsIgnoringCase(value));
			if (security == null)
			{
				return new ZQuery() { IsNoResultQuery = true };
			}

			var result = new ZDBOnlyQuery(typeof(OrgContact));
			result.AddToFilter(OrgContactSchema.OC_OH, MasterOrg.PK);

			var inCondition = comparisonOperator == SQLComparisonOperator.Equal ? "in" : "not in";

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@OrgPk", MasterOrg.PK, OrgHeaderSchema.PK);
			parameters.Add("@SecurityName", value, OrgSecuritySchema.OX_SecurityItemName);

			if (security.IsGrantedByDefault)
			{
				//secutiry right is granted by default
				//contact security right is granted if
				//1 - no org and no contact level override
				//2 - has org level granted and no contact level override
				//3 - has contact level granted

				result.AddFilterAndZSQLParameterCollection(
$@"OC_PK {inCondition}
(
	select OC_PK 
	from dbo.OrgContact
	where
		OC_OH = @OrgPk
		and
		(
			not exists
			(
				select 1 
				from 
					dbo.OrgSecurity
				where 
					OX_OH = @OrgPk
					and OX_SecurityItemName = @SecurityName 
			)
			or exists 
			(
				select 1 
				from 
					dbo.OrgSecurity 
					left join dbo.OrgSecurityContacts on OZ_OX = OX_PK and OZ_OC = OC_PK 
				where 
					OX_OH = @OrgPk
					and OX_SecurityItemName = @SecurityName 
					and OX_Granted = 1 
					and OZ_PK IS NULL
			)
			or exists 
			(
				select 1 
				from
					dbo.OrgSecurity 
					join dbo.OrgSecurityContacts on OZ_OX = OX_PK and OZ_OC = OC_PK
				where
					OX_OH = @OrgPk
					and OX_SecurityItemName = @SecurityName 
					and OZ_Granted = 1
			)
		)
)
", parameters);

				return result;
			}
			else
			{
				//secutiry right is not granted by default
				//contact security right is granted if
				//1 - has org level granted and no contact level override
				//2 - has contact level granted

				result.AddFilterAndZSQLParameterCollection(
$@"OC_PK {inCondition}
(
	select OC_PK 
	from dbo.OrgContact
	where
		OC_OH = @OrgPk
		and
		(
			exists 
			(
				select 1 
				from 
					dbo.OrgSecurity 
					left join dbo.OrgSecurityContacts on OZ_OX = OX_PK and OZ_OC = OC_PK 
				where 
					OX_OH = @OrgPk
					and OX_SecurityItemName = @SecurityName 
					and OX_Granted = 1 
					and OZ_PK IS NULL
			)
			or exists 
			(
				select 1 
				from
					dbo.OrgSecurity 
					join dbo.OrgSecurityContacts on OZ_OX = OX_PK and OZ_OC = OC_PK
				where
					OX_OH = @OrgPk
					and OX_SecurityItemName = @SecurityName 
					and OZ_Granted = 1
			)
		)
)
", parameters);
				return result;
			}
		}

		#endregion Security
	}
}
