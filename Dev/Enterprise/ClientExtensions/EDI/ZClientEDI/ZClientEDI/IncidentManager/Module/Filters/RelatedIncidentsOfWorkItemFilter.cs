using System;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ProcessManagement.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class RelatedIncidentsOfWorkItemFilter : GenPivotDualDirectionRelatedEntityFilter
	{
		public RelatedIncidentsOfWorkItemFilter(ZString description, GetList listDelegate)
			: base(description, listDelegate, typeof(NewWorkItem), WorkItemSchema.Constants.Prefix, IncidentMainSchema.Constants.Prefix, Modules.ClientModuleRegistration.SupportIncident)
		{
			Category = WorkItemFilterBusinessObject.RelatedItemsFilterCategory;
		}

		RelatedIncidentsOfWorkItemFilter(ZString description, GetList listDelegate, SchemaGuidColumn fromColumn, SchemaGuidColumn toColumn, Type parentBusinessObjectType, string parentTablePrefix, string relatedTablePrefix, ModuleIdentifier otherModuleID)
			: base(description, listDelegate, fromColumn, toColumn, parentBusinessObjectType, parentTablePrefix, relatedTablePrefix, otherModuleID)
		{
		}

		protected RelatedIncidentsOfWorkItemFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		protected override SchemaGuidColumn ParentPKColumn => WorkItemSchema.PK;
		protected override SchemaColumn SubQueryColumn => IncidentMainSchema.PK;

		protected override MultilingualString GetMultilingualDescription() => ResString.GetMultilingualString("d7683ea8-e8fe-4162-8099-6a2c296538e7", "Related Incidents");

		protected override GenPivotDualDirectionRelatedEntityFilter CreateFilterForOtherPivotDirection(ZString description, GetList listDelegate, SchemaGuidColumn fromColumn, SchemaGuidColumn toColumn, Type parentBusinessObjectType, string parentTablePrefix, string relatedTablePrefix, ModuleIdentifier otherModuleID)
		{
			return new RelatedIncidentsOfWorkItemFilter(description, listDelegate, fromColumn, toColumn, parentBusinessObjectType, parentTablePrefix, relatedTablePrefix, otherModuleID);
		}
	}
}
