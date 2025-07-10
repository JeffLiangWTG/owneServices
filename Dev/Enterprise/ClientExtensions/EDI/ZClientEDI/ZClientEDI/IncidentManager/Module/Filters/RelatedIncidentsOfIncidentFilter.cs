using System;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ProcessManagement.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class RelatedIncidentsOfIncidentFilter : GenPivotDualDirectionRelatedEntityFilter
	{
		public RelatedIncidentsOfIncidentFilter(ZString description, GetList listDelegate)
			: base(description, listDelegate, typeof(SupportIncident), IncidentMainSchema.Constants.Prefix, IncidentMainSchema.Constants.Prefix, ClientModuleRegistration.SupportIncident)
		{
		}

		RelatedIncidentsOfIncidentFilter(ZString description, GetList listDelegate, SchemaGuidColumn fromColumn, SchemaGuidColumn toColumn, Type parentBusinessObjectType, string parentTablePrefix, string relatedTablePrefix, ModuleIdentifier otherModuleID)
			: base(description, listDelegate, fromColumn, toColumn, parentBusinessObjectType, parentTablePrefix, relatedTablePrefix, otherModuleID)
		{
		}

		protected RelatedIncidentsOfIncidentFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		protected override SchemaGuidColumn ParentPKColumn => IncidentMainSchema.PK;
		protected override SchemaColumn SubQueryColumn => IncidentMainSchema.PK;

		protected override MultilingualString GetMultilingualDescription() => ResString.GetMultilingualString("0E3B60DF-8DBC-4814-A30D-B1503A86E3B4", "Related Incidents");

		protected override GenPivotDualDirectionRelatedEntityFilter CreateFilterForOtherPivotDirection(ZString description, GetList listDelegate, SchemaGuidColumn fromColumn, SchemaGuidColumn toColumn, Type parentBusinessObjectType, string parentTablePrefix, string relatedTablePrefix, ModuleIdentifier otherModuleID)
		{
			return new RelatedIncidentsOfIncidentFilter(description, listDelegate, fromColumn, toColumn, parentBusinessObjectType, parentTablePrefix, relatedTablePrefix, otherModuleID);
		}
	}
}
