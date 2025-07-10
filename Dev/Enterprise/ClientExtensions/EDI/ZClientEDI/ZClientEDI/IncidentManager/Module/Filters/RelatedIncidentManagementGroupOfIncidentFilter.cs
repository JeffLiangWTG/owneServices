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
	public class RelatedIncidentManagementGroupOfIncidentFilter : GenPivotDualDirectionRelatedEntityFilter
	{
		public RelatedIncidentManagementGroupOfIncidentFilter(ZString description, GetList listDelegate)
			: base(description, listDelegate, typeof(SupportIncident), IncidentMainSchema.Constants.Prefix, IncidentManagementGroupSchema.Constants.Prefix, ClientModuleRegistration.IncidentManagementGroup)
		{
		}

		RelatedIncidentManagementGroupOfIncidentFilter(ZString description, GetList listDelegate, SchemaGuidColumn fromColumn, SchemaGuidColumn toColumn, Type parentBusinessObjectType, string parentTablePrefix, string relatedTablePrefix, ModuleIdentifier otherModuleID)
			: base(description, listDelegate, fromColumn, toColumn, parentBusinessObjectType, parentTablePrefix, relatedTablePrefix, otherModuleID)
		{
		}

		protected RelatedIncidentManagementGroupOfIncidentFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		protected override SchemaGuidColumn ParentPKColumn => IncidentMainSchema.PK;
		protected override SchemaColumn SubQueryColumn => IncidentManagementGroupSchema.PK;

		protected override MultilingualString GetMultilingualDescription() => ResString.GetMultilingualString("FC4861DA-F9C1-4C1D-BDE2-B5D81E7C87E3", "Related Incident Management Groups");

		protected override GenPivotDualDirectionRelatedEntityFilter CreateFilterForOtherPivotDirection(ZString description, GetList listDelegate, SchemaGuidColumn fromColumn, SchemaGuidColumn toColumn, Type parentBusinessObjectType, string parentTablePrefix, string relatedTablePrefix, ModuleIdentifier otherModuleID)
		{
			return new RelatedIncidentManagementGroupOfIncidentFilter(description, listDelegate, fromColumn, toColumn, parentBusinessObjectType, parentTablePrefix, relatedTablePrefix, otherModuleID);
		}
	}
}
