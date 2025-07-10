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
	public class RelatedWorkItemsOfIncidentFilter : GenPivotDualDirectionRelatedEntityFilter
	{
		public RelatedWorkItemsOfIncidentFilter(ZString description, GetList listDelegate)
			: base(description, listDelegate, typeof(SupportIncident), IncidentMainSchema.Constants.Prefix, WorkItemSchema.Constants.Prefix, ModuleIDs.WorkItem)
		{
		}

		RelatedWorkItemsOfIncidentFilter(ZString description, GetList listDelegate, SchemaGuidColumn fromColumn, SchemaGuidColumn toColumn, Type parentBusinessObjectType, string parentTablePrefix, string relatedTablePrefix, ModuleIdentifier otherModuleID)
			: base(description, listDelegate, fromColumn, toColumn, parentBusinessObjectType, parentTablePrefix, relatedTablePrefix, otherModuleID)
		{
		}

		protected RelatedWorkItemsOfIncidentFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		protected override SchemaGuidColumn ParentPKColumn => IncidentMainSchema.PK;
		protected override SchemaColumn SubQueryColumn => WorkItemSchema.PK;

		protected override MultilingualString GetMultilingualDescription() => ResString.GetMultilingualString("d6cb54a0-40ff-4b26-a7cb-6036911feb65", "Related Work Items");

		protected override GenPivotDualDirectionRelatedEntityFilter CreateFilterForOtherPivotDirection(ZString description, GetList listDelegate, SchemaGuidColumn fromColumn, SchemaGuidColumn toColumn, Type parentBusinessObjectType, string parentTablePrefix, string relatedTablePrefix, ModuleIdentifier otherModuleID)
		{
			return new RelatedWorkItemsOfIncidentFilter(description, listDelegate, fromColumn, toColumn, parentBusinessObjectType, parentTablePrefix, relatedTablePrefix, otherModuleID);
		}
	}
}
