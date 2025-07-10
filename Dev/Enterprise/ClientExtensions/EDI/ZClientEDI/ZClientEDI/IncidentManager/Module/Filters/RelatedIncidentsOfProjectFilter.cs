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
	public class RelatedIncidentsOfProjectFilter : GenPivotDualDirectionRelatedEntityFilter
	{
		public RelatedIncidentsOfProjectFilter(ZString description, GetList listDelegate)
			: base(description, listDelegate, typeof(EDIProject), WorkProjectSchema.Constants.Prefix, IncidentMainSchema.Constants.Prefix, Modules.ClientModuleRegistration.SupportIncident)
		{
		}

		RelatedIncidentsOfProjectFilter(ZString description, GetList listDelegate, SchemaGuidColumn fromColumn, SchemaGuidColumn toColumn, Type parentBusinessObjectType, string parentTablePrefix, string relatedTablePrefix, ModuleIdentifier otherModuleID)
			: base(description, listDelegate, fromColumn, toColumn, parentBusinessObjectType, parentTablePrefix, relatedTablePrefix, otherModuleID)
		{
		}

		protected RelatedIncidentsOfProjectFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		protected override SchemaGuidColumn ParentPKColumn => WorkProjectSchema.PK;
		protected override SchemaColumn SubQueryColumn => IncidentMainSchema.PK;

		protected override MultilingualString GetMultilingualDescription() => ResString.GetMultilingualString("cbca7788-5e18-495d-8031-1b49ee6ae5dc", "Related Incidents");

		protected override GenPivotDualDirectionRelatedEntityFilter CreateFilterForOtherPivotDirection(ZString description, GetList listDelegate, SchemaGuidColumn fromColumn, SchemaGuidColumn toColumn, Type parentBusinessObjectType, string parentTablePrefix, string relatedTablePrefix, ModuleIdentifier otherModuleID)
		{
			return new RelatedIncidentsOfProjectFilter(description, listDelegate, fromColumn, toColumn, parentBusinessObjectType, parentTablePrefix, relatedTablePrefix, otherModuleID);
		}
	}
}
