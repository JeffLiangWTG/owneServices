using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class IncidentTriageChecklistItemFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddFlagFilters(filters);
			return filters;
		}

		#region Text Filters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Response Type", IncidentTriageChecklistItemSchema.IMC_ResponseType);
			filters.AddTextFilter("Checklist Description", IncidentTriageChecklistItemSchema.IMC_SupportDescription);
			filters.AddTextFilter("Checklist Category", IncidentTriageChecklistItemSchema.IMC_Category, Lookups.Categorys);
		}

		#endregion

		#region Flag Filters

		void AddFlagFilters(ModuleFilterCollection filters)
		{
			var isChecklistPublishedFlag = ResString.GetMultilingualString("6ef51a7c-a76e-40aa-b9be-8cfb6e1db665", "Is Checklist Published");
			filters.AddFlagFilter("Is Checklist Published", isChecklistPublishedFlag, IncidentTriageChecklistItemSchema.IMC_IsPublished, ModuleFilterSubGroup.Default)
				.MultilingualDescription = isChecklistPublishedFlag;
		}

		#endregion

		protected IncidentTriageChecklistItemLookups Lookups
		{
			get { return lookups ?? (lookups = GetNewLookups()); }
		}

		protected IncidentTriageChecklistItemLookups GetNewLookups()
		{
			return new IncidentTriageChecklistItemLookups(Factory);
		}

		IncidentTriageChecklistItemLookups lookups;

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			return new ModuleFountainFilter("Checklist Number", IncidentTriageChecklistItemSchema.IMC_ChecklistNumber, "TRC");
		}
	}
}
