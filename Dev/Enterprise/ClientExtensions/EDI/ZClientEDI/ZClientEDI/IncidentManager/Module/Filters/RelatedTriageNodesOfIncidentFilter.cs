using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Module;

public class RelatedTriageNodesOfIncidentFilter : ModuleGuidForeignCollectionFilter
{
	public RelatedTriageNodesOfIncidentFilter(ZString description, ModuleIdentifier moduleId, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, IBusinessObjectCollection list, Type parentBusinessObjectType)
		: base(description, moduleId, primaryKeyColumn, foreignKeyColumn, list, parentBusinessObjectType)
	{
	}

	public RelatedTriageNodesOfIncidentFilter(ZString description, ModuleIdentifier moduleId, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, GetList listDelegate, Type parentBusinessObjectType)
		: base(description, moduleId, primaryKeyColumn, foreignKeyColumn, listDelegate, parentBusinessObjectType)
	{
	}

	protected RelatedTriageNodesOfIncidentFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		: base(category, parentCollection)
	{
	}

	protected override ZQuery GetQueryForSelectedFiltersCore(FilterStripBusinessObject filterBusinessObject, ZQuery subModuleFilter)
	{
		var query = new ZDBOnlyQuery(typeof(SupportIncident));

		if (ComparisonOperator != ModuleTextFilter.ComparisonConstants.AllMatch || filterBusinessObject.ActiveModuleFilters.Count != 0)
		{
			var notIn = ComparisonOperator == ModuleTextFilter.ComparisonConstants.NoneMatch;

			var subQueryDiagnosticCriteria = new ZDBOnlySubQuery(typeof(IncidentTriage), IncidentTriageSchema.PK, notIn: notIn);
			subQueryDiagnosticCriteria.AddToFilter(subModuleFilter);

			query.AddSubQuery(IncidentMainSchema.IM_IMT_Triage, subQueryDiagnosticCriteria, JoinCondition.And);

			if (notIn)
			{
				query.AddToFilter(JoinCondition.Or, IncidentMainSchema.IM_IMT_Triage, null);
			}
		}

		return query;
	}
}
