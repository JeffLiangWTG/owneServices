using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class IncidentTriageFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddFlagFilters(filters);
			AddGuidFilters(filters);
			AddDiagnosticCriteriaFilter(filters);
			return filters;
		}

		#region Text Filters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Triage Description", IncidentTriageSchema.IMT_SupportDescription);
			filters.AddTextFilter("Type", IncidentTriageSchema.IMT_Type, Lookups.Types);
			filters.AddTextFilter("Level", IncidentTriageSchema.IMT_Level, Lookups.Levels);
			filters.AddTextFilter("Product Area", IncidentTriageSchema.IMT_ProductArea, Lookups.ProductAreaList);
			filters.AddTextFilter("Sec. / Req. / Srv. Code", IncidentTriageSchema.IMT_Module);
			filters.AddTextFilter("Product", IncidentTriageSchema.IMT_Product, Lookups.ProductList);
			var checklistDesFilter = filters.AddTextFilter("Checklist Description", GetIsChecklistDescriptionQuery);
			checklistDesFilter.MaxLength = 160;
			var checklistCategoryFilter = filters.AddTextFilter("Checklist Category", GetIsChecklistCategoryQuery, IncidentTriageChecklistItemLookups.Categorys);
			checklistCategoryFilter.MaxLength = 3;
			var responseTypeFilter = filters.AddTextFilter("Checklist Response Type", GetIsChecklistResponseTypeQuery);
			responseTypeFilter.MaxLength = 3;
		}

		#endregion

		#region Flag Filters

		void AddFlagFilters(ModuleFilterCollection filters)
		{
			var isInternalFlag = ResString.GetMultilingualString("6672b588-d8c1-4106-aab4-1f9ba938d352", "Is Internal?");
			filters.AddFlagFilter("Is Internal?", isInternalFlag, IncidentTriageSchema.IMT_IsInternal, ModuleFilterSubGroup.Default)
				.MultilingualDescription = isInternalFlag;
			var isPublishedToERequestFlag = ResString.GetMultilingualString("964411ea-7ef3-42bb-8a87-5de224cc963a", "Is Published to eRequest");
			filters.AddFlagFilter("Is Published to eRequest", isPublishedToERequestFlag, IncidentTriageSchema.IMT_IsPublished, ModuleFilterSubGroup.Default)
				.MultilingualDescription = isPublishedToERequestFlag;
			var isChecklistPublishedFlag = ResString.GetMultilingualString("e621bed2-cc77-4b48-943e-11d17d148aaf", "Is Checklist Published?");
			filters.AddFlagsFilter("Is Checklist Published?", new string[] { isChecklistPublishedFlag }, new GetFlagsQuery[] { GetIsChecklistPublishedQuery });
			var isPublishedToTriageAssistFlag = ResString.GetMultilingualString("425c99cc-793b-476f-ba9a-66731a009eea", "Is Published to Triage Assist");
			filters.AddFlagFilter("Is Published to Triage Assist", isPublishedToTriageAssistFlag, IncidentTriageSchema.IMT_IsPublishedToAssist, ModuleFilterSubGroup.Default)
				.MultilingualDescription = isPublishedToTriageAssistFlag;
		}

		#endregion

		#region Guid Filters

		void AddGuidFilters(ModuleFilterCollection filters)
		{
			var checklistItemFilter = filters.AddGuidFilter("Checklist Item", ClientModuleRegistration.IncidentTriageChecklistItem, GetChecklistItemQuery, new IncidentTriageChecklistItemCollection(Factory));
			checklistItemFilter.MaxLength = 160;
		}

		#endregion

		#region Diagnostic Criteria Filters

		void AddDiagnosticCriteriaFilter(ModuleFilterCollection filters)
		{
			var diagnosticCriteriaFilter = new IncidentDiagnosticCriteriaModuleFilter("DiagnosticCriteria", ClientModuleRegistration.IncidentDiagnosticCriteria, IncidentTriageSchema.PK, IncidentTriageDiagnosticCriteriaPivotSchema.IMO_IMT_Triage, new IncidentDiagnosticCriteriaCollection(Factory), typeof(IncidentTriage));
			diagnosticCriteriaFilter.MultilingualDescription = ResString.GetMultilingualString("4a913a6b-fee8-49f8-b13a-038439cc2709", "Diagnostic Criteria");
			diagnosticCriteriaFilter.Category = new FilterCategory((NoResString)"Related Items");
			filters.AddFilter(diagnosticCriteriaFilter);
		}

		#endregion

		#region  GetChecklistItemQuery

		ZQuery GetChecklistItemQuery(ZGuid value)
		{
			var result = new ZDBOnlyQuery(typeof(IncidentTriage));
			var subQuery = new ZDBOnlySubQuery(typeof(IncidentTriageChecklistItemPivot), IncidentTriageChecklistItemPivotSchema.IMP_IMT_Triage);
			subQuery.AddToFilter(IncidentTriageChecklistItemPivotSchema.IMP_IMC_ChecklistItem, value);
			result.AddSubQuery(subQuery, JoinCondition.And);

			return result;
		}
		#endregion

		#region  GetIsChecklistDescriptionQuery

		ZQuery GetIsChecklistDescriptionQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var triageQuery = new ZDBOnlyQuery(typeof(IncidentTriage));
			var checklistPivotSubQuery = new ZDBOnlySubQuery(typeof(IncidentTriageChecklistItemPivot), IncidentTriageChecklistItemPivotSchema.IMP_IMT_Triage);
			var checklistSubQuery = new ZDBOnlySubQuery(typeof(IncidentTriageChecklistItem), IncidentTriageChecklistItemSchema.PK);
			checklistSubQuery.AddToFilter(IncidentTriageChecklistItemSchema.IMC_SupportDescription, comparisonOperator, value);
			checklistPivotSubQuery.AddSubQuery(IncidentTriageChecklistItemPivotSchema.IMP_IMC_ChecklistItem, checklistSubQuery, JoinCondition.And);
			triageQuery.AddSubQuery(IncidentTriageSchema.PK, checklistPivotSubQuery, JoinCondition.And);
			return triageQuery;
		}
		#endregion

		#region  GetIsChecklistCategoryQuery

		ZQuery GetIsChecklistCategoryQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var triageQuery = new ZDBOnlyQuery(typeof(IncidentTriage));
			var checklistPivotSubQuery = new ZDBOnlySubQuery(typeof(IncidentTriageChecklistItemPivot), IncidentTriageChecklistItemPivotSchema.IMP_IMT_Triage);
			var checklistSubQuery = new ZDBOnlySubQuery(typeof(IncidentTriageChecklistItem), IncidentTriageChecklistItemSchema.PK);
			checklistSubQuery.AddToFilter(IncidentTriageChecklistItemSchema.IMC_Category, comparisonOperator, value);
			checklistPivotSubQuery.AddSubQuery(IncidentTriageChecklistItemPivotSchema.IMP_IMC_ChecklistItem, checklistSubQuery, JoinCondition.And);
			triageQuery.AddSubQuery(IncidentTriageSchema.PK, checklistPivotSubQuery, JoinCondition.And);
			return triageQuery;
		}
		#endregion

		#region  GetIsChecklistResponseTypeQuery

		ZQuery GetIsChecklistResponseTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var triageQuery = new ZDBOnlyQuery(typeof(IncidentTriage));
			var checklistPivotSubQuery = new ZDBOnlySubQuery(typeof(IncidentTriageChecklistItemPivot), IncidentTriageChecklistItemPivotSchema.IMP_IMT_Triage);
			var checklistSubQuery = new ZDBOnlySubQuery(typeof(IncidentTriageChecklistItem), IncidentTriageChecklistItemSchema.PK);
			checklistSubQuery.AddToFilter(IncidentTriageChecklistItemSchema.IMC_ResponseType, comparisonOperator, value);
			checklistPivotSubQuery.AddSubQuery(IncidentTriageChecklistItemPivotSchema.IMP_IMC_ChecklistItem, checklistSubQuery, JoinCondition.And);
			triageQuery.AddSubQuery(IncidentTriageSchema.PK, checklistPivotSubQuery, JoinCondition.And);
			return triageQuery;
		}
		#endregion

		#region  GetIsChecklistPublishedQuery

		ZQuery GetIsChecklistPublishedQuery(ZBool value)
		{
			var triageQuery = new ZDBOnlyQuery(typeof(IncidentTriage));
			var checklistPivotSubQuery = new ZDBOnlySubQuery(typeof(IncidentTriageChecklistItemPivot), IncidentTriageChecklistItemPivotSchema.IMP_IMT_Triage);
			var checklistSubQuery = new ZDBOnlySubQuery(typeof(IncidentTriageChecklistItem), IncidentTriageChecklistItemSchema.PK);
			checklistSubQuery.AddToFilter(IncidentTriageChecklistItemSchema.IMC_IsPublished, value);
			checklistPivotSubQuery.AddSubQuery(IncidentTriageChecklistItemPivotSchema.IMP_IMC_ChecklistItem, checklistSubQuery, JoinCondition.And);
			triageQuery.AddSubQuery(IncidentTriageSchema.PK, checklistPivotSubQuery, JoinCondition.And);
			return triageQuery;
		}
		#endregion

		protected IncidentTriageLookups Lookups
		{
			get { return lookups ?? (lookups = GetNewLookups()); }
		}

		protected IncidentTriageLookups GetNewLookups()
		{
			return new IncidentTriageLookups(Factory);
		}

		IncidentTriageLookups lookups;

		IncidentTriageChecklistItemLookups IncidentTriageChecklistItemLookups => incidentTriageChecklistItemLookups ?? (incidentTriageChecklistItemLookups = new IncidentTriageChecklistItemLookups(Factory));
		IncidentTriageChecklistItemLookups incidentTriageChecklistItemLookups;

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			return new ModuleFountainFilter("Triage Number", IncidentTriageSchema.IMT_TriageNumber, "TRI");
		}
	}
}
