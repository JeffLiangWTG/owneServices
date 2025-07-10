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
	public class RelatedProjectsOfIncidentFilter : GenPivotDualDirectionRelatedEntityFilter
	{
		public RelatedProjectsOfIncidentFilter(ZString description, GetList listDelegate)
			: base(description, listDelegate, typeof(SupportIncident), IncidentMainSchema.Constants.Prefix, WorkProjectSchema.Constants.Prefix, ModuleIDs.Project)
		{
		}

		RelatedProjectsOfIncidentFilter(ZString description, GetList listDelegate, SchemaGuidColumn fromColumn, SchemaGuidColumn toColumn, Type parentBusinessObjectType, string parentTablePrefix, string relatedTablePrefix, ModuleIdentifier otherModuleID)
			: base(description, listDelegate, fromColumn, toColumn, parentBusinessObjectType, parentTablePrefix, relatedTablePrefix, otherModuleID)
		{
		}

		protected RelatedProjectsOfIncidentFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		protected override SchemaGuidColumn ParentPKColumn => IncidentMainSchema.PK;
		protected override SchemaColumn SubQueryColumn => WorkProjectSchema.PK;

		protected override MultilingualString GetMultilingualDescription() => ResString.GetMultilingualString("9f335470-a03a-49f1-ac22-7f0a51a3c0d1", "Related Projects");

		protected override GenPivotDualDirectionRelatedEntityFilter CreateFilterForOtherPivotDirection(ZString description, GetList listDelegate, SchemaGuidColumn fromColumn, SchemaGuidColumn toColumn, Type parentBusinessObjectType, string parentTablePrefix, string relatedTablePrefix, ModuleIdentifier otherModuleID)
		{
			return new RelatedProjectsOfIncidentFilter(description, listDelegate, fromColumn, toColumn, parentBusinessObjectType, parentTablePrefix, relatedTablePrefix, otherModuleID);
		}
	}
}
