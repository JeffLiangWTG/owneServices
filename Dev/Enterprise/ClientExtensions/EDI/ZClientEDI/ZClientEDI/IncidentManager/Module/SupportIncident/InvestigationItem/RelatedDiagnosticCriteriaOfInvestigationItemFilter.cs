using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Module;

public class RelatedDiagnosticCriteriaOfInvestigationItemFilter : ModuleGuidForeignCollectionFilter
{
	public RelatedDiagnosticCriteriaOfInvestigationItemFilter(ZString description, ModuleIdentifier moduleId, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, IBusinessObjectCollection list, Type parentBusinessObjectType)
		: base(description, moduleId, primaryKeyColumn, foreignKeyColumn, list, parentBusinessObjectType)
	{
	}

	public RelatedDiagnosticCriteriaOfInvestigationItemFilter(ZString description, ModuleIdentifier moduleId, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, GetList listDelegate, Type parentBusinessObjectType)
		: base(description, moduleId, primaryKeyColumn, foreignKeyColumn, listDelegate, parentBusinessObjectType)
	{
	}

	protected RelatedDiagnosticCriteriaOfInvestigationItemFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		: base(category, parentCollection)
	{
	}

	protected override ZQuery GetQueryForSelectedFiltersCore(FilterStripBusinessObject filterBusinessObject, ZQuery subModuleFilter)
	{
		var query = new ZDBOnlyQuery(typeof(InvestigationItem));
		var subQueryDiagnosticCriteria = new ZDBOnlySubQuery(typeof(IncidentDiagnosticCriteria), IncidentDiagnosticCriteriaSchema.PK,
			notIn: ComparisonOperator == ModuleTextFilter.ComparisonConstants.NoneMatch);

		subQueryDiagnosticCriteria.AddToFilter(subModuleFilter);

		var subQueryDiagnosticCriteriaInvestigationItemLink = new ZDBOnlySubQuery(typeof(DiagnosticCriteriaInvestigationItemLink), DiagnosticCriteriaInvestigationItemLinkSchema.DIL_INV_InvestigationItem);
		subQueryDiagnosticCriteriaInvestigationItemLink.AddSubQuery(DiagnosticCriteriaInvestigationItemLinkSchema.DIL_IMD_DiagnosticCriteria, subQueryDiagnosticCriteria, JoinCondition.And);

		query.AddSubQuery(subQueryDiagnosticCriteriaInvestigationItemLink, JoinCondition.And);
		return query;
	}
}
