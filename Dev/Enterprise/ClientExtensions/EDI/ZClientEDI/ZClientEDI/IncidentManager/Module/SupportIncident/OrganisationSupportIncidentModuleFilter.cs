using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class OrganisationSupportIncidentModuleFilter : ModuleGuidForeignCollectionFilter
	{
		readonly SupportIncidentFilterBusinessObject parent;

		public OrganisationSupportIncidentModuleFilter(SupportIncidentFilterBusinessObject parent, ZString description, ModuleIdentifier moduleID, SchemaGuidColumn primaryKeyColumn, BusinessObjectFactory factory, Type parentBusinessObjectType)
			: base(description, moduleID, primaryKeyColumn, OrgHeaderSchema.PK, () => new OrgHeaderCollection(factory), parentBusinessObjectType)
		{
			this.parent = parent;
		}

		public override IReadOnlyList<string> AllowedComparisonOperators => new[]
		{
			string.Empty,
			ModuleTextFilter.ComparisonConstants.Exact,
			ModuleTextFilter.ComparisonConstants.NotEqual,
			ModuleTextFilter.ComparisonConstants.IsBlank,
			ModuleTextFilter.ComparisonConstants.IsNotBlank,
			ModuleTextFilter.ComparisonConstants.AnyMatch,
			ModuleTextFilter.ComparisonConstants.NoneMatch
		};

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var query = base.GetQueryUsingFilterColumns();
			if (ComparisonOperator == ModuleTextFilter.ComparisonConstants.Exact || ComparisonOperator == ModuleTextFilter.ComparisonConstants.NotEqual)
			{
				// Clean filters when no organisation is selected, instead filter by empty guid
				if (Property == ZGuid.Empty)
				{
					query = new ZQuery();
				}
			}
			return query;
		}

		protected override ZQuery GetQueryForSelectedFiltersCore(FilterStripBusinessObject filterBusinessObject, ZQuery subModuleFilter)
		{
			var subQueryOrgHeader = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK, ComparisonOperator == ModuleTextFilter.ComparisonConstants.NoneMatch);
			subQueryOrgHeader.AddToFilter(subModuleFilter);

			ModuleFlagsFilter clientManagementGroupFilter = null;
			foreach (ModuleFlagsFilter filter in parent.GetActiveFiltersByDescription(SupportIncidentFilterBusinessObject.FilterDescriptionConstants.ClientManagementGroup))
			{
				clientManagementGroupFilter = filter;
				break;
			}

			if (clientManagementGroupFilter != null && clientManagementGroupFilter.Property0)
			{
				return GetMatchingClientOrMatchingManagementGroupQuery(subQueryOrgHeader);
			}
			else
			{
				var query = new ZDBOnlyQuery(typeof(SupportIncident));
				query.AddSubQuery(IncidentMainSchema.IM_OH_Client, subQueryOrgHeader, JoinCondition.And);
				return query;
			}
		}

		ZDBOnlyQuery GetMatchingClientOrMatchingManagementGroupQuery(ZDBOnlySubQuery subQueryOrgHeader)
		{
			var matchingOrgOrInManagementGroupQuery = new ZDBOnlyQuery(typeof(ProfessionalServicesQuote));

			var managementGroupQuery = new ZDBOnlySubQuery(typeof(OrgRelatedParty), OrgRelatedPartySchema.PR_OH_Parent);
			managementGroupQuery.AddSubQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, subQueryOrgHeader, JoinCondition.And);
			managementGroupQuery.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.ManagementGrouping);
			managementGroupQuery.AddToFilter(OrgRelatedPartySchema.PR_FreightDirection, RelatedPartyDirectionList.Codes.Forwarder);

			matchingOrgOrInManagementGroupQuery.AddSubQuery(IncidentMainSchema.IM_OH_Client, subQueryOrgHeader, JoinCondition.And);
			matchingOrgOrInManagementGroupQuery.AddSubQuery(IncidentMainSchema.IM_OH_Client, managementGroupQuery, JoinCondition.Or);

			return matchingOrgOrInManagementGroupQuery;
		}
	}
}
