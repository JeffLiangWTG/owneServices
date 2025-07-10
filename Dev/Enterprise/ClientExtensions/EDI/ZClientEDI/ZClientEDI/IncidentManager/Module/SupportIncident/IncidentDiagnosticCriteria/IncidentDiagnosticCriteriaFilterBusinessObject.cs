using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class IncidentDiagnosticCriteriaFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddIncidentTriageFilter(filters);
			return filters;
		}

		#region Text Filters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Description", IncidentDiagnosticCriteriaSchema.IMD_Description);
			filters.AddTextFilter("Type", IncidentDiagnosticCriteriaSchema.IMD_Type, Lookups.Types);
			filters.AddTextFilter("Keywords", IncidentDiagnosticCriteriaSchema.IMD_Keywords);
			filters.AddTextFilter("Question", IncidentDiagnosticCriteriaSchema.IMD_Question);
		}

		#endregion

		#region Incident Triage Filters

		void AddIncidentTriageFilter(ModuleFilterCollection filters)
		{
			var incidentTriageFilter = new IncidentTriageModuleFilter("IncidentTriage", ClientModuleRegistration.IncidentTriage, IncidentTriageDiagnosticCriteriaPivotSchema.PK, IncidentTriageDiagnosticCriteriaPivotSchema.IMO_IMT_Triage, new IncidentTriageCollection(Factory), typeof(IncidentDiagnosticCriteria));
			incidentTriageFilter.MultilingualDescription = ResString.GetMultilingualString("3ada92eb-7cab-464e-a8c8-0936304f3c63", "Triage Nodes");
			incidentTriageFilter.Category = new FilterCategory((NoResString)"Related Items");
			filters.AddFilter(incidentTriageFilter);
		}

		#endregion

		protected IncidentDiagnosticCriteriaLookups Lookups
		{
			get { return lookups ?? (lookups = GetNewLookups()); }
		}

		protected IncidentDiagnosticCriteriaLookups GetNewLookups()
		{
			return new IncidentDiagnosticCriteriaLookups(Factory);
		}

		IncidentDiagnosticCriteriaLookups lookups;
	}
}
