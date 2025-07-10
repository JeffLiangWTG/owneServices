using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Module;

public class IncidentDiagnosticCriteriaModuleFilter : ModuleGuidForeignCollectionFilter
{
	public IncidentDiagnosticCriteriaModuleFilter(ZString description, ModuleIdentifier moduleId, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, IBusinessObjectCollection list, Type parentBusinessObjectType)
		: base(description, moduleId, primaryKeyColumn, foreignKeyColumn, list, parentBusinessObjectType)
	{
	}

	public IncidentDiagnosticCriteriaModuleFilter(ZString description, ModuleIdentifier moduleId, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, GetList listDelegate, Type parentBusinessObjectType)
		: base(description, moduleId, primaryKeyColumn, foreignKeyColumn, listDelegate, parentBusinessObjectType)
	{
	}

	protected IncidentDiagnosticCriteriaModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		: base(category, parentCollection)
	{
	}

	protected override ZQuery GetQueryForSelectedFiltersCore(FilterStripBusinessObject filterBusinessObject, ZQuery subModuleFilter)
	{
		var query = new ZDBOnlyQuery(typeof(IncidentTriage));
		var subQueryDiagnosticCriteria = new ZDBOnlySubQuery(typeof(IncidentDiagnosticCriteria), IncidentDiagnosticCriteriaSchema.PK,
			notIn: ComparisonOperator == ModuleTextFilter.ComparisonConstants.NoneMatch);

		subQueryDiagnosticCriteria.AddToFilter(subModuleFilter);

		var subQueryIncidentTriageDiagnosticCriteriaPivot = new ZDBOnlySubQuery(typeof(IncidentTriageDiagnosticCriteriaPivot), IncidentTriageDiagnosticCriteriaPivotSchema.IMO_IMT_Triage);
		subQueryIncidentTriageDiagnosticCriteriaPivot.AddSubQuery(IncidentTriageDiagnosticCriteriaPivotSchema.IMO_IMD_DiagnosticCriteria, subQueryDiagnosticCriteria, JoinCondition.And);

		query.AddSubQuery(subQueryIncidentTriageDiagnosticCriteriaPivot, JoinCondition.And);
		return query;
	}
}
