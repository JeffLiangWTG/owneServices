using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MarketingManager.GUI
{
	public class OrganisationModuleFilter : ModuleGuidForeignCollectionFilter
	{
		protected OrganisationModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public OrganisationModuleFilter(ModuleIdentifier moduleID, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn organisationKeyColumn, IBusinessObjectCollection list, Type parentBusinessObjectType)
			: base(moduleID.Description.GetUnresolvedString() + " (Multiple)", moduleID, primaryKeyColumn, OrgHeaderSchema.PK, list, parentBusinessObjectType)
		{
			this.organisationKeyColumn = organisationKeyColumn;
		}

		public OrganisationModuleFilter(ModuleIdentifier moduleID, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn organisationKeyColumn, BusinessObjectFactory factory, Type parentBusinessObjectType)
			: base(moduleID.Description.GetUnresolvedString() + " (Multiple)", moduleID, primaryKeyColumn, OrgHeaderSchema.PK, () => new OrgHeaderCollection(factory), parentBusinessObjectType)
		{
			this.organisationKeyColumn = organisationKeyColumn;
		}

		readonly SchemaGuidColumn organisationKeyColumn;

		public override IReadOnlyList<string> AllowedComparisonOperators => new[]
		{
			string.Empty,
			ModuleTextFilter.ComparisonConstants.AnyMatch,
			ModuleTextFilter.ComparisonConstants.NoneMatch
		};

		protected override ZQuery GetQueryForSelectedFiltersCore(FilterStripBusinessObject filterBusinessObject, ZQuery subModuleFilter)
		{
			var query = GetNewQueryForSelectedFilters();
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK, ComparisonOperator == ModuleTextFilter.ComparisonConstants.NoneMatch);

			subQuery.AddToFilter(subModuleFilter);

			query.AddSubQuery(organisationKeyColumn, subQuery, JoinCondition.And);
			return query;
		}
	}
}
