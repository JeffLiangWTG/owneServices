using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class InvestigationItemFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddFlagFilters(filters);
			AddDiagnosticCriteriaFilter(filters);
			return filters;
		}

		#region Text Filters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Description", InvestigationItemSchema.INV_Description);
			filters.AddTextFilter("Type", InvestigationItemSchema.INV_Type, Lookups.Types);
			filters.AddTextFilter("Item Text", InvestigationItemSchema.INV_ItemText);
		}

		#endregion

		#region Flag Filters

		void AddFlagFilters(ModuleFilterCollection filters)
		{
			var askClient = ResString.GetMultilingualString("4f587e8f-fc3a-47be-8996-4ab3ca87f8c3", "Ask the Client");
			filters.AddFlagFilter("Ask the Client", askClient, InvestigationItemSchema.INV_AskClient, ModuleFilterSubGroup.Default)
				.MultilingualDescription = askClient;
		}

		#endregion

		#region  Incident Diagnostic Criteria Filters

		void AddDiagnosticCriteriaFilter(ModuleFilterCollection filters)
		{
			var diagnosticCriteriaFilter = new RelatedDiagnosticCriteriaOfInvestigationItemFilter("DiagnosticCriteria", ClientModuleRegistration.IncidentDiagnosticCriteria, DiagnosticCriteriaInvestigationItemLinkSchema.PK, DiagnosticCriteriaInvestigationItemLinkSchema.DIL_IMD_DiagnosticCriteria, new IncidentDiagnosticCriteriaCollection(Factory), typeof(InvestigationItem));
			diagnosticCriteriaFilter.MultilingualDescription = ResString.GetMultilingualString("ce8e9260-58fe-4759-bbfd-e7623a6b1f03", "Related Diagnostic Criteria");
			diagnosticCriteriaFilter.Category = new FilterCategory((NoResString)"Related Items");
			filters.AddFilter(diagnosticCriteriaFilter);
		}

		#endregion

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			return new ModuleFountainFilter("Investigation Item ID", InvestigationItemSchema.INV_ItemNumber, "INV");
		}

		protected InvestigationItemLookups Lookups
		{
			get { return lookups ?? (lookups = GetNewLookups()); }
		}

		protected InvestigationItemLookups GetNewLookups()
		{
			return new InvestigationItemLookups(Factory);
		}

		InvestigationItemLookups lookups;
	}
}
